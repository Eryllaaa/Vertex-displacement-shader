using UnityEngine;

public class Level : MonoBehaviour
{
    [SerializeField] private Transform _ballSpawnPos;
    [SerializeField] private GameObject _ballTemplate;
    [SerializeField] public Renderer levelRenderer;

    public Color bgColor = Color.white;

    public void SpawnBall()
    {
        GameObject lBall = Instantiate(_ballTemplate);
        lBall.transform.position = _ballSpawnPos.position;
    }
}
