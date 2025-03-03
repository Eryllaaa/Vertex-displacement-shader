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
    [SerializeField] private float _sculptingRadius;
    [SerializeField] private SculptDirection _sculptingDirection;
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
        return new SculptHit2(pHit.point, _sculptingDirection, _sculptingRadius);
    }

    private void InputsHandling()
    {
        if (Input.GetMouseButton(0))
        {
            DetectionHandling(RaycastToWorld());
        }
    }
}
