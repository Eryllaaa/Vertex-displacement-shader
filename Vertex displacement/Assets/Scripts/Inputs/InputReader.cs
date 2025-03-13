using UnityEngine;
using UnityEngine.InputSystem;

public class InputReader : MonoBehaviour
{
    [SerializeField] private InputActionAsset _InputActions;

    public static InputReader Instance { get; private set; }

    private const string SCULPT_UP = "SculptUp";
    private const string SCULPT_DOWN = "SculptDown";
    private const string RESET_LEVEL = "ResetLevel";
    private const string MODIFIER = "Modifier";
    private const string SCROLL_ACTION = "Scroll";
    //Debug
    private const string DEBUG_RESET_BALL = "DebugResetBall";
    private const string DEBUG_RESET_TERRAIN = "DebugResetTerrain";

    [HideInInspector] public InputAction sculptUpAction;
    [HideInInspector] public InputAction sculptDownAction;
    [HideInInspector] public InputAction resetLevelAction;
    //Modifier
    [HideInInspector] public InputAction modifierAction;
    [HideInInspector] public InputAction scrollAction;
    //Debug
    [HideInInspector] public InputAction DebugResetBallAction;
    [HideInInspector] public InputAction DebugResetTerrainAction;

    [HideInInspector] public bool isScultpingUp;
    [HideInInspector] public bool isScultpingDown;
    [HideInInspector] public bool isResettingLevel;
    [HideInInspector] public float scrollDelta;
    [HideInInspector] public bool isModifier;
    [HideInInspector] public bool isResettingBall;
    [HideInInspector] public bool isResettingTerrain;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }

        sculptUpAction = _InputActions.FindAction(SCULPT_UP);
        sculptDownAction = _InputActions.FindAction(SCULPT_DOWN);
        resetLevelAction = _InputActions.FindAction(RESET_LEVEL);

        modifierAction = _InputActions.FindAction(MODIFIER);
        scrollAction = _InputActions.FindAction(SCROLL_ACTION);

        DebugResetBallAction = _InputActions.FindAction(DEBUG_RESET_BALL);
        DebugResetTerrainAction = _InputActions.FindAction(DEBUG_RESET_TERRAIN);

        RegisterInputs();   
    }

    private void RegisterInputs()
    {
        sculptUpAction.performed += context => isScultpingUp = true;
        sculptUpAction.canceled += context => isScultpingUp = false;

        sculptDownAction.performed += context => isScultpingDown = true;
        sculptDownAction.canceled += context => isScultpingDown = false;

        resetLevelAction.performed += context => isResettingLevel = true;
        resetLevelAction.canceled += context => isResettingLevel = false;

        //Modifier
        modifierAction.performed += context => isModifier = true;
        modifierAction.canceled += context => isModifier = false;

        scrollAction.performed += context => scrollDelta = context.ReadValue<Vector2>().y;
        scrollAction.canceled += context => scrollDelta = 0;

        //Debug
        DebugResetBallAction.performed += context => isResettingBall = true;
        DebugResetBallAction.canceled += context => isResettingBall = false;

        DebugResetTerrainAction.performed += context => isResettingTerrain = true;
        DebugResetTerrainAction.canceled += context => isResettingTerrain = false;
    }

    private void OnEnable()
    {
        sculptUpAction.Enable();
        sculptDownAction.Enable();
        resetLevelAction.Enable();
        modifierAction.Enable();
        scrollAction.Enable();

        DebugResetBallAction.Enable();
        DebugResetTerrainAction.Enable();
    }

    private void OnDisable()
    {
        sculptUpAction.Disable();
        sculptDownAction.Disable();
        resetLevelAction.Disable();
        modifierAction.Disable();
        scrollAction.Disable();

        DebugResetBallAction.Disable();
        DebugResetTerrainAction.Disable();
    }
}
