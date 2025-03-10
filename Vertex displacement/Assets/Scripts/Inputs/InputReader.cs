using UnityEngine;
using UnityEngine.InputSystem;

public class InputReader : MonoBehaviour
{
    [SerializeField]InputActionAsset _InputActions;

    public static InputReader Instance { get; private set; }

    private const string ACTION_MAP_NAME = "ActionMap";

    private const string SCULPT_UP = "SculptUp";
    private const string SCULPT_DOWN = "SculptDown";
    private const string RESET_LEVEL = "ResetLevel";
    private const string STRENGHT_VARIABLE = "StrenghtVariable";
    private const string STRENGHT_MODIFIER = "StrenghtModifier";
    //Debug
    private const string DEBUG_RESET_BALL = "DebugResetBall";
    private const string DEBUG_RESET_TERRAIN = "DebugResetTerrain";

    [HideInInspector] public InputAction sculptUpAction;
    [HideInInspector]public InputAction sculptDownAction;
    [HideInInspector]public InputAction resetLevelAction;
    [HideInInspector]public InputAction strenghtVariableAction;
    [HideInInspector]public InputAction strenghtModifierAction;
    //Debug
    [HideInInspector]public InputAction DebugResetBallAction;
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
        strenghtVariableAction = _InputActions.FindAction(STRENGHT_VARIABLE);
        strenghtModifierAction = _InputActions.FindAction(STRENGHT_MODIFIER);
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

        strenghtVariableAction.performed += context => scrollDelta = context.ReadValue<float>();
        strenghtVariableAction.canceled += context => scrollDelta = 0;

        strenghtModifierAction.performed += context => isModifier = true;
        strenghtModifierAction.canceled += context => isModifier = false;

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
        strenghtVariableAction.Enable();
        strenghtModifierAction.Enable();

        DebugResetBallAction.Enable();
        DebugResetTerrainAction.Enable();
    }

    private void OnDisable()
    {
        sculptUpAction.Disable();
        sculptDownAction.Disable();
        resetLevelAction.Disable();
        strenghtVariableAction.Disable();
        strenghtModifierAction.Disable();

        DebugResetBallAction.Disable();
        DebugResetTerrainAction.Disable();
    }
}
