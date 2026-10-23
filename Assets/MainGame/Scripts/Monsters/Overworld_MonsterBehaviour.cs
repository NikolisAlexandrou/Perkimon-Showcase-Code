using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Serialization;

[RequireComponent(typeof(SpriteAnimator), typeof(Rigidbody2D))]
public class Overworld_MonsterBehaviour : MonoBehaviour
{
    [Header("Spawn Settings")]

    [Tooltip("The monster that has this behaviour")]
    [SerializeField] private BattleMonster _monster;

    [Tooltip("The FOV object of this monster")]
    [SerializeField] private GameObject _FOVObject;

    [Tooltip("Chance for monster to spawn")]
    [Range(0, 100)]
    [SerializeField] private float _chanceToSpawn = 100;

    [Tooltip("Set if monster will not attack player in overworld only")]
    [SerializeField] private bool _isFriendly;

    [Tooltip("Chance for monster to spawn friendly")]
    [Range(0, 100)]
    [SerializeField] private float _friendlyChance = 30;


    private bool _interrupt; //  Flag that determines if monster behaviour is interrupted and temporary disable it

    [Header("Sleep Settings")]

    [Tooltip("Prefab of the sleep particles that will be displayed when monster is sleeping")]
    [SerializeField] private GameObject _sleepParticleOb;

    [Tooltip("Chance of monster to sleep after timer is checked")]
    [Range(0,100)]
    [SerializeField] private float _sleepChance = 10;

    [Tooltip("Timer in seconds that will check for monster to sleep")]
    [SerializeField] private float _sleepCheckCounter = 5;

    [Tooltip("Chance of monster to wake up after a timer is checked")]
    [Range(0, 100)]
    [SerializeField] private float _wakeupChance = 25;

    [Tooltip("Timer in seconds that wikk check for monster to wake up")]
    [SerializeField] private float _wakeupCheckCounter = 3;

    [Tooltip("Forces the monster to wake up")]
    [SerializeField] private bool _forceWakeup;

    private GameObject _sleepParticleObActive;   //  Current sleep particles object spawned
    private bool _checkedSleep;  //  flag to determine if monster sleep is temporarily checked
    private bool _checkedWakeup; //  flag to determine if monster wake up is temporarily checked

    [Header("Patrol Settings")]
    [Tooltip("If true, disables movement of monster in the overworld")]
    [SerializeField] private bool _HasStoppedAllMovement; // false to enable movement / true to disable movement

    [Tooltip("List of waypoints the monster will follow (loops) if empty, monster will follow random waypoints")]
    [SerializeField] private List<Vector2> _patrolWaypoints;   // List of patrol points

    [Tooltip("Move to next waypoint speed")]
    [SerializeField] private float _moveSpeed = 2f;  // Enemy movement speed

    [Tooltip("Minimum distance from arrived waypoint for monster to stop moving")]
    [SerializeField] private float _waypointArriveThreshold = 0.2f; // Distance threshold for waypoint arrival

    [Tooltip("Maximum distance between starting position waypoints will spawn for monster to move")]
    [SerializeField] private float _randomMovementOffset; // eg from 1 to 5

    [Tooltip("Timer for monster to stay in a waypoint before moving to the next one")]
    [SerializeField] private float _changePatrolWaypointTimer = 3f;

    private Vector2 _startPosition;   //  Initial position of monster when spawned
    private int _currentWaypointIndex = 0;   //  current index of waypoint in waypoints lists the monster is at  
    private bool _HasAppliedToOtherPoint;   //  Flag that determines if monster has found another waypoint to move

    [Header("Detection Settings")]
    
    [Tooltip("Gets the capsule collider and adds this value to determine stopFollowingRange")]
    [SerializeField] private float _stopFollowingThreshold = 0.05f;

    [Tooltip("Maximum distance that the monster can detect the player")]
    [SerializeField] private float _detectionRange = 5f;     // Maximum distance to detect the player

    [Tooltip("Field of view angle for detection in degrees")]
    [SerializeField] private float _fovAngle = 90f;          // Field of view angle (in degrees)

    [Tooltip("If true, obstacles can block the monsters vision")]
    [SerializeField] private bool _isUsingLineOfSight = true;    // Whether obstacles block vision

    [Tooltip("The layer of the obstacle that can block vision of monster")]
    [SerializeField] private LayerMask _obstacleMask;        // Layer for obstacles (e.g., walls)

    private float _stopFollowingRange;    //  Distance between monster and player for monster to stop moving when attacking player

    [Header("Attack Settings")]

    [Tooltip("Movement speed of monster when is attacking the player")]
    [SerializeField] private float _attackMoveSpeed = 2.5f;  // Speed while attacking/chasing

    private bool _HasSpawnedExclamationMark; //  Flag to determine if exclamation mark is spawned when monster sees player


    private Transform player;   //  Current player transform
    private Rigidbody2D rb;     //  Current rigidbody of monster
    private SpriteAnimator spriteAnimator;
   

    // --- STATE MACHINE ---
    private enum EnemyState { Patrol, Attack, Sleep }
    private EnemyState currentState = EnemyState.Patrol;

    /// <summary>
    /// Get or set the interrupt flag to a value
    /// </summary>
    /// <param name="interupt"></param>
    public bool Interrupt { get => _interrupt; set => _interrupt = value; }

    /// <summary>
    /// Get the current monster that has this behaviour
    /// </summary>
    /// <returns></returns>
    public BattleMonster GetMonster => _monster;

    /// <summary>
    /// Get the FOV of this monster that has this behaviour
    /// </summary>
    /// <returns></returns>
    public GameObject FOVObject => _FOVObject;

    /// <summary>
    /// Set to true to disable all monster movements or false to re-enable them
    /// </summary>
    /// <param name="enabled"></param>
    public void HasStoppedAllMovements(bool enabled = true) => _HasStoppedAllMovement = enabled;

    void Start()
    {
        //  Find player if not assigned
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject)
            player = playerObject.transform;

        spriteAnimator = GetComponent<SpriteAnimator>();
        //  Initialise FOV object to animator
        spriteAnimator.InitialiseFOVObject(_FOVObject);
        //  Initialise all sprite lists of monster to animator
        spriteAnimator.InitialiseSprites(_monster.Base.overworldSprites,
                                         _monster.Base.overworldWalkSpritesFront,
                                         _monster.Base.overworldWalkSpritesBack,
                                         _monster.Base.overworldWalkSpritesLeft,
                                         _monster.Base.overworldWalkSpritesRight);

        rb = GetComponent<Rigidbody2D>();   //  Assign the rigidbody
        _startPosition = transform.position; //  Initialise starting position
        AssignNewRandomRoute();             //  Assign a new random waypoint for monster to move to
       
        //  Safety check for missing waypoints
        if (_patrolWaypoints.Count == 0)
            DebugLogs.Warning($"No patrolWaypoints assigned to {gameObject.name}");
        
        //  Assign friendly settings
        if (Random.Range(0, 100) < _friendlyChance)
            _isFriendly = true;

        // set not following range to max collision range
        _stopFollowingRange = GetComponent<CapsuleCollider2D>().size.y + _stopFollowingThreshold;
        _HasStoppedAllMovement = true;    //  Disable movement
    }

    //  Handles state switching
    void Update()
    {
        if (!player) return;  // If no player in scene, do nothing

        // Check if player is in FOV
        bool playerInFOV = IsPlayerInFOV();

        // SWITCH STATES
        switch (currentState)
        {
            case EnemyState.Patrol:
                if (playerInFOV)    //  when patrolling and player is in monster FOV
                {
                    //  Change state to attacking
                    currentState = EnemyState.Attack;
                    //  Temporarily spawn exclamation mark above monsters head
                    if (_HasSpawnedExclamationMark == false)
                    {
                        _HasSpawnedExclamationMark = true;
                        GameUtils.i.spawnExclamationMarkOB(transform);
                    }
                }
                break;

            case EnemyState.Attack:
                if (!playerInFOV)   //  When attacking and player is not in FOV
                {
                    //  Change state back to patrol
                    currentState = EnemyState.Patrol;
                    _HasSpawnedExclamationMark = false;
                }
                break;

            case EnemyState.Sleep:
                if (_forceWakeup == true)    //  If in sleep and wake up is forced, move to patrol state
                {
                    _forceWakeup = false;
                    currentState = EnemyState.Patrol;
                }
                break;
        }
    }

    // Handles behaviour at each state
    void FixedUpdate()
    {
        //  If Game is in freeze state or player is in battle state, disable overworld movement of monster
        if (GameUtils.i.battleSystem.CurrentState is FreezeState || GameUtils.i.battleSystem.IsInBattle == true)
        {
            HasStoppedAllMovements();
        }
        else
        {
            //  If interrupt is true, dont do any behaviour
            if (_interrupt == true)
                return;

            switch (currentState)
            {
                case EnemyState.Patrol:
                    //  Enable movement, do patrol behaviour and check for sleeping
                    HasStoppedAllMovements(false);
                    PatrolBehavior();

                    if (_checkedSleep == false)
                        StartCoroutine(SleepCheck());
                    
                    break;
                case EnemyState.Attack:
                    //  Enable movement
                    HasStoppedAllMovements(false);
                    if (_isFriendly) //   if is friendly, stare at player otherwise attack player
                        StareBehavior();
                    else
                        AttackBehavior();
                   
                    break;
                case EnemyState.Sleep:
                    //  Disable movement
                    HasStoppedAllMovements();
                    //  Check for wake up
                    if (_checkedWakeup == false)
                        StartCoroutine(WakeUpCheck());

                    break;
            }
        }
    }

    private void OnDisable()
    {
        _HasAppliedToOtherPoint = false;
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        //  If monster enters triggers of NPC/Monster/Player stop monster movement to prevent pushing
        if (collision.CompareTag("NPC") || collision.CompareTag("Monster") || collision.CompareTag("Player"))
            _HasStoppedAllMovement = true;

    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        //  If monster exits triggers of NPC/Monster/Player enable monster movement
        if (collision.CompareTag("NPC") || collision.CompareTag("Monster") || collision.CompareTag("Player"))
            _HasStoppedAllMovement = false;
    }

    //  Visualize the FOV in editor (for debugging purposes)
    private void OnDrawGizmos()
    {
        if (_FOVObject == null)
            return;
        Gizmos.color = Color.green;
        Vector2 forward = _FOVObject.transform.right;

        //  Draw center line of FOV
        Gizmos.DrawLine(transform.position, (Vector2)_FOVObject.transform.position + forward * _detectionRange); // Forward line

        // Draw the FOV cone (other 2 lines that are angled)
        float angle = _fovAngle / 2f;
        for (int i = -1; i <= 1; i += 2)
        {
            Quaternion rotation = Quaternion.Euler(0, 0, i * angle);
            Vector2 direction = rotation * forward;
            Gizmos.DrawLine(transform.position, (Vector2)_FOVObject.transform.position + direction * _detectionRange);
        }
    }

    /// <summary>
    /// Spawns sleep particle object at monster position and sets its sprite to sleep sprite
    /// </summary>
    private void SpawnSleepParticles()
    {
        //  Check if sleep particle is not already spawned and spawn it
        if (_sleepParticleObActive == null)
        {
            _sleepParticleObActive = Instantiate(_sleepParticleOb, new Vector2(transform.position.x,transform.position.y + .5f), Quaternion.identity, transform);
            GetComponent<SpriteRenderer>().sprite = _monster.Base.OverworldSleepSprite;
        }
    }

    /// <summary>
    /// Destroy sleep particle object that is in monster position
    /// </summary>
    private void DespawnSleepParticles()
    {
        //  Check if sleep particle is spawned and destroy it
        if (_sleepParticleObActive != null)
            Destroy(_sleepParticleObActive.gameObject);
    }

    /// <summary>
    /// Checks for monster to sleep at regular time intervals (SleepCheckCounter) 
    /// and put it to sleep state if monster is not attacking
    /// </summary>
    /// <returns></returns>
    private IEnumerator SleepCheck()
    {
        _checkedSleep = true;
        yield return new WaitForSeconds(_sleepCheckCounter); //  Check for sleep counter
        //  Check if sleep chance is met and if monster is not in attacking state
        if (Random.Range(0, 100) < _sleepChance && currentState != EnemyState.Attack)
        {
            //  Set state to sleep and spawn sleep particles
            currentState = EnemyState.Sleep;
            SpawnSleepParticles();
        }
        _checkedSleep = false;
    }

    /// <summary>
    /// Check for wakeup at regular time intervals (WakeupCheckCounter)
    /// </summary>
    /// <returns></returns>
    private IEnumerator WakeUpCheck()
    {
        _checkedWakeup = true;
        yield return new WaitForSeconds(_wakeupCheckCounter);
        //  Check if wake up chance is met
        if (Random.Range(0, 100) < _wakeupChance)
        {
            // force wake up the monster and dispawn particles
            _forceWakeup = true;
            DespawnSleepParticles();
        }
        _checkedWakeup = false;
    }


    // --------------------------------------------------------------
    // PATROL
    // --------------------------------------------------------------

    /// <summary>
    /// Gets the current waypoint in the waypoints list and moves monster to that waypoint,
    /// if monster reaches that waypoint move to next one in the list and if monster reaches
    /// the final waypoint in the list, repeat from the start of the list
    /// </summary>
    private void PatrolBehavior()
    {
        if (_patrolWaypoints.Count == 0) return;

        // Get current waypoint from list
        Vector2 waypoint = _patrolWaypoints[_currentWaypointIndex];

        // Move towards the waypoint
        Vector2 direction = (waypoint - (Vector2)transform.position).normalized;

        //  if movement is enabled move monster to direction
        MoveMonster(direction);
      
        float distanceToWaypoint = Vector2.Distance(transform.position, waypoint);
        if (_HasAppliedToOtherPoint == false)
            StartCoroutine(ChangeWaypoint(true));   //  Go to next random waypoint if monster did not apply to other point

        // Check if close enough to proceed to next waypoint
        if (distanceToWaypoint <= _waypointArriveThreshold)
        {
            // Go to next waypoint (loop around)
            rb.linearVelocity = Vector2.zero;
            spriteAnimator.getStationarySpriteFromDirection(direction);
            _patrolWaypoints.Clear();
        }
    }

    /// <summary>
    /// Assign a new waypoint to go, if random get a random waypoint, if not random
    /// move to next one in the waypoints list
    /// </summary>
    /// <param name="isRandom"></param>
    /// <returns></returns>
    private IEnumerator ChangeWaypoint(bool isRandom)
    {
        _HasAppliedToOtherPoint = true;
        yield return new WaitForSeconds(_changePatrolWaypointTimer);
        if (isRandom == true)
        {
            _patrolWaypoints.Clear();
            AssignNewRandomRoute();
        }
        else
        {
            _currentWaypointIndex = (_currentWaypointIndex + 1) % _patrolWaypoints.Count;
        }
        _HasAppliedToOtherPoint = false;
    }

    // --------------------------------------------------------------
    // Stare At player (STARE)
    // --------------------------------------------------------------
    /// <summary>
    /// Monster looks at the directiom where the player is and updates its sprite
    /// accordingly.
    /// </summary>
    private void StareBehavior()
    {
        if (!player) return;

        //  Get direction towards player
        Vector2 direction = (player.position - transform.position).normalized;

        //  Set the sprite of monster to the direction looking
        spriteAnimator.getStationarySpriteFromDirection(direction);

        //  Face the player by adjusting rotation of FOV object
        MonsterLookAtDirection(direction);
    }
    // --------------------------------------------------------------
    // ATTACK (Chase)
    // --------------------------------------------------------------
    /// <summary>
    /// When monster is in attack state, move towards the player and look at the player,
    /// stop if the distance between player is less than or equal to stopFollowingRange.
    /// </summary>
    private void AttackBehavior()
    {
        if (!player) return;

        // Move towards the player
        float distToPlayer = Vector2.Distance(transform.position, player.position);
        if (distToPlayer <= _stopFollowingRange) return;

        Vector2 direction = (player.position - transform.position).normalized;
        
        //  Move towards the player
        MoveMonster(direction);
        MonsterLookAtDirection(direction);

        // (Optional) Implement actual "attack" logic or animations
        // e.g., If within some 'attackRange', do damage, play animation, etc.
    }

    /// <summary>
    /// Moves monster towards a direction and plays walk animation
    /// </summary>
    /// <param name="direction"></param>
    private void MoveMonster(Vector2 direction)
    {
        if (_HasStoppedAllMovement == true)
            return;
        
            rb.MovePosition(rb.position + direction * _attackMoveSpeed * Time.fixedDeltaTime);
            spriteAnimator.PlayWalkAnimation(direction);
       
    }

    /// <summary>
    /// Monster FOV looks at the direction given
    /// </summary>
    /// <param name="direction"></param>
    private void MonsterLookAtDirection(Vector2 direction)
    {
        //  Monster FOV rotates towards direction looking
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        _FOVObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }

    // --------------------------------------------------------------
    // FOV CHECK
    // --------------------------------------------------------------
    /// <summary>
    /// Returns true if player is in FOV range of monster and if there are
    /// no obstacles blocking the vision of the monster
    /// </summary>
    /// <returns></returns>
    private bool IsPlayerInFOV()
    {
        //  Distance Check
        float distToPlayer = Vector2.Distance(transform.position, player.position);
        if (distToPlayer > _detectionRange)
            return false;

        //  Angle Check
        //  Determine the angle between the enemy forward direction and the direction to the player
        //  For top-down, lets assume "forward" is the enemy right (x-axis) if using rb.rotation
        //  Alternatively, you can define forward as 'transform.up' or 'transform.right' depending on your sprite orientation.

        Vector2 directionToPlayer = (player.position - _FOVObject.transform.position).normalized;
        //  We assume "forward" is transform.right if rotation = 0 means facing right
        Vector2 enemyForward = _FOVObject.transform.right;
        float angle = Vector2.Angle(enemyForward, directionToPlayer);

        if (angle > (_fovAngle / 2f))
            return false;

        //  Line-of-Sight Check (Raycast)
        if (_isUsingLineOfSight)
        {
            RaycastHit2D hit = Physics2D.Raycast(
                transform.position,
                directionToPlayer,
                distToPlayer,
                _obstacleMask
            );

            //  If the ray hits something before reaching the player, vision is blocked and player is not seen
            if (hit.collider != null)
            {
                //  The line of sight is blocked by an obstacle
                return false;
            }
        }

        // If we passed all checks, the player is in FOV
        return true;
    }

    /// <summary>
    /// Adds the patrolWaypoints list a random waypoint around the monster with max distance of randomMovementOffset
    /// </summary>
    public void AssignNewRandomRoute()
    {
        Vector2 offset = Random.insideUnitCircle * _randomMovementOffset;

        _patrolWaypoints.Add(_startPosition + offset);
    }
    /// <summary>
    /// Adds the patrolWaypoints list a new coordinate of x and y
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void AssignNewRoute(int x, int y) => _patrolWaypoints.Add(new Vector2(x, y));
}
