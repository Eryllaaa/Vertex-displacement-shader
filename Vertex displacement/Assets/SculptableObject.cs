using UnityEngine;

[RequireComponent (typeof(ComputeShaderDisplacement))]
public class SculptableObject : MonoBehaviour
{
    private ComputeShaderDisplacement _displacer;

    void Start()
    {
        _displacer = GetComponent<ComputeShaderDisplacement>();    
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            _displacer.ClearDisplacementPoints();
        }
    }

    public void OnHit(RaycastHit pHit)
    {
        print("hit registered");
        Vector3 lLocalPos = transform.InverseTransformPoint(pHit.point);
        _displacer.AddDisplacementPoint(lLocalPos);
    }
}
