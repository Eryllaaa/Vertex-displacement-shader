using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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
    [SerializeField, Range(5f, 20f)] private float _sculptSpeed = 1f;

    [SerializeField] private float _minSculptRadius = 0.1f;
    [SerializeField] private float _maxSculptRadius = 10f;

    [Header("Controls")]
    [SerializeField, Range(0.1f, 1f)] private float _mouseWheelSensitivity = 0.1f;
    #endregion

    InputReader _inputReader;
    private float _sculptRadius = 0.2f;

    private Camera _camera;
    private const float _MAX_RAYCAST_DISTANCE = 1000f;
    private Vector3 _previousPos = Vector3.zero;
    private SculptHit2 _latestHit = SculptHit2.none;
    private bool _interpolate = false;

    private void Start()
    {
        SingletonCheck();
        _camera = Camera.main;
        InputCheck();
        BindInputs();
    }

    private void InputCheck()
    {
        _inputReader = InputReader.Instance;
    }

    private void BindInputs()
    {
        _inputReader.strengthVariableAction.performed += BindScroll;
    }

    private void Update()
    {
        InputsHandling();
    }

    private RaycastHit RaycastToWorld()
    {
        RaycastHit lHit;
        //Physics.SphereCastAll(_camera.ScreenPointToRay(Input.mousePosition), _sculptRadius, _MAX_RAYCAST_DISTANCE, sculptableLayer);
        Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out lHit, _MAX_RAYCAST_DISTANCE, sculptableLayer);
        return lHit;
    }

    private void DetectionHandling(RaycastHit pHit, SculptDirection pDir)
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
        pHit.transform.GetComponent<SculptableObject2>().OnHit(RaycastToSculptHit(pHit, pDir));
    }

    private SculptHit2 RaycastToSculptHit(RaycastHit pHit,SculptDirection pDir)
    {
        if (_interpolate)
        {
            _previousPos = _latestHit.position;
        }
        else
        {
            _previousPos = pHit.point;
        }
        _latestHit = new SculptHit2(pHit.point, _previousPos, pDir, _sculptRadius, _sculptSpeed);
        return _latestHit;
    }

    private void InputsHandling()
    {
        if (_inputReader.isScultpingUp)
        {
            DetectionHandling(RaycastToWorld(),SculptDirection.up);
            _interpolate = true;
        }
        else if(_inputReader.isScultpingDown)
        {
            DetectionHandling(RaycastToWorld(), SculptDirection.down);
            _interpolate = true;
        }
        else _interpolate = false;
    }

    private void BindScroll(InputAction.CallbackContext pContext)
    {
        _sculptRadius -= pContext.ReadValue<Vector2>().y * _mouseWheelSensitivity * Time.deltaTime;
        _sculptRadius = Mathf.Clamp(_sculptRadius, _minSculptRadius, _maxSculptRadius);
    }
}
