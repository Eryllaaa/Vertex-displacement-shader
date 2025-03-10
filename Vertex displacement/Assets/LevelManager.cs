using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private List<Level> levels = new List<Level>();

    private int _currentLevelIndex = 0;

    private Vector3 _levelStartPos = Vector3.zero;
    private Level _currentLevel = null;

    private IEnumerator _levelChangeRoutine = null;

    private void Start()
    {
        StartLevel(levels[_currentLevelIndex]);
    }

    public void StartNextLevel()
    {
        _currentLevelIndex++;
        StartLevel(levels[_currentLevelIndex]);
    }

    public void StartPreviousLevel()
    {
        _currentLevelIndex--;
        StartLevel(levels[_currentLevelIndex]);
    }

    public void StartLevelN(int pLevelIndex)
    {
        StartLevel(levels[pLevelIndex]);
    }

    private void StartLevel(Level pLevel)
    {
        Level lLevel = Instantiate(pLevel);
        lLevel.transform.position = _levelStartPos;

        if (_currentLevel != null)
        {
            StartLevelChangeRoutine(_currentLevel.transform, pLevel.transform);
        }

        _currentLevel = pLevel;
    }

    private void StartLevelChangeRoutine(Transform pCurrent, Transform pNext)
    {
        if (_levelChangeRoutine != null) StopCoroutine(_levelChangeRoutine);
        StartCoroutine(ChangeLevelRoutine(pCurrent, pNext));
    }

    private IEnumerator ChangeLevelRoutine(Transform pCurrent, Transform pNext)
    {
        yield return 0;
    }
}
