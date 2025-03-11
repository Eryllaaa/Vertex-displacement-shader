using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private List<Level> levels = new List<Level>();

    [SerializeField] private float _levelChangeDistance;
    [SerializeField] private float _levelChangeDuration;

    private int _currentLevelIndex = 0;

    private Vector3 _levelPlayingPos = Vector3.zero;
    private Vector3 _levelInitPos = new Vector3(10000, 0, 0);
    private Level _currentLevel = null;

    private void Start()
    {
        InitLevels();
        StartLevel(levels[_currentLevelIndex]);
    }

    private void InitLevels()
    {
        for (int i = 0; i < levels.Count; i++)
        {
            levels[i] = Instantiate(levels[i]);
            levels[i].transform.position = _levelInitPos;
        }
    }

    private void Update()
    {
        // for debug purposes only
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            StartNextLevel(Vector3.right);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            StartPreviousLevel(Vector3.left);
        }
    }
    public void StartNextLevel(Vector3 pDir)
    {
        _currentLevelIndex = Mathf.Clamp(_currentLevelIndex + 1, 0, levels.Count - 1);
        StartLevel(_currentLevelIndex % levels.Count, pDir, _levelChangeDistance, _levelChangeDuration);
    }

    public void StartPreviousLevel(Vector3 pDir)
    {
        _currentLevelIndex = Mathf.Clamp(_currentLevelIndex - 1, 0, levels.Count - 1);
        StartLevel(_currentLevelIndex, pDir, _levelChangeDistance, _levelChangeDuration);
    }

    public void StartLevel(Level pLevel)
    {
        pLevel.transform.position = _levelPlayingPos;
        _currentLevel = pLevel;
        if (_levelBGChangeRoutine != null) StopCoroutine(_levelBGChangeRoutine);
        StartCoroutine(_levelBGChangeRoutine = LevelBGChangeRoutine(Camera.main.backgroundColor, _currentLevel.bgColor, _levelChangeDuration * 0.233f));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pLevelIndex"></param>
    /// <param name="pDir">The direction in which the transition is going to happen, meaning the current level will go in the opposite direction while the selected level will go in that direction to travel to the center of the screen.</param>
    /// <param name="pDistance"></param>
    public void StartLevel(int pLevelIndex, Vector3 pDir, float pDistance, float pDuration)
    {
        StartLevel(levels[pLevelIndex], pDir, pDistance, pDuration);
    }

    private void StartLevel(Level pLevel, Vector3 pDir, float pDistance, float pDuration)
    {
        //pLevel.transform.position = _levelPlayingPos;
        
        if (_currentLevel != null && _currentLevel != pLevel)
        {
            StartLevelSwitchAnimation(_currentLevel, pLevel, pDir, pDistance, pDuration);
        }

        _currentLevel = pLevel;
    }

    private void StartLevelSwitchAnimation(Level pCurrent, Level pNext, Vector3 pDir, float pDistance, float pDuration)
    {
        if (_levelInChangeRoutine != null) StopCoroutine(_levelInChangeRoutine);
        if (_levelOutChangeRoutine != null) StopCoroutine(_levelOutChangeRoutine);
        if (_levelBGChangeRoutine != null) StopCoroutine(_levelBGChangeRoutine);

        StartCoroutine(_levelInChangeRoutine = LevelInChangeRoutine(pNext, pDir, pDuration));
        StartCoroutine(_levelOutChangeRoutine = LevelOutChangeRoutine(pCurrent, pDir, pDistance, pDuration));
        StartCoroutine(_levelBGChangeRoutine = LevelBGChangeRoutine(pCurrent.bgColor, pNext.bgColor, pDuration));
    }

    private IEnumerator _levelBGChangeRoutine = null;
    private IEnumerator LevelBGChangeRoutine(Color pCurrent, Color pNext, float pDuration)
    {
        Camera lCamera = Camera.main;

        float lDuration = pDuration;
        float lTime = 0f;

        while (lTime <= pDuration)
        {
            lCamera.backgroundColor = Color.Lerp(pCurrent, pNext, Curves.EaseInOutSine(lTime / lDuration));

            lTime += Time.deltaTime;
            yield return 0;
        }
    }

    private IEnumerator _levelInChangeRoutine = null;
    private IEnumerator LevelInChangeRoutine(Level pLevel, Vector3 pDirection, float pDuration)
    {
        Vector3 lStartPos;

        if (pLevel.levelRenderer.isVisible) lStartPos = pLevel.transform.position;
        else lStartPos = pDirection.normalized * -1 * 100;
        
        Vector3 lEndPos = _levelPlayingPos;

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
