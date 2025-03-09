using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Mesh), typeof(MeshFilter), typeof(MeshCollider))]
public class SculptableObject2 : MonoBehaviour
{
    [SerializeField, Range(0.1f, 10f)] private float _maxDisplacement = 1f;

    #region Materials and Shaders
    [SerializeField] private ComputeShader _computeShaderTemplate;
    [SerializeField] private MeshCollider _physicsCollider; // not the raycast collider but the collider used for physics with the ball that is a child of this game object.

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

    private void Start()
    {
        StartComponents();
        StartComputeShader();
        StartCoroutine(ColliderUpdate());
    }

    private void StartComponents()
    {
        _meshFilter = GetComponent<MeshFilter>();

        Mesh lMesh = Instantiate(_meshFilter.sharedMesh); // Clone the mesh

        _mesh = lMesh;
        _meshFilter.mesh = _mesh;
        _physicsCollider.sharedMesh = _mesh;

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

    private SculptHit2 _currentHit = SculptHit2.none;

    private void Update()
    {
        if (Input.GetMouseButton(1)) ResetDisplacement();
        
        _computeShader.SetFloat(_DELTA_TIME, Time.deltaTime);
        _computeShader.SetVector(_PREVIOUS_SCULPT_POS, _previousHitPos);

        _computeShader.Dispatch(CSMainKernelId, _groupCount, 1, 1);

        ClearSculptHit();

        ApplyDisplacementToMesh();
    }

    private float diff = 0;
    private void ApplyDisplacementToMesh()
    {
        Vector3[] lVertices = new Vector3[_vertexCount];

        _vertexBuffer.GetData(lVertices);
        _mesh.vertices = lVertices;

        _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();
    }

    private IEnumerator ColliderUpdate()
    {
        while (true)
        {
            _physicsCollider.sharedMesh = _mesh;
            yield return new WaitForSeconds(0.005f);
        }
    }

    private void SendSculptHit(SculptHit2 lHit)
    {
        if (_computeShader == null)
        {
            print("NO COMPUTE SHADER INSTANCE");
            return;
        }
        _computeShader.SetVector(_CURRENT_SCULPT_POS, transform.InverseTransformPoint(lHit.position));
        _computeShader.SetInt(_CURRENT_SCULPT_DIR, lHit.direction);
        // scaled radius
        _computeShader.SetFloat(_CURRENT_SCULPT_RADIUS, lHit.radius / transform.localScale.magnitude);
        _computeShader.SetFloat(_SCULPT_SPEED, lHit.speed);
    }

    private void ClearSculptHit()
    {
        SculptHit2 lHit = SculptHit2.none;
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
        _targetDisplacementsBuffer.SetData(lDefaultDisplacement);
    }

    public void SetMaxDisplacement(float pValue)
    {
        _maxDisplacement = pValue;
        if (_computeShader != null) _computeShader.SetFloat(_MAX_DISPLACEMENT, _maxDisplacement);
    }

    private Vector3 _previousHitPos = Vector3.zero;
    public void OnHit(SculptHit2 pHit)
    {
        _currentHit = pHit;
        _previousHitPos = transform.InverseTransformPoint(_currentHit.previousPos);
        SendSculptHit(_currentHit);
    }

    #region Destroy
    private void OnDestroy()
    {
        _computeShader = null;
        _displacementsBuffer.Release();
        _vertexBuffer.Release();
        _startNormalsBuffer.Release();
        _verticesStartPosBuffer.Release();
        _targetDisplacementsBuffer.Release();
    }
    #endregion
}
