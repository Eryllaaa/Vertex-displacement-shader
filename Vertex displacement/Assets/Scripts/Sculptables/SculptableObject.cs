using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;

public class SculptableObject : MonoBehaviour
{
    [SerializeField] private ComputeShader _computeShaderTemplate; // the template compute shader that writes to the displacement buffer
    private ComputeShader _computeShaderInstance; // the actual instance of the compute shader that will run on this mesh

    private const int _THREAD_PER_GROUP = 256;
    private int _groupCount = 0; // should change based on the mesh's vertex count
    private int _vertexCount = 0;

    // compute bufffers
    private ComputeBuffer _displacementBuffer;
    private ComputeBuffer _displacementPointsBuffer;
    private ComputeBuffer _vertexBuffer;
    private ComputeBuffer _debugBuffer;

    private Mesh _mesh;
    private MeshFilter _meshFilter;

    private List<Vector3> _displacementPoints = new List<Vector3>() { Vector3.zero };
    private Vector3[] _startNormals;

    #region NAME CONSTANTS
    private const string _CS_MAIN_KERNEL = "CSMain";
    private const string _SET_COUNT_KERNEL = "SetCount";
    private const string _DISPLACEMENT_BUFFER = "displacementBuffer";
    private const string _DISPLACEMENT_POINTS_BUFFER = "displacementPoints";
    private const string _VERTICES_BUFFER = "vertices";
    private const string _VERTEX_COUNT = "vertexCount";
    private const string _POINT_COUNT = "pointCount";
    private const string _DEBUG_BUFFER = "debugBuffer";
    private const string _DIPLACEMENT_BUFFER_SIZE = "displacementBufferSize";
    #endregion

    public void OnHit(RaycastHit pHit)
    {
        print("hit registered");
        Vector3 lLocalPos = transform.InverseTransformPoint(pHit.point);
        AddDisplacementPoint(lLocalPos);
    }

    private void OnValidate()
    {
        //SetDisplacementMaterial();
    }

    /// <summary>
    /// Insures taht the displacement material is in the sharedMaterials array and at index 0.
    /// </summary>
    private void SetDisplacementMaterial()
    {
        //MeshRenderer _meshRenderer = GetComponent<MeshRenderer>();
        //if (!_meshRenderer.sharedMaterials.Contains(_displacementMaterial))
        //{
        //    Material[] lMaterials = new Material[] { _displacementMaterial };
        //    _meshRenderer.sharedMaterials = lMaterials;
        //}
        //if (!_meshRenderer.sharedMaterials.Contains(_displacementMaterial))
        //{
        //    Material[] lMaterials = new Material[_meshRenderer.sharedMaterials.Length + 1];
        //    lMaterials[0] = _displacementMaterial;
        //    for (int i = 1; i < lMaterials.Length - 1; i++)
        //    {
        //        lMaterials[i] = _meshRenderer.sharedMaterials[i];
        //    }
        //    _meshRenderer.sharedMaterials = lMaterials;
        //}
        //else if (_meshRenderer.sharedMaterials[0] != _displacementMaterial)
        //{
        //    Material[] lMaterials = new Material[_meshRenderer.sharedMaterials.Length];
        //    int lIndex = 1;
        //    for (int i = 0; i < lMaterials.Length; i++)
        //    {
        //        if (_meshRenderer.sharedMaterials[i] != _displacementMaterial)
        //        {
        //            lMaterials[lIndex] = _meshRenderer.sharedMaterials[i];
        //            lIndex++;
        //        }
        //        else
        //        {
        //            lMaterials[0] = _meshRenderer.sharedMaterials[i];
        //        }
        //    }
        //    _meshRenderer.sharedMaterials = lMaterials;
        //}
    }

    private void Start()
    {
        StartComponents();
        StartComputeShader();
        _vertexCount = _mesh.vertexCount;
        _startNormals = _mesh.normals;
        SetGroupCount(_vertexCount);
        StartBuffers();
    }

    private void StartComponents()
    {
        _meshFilter = GetComponent<MeshFilter>();
        Mesh lMesh = Instantiate(_meshFilter.sharedMesh); // Clone the mesh
        _meshFilter.mesh = lMesh;
        _mesh = _meshFilter.mesh;
    }

    private void StartComputeShader()
    {
        _computeShaderInstance = Instantiate(_computeShaderTemplate);
    }

    private void StartBuffers()
    {
        // displacement buffer
        _displacementBuffer = new ComputeBuffer(_vertexCount, sizeof(float));
        _computeShaderInstance.SetBuffer(_computeShaderInstance.FindKernel(_CS_MAIN_KERNEL), _DISPLACEMENT_BUFFER, _displacementBuffer);
        _displacementBuffer.SetData(new float[_vertexCount]);

        // displacement points buffer
        _displacementPointsBuffer = new ComputeBuffer(1, sizeof(float) * 3);
        _computeShaderInstance.SetBuffer(_computeShaderInstance.FindKernel(_CS_MAIN_KERNEL), _DISPLACEMENT_POINTS_BUFFER, _displacementPointsBuffer);

        // vertices buffer
        _vertexBuffer = new ComputeBuffer(_vertexCount, sizeof(float) * 3);
        _computeShaderInstance.SetBuffer(_computeShaderInstance.FindKernel(_CS_MAIN_KERNEL), _VERTICES_BUFFER, _vertexBuffer);
        _vertexBuffer.SetData(_mesh.vertices);

        // debug buffer
        _debugBuffer = new ComputeBuffer(10, sizeof(float) * 3);
        _computeShaderInstance.SetBuffer(_computeShaderInstance.FindKernel(_CS_MAIN_KERNEL), _DEBUG_BUFFER, _debugBuffer);
        Vector3[] lDebugArray = new Vector3[10];
        _debugBuffer.SetData(lDebugArray);

        // set values on compute shader
        _computeShaderInstance.SetInt(_VERTEX_COUNT, _vertexCount);
        _computeShaderInstance.SetInt(_POINT_COUNT, 1);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1)) ClearDisplacementPoints();
        SendDisplacementPoints();
        _computeShaderInstance.Dispatch(_computeShaderInstance.FindKernel(_CS_MAIN_KERNEL), _groupCount, 1, 1);
        ApplyDisplacementToMesh();

        //oe faut juste que tu codes le shading sur le displacementshader.shader c'est trop relou la sinon
    }

    private void ApplyDisplacementToMesh()
    {
        Vector3[] vertices = _mesh.vertices;
        float[] lDisplacements = new float[_vertexCount];
        _displacementBuffer.GetData(lDisplacements);
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] += _startNormals[i] * lDisplacements[i] * Time.deltaTime;
        }
        _mesh.vertices = vertices;
        _mesh.RecalculateNormals(); // Optional: updates lighting
        _mesh.RecalculateBounds(); // Prevents culling issues
    }

    private void SetGroupCount(int pVertexCount)
    {
        _groupCount = Math.Max(1, Mathf.CeilToInt(pVertexCount / _THREAD_PER_GROUP));
    }

    private void SendDisplacementPoints()
    {
        _computeShaderInstance.SetInt(_POINT_COUNT, _displacementPoints.Count);
        _displacementPointsBuffer = new ComputeBuffer(_displacementPoints.Count, sizeof(float) * 3);
        _displacementPointsBuffer.SetData(_displacementPoints);
        _computeShaderInstance.SetBuffer(_computeShaderInstance.FindKernel(_CS_MAIN_KERNEL), _DISPLACEMENT_POINTS_BUFFER, _displacementPointsBuffer);
    }

    public void AddDisplacementPoint(Vector3 pDisplacementPos)
    {
        if (_displacementPoints.Count >= 999)
        {
            print("too many hits");
            return;
        }
        _displacementPoints.Add(pDisplacementPos);
    }

    public void ClearDisplacementPoints()
    {
        _displacementPoints.Clear();
        _displacementPoints.Add(Vector3.zero);
    }

    private void OnDestroy()
    {
        _displacementBuffer.Release();
        _displacementPointsBuffer.Release();
        _vertexBuffer.Release();
        _debugBuffer.Release();
    }
}
