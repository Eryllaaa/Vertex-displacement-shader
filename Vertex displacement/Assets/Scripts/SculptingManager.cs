using UnityEngine;

public class SculptingManager : MonoBehaviour
{
    [SerializeField] private LayerMask _sculptableLayer;
    private Camera _camera;
    private const float _MAX_RAYCAST_DISTANCE = 1000f;

    private void Start()
    {
        _camera = Camera.main;
    }

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            SculptingHandling(RaycastToWorld());
        }
    }

    private RaycastHit RaycastToWorld()
    {
        RaycastHit lHit;
        Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out lHit, _MAX_RAYCAST_DISTANCE, _sculptableLayer);
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
