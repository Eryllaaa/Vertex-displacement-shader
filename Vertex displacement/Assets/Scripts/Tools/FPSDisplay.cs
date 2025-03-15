using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class FPSCounter : MonoBehaviour
{
    private TextMeshProUGUI _fpsText;
    private float _deltaTime = 0.0f;
    private int _timer = 0;
    private float _averageDelta = 0;
    private int _frameCount = 60;

    private void Start()
    {
        TryGetComponent(out _fpsText);
    }

    void Update()
    {
        if (_timer > _frameCount)
        {
            _deltaTime = _frameCount / _averageDelta;
            _averageDelta = 0.0f;
            _timer = 0;
        }
        else
        {
            _timer++;
            _averageDelta += Time.deltaTime;
        }

        float fps = _deltaTime;
        _fpsText.text = $"fps: {fps}";
    }
}