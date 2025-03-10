using System;
using System.Collections.Generic;
using UnityEngine;

public class SculptingManager2 : MonoBehaviour
{
    #region Singleton
    private static SculptingManager2 _instance;
    public static SculptingManager2 Instance { get { return _instance; } }
    private void SingletonCheck()
    {
        if (_instance != null)
        {
            print($"an instance of {name} already exists");
            Destroy(_instance);
        }
        _instance = this;
    }
    #endregion

    #region Serialized values
    [SerializeField] public LayerMask sculptableLayer;

    [Header("Sculpting")]
    [SerializeField, Range(0.1f, MAX_SCULPT_RADIUS)] private float _sculptRadius = 0.2f;
    [SerializeField] private SculptDirection _sculptDirection;
    [SerializeField, Range(5f, 20f)] private float _sculptSpeed = 1f;

    [Header("Controls")]
    [SerializeField, Range(0.1f, 1f)] private float _mouseWheelSensitivity = 0.1f;

    public List<SculptableObject2> sculptableObjects = new List<SculptableObject2>();
    #endregion

    private const float MIN_SCULPT_RADIUS = 0.1f;
    private const float MAX_SCULPT_RADIUS = 10f;

    private Camera _camera;
    private const float _MAX_RAYCAST_DISTANCE = 1000f;
    private Vector3 _previousPos = Vector3.zero;
    private SculptHit2 _latestHit = SculptHit2.none;
    private bool _interpolate = false;

    private void Start()
    {
        SingletonCheck();
        _camera = Camera.main;
    }

    private void Update()
    {
        InputsHandling();
    }

    private RaycastHit RaycastToWorld()
    {
        RaycastHit lHit;
        Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out lHit, _MAX_RAYCAST_DISTANCE, sculptableLayer);
        return lHit;
    }

    private void DetectionHandling(RaycastHit pHit)
    {
        if (pHit.collider == null)
        {
            print("no collider hit");
            return;
        }
        else if (pHit.transform.GetComponent<SculptableObject2>() == null)
        {
            print("collider hit has no SculptableObject component");
            return;
        }
        foreach (SculptableObject2 obj in sculptableObjects)
        {
            obj.OnHit(RaycastToSculptHit(pHit));
        }
    }

    private SculptHit2 RaycastToSculptHit(RaycastHit pHit)
    {
        if (_interpolate)
        {
            _previousPos = _latestHit.position;
        }
        else
        {
            _previousPos = pHit.point;
        }
        _latestHit = new SculptHit2(pHit.point, _previousPos, _sculptDirection, _sculptRadius, _sculptSpeed);
        return _latestHit;
    }

    private void InputsHandling()
    {
        if (Input.GetMouseButton(0))
        {
            DetectionHandling(RaycastToWorld());
            _interpolate = true;
        }
        else _interpolate = false;

        if (Input.GetKeyDown(KeyCode.Space)) _sculptDirection = (SculptDirection)((((int)_sculptDirection) + 1) % 2);

        _sculptRadius -= Input.mouseScrollDelta.y * (_sculptRadius * _mouseWheelSensitivity);
    }
}
