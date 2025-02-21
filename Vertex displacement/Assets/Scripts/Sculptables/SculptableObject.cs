using System.Collections.Generic;
using System;
using UnityEngine;

public class SculptableObject : MonoBehaviour
{
    [SerializeField] private Material _sculptMaterial;
    [SerializeField] private ComputeShader _computeShader;
    private const int _THREAD_PER_GROUP = 256;
    private int _groupCount = 0; // should change based on the mesh's vertex count
    private MeshFilter _meshFilter;

    private ComputeBuffer _displacementBuffer;
    private ComputeBuffer _displacementPointsBuffer;
    private ComputeBuffer _vertexBuffer;
    private ComputeBuffer _DebugBuffer;

    private Mesh _mesh;

    private List<Vector3> _displacementPoints = new List<Vector3>() { Vector3.zero };

    private const string _CS_MAIN_KERNEL = "CSMain";
    private const string _SET_COUNT_KERNEL = "SetCount";
    private const string _DISPLACEMENT_BUFFER = "displacementBuffer";
    private const string _DISPLACEMENT_POINTS_BUFFER = "displacementPoints";
    private const string _VERTICES_BUFFER = "vertices";
    private const string _POINT_COUNT = "pointCount";
    private const string _DEBUG_BUFFER = "debugBuffer";

    public void OnHit(RaycastHit pHit)
    {
        print("hit registered");
        Vector3 lLocalPos = transform.InverseTransformPoint(pHit.point);
        AddDisplacementPoint(lLocalPos);
    }

    private void OnValidate()
    {
        GetComponent<MeshRenderer>().material = _sculptMaterial;
    }

    void Start()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _mesh = _meshFilter.mesh;

        int vertexCount = _mesh.vertexCount;
        SetGroupCount(vertexCount);

        // Create buffer
        _displacementBuffer = new ComputeBuffer(vertexCount, sizeof(float));
        _displacementBuffer.SetData(new float[vertexCount]);
        _displacementPointsBuffer = new ComputeBuffer(1, sizeof(float) * 3);
        _vertexBuffer = new ComputeBuffer(vertexCount, sizeof(float) * 3);

        _DebugBuffer = new ComputeBuffer(10, sizeof(float) * 3);
        Vector3[] lDebugArray = new Vector3[10];
        _DebugBuffer.SetData(lDebugArray);

        _computeShader.SetBuffer(_computeShader.FindKernel(_CS_MAIN_KERNEL), _DEBUG_BUFFER, _DebugBuffer);

        // Bind buffer on compute shader
        _computeShader.SetBuffer(_computeShader.FindKernel(_CS_MAIN_KERNEL), _DISPLACEMENT_BUFFER, _displacementBuffer);

        // Bind displacement points buffer
        _computeShader.SetBuffer(_computeShader.FindKernel(_CS_MAIN_KERNEL), _DISPLACEMENT_POINTS_BUFFER, _displacementPointsBuffer);

        // Communicate the vertices of the mesh to the compute shader
        _computeShader.SetBuffer(_computeShader.FindKernel(_CS_MAIN_KERNEL), _VERTICES_BUFFER, _vertexBuffer);
        _vertexBuffer.SetData(_mesh.vertices);

        // Set buffer on material
        _meshFilter.GetComponent<Renderer>().material.SetBuffer(_DISPLACEMENT_BUFFER, _displacementBuffer);
        _meshFilter.GetComponent<Renderer>().material.SetInt("displacementBufferSize", vertexCount);

        _computeShader.SetInt("vertexCount", vertexCount);
        _computeShader.SetInt("pointCount", 1);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            ClearDisplacementPoints();
        }

        SendDisplacementPoints();
        _computeShader.Dispatch(_computeShader.FindKernel(_CS_MAIN_KERNEL), _groupCount, 1, 1);

        Vector3[] lDebug = new Vector3[1];
        _DebugBuffer.GetData(lDebug);

        foreach (Vector3 i in lDebug)
        {
            //print(i);
        }

        //Vector3[] data = new Vector3[_mesh.vertexCount];
        //_vertexBuffer.GetData(data);
        //int i = 0;
        //foreach (Vector3 d in data)
        //{
        //    print(i++ + ": " + d);
        //}
    }

    private void SetGroupCount(int pVertexCount)
    {
        _groupCount = Math.Max(1, Mathf.CeilToInt(pVertexCount / _THREAD_PER_GROUP));
    }

    private void SendDisplacementPoints()
    {
        _computeShader.SetInt(_POINT_COUNT, _displacementPoints.Count);
        _displacementPointsBuffer = new ComputeBuffer(_displacementPoints.Count, sizeof(float) * 3);
        _displacementPointsBuffer.SetData(_displacementPoints);
        _computeShader.SetBuffer(_computeShader.FindKernel(_CS_MAIN_KERNEL), _DISPLACEMENT_POINTS_BUFFER, _displacementPointsBuffer);
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
    }
}
