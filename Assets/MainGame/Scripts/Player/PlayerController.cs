using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using island;
using UnityEngine;

/// <summary>
/// Handles player input, physics movement, aiming, throwing capsules,
/// footsteps, menus, and overworld battle encounters.
///
/// PlayerWaterController handles water detection, swimming permission,
/// water movement modifiers, and water-related visuals.
/// </summary>
[RequireComponent(typeof(PlayerWaterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Player Objects")]

    [Tooltip("Camera used for overworld gameplay and mouse aiming.")]
    [SerializeField] private Camera _playerCamera;

    [Tooltip("Controller responsible for the player's held tools.")]
    [SerializeField] private PlayerToolController _toolController;

    [Tooltip("Player legs object. Also assign this in PlayerWaterController for water visibility.")]
    [SerializeField] private GameObject _legsObject;

    [Tooltip("Position where normal ground footsteps are spawned.")]
    [SerializeField] private Transform _footTransform;

    [Tooltip("Player shadow object. Water visibility is handled by PlayerWaterController.")]
    [SerializeField] private GameObject _shadowObject;

    [Tooltip("Visual pointer rotated towards the player's aiming direction.")]
    [SerializeField] private GameObject _throwAimPointer;

    [Header("Water")]

    [Tooltip("Handles shallow/deep water detection, swimming requirements, and swimming visuals. Automatically found on this GameObject if unassigned.")]
    [SerializeField] private PlayerWaterController _waterController;

    [Header("Movement Settings")]

    [Tooltip("Base player movement speed in world units per second.")]
    [SerializeField] private float _walkSpeed = 5f;

    [Tooltip("Added to the movement multiplier while on stairs. For example, -0.25 reduces base speed by 25%.")]
    [SerializeField] private float _stairsWalkBoost = -0.25f;

    [Tooltip("Added to the movement multiplier while Run is held on land. For example, 0.5 increases base speed by 50%.")]
    [SerializeField] private float _runBoost = 0.5f;

    [Header("Area Settings")]

    [Tooltip("Area the player is currently inside. Updated when entering an Area-tagged trigger with a MapArea component.")]
    [SerializeField] private MapArea _currentArea;

    [Header("Footsteps Settings")]

    [Tooltip("Prefab spawned at the player's feet while walking on normal ground.")]
    [SerializeField] private GameObject _footstepObject;

    [Tooltip("Prefab spawned while walking through tall grass.")]
    [SerializeField] private GameObject _tallGrassFootstepObject;

    [Tooltip("Prefab spawned while moving through shallow or deep water.")]
    [SerializeField] private GameObject _waterBubbleObject;

    [Tooltip("Time in seconds between normal ground or grass footstep effects.")]
    [SerializeField] private float _footstepFrequency = 0.15f;

    [Tooltip("Time in seconds between water movement effects.")]
    [SerializeField] private float _waterbubbleFrequency = 0.2f;

    [Header("Buttons")]

    [Tooltip("Legacy key cooldown setting. Currently not used by this script.")]
    [SerializeField] private float _keyPressCooldown = 0.1f;

    [Tooltip("Minimum time in seconds between opening or closing the selection menu.")]
    [SerializeField] private float _selectionMenuToggleCooldown = 0.25f;

    [Header("Throw Monster Out")]

    [Tooltip("Overworld follower GameObject containing MonsterFollowScriptTest.")]
    [SerializeField] private GameObject _followingMonster;

    [Tooltip("Base capsule throw strength used to calculate the initial throw and return values.")]
    [SerializeField] private float _throwSpeed;

    [Tooltip("Thrown capsule prefab. Expected to contain Rigidbody2D and PlayerCatchingObject.")]
    [SerializeField] private GameObject _throwMonsterObject;

    [Tooltip("Delay in seconds before throwing is enabled again after ResetThrowCooldown is started.")]
    [SerializeField] private float _throwCatchingObjectCooldown = 1f;

    // Capsule currently thrown by the player.
    private GameObject _currentThrowMonsterObject;

    // Temporary throw value used by the thrown capsule's behaviour.
    private float _temporaryThrowSpeed;

    // Blocks input actions handled in Update.
    private bool _disableActions;

    // Controls whether physics movement is allowed.
    private bool _canMove;

    // Prevents another throw until the current throw has been reset.
    private bool _threwCatchingObject;

    // Used by menu handling to prevent opening during restricted actions.
    private bool _canThrowMonsterOut = true;

    // Tracks whether the selection menu is currently open.
    private bool _initialisedMainMenu;

    // Remaining selection-menu toggle cooldown.
    private float _selectionMenuToggleTimer;

    // Prevents multiple footstep coroutines starting simultaneously.
    private bool _spawnedFootstep;

    // Surface flags controlled by trigger callbacks.
    private bool _onGrass;
    private bool _onStairs;

    // Input and animation state.
    private bool _isAiming;
    private bool _isSprinting;

    // Previous water state, used to play the entry splash once.
    private bool _wasInWater;

    // Final multiplier applied to the base walking speed.
    private float _walkSpeedBoost = 1f;

    // Last movement direction, retained when the player stops.
    private Vector2 moveDirection;

    // Input collected in Update and consumed in FixedUpdate.
    private Vector2 _movementInput;

    private Rigidbody2D rb;
    private Animator anim;

    /// <summary>
    /// Player legs GameObject, exposed for other systems.
    /// </summary>
    public GameObject LegsObject => _legsObject;

    /// <summary>
    /// Current movement speed including run, stairs, and water modifiers.
    /// </summary>
    public float WalkSpeed => _walkSpeed * _walkSpeedBoost;

    /// <summary>
    /// Throw value exposed to the capsule behaviour with its sign reversed.
    /// </summary>
    public float TemporaryThrowCatchingObjectSpeed =>
        -_temporaryThrowSpeed;

    /// <summary>
    /// Overworld follower GameObject.
    /// </summary>
    public GameObject FollowingMonster => _followingMonster;

    /// <summary>
    /// Map area the player most recently entered.
    /// </summary>
    public MapArea CurrentArea => _currentArea;

    /// <summary>
    /// True in either shallow or deep water.
    /// </summary>
    public bool InWater =>
        _waterController != null && _waterController.InWater;

    /// <summary>
    /// True while the player's footprint overlaps deep water.
    /// </summary>
    public bool InDeepWater =>
        _waterController != null && _waterController.InDeepWater;

    /// <summary>
    /// Enables or disables player actions handled by this controller.
    /// </summary>
    public bool IsActionsDisabled
    {
        get => _disableActions;
        set => _disableActions = value;
    }

    /// <summary>
    /// Whether the current footstep effect cooldown is active.
    /// </summary>
    public bool SpawnedFootstep
    {
        get => _spawnedFootstep;
        set => _spawnedFootstep = value;
    }

    /// <summary>
    /// Last cardinal direction faced by the player.
    /// </summary>
    public FacingDirection FacingDirection { get; private set; } =
        FacingDirection.Down;

    /// <summary>
    /// Enables or disables player movement.
    /// </summary>
    public bool CanMove
    {
        get => _canMove;
        set => _canMove = value;
    }

    /// <summary>
    /// Re-enables player movement.
    /// </summary>
    public void EnableMovement()
    {
        _canMove = true;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        // Find the water controller if it was not assigned manually.
        if (_waterController == null)
        {
            _waterController = GetComponent<PlayerWaterController>();
        }

        _canMove = true;
    }

    private void Update()
    {
        // Clear old input so movement cannot persist when Move is skipped.
        _movementInput = Vector2.zero;

        UpdateWaterSounds();

        if (_disableActions)
        {
            // Close the menu while actions are disabled.
            UIManager.i.SelectionMenusOB.SetActive(false);

            // Remove an active thrown capsule.
            if (_currentThrowMonsterObject != null)
            {
                Destroy(_currentThrowMonsterObject.gameObject);
                _threwCatchingObject = false;
            }

            anim.SetBool("isMoving", false);
            anim.SetBool("isRunning", false);
            return;
        }

        // Allow selection-menu input outside battles and dialogue.
        if (GameUtils.i.battleSystem.CurrentState is not InBattleState &&
            GameUtils.i.dialogueSystem.disableText)
        {
            if (!GameUtils.i.IsInCutscene())
            {
                MainMenuPressed();

                anim.SetBool("isMoving", false);
                anim.SetBool("isRunning", false);
            }
        }

        if (GameUtils.i.battleSystem.CurrentState is OverworldState)
        {
            // Read movement input and update movement animations.
            // Actual Rigidbody movement occurs in FixedUpdate.
            Move();

            ThrowMonsterOut();

            // Show the aiming pointer while the Aim button is held.
            if (Input.GetButtonDown("Aim"))
            {
                _throwAimPointer.SetActive(true);
                _isAiming = true;
            }
            else if (Input.GetButtonUp("Aim"))
            {
                _throwAimPointer.SetActive(false);
                _isAiming = false;
            }

            if (GameUtils.i.ControllerConnected())
            {
                RotateAimPointer_Controller();
            }
            else
            {
                RotateAimPointer_Mouse();
            }

            _playerCamera.gameObject.SetActive(true);
        }
        else
        {
            // Battle scenes use their own camera.
            if (GameUtils.i.battleSystem.CurrentState is InBattleState)
            {
                _playerCamera.gameObject.SetActive(false);
            }

            anim.SetBool("isMoving", false);
            anim.SetBool("isRunning", false);
        }
    }

    private void FixedUpdate()
    {
        if (rb == null)
            return;

        // Refresh water state and recover to shore if swimming
        // permission has been lost while in deep water.
        bool waterStateValid = _waterController.PreparePhysicsStep();

        UpdateMovementSpeed();

        if (!waterStateValid)
        {
            _movementInput = Vector2.zero;
            return;
        }

        bool canMoveNow = _canMove && !_disableActions && GameUtils.i.battleSystem.CurrentState is OverworldState;

        if (!canMoveNow)
        {
            _movementInput = Vector2.zero;
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 movement = _movementInput * WalkSpeed * Time.fixedDeltaTime;

        if (movement.sqrMagnitude < 0.000001f)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 targetPosition = rb.position + movement;

        // The water controller blocks unauthorised deep-water entry.
        // Normal solid collisions remain handled by player physics.
        if (_waterController.CanMoveTo(targetPosition))
        {
            rb.MovePosition(targetPosition);
            return;
        }

        // If diagonal movement enters forbidden deep water,
        // try moving along one axis to slide along the shoreline.
        Vector2 xTarget = rb.position + new Vector2(movement.x, 0f);

        Vector2 yTarget = rb.position + new Vector2(0f, movement.y);

        if (Mathf.Abs(movement.x) >= Mathf.Abs(movement.y))
        {
            if (TryWaterCheckedMove(xTarget, movement.x))
                return;

            if (TryWaterCheckedMove(yTarget, movement.y))
                return;
        }
        else
        {
            if (TryWaterCheckedMove(yTarget, movement.y))
                return;

            if (TryWaterCheckedMove(xTarget, movement.x))
                return;
        }

        rb.linearVelocity = Vector2.zero;
    }

    /// <summary>
    /// Attempts a single-axis movement after checking deep-water access.
    /// </summary>
    private bool TryWaterCheckedMove(Vector2 target, float axisMovement)
    {
        if (Mathf.Abs(axisMovement) < 0.000001f)
            return false;

        if (!_waterController.CanMoveTo(target))
            return false;

        rb.MovePosition(target);
        return true;
    }

    /// <summary>
    /// Combines run, stairs, and water speed modifiers.
    /// Running is disabled in both shallow and deep water.
    /// </summary>
    private void UpdateMovementSpeed()
    {
        bool hasInput = _movementInput.sqrMagnitude > 0.000001f;

        if (!hasInput)
        {
            _isSprinting = false;
            _walkSpeedBoost = 1f;
            return;
        }

        _isSprinting = Input.GetButton("Run") && !InWater;

        float runBoost = _isSprinting ? _runBoost : 0f;
        float stairsBoost = _onStairs ? _stairsWalkBoost : 0f;

        // Prevent modifiers from reducing movement to zero or reversing it.
        _walkSpeedBoost = Mathf.Max(0.05f, 1f + runBoost + stairsBoost + _waterController.MovementSpeedBoost);
    }

    /// <summary>
    /// Plays a splash when transitioning from land into water.
    /// </summary>
    private void UpdateWaterSounds()
    {
        bool currentlyInWater = InWater;

        if (currentlyInWater && !_wasInWater)
        {
            AudioSystem.i.PlaySoundClip(AudioSystem.i.PlayRandomSound(AudioSystem.i.WaterSplashSounds), AudioMaster.SFX);
        }

        _wasInWater = currentlyInWater;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Water detection and visuals belong to PlayerWaterController.
        if (other.CompareTag("Stairs"))
        {
            _onStairs = true;
        }
        else if (other.CompareTag("Area"))
        {
            MapArea area = other.GetComponent<MapArea>();

            if (area != null)
            {
                _currentArea = area;

                if (GameUtils.i.battleSystem.CurrentState
                    is not PostBattleState)
                {
                    // Restart the area loading-screen effect.
                    UIManager.i.EnableLoadingScreen(false);
                    UIManager.i.EnableLoadingScreen(true);

                    DebugLogs.Log(
                        $"Entering new area: {_currentArea.AreaName}"
                    );
                }
            }
            else
            {
                DebugLogs.Warning(
                    "No MapArea script assigned to new area the player has entered"
                );
            }
        }
        else if (other.CompareTag("Monster"))
        {
            StartBattleMonster(other);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("TallGrass"))
        {
            _onGrass = false;
        }
        else if (collision.CompareTag("Stairs"))
        {
            _onStairs = false;
        }
    }

    /// <summary>
    /// Waits for the throw cooldown, then allows another capsule throw.
    /// Must be started by the system that finishes the current throw.
    /// </summary>
    public IEnumerator ResetThrowCooldown()
    {
        yield return new WaitForSeconds(
            _throwCatchingObjectCooldown
        );

        _temporaryThrowSpeed = _throwSpeed;
        _threwCatchingObject = false;
    }

    /// <summary>
    /// Rotates the aiming pointer using the controller's right stick.
    /// Returns the resulting angle in degrees.
    /// </summary>
    private float RotateAimPointer_Controller()
    {
        Vector3 input = new Vector3(Input.GetAxis("Right Joystick X"), Input.GetAxis("Right Joystick Y"), 0f);

        float angle = Mathf.Atan2(-input.y, input.x) * Mathf.Rad2Deg;

        _throwAimPointer.transform.eulerAngles = new Vector3(0f, 0f, angle);

        return angle;
    }

    /// <summary>
    /// Rotates the aiming pointer towards the mouse's world position.
    /// Returns the resulting angle in degrees.
    /// </summary>
    private float RotateAimPointer_Mouse()
    {
        Vector3 mouseWorld = _playerCamera.ScreenToWorldPoint(Input.mousePosition);

        Vector2 direction = mouseWorld - transform.position;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        _throwAimPointer.transform.eulerAngles = new Vector3(0f, 0f, angle);

        return angle;
    }

    /// <summary>
    /// Recalls the follower and throws a capsule when Attack is held,
    /// provided no previous throw is still active.
    /// </summary>
    private void ThrowMonsterOut()
    {
        if (PlayerStats.i.PlayerParty.Count == 0)
            return;

        if (GameUtils.i.IsInCutscene())
            return;

        if (Input.GetButton("Attack") && !_threwCatchingObject)
        {
            AudioSystem.i.PlaySoundClip(
                AudioSystem.i.ThrowCatchingObjectSound
            );

            _threwCatchingObject = true;
            _temporaryThrowSpeed = -_throwSpeed / 100f;

            if (_followingMonster != null)
            {
                MonsterFollowScriptTest follower = _followingMonster.GetComponent<MonsterFollowScriptTest>();

                if (follower != null)
                {
                    follower.RecallIntoCapsule();
                }
            }

            ThrowObjectAtAimingDirection(_throwMonsterObject);
        }
    }

    /// <summary>
    /// Spawns a movement effect, optionally plays a sound and fades it,
    /// then releases the footstep cooldown after spawnDelay.
    /// </summary>
    private IEnumerator SpawnIndividualFootstep(float spawnDelay, GameObject footstepObject, Vector2 spawnPosition, List<AudioClip> stepSounds, bool isFading = true)
    {
        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;

        GameObject currentFootstep = Instantiate(footstepObject, spawnPosition, Quaternion.Euler(0f, 0f, angle));

        if (stepSounds != null)
        {
            AudioSystem.i.PlaySoundClip(AudioSystem.i.PlayRandomSound(stepSounds), AudioMaster.SFX);
        }

        if (isFading)
        {
            currentFootstep.GetComponent<SpriteRenderer>().DOFade(0f, 1f);
        }

        Destroy(currentFootstep, 1f);

        yield return new WaitForSeconds(spawnDelay);

        _spawnedFootstep = false;
    }

    /// <summary>
    /// Selects the movement effect for water, grass, or normal ground.
    /// </summary>
    private void SpawnTypedFootsteps(float delay)
    {
        if (_spawnedFootstep)
            return;

        _spawnedFootstep = true;

        // Water uses bubbles and water sounds instead of ground footsteps.
        if (InWater)
        {
            StartCoroutine(SpawnIndividualFootstep(_waterbubbleFrequency, _waterBubbleObject, new Vector2(transform.position.x, transform.position.y - 0.6f), AudioSystem.i.WaterMoveSounds, false));
            return;
        }

        GameUtils.i.getFootstepData().PlayFootstep();

        if (_onGrass)
        {
            StartCoroutine(SpawnIndividualFootstep(delay, _tallGrassFootstepObject, transform.position, AudioSystem.i.GrassFootstepsSounds, false));
        }
        else
        {
            StartCoroutine(SpawnIndividualFootstep(delay, _footstepObject, _footTransform.position, null, true));
        }
    }

    /// <summary>
    /// Creates and launches a capsule in the aiming direction.
    /// Assigns its player reference and the first party monster's capsule sprite.
    /// </summary>
    private GameObject ThrowObjectAtAimingDirection(GameObject throwingObject)
    {
        float angle;

        if (GameUtils.i.ControllerConnected())
        {
            angle = _throwAimPointer.transform.eulerAngles.z;
        }
        else
        {
            Vector2 mousePos = GameUtils.i.MainCamera.ScreenToWorldPoint(Input.mousePosition);

            Vector2 origin = transform.position;

            angle = Mathf.Atan2(mousePos.y - origin.y, mousePos.x - origin.x) * Mathf.Rad2Deg;
        }

        GameObject currentObject = Instantiate(
            throwingObject,
            transform.position,
            Quaternion.Euler(0f, 0f, angle)
        );

        _currentThrowMonsterObject = currentObject;

        currentObject.GetComponent<Rigidbody2D>()?.AddForce(
            currentObject.transform.right * _temporaryThrowSpeed,
            ForceMode2D.Impulse
        );

        PlayerCatchingObject catchingObject =
            currentObject.GetComponent<PlayerCatchingObject>();

        catchingObject?.setPlayer(this);

        catchingObject?.SetSprite(
            PlayerStats.i.PlayerParty[0].CapsuleCaught.Capsule_Sprite
        );

        return currentObject;
    }

    /// <summary>
    /// Reads movement input, updates facing and animation, and requests
    /// movement effects. Physics movement is applied in FixedUpdate.
    /// </summary>
    private void Move()
    {
        if (!_canMove)
        {
            _movementInput = Vector2.zero;

            anim.SetBool("isMoving", false);
            anim.SetBool("isRunning", false);
            return;
        }

        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");

        // Normalising prevents faster diagonal movement.
        _movementInput = new Vector2(x, y).normalized;

        bool hasInput = _movementInput.sqrMagnitude > 0.000001f;

        UpdateMovementSpeed();

        if (hasInput)
        {
            moveDirection = new Vector2(x, y);

            SpawnTypedFootsteps(_footstepFrequency);
            DetermineFacingDirection();

            anim.SetFloat("MoveX", x);
            anim.SetFloat("MoveY", y);
        }

        anim.speed = _walkSpeedBoost;

        anim.SetBool("isMoving", hasInput);
        anim.SetBool("isRunning", _isSprinting);
    }

    /// <summary>
    /// Converts the last movement direction into a cardinal facing direction.
    /// The axis with the larger input magnitude takes priority.
    /// </summary>
    public void DetermineFacingDirection()
    {
        if (moveDirection.sqrMagnitude <= 0.01f)
            return;

        if (Mathf.Abs(moveDirection.x) >
            Mathf.Abs(moveDirection.y))
        {
            FacingDirection = moveDirection.x > 0f
                ? FacingDirection.Right
                : FacingDirection.Left;
        }
        else
        {
            FacingDirection = moveDirection.y > 0f
                ? FacingDirection.Up
                : FacingDirection.Down;
        }
    }

    /// <summary>
    /// Toggles the selection menu, respecting the cooldown and active throws.
    /// Opening the menu places the game in its Freeze state.
    /// </summary>
    private void MainMenuPressed()
    {
        if (_currentThrowMonsterObject != null)
            return;

        if (_selectionMenuToggleTimer > 0f)
        {
            _selectionMenuToggleTimer -= Time.deltaTime;
        }

        if (!Input.GetButtonDown("SelectionMenu"))
            return;

        if (_selectionMenuToggleTimer > 0f)
            return;

        _selectionMenuToggleTimer =
            _selectionMenuToggleCooldown;

        if (!_initialisedMainMenu)
        {
            if (!_canThrowMonsterOut)
                return;

            _canThrowMonsterOut = false;
            _initialisedMainMenu = true;

            GameUtils.i.battleSystem.SetState(GameState.FREEZE);
        }
        else
        {
            _initialisedMainMenu = false;
            _canThrowMonsterOut = true;
        }

        StartCoroutine(
            UIManager.i.InitialiseMainMenu(_initialisedMainMenu)
        );
    }

    /// <summary>
    /// Starts a wild battle when allowed and removes the encountered
    /// overworld monster after assigning its battle data.
    /// </summary>
    public void StartBattleMonster(Collider2D other)
    {
        if (GameUtils.i.battleSystem.BattleStartCooldown)
            return;

        if (GameUtils.i.battleSystem.IsInBattle ||
            GameUtils.i.battleSystem.CurrentState is FreezeState)
        {
            return;
        }

        if (!PartyRules.HasAliveMonster(PlayerStats.i.PlayerParty))
            return;

        GameUtils.i.battleSystem.EnemyUnit.CurrentMonster =
            other.GetComponent<Overworld_MonsterBehaviour>().GetMonster;

        GameUtils.i.battleSystem.TriggerWildBattle();

        Destroy(other.gameObject);
    }
}