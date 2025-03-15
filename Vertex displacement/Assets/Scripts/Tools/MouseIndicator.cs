using UnityEngine;
using UnityEngine.InputSystem;

public class MouseIndicator : MonoBehaviour
{
    [Header("Container")]
    [SerializeField] private Transform _indicatorsContainer;
    [Header("Indicators")]
    [SerializeField] private Transform _sizeIndicator;
    [SerializeField] private Transform _speedIndicator;
    [Header("Values")]
    [SerializeField] private float _indicatorSizeLerpSpeed;
    [SerializeField] private float _minSpeedIndicatorSize;
    [SerializeField] private float _maxSpeedIndicatorSize;

    private SculptingManager _sculptingManager;
    private InputReader _inputReader;


    private void Start()
    {
        _sculptingManager = SculptingManager.Instance;
        _inputReader = InputReader.Instance;
        
        BindInputs();
        Hide();
        ShowSize();
    }

    private void BindInputs()
    {
        _inputReader.modifierAction.started += OnModifierPressed;
        _inputReader.modifierAction.canceled += OnModifierReleased;
    }

    private void Update()
    {
        if (_sculptingManager.latestHitPos.HasValue) transform.position = _sculptingManager.latestHitPos.Value;
        CheckIfShouldHide();
        UpdateIndicators();
    }

    private void ShowSize()
    {
        _sizeIndicator.gameObject.SetActive(true);
        _speedIndicator.gameObject.SetActive(false);
    }

    private void ShowStrength()
    {
        _sizeIndicator.gameObject.SetActive(false);
        _speedIndicator.gameObject.SetActive(true);
    }

    private void Show()
    {
        _indicatorsContainer.gameObject.SetActive(true);
    }

    private void Hide()
    {
        _indicatorsContainer.gameObject.SetActive(false);
    }

    private bool isHidden => !_indicatorsContainer.gameObject.activeSelf;
    private void CheckIfShouldHide()
    {
        if (_sculptingManager.latestHitPos == null && !isHidden)
        {
            Hide();
        }
        else if (_sculptingManager.latestHitPos != null && isHidden)
        {
            Show();
        }
    }

    private void UpdateIndicators()
    {
        _sizeIndicator.localScale = Vector3.Lerp(_sizeIndicator.localScale, Vector3.one * 1.15f * _sculptingManager.SculptRadius, Curves.EaseOutCubic(Time.deltaTime) * _indicatorSizeLerpSpeed);

        float lSpeedIndicatorSize = Mathf.Lerp(_minSpeedIndicatorSize, _maxSpeedIndicatorSize, GetSculptSpeedRatio());
        _speedIndicator.localScale = Vector3.Lerp(_speedIndicator.localScale, new Vector3(1, lSpeedIndicatorSize, 1), Curves.EaseOutCubic(Time.deltaTime) * _indicatorSizeLerpSpeed);
    }

    private float GetSculptSpeedRatio()
    {
        return Mathf.InverseLerp(_sculptingManager.MinSculptSpeed, _sculptingManager.MaxSculptSpeed, _sculptingManager.SculptSpeed);
    }

    private void OnModifierPressed(InputAction.CallbackContext pContext)
    {
        ShowStrength();
    }

    private void OnModifierReleased(InputAction.CallbackContext pContext)
    {
        ShowSize();
    }
}
