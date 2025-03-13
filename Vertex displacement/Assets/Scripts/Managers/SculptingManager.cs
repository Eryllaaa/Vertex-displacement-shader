using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class SculptingManager : MonoBehaviour
{
    #region Singleton
    private static SculptingManager _instance;
    public static SculptingManager Instance { get { return _instance; } }
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
    [SerializeField] public LayerMask[] sculptableLayers;
    private LayerMask _SculptableLayer;

    [Header("Sculpting")]
    [SerializeField] private float _sculptSpeed = 1f;
    [SerializeField] private float _sculptRadius = 0.5f;

    [SerializeField] private float _minSculptRadius = 0.1f;
    [SerializeField] private float _maxSculptRadius = 10f;

    [SerializeField] private float _minSculptSpeed = 5f;
    [SerializeField] private float _maxSculptSpeed = 20f;

    [Header("Controls")]
    [SerializeField, Range(0.1f, 1f)] private float _mouseWheelSensitivity = 0.1f;
    #endregion

    InputReader _inputReader;

    private Camera _camera;
    private const float _MAX_RAYCAST_DISTANCE = 1000f;
    private Vector3 _previousPos = Vector3.zero;
    private SculptHit _latestHit = SculptHit.none;
    private bool _interpolate = false;

    private void Start()
    {
        SingletonCheck();
        
        InputCheck();
        BindInputs();
        MergeLayers();
        _camera = Camera.main;
        _sculptRadius = Mathf.Clamp(_sculptRadius, _minSculptRadius, _maxSculptRadius);
    }

    private void MergeLayers()
    {
        foreach(LayerMask layer in sculptableLayers)
        {
            _SculptableLayer |= layer;
        }
    }
    private void InputCheck()
    {
        _inputReader = InputReader.Instance;
    }

    private void BindInputs()
    {
        _inputReader.scrollAction.performed += BindScroll;
    }

    private void Update()
    {
        InputsHandling();
    }

    private RaycastHit RaycastToWorld()
    {
        RaycastHit lHit;
        Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out lHit, _MAX_RAYCAST_DISTANCE, _SculptableLayer);
        return lHit;
    }

    private void DetectionHandling(RaycastHit pHit, SculptDirection pDir)
    {
        if (pHit.collider == null)
        {
            //print("no collider hit");
            return;
        }
        else if (pHit.transform.GetComponent<SculptableObject>() == null)
        {
            print("collider hit has no SculptableObject component");
            return;
        }
        pHit.transform.GetComponent<SculptableObject>().OnHit(RaycastToSculptHit(pHit, pDir));
    }

    private SculptHit RaycastToSculptHit(RaycastHit pHit,SculptDirection pDir)
    {
        if (_interpolate)
        {
            _previousPos = _latestHit.position;
        }
        else
        {
            _previousPos = pHit.point;
        }
        _latestHit = new SculptHit(pHit.point, _previousPos, pDir, _sculptRadius, _sculptSpeed);
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
        if (_inputReader.isModifier)
        {
            _sculptSpeed -= pContext.ReadValue<Vector2>().y * _mouseWheelSensitivity * Time.deltaTime;
            _sculptSpeed = Mathf.Clamp(_sculptSpeed, _minSculptSpeed, _maxSculptSpeed);
        }
        // if not modifier, change the radius
        else
        {
            _sculptRadius -= pContext.ReadValue<Vector2>().y * _mouseWheelSensitivity * Time.deltaTime;
            _sculptRadius = Mathf.Clamp(_sculptRadius, _minSculptRadius, _maxSculptRadius);
        }
    }
}
