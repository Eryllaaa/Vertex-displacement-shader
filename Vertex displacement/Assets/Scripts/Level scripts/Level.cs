using UnityEngine;

public class Level : MonoBehaviour
{
    [SerializeField] private Transform _ballSpawnPos;
    [SerializeField] private GameObject _ballTemplate;

    public void SpawnBall()
    {
        GameObject lBall = Instantiate(_ballTemplate);
        lBall.transform.position = _ballSpawnPos.position;
    }
}
