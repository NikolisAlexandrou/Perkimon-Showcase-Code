using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
public enum NPCState
{
    IDLE,
    WALK,
}

/// <summary>
/// Handles behaviour for NPC's such as walking in a specific pattern,
/// Looking at directions in a specific pattern, looking at just one direction,
/// Also stores all walk sprites in lists so that animations are handled in script
/// and not in Animator so that it is more robust
/// </summary>
public class NPC_Behaviour : MonoBehaviour
{
    private NPCState _npcState = NPCState.IDLE;

    [Header("Movement Sprites")]

    [Tooltip("Sprite frame sequence of animation playing when npc walks down (index 1: should indicate an idle sprite)")]
    [SerializeField] private List<Sprite> _walkSpritesDown = new List<Sprite>();

    [Tooltip("Sprite frame sequence of animation playing when npc walks left (index 1: should indicate an idle sprite)")]
    [SerializeField] private List<Sprite> _walkSpritesLeft = new List<Sprite>();

    [Tooltip("Sprite frame sequence of animation playing when npc walks right (index 1: should indicate an idle sprite)")]
    [SerializeField] private List<Sprite> _walkSpritesRight = new List<Sprite>();

    [Tooltip("Sprite frame sequence of animation playing when npc walks up (index 1: should indicate an idle sprite)")]
    [SerializeField] private List<Sprite> _walkSpritesUp = new List<Sprite>();

    [Header("Movement Pattern")]

    [Tooltip("Sequence of directions that npc will move, values in x and y represent how many tiles the npc will move towards that direction (loops)")]
    [SerializeField] private List<Vector2> _movePattern;

    private Vector2 _lastPositionStopped;  // The position the npc last arrived when walking in a pattern
    private Vector2 _currentTargetPosition;  // The position the npc is currently walking towards to
    private int _currentMovePattern;  // The current index in _movePattern that npc is currently walking to

    [Header("Rotation Pattern")]

    [Tooltip("Directions sequence the npc will look at (loops) (example: x: 1 y: 0 -> looks to the right)")]
    [SerializeField] private List<Vector2> _directionsLooking;

    [Tooltip("Time in seconds to change to next direction in the Directions Looking list")]
    [SerializeField] private float _changeDirectionTime;

    private float _currentChangeDirectionTime;  // The current change direction time
    private int _directionLookingIndex;  // The current direction looking index in the Directions Looking list

    [Header("Movement Settings")]

    [Tooltip("Current direction looking (example: x: 0 y: -1 -> npc looks down)")]
    [SerializeField] private Vector2 _directionLooking;

    [Tooltip("Movement speed of the NPC when walking")]
    [SerializeField] private float _moveSpeed = 3f;

    [Tooltip("Distance from target position at which NPC will stop moving and consider itself arrived at destination")]
    [SerializeField] private float _stoppingDistance = 0.2f;  // Distance at which NPC considers itself "arrived"

    [Tooltip("How fast the walk animation frames are changing per second when NPC is walking")]
    [SerializeField] private float _moveAnimationSpeed = 0.2f;

    [Header("Obstacle Detection Settings")]

    [Tooltip("Layer(s) that will block the vision of the NPC (Set the player layer aswell so that NPCs do not walk through the player)")]
    [SerializeField] private LayerMask _obstacleLayer;    // Layer(s) that block the NPC, e.g., Player

    [Tooltip("How far in direction looking the NPC will check for obstacles")]
    [SerializeField] private float _detectionDistance = 1f;  // How far in front to check for obstacles

    [Tooltip("Distance at which NPC will stop moving if an obstacle is near")]
    [SerializeField] private float _detectionRadius = 0.4f;  // Radius for circlecast

    private float _animationTimer;
    private int _currentAnimationFrame;

    Rigidbody2D rb;

    /// <summary>
    /// Get or set the NPCs current direction looking
    /// </summary>
    public Vector2 DirectionLooking { get => _directionLooking; set => _directionLooking = value; }

    /// <summary>
    /// Get or set the NPCs current state
    /// </summary>
    public NPCState GetNPCState { get => _npcState; set => _npcState = value; }

    /// <summary>
    /// Get or set the NPCs current target position that is moving to
    /// </summary>
    public Vector2 CurrentTargetPosition { get => _currentTargetPosition; set => _currentTargetPosition = value; }

    /// <summary>
    /// Get or set the NPCs last position stopped
    /// </summary>
    public Vector2 LastPositionStopped { get => _lastPositionStopped; set => _lastPositionStopped = value; }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        _npcState = NPCState.IDLE;

        // Set the last position stopped to NPC's initial position
        _lastPositionStopped = transform.position;

        if (_movePattern.Count == 0)
        {
            // Set current target position to the NPC initial position for no movement
            _currentTargetPosition = new Vector2(0, 0);
            DebugLogs.Warning("NPCMovement: No targetPosition assigned.");
        }
        else
        {
            // Set the current target position to first element on move Pattern list
            _currentTargetPosition = _movePattern[_currentMovePattern];
        }
    }

    void Update()
    {
        switch (_npcState)
        {
            case NPCState.IDLE:
                // while npc is idle, look at direction facing and dont move
                rb.linearVelocity = Vector2.zero;
                PlayIdleAnimation(_directionLooking);
                break;
            case NPCState.WALK:
                break;
            default:
                break;
        }

        // if player and trainer are in battle, dont run any events
        if (GameUtils.i.battleSystem.IsInBattle == true || GameUtils.i.battleSystem.CurrentState is FreezeState)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }
        // if player has interacted with npc, move to idle state
        if (GetComponent<NPC_Interact_Script>() != null && GetComponent<NPC_Interact_Script>().HasBeenInteracted == true)
        {
            _npcState = NPCState.IDLE;
            return;
        }

        ChangeDirection();

        // if there are no movement patterns, dont move but still run some trainer events
        if (_movePattern.Count == 0)
            return;

        // 1. Check if we've reached the target
        float distanceToTarget = Vector2.Distance(transform.position, _lastPositionStopped + _currentTargetPosition);

        if (distanceToTarget <= _stoppingDistance)
        {
            // Optional: NPC can idle, play an animation, etc.
            // For now, we'll just stop moving
            // continue moving with move pattern if NPC is not a trainer or if NPC hasnt detected player yet

            if (GetComponent<BattleTrainer>() == null || (GetComponent<BattleTrainer>() != null && GetComponent<BattleTrainer>().DetectedPlayer == false))
            {
                if (_currentMovePattern < _movePattern.Count - 1)
                    _currentMovePattern++;
                else
                    _currentMovePattern = 0;

                _lastPositionStopped = transform.position;
                _currentTargetPosition = _movePattern[_currentMovePattern];
            }
            _npcState = NPCState.IDLE;
            return;
        }
        // 2. Determine direction toward the target
        if (_directionsLooking.Count > 0)
            return;

        Vector2 direction = ((_lastPositionStopped + _currentTargetPosition) - (Vector2)transform.position).normalized;
        _directionLooking = direction;

        // Move towards that direction and play walk animation
        Move(_directionLooking);
        PlayWalkAnimation(_directionLooking);
    }

    /// <summary>
    /// Plays the walk animation frame by frame at each direction the NPC is looking at when walking
    /// </summary>
    /// <param name="direction"></param>
    private void PlayWalkAnimation(Vector2 direction)
    {
        // If the NPC is in walk state continue
        if (_npcState == NPCState.IDLE)
            return;

        if (_animationTimer > 0)
        {
            // Set the sprite direction animation by calculating direction looking as a circlular angle
            float angle = (Mathf.Atan2(direction.y, direction.x) * 180) / Mathf.PI;
            if (angle <= 45 && angle >= -45)
                GetComponent<SpriteRenderer>().sprite = _walkSpritesRight[_currentAnimationFrame];
            else if (angle > 45 && angle < 135)
                GetComponent<SpriteRenderer>().sprite = _walkSpritesUp[_currentAnimationFrame];
            else if (angle < -45 && angle > -135)
                GetComponent<SpriteRenderer>().sprite = _walkSpritesDown[_currentAnimationFrame];
            else
                GetComponent<SpriteRenderer>().sprite = _walkSpritesLeft[_currentAnimationFrame];
           
            // If the NPC is also a battle Trainer, rotate its FOV object to match the direction looking
            if (GetComponent<BattleTrainer>() != null)
                GetComponent<BattleTrainer>().FOVObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

            _animationTimer -= Time.deltaTime;
        }
        else
        {
            // When animation time is 0, moves to next frame index in the walk list
            // if the next index is more than list size, move back to first element (loops)
            _animationTimer = _moveAnimationSpeed;
            if (_currentAnimationFrame + 1 < _walkSpritesDown.Count)
                _currentAnimationFrame++;
            else
                _currentAnimationFrame = 0;
        }
    }

    /// <summary>
    /// Sets the direction of NPC sprite based on direction given
    /// </summary>
    /// <param name="direction"></param>
    private void PlayIdleAnimation(Vector2 direction)
    {
        // If the NPC is in walk state continue
        if (_npcState != NPCState.IDLE)
            return;

        // Set the sprite direction animation by calculating direction looking as a circlular angle
        // index 1 indicate a sprite from idle state
        float angle = (Mathf.Atan2(direction.y, direction.x) * 180) / Mathf.PI;
        if (angle <= 45 && angle >= -45)
            GetComponent<SpriteRenderer>().sprite = _walkSpritesRight[1];
        else if (angle > 45 && angle < 135)
            GetComponent<SpriteRenderer>().sprite = _walkSpritesUp[1];
        else if (angle < -45 && angle > -135)
            GetComponent<SpriteRenderer>().sprite = _walkSpritesDown[1];
        else
            GetComponent<SpriteRenderer>().sprite = _walkSpritesLeft[1];

        // If the NPC is also a battle Trainer, rotate its FOV object to match the direction looking
        if (GetComponent<BattleTrainer>() != null)
            GetComponent<BattleTrainer>().FOVObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }

    /// <summary>
    /// If there are any elements in directions looking list, set the direction looking
    /// of the NPC in sequence after some changeDirectionTime time and repeat
    /// </summary>
    private void ChangeDirection()
    {
        if (_directionsLooking.Count == 0)
            return;

        // used to change currentDirection pattern with Time and repeat
        if (_currentChangeDirectionTime >= _changeDirectionTime)
        {
            _currentChangeDirectionTime = 0;
            _directionLookingIndex++;

            // if the current direction is out of list count, move to 0 (loop)
            if (_directionLookingIndex >= _directionsLooking.Count)
                _directionLookingIndex = 0;
        }
        else
        {
            // Continue time
            _currentChangeDirectionTime += Time.deltaTime;
        }

        // Set the direction looking based on current index in directions looking list
        _directionLooking = _directionsLooking[_directionLookingIndex];  

    }

    /// <summary>
    /// Moves the NPC to the direction given
    /// </summary>
    /// <param name="direction"></param>
    private void Move(Vector2 direction)
    {

        // 1. Check if something (e.g., player) is blocking the path
        bool isBlocked = IsPathBlocked(direction);
        if (GetComponent<BattleTrainer>() != null && GetComponent<BattleTrainer>().IsDefeated == false)
            isBlocked = isBlocked || GetComponent<BattleTrainer>().IsPlayerInFOV();

        // 2. Move or stop based on whether the path is blocked
        if (!isBlocked)
        {
            // Move toward the target
            rb.linearVelocity = direction * _moveSpeed;

            _npcState = NPCState.WALK;
        }
        else
        {
            // Stop movement when player is blocking path
            DebugLogs.Log($"NPC: {gameObject.name}'s path is blocked.");
            _npcState = NPCState.IDLE;
        }
    }

    /// <summary>
    /// Checks if an obstacle (on obstacleLayer) is in front of the NPC.
    /// </summary>
    private bool IsPathBlocked(Vector2 direction)
    {
        // CircleCast (or Raycast) in the direction of movement
        // from the NPC's position
        RaycastHit2D hit = Physics2D.CircleCast(
            transform.position,        // center
            _detectionRadius,          // radius of the circle
            direction,                 // direction
            _detectionDistance,        // how far to check
            _obstacleLayer             // layers to check
        );

        // If hit is not null, something is blocking the path
        return (hit.collider != null);
    }

    /// <summary>
    /// Moves the NPC to idle state to stop it from moving
    /// </summary>
    public void ResetMovement() => _npcState = NPCState.IDLE;
}
