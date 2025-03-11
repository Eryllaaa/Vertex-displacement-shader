using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private List<Level> levels = new List<Level>();

    private LevelChangeAnimator _levelChangeAnimator;

    private Vector3 _levelInitPos = new Vector3(10000, 0, 0);
    [HideInInspector] public Vector3 levelPlayingPos = Vector3.zero;
    private int _currentLevelIndex = 0;

    private Level _currentLevel = null;

    private void Start()
    {
        _levelChangeAnimator = GetComponent<LevelChangeAnimator>();
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
        StartLevel(_currentLevelIndex % levels.Count, pDir, _levelChangeAnimator.levelChangeDistance, _levelChangeAnimator.levelChangeDuration);
    }

    public void StartPreviousLevel(Vector3 pDir)
    {
        _currentLevelIndex = Mathf.Clamp(_currentLevelIndex - 1, 0, levels.Count - 1);
        StartLevel(_currentLevelIndex, pDir, _levelChangeAnimator.levelChangeDistance, _levelChangeAnimator.levelChangeDuration);
    }

    public void StartLevel(Level pLevel)
    {
        pLevel.transform.position = levelPlayingPos;
        _currentLevel = pLevel;
        _levelChangeAnimator.StartLevel(pLevel);
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
            _levelChangeAnimator.StartLevelSwitchAnimation(_currentLevel, pLevel, pDir, _levelChangeAnimator.levelChangeDistance, _levelChangeAnimator.levelChangeDuration);
        }

        _currentLevel = pLevel;
    }
}
