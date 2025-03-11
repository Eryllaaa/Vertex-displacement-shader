using System.Collections;
using UnityEngine;

public class LevelChangeAnimator : MonoBehaviour
{
    [SerializeField, Min(10f)] public float levelChangeDistance;
    [SerializeField, Min(0.1f)] public float levelChangeDuration;

    private LevelManager _levelManager;

    private void Start()
    {
        _levelManager = GetComponent<LevelManager>();
    }

    public void StartLevel(Level pCurrentLevel)
    {
        if (_levelBGChangeRoutine != null) StopCoroutine(_levelBGChangeRoutine);
        StartCoroutine(_levelBGChangeRoutine = LevelBGChangeRoutine(Camera.main.backgroundColor, pCurrentLevel.bgColor, levelChangeDuration * 0.33f));
    }

    public void StartLevelSwitchAnimation(Level pCurrent, Level pNext, Vector3 pDir, float pDistance, float pDuration)
    {
        if (_levelInChangeRoutine != null) StopCoroutine(_levelInChangeRoutine);
        if (_levelOutChangeRoutine != null) StopCoroutine(_levelOutChangeRoutine);
        if (_levelBGChangeRoutine != null) StopCoroutine(_levelBGChangeRoutine);

        StartCoroutine(_levelInChangeRoutine = LevelInChangeRoutine(pNext, pDir, pDistance, pDuration));
        StartCoroutine(_levelOutChangeRoutine = LevelOutChangeRoutine(pCurrent, pDir, pDistance, pDuration));
        StartCoroutine(_levelBGChangeRoutine = LevelBGChangeRoutine(pCurrent.bgColor, pNext.bgColor, pDuration));
    }

    private IEnumerator _levelBGChangeRoutine = null;
    private IEnumerator LevelBGChangeRoutine(Color pCurrent, Color pNext, float pDuration)
    {
        Camera lCamera = Camera.main;

        Color lStartColor = lCamera.backgroundColor;

        float lDuration = pDuration;
        float lTime = 0f;

        while (lTime <= pDuration)
        {
            lCamera.backgroundColor = Color.Lerp(lStartColor, pNext, Curves.EaseInOutSine(lTime / lDuration));

            lTime += Time.deltaTime;
            yield return 0;
        }
    }

    private IEnumerator _levelInChangeRoutine = null;
    private IEnumerator LevelInChangeRoutine(Level pLevel, Vector3 pDirection, float pDistance, float pDuration)
    {
        Vector3 lStartPos;

        if (pLevel.levelRenderer.isVisible) lStartPos = pLevel.transform.position;
        else lStartPos = pDirection.normalized * -1 * pDistance;

        Vector3 lEndPos = _levelManager.levelPlayingPos;

        float lDuration = pDuration;
        float lTime = 0f;

        while (lTime <= lDuration)
        {
            pLevel.transform.position = Vector3.Lerp(lStartPos, lEndPos, Curves.EaseInOutSine(lTime / lDuration));

            lTime += Time.deltaTime;
            yield return 0;
        }
    }

    private IEnumerator _levelOutChangeRoutine = null;
    private IEnumerator LevelOutChangeRoutine(Level pLevel, Vector3 pDirection, float pDistance, float pDuration)
    {
        Vector3 lStartPos = pLevel.transform.position;
        Vector3 lEndPos = pDirection.normalized * pDistance;

        float lDuration = pDuration;
        float lTime = 0f;

        while (lTime <= lDuration)
        {
            pLevel.transform.position = Vector3.Lerp(lStartPos, lEndPos, Curves.EaseInOutSine(lTime / lDuration));

            lTime += Time.deltaTime;
            yield return 0;
        }
    }
}
