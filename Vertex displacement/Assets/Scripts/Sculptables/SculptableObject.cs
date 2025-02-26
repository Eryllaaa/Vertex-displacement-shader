using System.Collections.Generic;
using System;
using UnityEngine;

public class SculptableObject : MonoBehaviour
{
    [SerializeField, Range(0.1f, 10f)] private float _displacementDistance = 1f;
    private float MaxDisplacementDistance
    {
        get { return _displacementDistance; }
        set
        {
            _displacementDistance = value;
            _computeShaderInstance.SetFloat("maxDisplacementDistance", value);
        }
    }

    [SerializeField] private ComputeShader _computeShaderTemplate; // the template compute shader that writes to the displacement buffer
    [SerializeField] private Material _tesselationMaterialTemplate; // the template compute shader that writes to the displacement buffer
    private ComputeShader _computeShaderInstance; // the actual instance of the compute shader that will run on this mesh
    private Material _tesselationMaterialInstance;

    private const int _THREAD_PER_GROUP = 256;
    private int _groupCount = 0; // should change based on the mesh's vertex count
    private int _vertexCount = 0;

    // compute bufffers

    // updated every frame
    private ComputeBuffer _sculptPointsBuffer;
    private ComputeBuffer _displacementBuffer;
    private ComputeBuffer _debugBuffer;
    private ComputeBuffer _vertexBuffer;
    // only at start
    private ComputeBuffer _startNormalsBuffer;
    private ComputeBuffer _verticesStartPosBuffer;

    private Mesh _mesh;
    private MeshFilter _meshFilter;

    private List<SculptPoint> _sculptPoints = new List<SculptPoint>() { new SculptPoint()};
    private Vector3[] _startNormals;

    private int CSMainKernelID;

    #region NAME CONSTANTS
    private const string _CS_MAIN_KERNEL = "CSMain";

    private const string _SCULPT_POINTS_BUFFER = "sculptPoints";
    private const string _SCULPT_POINTS_COUNT = "sculptPointsCount";

    private const string _VERTEX_COUNT = "vertexCount";
    private const string _VERTICES_BUFFER = "vertices";

    private const string _VERTICES_START_POS_BUFFER = "verticesStartPos";
    private const string _START_NORMALS_BUFFER = "startNormals";

    private const string _DISPLACEMENTS_BUFFER = "displacements";

    private const string _DEBUG_BUFFER = "debugBuffer";

    private const string _DISPLACEMENT_DISTANCE = "displacementDistance";

    private const string _PREVIOUS_SCULPT_POS = "previousSculptPos";
    #endregion

    public void OnHit(RaycastHit pHit)
    {
        Vector3 lLocalPos = transform.InverseTransformPoint(pHit.point);
        //AddDisplacementPoint(lLocalPos);
        SetDisplacementPoint(lLocalPos);
        print("hit at : " + lLocalPos);
    }

    private void OnValidate()
    {
        SetDisplacementMaterial();
        if (_computeShaderInstance != null) _computeShaderInstance.SetFloat(_DISPLACEMENT_DISTANCE, _displacementDistance);
    }

    /// <summary>
    /// Insures taht the displacement material is in the sharedMaterials array and at index 0.
    /// </summary>
    private void SetDisplacementMaterial()
    {
        //MeshRenderer _meshRenderer = GetComponent<MeshRenderer>();
        //_tesselationMaterialInstance = Instantiate(_tesselationMaterialTemplate);
        //if (!_meshRenderer.sharedMaterials.Contains(_tesselationMaterialTemplate))
        //{
        //    Material[] lMaterials = new Material[_meshRenderer.sharedMaterials.Length + 1];
        //    for (int i = 0; i < _meshRenderer.sharedMaterials.Length; i++)
        //    {   
        //        lMaterials[i] = _meshRenderer.sharedMaterials[i];
        //    }
        //    lMaterials[lMaterials.Length - 1] = _tesselationMaterialInstance;
        //    _meshRenderer.sharedMaterials = lMaterials;
        //}
    }

    private void Start()
    {
        StartComponents();
        StartComputeShader();
        StartBuffers();
    }

    private void StartComponents()
    {
        _meshFilter = GetComponent<MeshFilter>();
        Mesh lMesh = Instantiate(_meshFilter.sharedMesh); // Clone the mesh
        _meshFilter.mesh = lMesh;
        _mesh = _meshFilter.mesh;

        _vertexCount = _mesh.vertexCount;
        _startNormals = _mesh.normals;
        SetGroupCount(_vertexCount);
    }

    private void StartComputeShader()
    {
        _computeShaderInstance = Instantiate(_computeShaderTemplate);
        CSMainKernelID = _computeShaderInstance.FindKernel(_CS_MAIN_KERNEL);
    }

    private void StartBuffers()
    {
        // displacement points buffer
        _sculptPointsBuffer = new ComputeBuffer(1, sizeof(float) * 6);
        _computeShaderInstance.SetBuffer(CSMainKernelID, _SCULPT_POINTS_BUFFER, _sculptPointsBuffer);

        // vertices buffer
        _vertexBuffer = new ComputeBuffer(_vertexCount, sizeof(float) * 3);
        _computeShaderInstance.SetBuffer(CSMainKernelID, _VERTICES_BUFFER, _vertexBuffer);
        _vertexBuffer.SetData(_mesh.vertices);

        // vertices start pos buffer
        _verticesStartPosBuffer = new ComputeBuffer(_vertexCount, sizeof(float) * 3);
        _computeShaderInstance.SetBuffer(CSMainKernelID, _VERTICES_START_POS_BUFFER, _verticesStartPosBuffer);
        _verticesStartPosBuffer.SetData(_mesh.vertices);

        // displacement buffer
        _displacementBuffer = new ComputeBuffer(_vertexCount, sizeof(float));
        _computeShaderInstance.SetBuffer(CSMainKernelID, _DISPLACEMENTS_BUFFER, _displacementBuffer);
        _displacementBuffer.SetData(new float[_vertexCount]);

        // start normals buffer
        _startNormalsBuffer = new ComputeBuffer(_vertexCount, sizeof(float) * 3);
        _computeShaderInstance.SetBuffer(CSMainKernelID, _START_NORMALS_BUFFER, _startNormalsBuffer);
        _startNormalsBuffer.SetData(_mesh.normals);

        // debug buffer
        _debugBuffer = new ComputeBuffer(10, sizeof(float) * 3);
        _computeShaderInstance.SetBuffer(CSMainKernelID, _DEBUG_BUFFER, _debugBuffer);
        Vector3[] lDebugArray = new Vector3[10];
        _debugBuffer.SetData(lDebugArray);

        // set values on compute shader
        _computeShaderInstance.SetInt(_VERTEX_COUNT, _vertexCount);
        _computeShaderInstance.SetInt(_SCULPT_POINTS_COUNT, 1);
        _computeShaderInstance.SetFloat(_DISPLACEMENT_DISTANCE, _displacementDistance);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1)) ClearDisplacementPoints();
        SendDisplacementPoints();
        _computeShaderInstance.Dispatch(CSMainKernelID, _groupCount, 1, 1);
        ApplyDisplacementToMesh();
    }

    private void ApplyDisplacementToMesh()
    {
        Vector3[] lVertices = new Vector3[_vertexCount];
        _vertexBuffer.GetData(lVertices);
        _mesh.vertices = lVertices;

        _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();
    }

    private void SetGroupCount(int pVertexCount)
    {
        _groupCount = Math.Max(1, Mathf.CeilToInt(pVertexCount / _THREAD_PER_GROUP));
    }

    private void SendDisplacementPoints()
    {
        print(_previousSculptPointPos);
        _computeShaderInstance.SetVector(_PREVIOUS_SCULPT_POS, _previousSculptPointPos);
        _computeShaderInstance.SetInt(_SCULPT_POINTS_COUNT, _sculptPoints.Count);

        _sculptPointsBuffer = new ComputeBuffer(_sculptPoints.Count, sizeof(float) * 6);
        UpdateSculptPoints();
        _sculptPointsBuffer.SetData(_sculptPoints);
        _computeShaderInstance.SetBuffer(CSMainKernelID, _SCULPT_POINTS_BUFFER, _sculptPointsBuffer);
    }

    private void UpdateSculptPoints()
    {
        float lDeltaTime = Time.deltaTime;
        float lLen = _sculptPoints.Count;
        SculptPoint lPoint;
        for (int i = 0; i < lLen; i++)
        {
            lPoint = new SculptPoint(_sculptPoints[i]);
            lPoint.ratio = Mathf.Clamp(lPoint.ratio + Time.deltaTime, 0f, 1f);
            _sculptPoints[i] = lPoint;
        }
    }

    private Vector3 _previousSculptPointPos = Vector3.zero;
    public void AddDisplacementPoint(Vector3 pSculptPosition)
    {
        if (_sculptPoints.Count >= 999)
        {
            print("too many hits");
            return;
        }

        SculptPoint lSculptPoint = new SculptPoint(pSculptPosition, 0.25f, SculptDirection.up);
        
        _previousSculptPointPos = _sculptPoints[_sculptPoints.Count - 1].position;
        _sculptPoints.Add(lSculptPoint);
    }

    public void SetDisplacementPoint(Vector3 pSculptPosition)
    {
        _previousSculptPointPos = _sculptPoints[_sculptPoints.Count - 1].position;
        SculptPoint lSculptPoint = new SculptPoint(pSculptPosition, 0.25f, SculptDirection.up);
        if (_sculptPoints.Count < 2) _sculptPoints.Add(lSculptPoint);
        else
        {
            lSculptPoint = new SculptPoint(_sculptPoints[1]);
            lSculptPoint.position = pSculptPosition;
            _sculptPoints[1] = lSculptPoint;
        } 
    }

    public void ClearDisplacementPoints()
    {
        float[] lDisplacementReset = new float[_vertexCount];
        _displacementBuffer.SetData(lDisplacementReset);
        _sculptPoints.Clear();
        _sculptPoints.Add(new SculptPoint());
    }

    private void SetDisplacementDistance(float pValue)
    {
        _displacementDistance = pValue;
        _computeShaderInstance.SetFloat(_DISPLACEMENT_DISTANCE, pValue);
    }

    private void OnDestroy()
    {
        _sculptPointsBuffer.Release();
        _vertexBuffer.Release();
        if (_debugBuffer != null)_debugBuffer.Release();
    }
}
