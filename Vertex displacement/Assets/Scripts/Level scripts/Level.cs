using System.Collections;
using UnityEngine;

public class Level : MonoBehaviour
{
    [SerializeField] private Transform _ballSpawnPos;
    [SerializeField] private Ball _ballTemplate;

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
        if (Camera.main.IsInCameraFrustum(gameObject.GetRendererBounds()))
        {
            //gameObject.SetActive(false);
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (_state == Playing) SetPaused();
            if (_state == Paused) SetPlaying();
        }
        _state();
    }

    #region Playing
    public void SetPlaying(float pDelay = 0f)
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
        else
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
    #endregion

    public void SetPaused()
    {
        _state = Paused;
        if (_ball != null) _ball.rb.Sleep();
    }

    private void Paused()
    {

    }

    public void SetDisabled()
    {
        _state = Disabled;
        if (_ball != null) _ball.DestroyBallAnimation();
    }

    private void Disabled()
    {
        if (Camera.main.IsInCameraFrustum(gameObject.GetRendererBounds()))
        {
            //gameObject.SetActive(false);
        }
    }

    public Ball SpawnBall(Vector3 pPos)
    {
        Ball lBall = Instantiate(_ballTemplate, transform);
        lBall.transform.position = pPos;
        
        return lBall;
    }
}
