using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class Ball : MonoBehaviour
{
    private Vector3 _startPos = Vector3.zero;
    private Rigidbody _rb;
    private Collider _Collider;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _Collider = GetComponent<Collider>();

        _startPos = transform.position;
        InputReader.Instance.DebugResetBallAction.started += BindToReset;
    }

    private void BindToReset(InputAction.CallbackContext pContext)
    {
        RestartBall();
    }

    public void RestartBall()
    {
        _rb.Move(_startPos, Quaternion.identity);
        _rb.velocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _Collider.enabled = true;
    }
}
