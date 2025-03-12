using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class Ball : MonoBehaviour
{
    [SerializeField] private float _hoverOnSpawnDuration = 0.5f;

    public static string TAG = "Ball";

    private Vector3 _startPos = Vector3.zero;
    public Rigidbody rb;
    private Collider _collider;
    public Renderer ballRenderer;

    private void Start()
    {
        Init();
        SpawnBallAnimation();
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
        SpawnBallAnimation();
    }

    private void SpawnBallAnimation()
    {
        if (_scaleBallRoutine != null) StopCoroutine(_scaleBallRoutine);
        StartCoroutine(_scaleBallRoutine = ScaleBallRoutine(Vector3.zero, Vector3.one));
        
        if (_freezeRoutine != null) StopCoroutine(_freezeRoutine);
        StartCoroutine(_freezeRoutine = FreezeRoutine());
    }

    public void DestroyBallAnimation()
    {
        if (_scaleBallRoutine != null) StopCoroutine(_scaleBallRoutine);
        StartCoroutine(_scaleBallRoutine = ScaleBallRoutine(transform.localScale, Vector3.zero));
    }

    private IEnumerator _scaleBallRoutine = null;
    private IEnumerator ScaleBallRoutine(Vector3 pStartScale, Vector3 pEndScale)
    {
        float lTime = 0f;
        float lDuration = 0.85f;

        while (lTime < lDuration)
        {
            transform.localScale = Vector3.Lerp(pStartScale, pEndScale, Curves.EaseOutExpo(lTime / lDuration));

            lTime += Time.deltaTime;
            yield return 0;
        }
    }

    private IEnumerator _freezeRoutine = null;
    private IEnumerator FreezeRoutine()
    {
        rb.isKinematic = true;

        yield return new WaitForSeconds(_hoverOnSpawnDuration);

        rb.isKinematic = false;
    }
}
