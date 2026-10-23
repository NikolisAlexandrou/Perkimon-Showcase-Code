using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using TMPro;
using Cinemachine;
using UnityEngine.UI;
using Unity.VisualScripting;

[RequireComponent(typeof(NPC_Stats), typeof(NPC_Behaviour), typeof(NPC_Interact_Script))]
public class BattleTrainer : MonoBehaviour
{
    [Header("Trainer data")]

    [Tooltip("The trainer sprite displayed in battle")]
    [SerializeField] private Sprite _inBattleTrainerSprite;

    [Tooltip("Music playing in battle when battling this trainer")]
    [SerializeField] private AudioSource _battleMusic;

    [Tooltip("Set to play player VS trainer animation transition just before battle starts")]
    [SerializeField] private bool _vsTitle;

    [Header("Money Reward")]

    [Tooltip("The amount of money the trainer will give to player when player wins the battle")]
    [SerializeField] private int _moneyReward;

    [Header("Detection Settings")]

    [Tooltip("FOV object, set as the child of the trainert object")]
    [SerializeField] private GameObject _fovObject;

    [Tooltip("If true, trainer can detect player otherwise trainer cant spot player")]
    [SerializeField] private bool _useLineOfSight = true;    // Whether obstacles block vision

    [Tooltip("Maximum distance to detect player")]
    [SerializeField] private float _detectionRange = 5f;     // Maximum distance to detect the player

    [Tooltip("Angle determines how much the trainer can see to spot the player from left/right")]
    [SerializeField] private float _fovAngle = 15;

    [Tooltip("The layer that will block the trainer vision")]
    [SerializeField] private LayerMask _obstacleMask;        // Layer for obstacles (e.g., walls)
    private bool _detectedPlayer;    //  Flag for when trainer has seen the player
    private bool _triggeredBattle;   //  Flag for when trainer has triggered a trainer battle
    private bool _isDefeated;          //  Flag for preventing trainer to rebattle player if already battled

    [Tooltip("Trainer party")]
    [SerializeField] private List<BattleMonster> _monstersOwned = new List<BattleMonster>();

    [Tooltip("Dialogue displayed when trainer detected player or player interacted trainer")]
    [TextArea]
    public List<string> PreBattleDialogue = new List<string>();

    [Tooltip("Dialogue displayed in battle and after battle when player has defeated the trainer")]
    [TextArea]
    public List<string> TrainerDefeatDialogue = new List<string>();

    [Tooltip("Dialogue displayed in battle when trainer has defeated the player")]
    [TextArea]
    public List<string> TrainerWinDialogue = new List<string>();

    /// <summary>
    /// Returns a list of all the monster party members of this trainer
    /// </summary>
    public List<BattleMonster> TrainerParty => _monstersOwned;

    /// <summary>
    /// Returns true if trainer has seen the player
    /// </summary>

    public bool DetectedPlayer => _detectedPlayer;
    /// <summary>
    /// Get how much money the trainer will give to player once defeated
    /// </summary>

    public int MoneyReward => _moneyReward;

    /// <summary>
    /// Get the in battle trainer sprite
    /// </summary>
    public Sprite InBattleTrainerSprite => _inBattleTrainerSprite;

    /// <summary>
    /// Get the music that will be playing in battle with this trainer
    /// </summary>
    public AudioSource BattleMusic => _battleMusic;

    /// <summary>
    /// Get the trainer FOV object
    /// </summary>
    public GameObject FOVObject => _fovObject;

    /// <summary>
    /// Check if trainer can detect player
    /// </summary>
    public bool UseLineOfSight => _useLineOfSight;

    /// <summary>
    /// Get or set if the player has defeated this trainer
    /// </summary>
    public bool IsDefeated { get => _isDefeated; set => _isDefeated = value; }

    private void Start() => InitialiseTrainerPartyMonsters();
    private void FixedUpdate() => DetectPlayer();

    /// <summary>
    /// Initialises trainer monsters in party
    /// </summary>
    public void InitialiseTrainerPartyMonsters()
    {
        for (int i = 0; i < _monstersOwned.Count; i++)
            _monstersOwned[i].Initialise();
    }

    /// <summary>
    /// Reset trainer back to initial state as if player has never battle this trainer
    /// </summary>
    public void ResetBattle()
    {
        _triggeredBattle = false;
        _detectedPlayer = false;
        if (!GetComponent<NPC_Interact_Script>()) GetComponent<NPC_Interact_Script>().HasBeenInteracted = false;
    }

    /// <summary>
    /// Handles logic for detecting player and going to battle preparation state if detected the player
    /// </summary>
    public void DetectPlayer()
    {
        if (GameUtils.i.battleSystem.IsInBattle || GameUtils.i.battleSystem.CurrentState is FreezeState)   //  Check if player is not in battle or freeze state
            return;
        if (PartyRules.HasAliveMonster(PlayerStats.i.PlayerParty) == false)  //  Check if player has atleast one alive monster
            return;
        if (_detectedPlayer) //  Check if player has not already been detected
            return;
        if (IsPlayerInFOV() == false)   //  Check if player is in trainer FOV
            return;
        if (_isDefeated)   //  Check if player has not already defeated this trainer
            return;

        _detectedPlayer = true;
        GameUtils.i.SetGameState(GameState.FREEZE);
        TrainerPrepareBattle();
    }
    public void TrainerPrepareBattle() => StartCoroutine(TrainerDetectPlayerAnimationStart());

    /// <summary>
    /// Play animation of trainer detecting player (spawn exclamation mark, move camera to trainer)
    /// </summary>
    public IEnumerator TrainerDetectPlayerAnimationStart()
    {
        GetComponent<NPC_Behaviour>().ResetMovement();  //  Sets the NPC state to idle
        GameUtils.i.virtualCamera.GetComponent<CinemachineVirtualCamera>().Follow = transform;  //  Camera centers to the trainer object for a time
        yield return new WaitForSeconds(.2f);
        GameUtils.i.spawnExclamationMarkOB(transform);  //  Spawn exclamation mark of detecting the player
        yield return new WaitForSeconds(.3f);
        TrainerDetectPlayerAnimationFinish();
    }

    /// <summary>
    /// Display pre battle dialogue and reset camera position back to player
    /// </summary>
    public void TrainerDetectPlayerAnimationFinish()
    {
        if (_triggeredBattle == true)
            return;

        _triggeredBattle = true;
        GameUtils.i.virtualCamera.GetComponent<CinemachineVirtualCamera>().Follow = GameUtils.i.player.transform;   //  Camera centers to player object
        UnityEvent events = new UnityEvent();
        if (_vsTitle == true)
            events.AddListener(SetUpVsTitleTransition);  //  Add VSTitle transition to battle
        else
            events.AddListener(SetUpTransitionToBattle); //  If not VSTitle, add normal transition

        // Initialise a new NPC dialogue for pre battle
        GameUtils.i.dialogueSystem.InitialiseNewTextNPC(PreBattleDialogue, GetComponent<NPC_Stats>().NPCName, GetComponent<NPC_Stats>().NPCDialogueSprite, events);
    }

    /// <summary>
    /// Set up the VSTitle transition and play battle music
    /// </summary>
    public void SetUpVsTitleTransition()
    {
        UIManager.i.VSTitleTransition.SetActive(true);
        UIManager.i.VSTitlePlayerName.GetComponent<TextMeshProUGUI>().text = "Player";
        UIManager.i.VSTitleTrainerName.GetComponent<TextMeshProUGUI>().text = GetComponent<NPC_Stats>().NPCName;
        UIManager.i.VSTitleTrainerImage.GetComponent<Image>().sprite = GetComponent<NPC_Stats>().NPCDialogueSprite;
        GameUtils.i.battleSystem.CurrentTrainer = this;
        _battleMusic.Play();
    }

    /// <summary>
    /// Set up the normal transition to battle and play battle music
    /// </summary>
    public void SetUpTransitionToBattle()
    {
        GameUtils.i.battleSystem.RadialBattleTransition.SetActive(true);
        GameUtils.i.battleSystem.CurrentTrainer = this;
        _battleMusic.Play();
    }

    /// <summary>
    /// Trigger trainer battle and remove transitions
    /// </summary>
    public void StartBattle()
    {
        GameUtils.i.battleSystem.RadialBattleTransition.SetActive(false);
        GameUtils.i.battleSystem.CurrentTrainer = this;
        GameUtils.i.battleSystem.TriggerTrainerBattle();
    }

    /// <summary>
    /// Returns if player is in trainer FOV
    /// </summary>
    public bool IsPlayerInFOV()
    {
        // 1. Distance Check
        float distToPlayer = Vector2.Distance(transform.position, GameUtils.i.player.transform.position);
        if (distToPlayer > _detectionRange)
            return false;

        // 2. Angle Check
        // Determine the angle between the enemy’s forward direction and the direction to the player.
        // For top-down, let's assume "forward" is the enemy's right (x-axis) if using rb.rotation.
        // Alternatively, you can define forward as 'transform.up' or 'transform.right' depending on your sprite orientation.

        Vector2 directionToPlayer = (GameUtils.i.player.transform.position - FOVObject.transform.position).normalized;
        Vector2 enemyForward = FOVObject.transform.right;
        float angle = Vector2.Angle(enemyForward, directionToPlayer);

        if (angle > (_fovAngle / 2f))
            return false;

        // 3. Optional: Line-of-Sight Check (Raycast)
        if (_useLineOfSight)
        {
            RaycastHit2D hit = Physics2D.Raycast(
                transform.position,
                directionToPlayer,
                distToPlayer,
                _obstacleMask
            );

            // If the ray hits something *before* reaching the player, the player is blocked
            if (hit.collider != null )
            {
                //  Set Player in FOV if player interacted trainer
                if (GetComponent<NPC_Interact_Script>() != null && GetComponent<NPC_Interact_Script>().HasBeenInteracted == true)
                    return true;
                else
                    return false;
                // The line of sight is blocked by an obstacle
            }
        }

        // If we passed all checks, the player is in FOV
        return true;
    }

    //  Draw the FOV lines of sight in editor
    private void OnDrawGizmos()
    {
        if (FOVObject.gameObject == null)
            return;

        Gizmos.color = Color.red;
        Vector2 forward = FOVObject.transform.right;
        Gizmos.DrawLine(transform.position, (Vector2)FOVObject.transform.position + forward * _detectionRange); // Forward line
        float angle = _fovAngle / 2f;
        for (int i = -1; i <= 1; i += 2)
        {
            Quaternion rotation = Quaternion.Euler(0, 0, i * angle);
            Vector2 direction = rotation * forward;
            Gizmos.DrawLine(transform.position, (Vector2)FOVObject.transform.position + direction * _detectionRange);
        }
    }
}
