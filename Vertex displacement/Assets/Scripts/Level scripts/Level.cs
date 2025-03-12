using System.Collections;
using UnityEngine;

public class Level : MonoBehaviour
{
    [SerializeField] private Transform _ballSpawnPos;
    [SerializeField] private Ball _ballTemplate;
    [SerializeField] public float _ballRespawnForce;

    private Ball _ball = null;

    public Color bgColor = Color.white;

    private delegate void LevelState();

    private LevelState _state;

    private void Start()
    {
        SetDisabled();
    }

    private void Update()
    {
        _state();
    }

    public void SetPlaying(float pDelay)
    {
        if (_delayedPlaying != null) StopCoroutine(_delayedPlaying);
        StartCoroutine(_delayedPlaying = DelayedPlaying(pDelay));
    }

    private IEnumerator _delayedPlaying = null;
    private IEnumerator DelayedPlaying(float pDelay)
    {
        yield return new WaitForSeconds(pDelay);
        _state = Playing;
        if (_ball == null)
        {
            _ball = SpawnBall(_ballSpawnPos.position);
        }
        else if (CheckBallRespawn())
        {
            _ball.RestartBall();
            _ball.transform.position = _ballSpawnPos.position;
        }
    }

    private void Playing()
    {
        if (CheckBallRespawn())
        {
            _ball.RestartBall();
            _ball.transform.position = _ballSpawnPos.position;
            _ball.rb.AddForce(Vector3.down * _ballRespawnForce, ForceMode.Impulse);
        }
    }

    private bool CheckBallRespawn()
    {
        if (!_ball.ballRenderer.isVisible)
        {
            return true;
        }
        return false;
    }

    public void SetPaused()
    {
        _state = Paused;
    }

    private void Paused()
    {

    }

    public void SetDisabled()
    {
        _state = Disabled;
    }

    private void Disabled()
    {

    }

    public Ball SpawnBall(Vector3 pPos)
    {
        Ball lBall = Instantiate(_ballTemplate, transform);
        lBall.transform.position = pPos;
        
        return lBall;
    }
}
