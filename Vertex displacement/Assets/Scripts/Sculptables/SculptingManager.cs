using UnityEngine;

public class SculptingManager : MonoBehaviour
{
    private SculptingManager _instance;
    public static SculptingManager Instance { get; private set; }

    [SerializeField] public LayerMask sculptableLayer;
    private Camera _camera;
    private const float _MAX_RAYCAST_DISTANCE = 1000f;

    private void Start()
    {
        SingletonCheck();
        _camera = Camera.main;
    }

    private void SingletonCheck()
    {
        if (_instance != null)
        {
            print($"an instance of {name} already exists");
            Destroy(_instance);
        }
        _instance = this;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SculptingHandling(RaycastToWorld());
        }
    }

    private RaycastHit RaycastToWorld()
    {
        RaycastHit lHit;
        Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out lHit, _MAX_RAYCAST_DISTANCE, sculptableLayer);
        return lHit;
    }

    private void SculptingHandling(RaycastHit pHit)
    {
        if (pHit.collider == null)
        {
            print("no collider hit");
            return;
        }
        else if (pHit.transform.GetComponent<SculptableObject>() == null)
        {
            print("collider hit has no SculptableObject component");
            return;
        }
        pHit.transform.GetComponent<SculptableObject>().OnHit(pHit);
    }
}
