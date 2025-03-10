using UnityEngine;

public class Ball : MonoBehaviour
{
    private Vector3 _startPos = Vector3.zero;

    private void Start()
    {
        _startPos = transform.position;    
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            transform.position = _startPos;
        }
    }
}
