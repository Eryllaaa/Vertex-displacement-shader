using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Mesh), typeof(MeshFilter), typeof(MeshCollider))]
public class SculptableObject : MonoBehaviour
{
    [SerializeField, Range(0.1f, 10f)] private float _maxDisplacement = 1f;
    [SerializeField] private bool _autoStart = false;

    private bool _sculptingEnabled = false;

    #region Materials and Shaders
    [SerializeField] private ComputeShader _computeShaderTemplate;
    [SerializeField] private MeshCollider _physicsCollider; // not the raycast collider but the collider used for physics with the ball that is a child of this game object.

    private MeshCollider _clickCollider;
    private Mesh _mesh;
    private MeshFilter _meshFilter;
    #endregion

    #region Compute shader values
    private ComputeShader _computeShader;

    private int CSMainKernelId;

    private const int _THREAD_PER_GROUP = 256;
    private int _groupCount = 0; // should change based on the mesh's vertex count
    private int _vertexCount = 0;

    private Vector3[] _startNormals;
    private Vector3[] _startVertexPos;

    // updated every frame
    private ComputeBuffer _displacementsBuffer;
    private ComputeBuffer _vertexBuffer;
    // only at start
    private ComputeBuffer _startNormalsBuffer;
    private ComputeBuffer _targetDisplacementsBuffer;
    private ComputeBuffer _verticesStartPosBuffer;
    #endregion

    #region Constants
    private const string _CS_MAIN_KERNEL = "CSMain";

    // Buffers
    private const string _VERTICES_BUFFER = "vertices";
    private const string _VERTICES_START_POS_BUFFER = "verticesStartPos";
    private const string _START_NORMALS_BUFFER = "startNormals";
    private const string _DISPLACEMENTS_BUFFER = "displacements";
    private const string _TARGET_DISPLACEMENTS_BUFFER = "targetDisplacements";

    // Values
    private const string _CURRENT_SCULPT_POS = "currentPos";
    private const string _CURRENT_SCULPT_DIR = "currentDir";
    private const string _CURRENT_SCULPT_RADIUS = "currentRadius";
    private const string _MAX_DISPLACEMENT = "maxDisplacement";
    private const string _DELTA_TIME = "deltaTime";
    private const string _SCULPT_SPEED = "sculptSpeed";
    private const string _PREVIOUS_SCULPT_POS = "prevPos";
    #endregion

    #region Initialization
    private void OnValidate()
    {
        if (_computeShader != null)
        {
            _computeShader.SetFloat(_MAX_DISPLACEMENT, _maxDisplacement);
        }
    }
    
    public void SetSculptableActive(bool pActivate)
    {
        if (pActivate)
        {
            ActivateSculptable();
        }
        else
        {
            DeactivateSculptable();
        }
    }

    private void ActivateSculptable()
    {
        _sculptingEnabled = true;
        StartComputeShader();

        if (_slowMeshUpdate != null) StopCoroutine(_slowMeshUpdate);
        StartCoroutine(_slowMeshUpdate = SlowMeshUpdate());

        if (_slowColliderUpdate != null) StopCoroutine(_slowColliderUpdate);
        StartCoroutine(_slowColliderUpdate = SlowColliderUpdate());

        BindToReset();
    }

    private void Start()
    {
        StartComponents();
        if (_autoStart)
        {
            ActivateSculptable();
        }
    }

    private void BindToReset()
    {
        InputReader.Instance.DebugResetTerrainAction.started += CallResetDisplacement;
    }

    private void CallResetDisplacement(InputAction.CallbackContext pContext)
    {
        ResetDisplacement();
    }

    private void StartComponents()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _clickCollider = GetComponent<MeshCollider>();
        Mesh lMesh = Instantiate(_meshFilter.sharedMesh); // Clone the mesh

        _mesh = lMesh;
        _meshFilter.mesh = _mesh;
        _physicsCollider.sharedMesh = _mesh;
        _clickCollider.sharedMesh = _mesh; // make sure the area we can click on is the same shape and size as the object area.

        _vertexCount = _mesh.vertexCount;
        _startNormals = _mesh.normals;
        _startVertexPos = _mesh.vertices;

        SetGroupCount(_vertexCount);
    }

    private void SetGroupCount(int pVertexCount)
    {
        _groupCount = Math.Max(1, Mathf.CeilToInt(pVertexCount / _THREAD_PER_GROUP));
    }

    private void StartComputeShader()
    {
        _computeShader = Instantiate(_computeShaderTemplate);
        CSMainKernelId = _computeShader.FindKernel(_CS_MAIN_KERNEL);

        // --- INITIALIZE VALUES ---
        _computeShader.SetVector(_CURRENT_SCULPT_POS, Vector3.zero);
        _computeShader.SetInt(_CURRENT_SCULPT_DIR, 0);
        _computeShader.SetFloat(_CURRENT_SCULPT_RADIUS, 0f);
        _computeShader.SetFloat(_MAX_DISPLACEMENT, _maxDisplacement);

        // --- INITIALIZE BUFFERS ---

        // vertices buffer
        _vertexBuffer = new ComputeBuffer(_vertexCount, sizeof(float) * 3);
        _computeShader.SetBuffer(CSMainKernelId, _VERTICES_BUFFER, _vertexBuffer);
        _vertexBuffer.SetData(_mesh.vertices);

        // vertices start pos buffer
        _verticesStartPosBuffer = new ComputeBuffer(_vertexCount, sizeof(float) * 3);
        _computeShader.SetBuffer(CSMainKernelId, _VERTICES_START_POS_BUFFER, _verticesStartPosBuffer);
        _verticesStartPosBuffer.SetData(_mesh.vertices);

        // start normals buffer
        _startNormalsBuffer = new ComputeBuffer(_vertexCount, sizeof(float) * 3);
        _computeShader.SetBuffer(CSMainKernelId, _START_NORMALS_BUFFER, _startNormalsBuffer);
        _startNormalsBuffer.SetData(_mesh.normals);

        // displacement buffer
        _displacementsBuffer = new ComputeBuffer(_vertexCount, sizeof(float));
        _computeShader.SetBuffer(CSMainKernelId, _DISPLACEMENTS_BUFFER, _displacementsBuffer);
        _displacementsBuffer.SetData(new float[_vertexCount]);

        // target displacement buffer
        _targetDisplacementsBuffer = new ComputeBuffer(_vertexCount, sizeof(float));
        _computeShader.SetBuffer(CSMainKernelId, _TARGET_DISPLACEMENTS_BUFFER, _targetDisplacementsBuffer);
        _targetDisplacementsBuffer.SetData(new float[_vertexCount]);
    }
    #endregion

    private SculptHit _currentHit = SculptHit.none;
    private Vector3 _previousHitPos = Vector3.zero;

    private IEnumerator _slowMeshUpdate = null;
    private IEnumerator SlowMeshUpdate()
    {
        const float FIXED_STEP = 0.02f;
        while (true)
        {
            _computeShader.SetFloat(_DELTA_TIME, FIXED_STEP);
            _computeShader.SetVector(_PREVIOUS_SCULPT_POS, _previousHitPos);

            _computeShader.Dispatch(CSMainKernelId, _groupCount, 1, 1);

            ClearSculptHit();

            ApplyDisplacementToMesh();

            yield return new WaitForSeconds(FIXED_STEP);
        }
    }

    private void ApplyDisplacementToMesh()
    {
        Vector3[] lVertices = new Vector3[_vertexCount];

        _vertexBuffer.GetData(lVertices);
        _mesh.vertices = lVertices;
    
        _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();
    }

    private IEnumerator _slowColliderUpdate = null;
    private IEnumerator SlowColliderUpdate()
    {
        const float FIXED_STEP = 0.05f;
        while (true)
        {
            _physicsCollider.sharedMesh = null;
            _physicsCollider.sharedMesh = _mesh;
            yield return new WaitForSeconds(FIXED_STEP);
        }
    }

    private void SendSculptHit(SculptHit lHit)
    {
        if (_computeShader == null) { print("NO COMPUTE SHADER INSTANCE"); return; }

        _computeShader.SetVector(_CURRENT_SCULPT_POS, transform.InverseTransformPoint(lHit.position));
        //print(name + ": " + transform.InverseTransformPoint(lHit.position));
        _computeShader.SetInt(_CURRENT_SCULPT_DIR, lHit.direction);
        _computeShader.SetFloat(_CURRENT_SCULPT_RADIUS, lHit.radius / transform.localScale.magnitude); // scaled radius
        _computeShader.SetFloat(_SCULPT_SPEED, lHit.speed);
    }

    private void ClearSculptHit()
    {
        SculptHit lHit = SculptHit.none;
        _computeShader.SetVector(_CURRENT_SCULPT_POS, lHit.position);
        _computeShader.SetInt(_CURRENT_SCULPT_DIR, lHit.direction);
        _computeShader.SetFloat(_CURRENT_SCULPT_RADIUS, lHit.radius);
    }

    private void ResetDisplacement()
    {
        float[] lDefaultDisplacement = new float[_vertexCount];
        for (int i = 0; i < _vertexCount; i++)
        {
            lDefaultDisplacement[i] = 0f;
        }
        if (_targetDisplacementsBuffer != null) _targetDisplacementsBuffer.SetData(lDefaultDisplacement);
    }

    public void SetMaxDisplacement(float pValue)
    {
        _maxDisplacement = pValue;
        if (_computeShader != null) _computeShader.SetFloat(_MAX_DISPLACEMENT, _maxDisplacement);
    }

    public void OnHit(SculptHit pHit)
    {
        if (!_sculptingEnabled) return;

        _currentHit = pHit;
        _previousHitPos = transform.InverseTransformPoint(_currentHit.previousPos);
        SendSculptHit(_currentHit);
    }

    #region Destroy
    private void OnDestroy()
    {
        DeactivateSculptable(true);
    }

    private void DeactivateSculptable(bool pOnDestroy = false)
    {
        _sculptingEnabled = false;

        if (!pOnDestroy) StartCoroutine(ReleaseBuffersAndShadersAndSlowUpdates());
        else ReleaseBuffersAndShadersAndSlowUpdates();

        UnbindToReset();
    }

    private IEnumerator ReleaseBuffersAndShadersAndSlowUpdates()
    {
        ResetDisplacement();

        yield return new WaitForSeconds(1.0f);

        _computeShader = null;
        if (_displacementsBuffer != null) _displacementsBuffer.Release();
        if (_vertexBuffer != null) _vertexBuffer.Release();
        if (_startNormalsBuffer != null) _startNormalsBuffer.Release();
        if (_verticesStartPosBuffer != null) _verticesStartPosBuffer.Release();
        if (_targetDisplacementsBuffer != null) _targetDisplacementsBuffer.Release();

        if (_slowMeshUpdate != null) StopCoroutine(_slowMeshUpdate);
        if (_slowColliderUpdate != null) StopCoroutine(_slowColliderUpdate);
    }

    private void UnbindToReset()
    {
        InputReader.Instance.DebugResetTerrainAction.started -= CallResetDisplacement;
    }
    #endregion
}
