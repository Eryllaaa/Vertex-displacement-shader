using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class Ball : MonoBehaviour
{
    public static string TAG = "Ball";

    private Vector3 _startPos = Vector3.zero;
    public Rigidbody rb;
    private Collider _collider;
    public Renderer ballRenderer;

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        TryGetComponent(out rb);
        TryGetComponent(out _collider);
        TryGetComponent(out ballRenderer);

        _startPos = transform.position;
        InputReader.Instance.DebugResetBallAction.started += BindToReset;
    }

    private void BindToReset(InputAction.CallbackContext pContext)
    {
        RestartBall();
    }

    public void RestartBall()
    {
        rb.Move(_startPos, Quaternion.identity);
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        _collider.enabled = true;
    }
}
