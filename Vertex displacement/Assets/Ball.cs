using UnityEngine;
using UnityEngine.InputSystem;

public class Ball : MonoBehaviour
{
    private Vector3 _startPos = Vector3.zero;

    private void Start()
    {
        _startPos = transform.position;
        InputReader.Instance.DebugResetBallAction.started += BindToReset;
    }

    private void BindToReset(InputAction.CallbackContext pContext)
    {
        transform.position = _startPos;
    }
}
