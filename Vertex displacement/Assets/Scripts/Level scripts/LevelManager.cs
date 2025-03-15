using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private List<Level> levels = new List<Level>();

    private LevelChangeAnimator _levelChangeAnimator;

    [HideInInspector] public Vector3 levelPlayingPos = Vector3.zero;
    private Vector3 _levelInitPos = new Vector3(10000, 0, 0);
    private int _currentLevelIndex = 0;

    private Level _currentLevel = null;

    private void Start()
    {
        _levelChangeAnimator = GetComponent<LevelChangeAnimator>();
        InitLevels();
        StartLevel(levels[_currentLevelIndex], Vector2.right);
    }

    private void InitLevels()
    {
        for (int i = 0; i < levels.Count; i++)
        {
            levels[i] = SpawnLevel(levels[i], _levelInitPos);
        }
    }

    private Level SpawnLevel(Level pLevel, Vector3 pPos)
    {
        Level lLevel = Instantiate(pLevel);
        lLevel.transform.position = pPos;
        return lLevel;
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
        // -----------------------
    }

    public void StartNextLevel(Vector3 pDir)
    {
        if (_currentLevelIndex + 1 > levels.Count - 1) return;
        StartLevel(++_currentLevelIndex, pDir);
    }

    public void StartPreviousLevel(Vector3 pDir)
    {
        if (_currentLevelIndex - 1 < 0) return;
        StartLevel(--_currentLevelIndex, pDir);
    }

    public void StartLevelWithoutAnimation(Level pLevel, Vector2 pDir)
    {
        pLevel.transform.position = levelPlayingPos;
        _currentLevel = pLevel;
        _levelChangeAnimator.StartLevelWithoutTransition(pLevel, pDir);
    }

    public void StartLevel(int pLevelIndex, Vector2 pDir)
    {
        StartLevel(levels[pLevelIndex], pDir);
    }

    private void StartLevel(Level pNextLevel, Vector2 pDir)
    {
        bool lCurrentLevelIsNull = _currentLevel == null;
        if (lCurrentLevelIsNull)
        {
            _levelChangeAnimator.StartLevelWithoutTransition(pNextLevel, pDir);
        }
        else if (_currentLevel != pNextLevel)
        {
            _levelChangeAnimator.LevelTransition(_currentLevel, pNextLevel, pDir);
        }

        if (!lCurrentLevelIsNull)
        {
            _currentLevel.SetDisabled();
            _currentLevel.levelWin -= OnLevelWin;
        }

        pNextLevel.levelWin += OnLevelWin;
        pNextLevel.gameObject.SetActive(true);
        pNextLevel.SetPlaying(_levelChangeAnimator.levelChangeDuration);
        _currentLevel = pNextLevel;
    }

    private void OnLevelWin()
    {
        print("--- level win ---");
        StartNextLevel(Vector3.right);
    }
}
