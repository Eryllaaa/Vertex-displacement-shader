using UnityEngine;
using UnityEngine.InputSystem;

public class InputReader : MonoBehaviour
{
    [SerializeField] private InputActionAsset _InputActions;

    public static InputReader Instance { get; private set; }

    private const string SCULPT_UP = "SculptUp";
    private const string SCULPT_DOWN = "SculptDown";
    private const string RESET_LEVEL = "ResetLevel";
    private const string STRENGTH_VARIABLE = "StrenghtVariable";
    private const string STRENGTH_MODIFIER = "StrenghtModifier";
    //Debug
    private const string DEBUG_RESET_BALL = "DebugResetBall";
    private const string DEBUG_RESET_TERRAIN = "DebugResetTerrain";

    [HideInInspector] public InputAction sculptUpAction;
    [HideInInspector] public InputAction sculptDownAction;
    [HideInInspector] public InputAction resetLevelAction;
    [HideInInspector] public InputAction strengthVariableAction;
    [HideInInspector] public InputAction strengthModifierAction;
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
            Destroy(gameObject);
        }

        sculptUpAction = _InputActions.FindAction(SCULPT_UP);
        sculptDownAction = _InputActions.FindAction(SCULPT_DOWN);
        resetLevelAction = _InputActions.FindAction(RESET_LEVEL);
        strengthVariableAction = _InputActions.FindAction(STRENGTH_VARIABLE);
        strengthModifierAction = _InputActions.FindAction(STRENGTH_MODIFIER);
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

        strengthVariableAction.performed += context => scrollDelta = context.ReadValue<Vector2>().y;
        strengthVariableAction.canceled += context => scrollDelta = 0;

        strengthModifierAction.performed += context => isModifier = true;
        strengthModifierAction.canceled += context => isModifier = false;

        DebugResetBallAction.performed += context => isResettingBall = true;
        DebugResetBallAction.canceled += context => isResettingBall = false;

        DebugResetTerrainAction.performed += context => isResettingTerrain = true;
        DebugResetTerrainAction.canceled += context => isResettingTerrain = false;
    }

    private void Update()
    {
        print(isScultpingUp);
    }

    private void OnEnable()
    {
        sculptUpAction.Enable();
        sculptDownAction.Enable();
        resetLevelAction.Enable();
        strengthVariableAction.Enable();
        strengthModifierAction.Enable();

        DebugResetBallAction.Enable();
        DebugResetTerrainAction.Enable();
    }

    private void OnDisable()
    {
        sculptUpAction.Disable();
        sculptDownAction.Disable();
        resetLevelAction.Disable();
        strengthVariableAction.Disable();
        strengthModifierAction.Disable();

        DebugResetBallAction.Disable();
        DebugResetTerrainAction.Disable();
    }
}
