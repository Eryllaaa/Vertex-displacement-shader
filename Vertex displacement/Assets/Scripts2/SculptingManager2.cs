using System;
using UnityEngine;

public class SculptingManager2 : MonoBehaviour
{
    #region Singleton
    private static SculptingManager2 _instance;
    public static SculptingManager2 Instance { get { return _instance; }}
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
    private const float MIN_SCULPT_RADIUS = 0.1f;
    private const float MAX_SCULPT_RADIUS = 10f;
    [SerializeField] private SculptDirection _sculptDirection;
    #endregion

    private Camera _camera;
    private const float _MAX_RAYCAST_DISTANCE = 1000f;

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
        pHit.transform.GetComponent<SculptableObject2>().OnHit(RaycastToSculptHit(pHit));
    }

    private SculptHit2 RaycastToSculptHit(RaycastHit pHit)
    {
        return new SculptHit2(pHit.point, _sculptDirection, _sculptRadius);
    }

    private void InputsHandling()
    {
        if (Input.GetMouseButton(0) || Input.GetKeyDown(KeyCode.Space))
        {
            DetectionHandling(RaycastToWorld());
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _sculptDirection = (SculptDirection)((((int)_sculptDirection) + 1) % 2);
        }
        //mouse scroll wheel to change radius --> "_sculptRadius * _sculptRadius" to increase rate of change the bigger the radius so we don't scroll for 1 hour when radius is large and stay precise when radius is small (it's just a x^3)
        _sculptRadius -= Input.mouseScrollDelta.y * _sculptRadius * _sculptRadius * 0.05f;
    }
}
