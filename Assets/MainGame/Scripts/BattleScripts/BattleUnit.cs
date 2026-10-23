using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Events;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))] 
public class BattleUnit : MonoBehaviour
{
    [Header("Events")]

    [Tooltip("Events playing when unit takes damage from any source")]
    public UnityEvent OnDamageTaken;

    [Tooltip("Events playing when unit no longer has knockback effect")]
    public UnityEvent OnKnockbackEnd;

    [Header("Stats")]

    [Tooltip("Holds the monster data of the unit")]
    private BattleMonster _monster;

    [Tooltip("How fast the animation monster sprite is changing frames (in seconds) (Default: 0.2)")]
    [SerializeField] private float _walkSpriteAnimationSpeed = 0.2f;

    [Tooltip("Set true if this is the enemy battle unit")]
    [SerializeField] private bool _isEnemy;  // Detects if this unit is an enemy monster

    private bool _isImmune;  //  Flag that makes monster immune to any damage

    private bool _disableAttacking;  //  disables/enables all monsters attacking abilities (cannot attack)

    private bool _disableMovement;  //  disables/enables all monster movement

    [Tooltip("Prefab that spawns when monster dashes (the blur effect for example)")]
    [SerializeField] private GameObject _dashEffectObject;

    private int _smartness;  //  Not used anywhere, can add it to make AI better if you change AI later on

    private float _stamina;  //  Holds the current stamina values of the monster

    private float _walkSpeed;  //  Holds the current walk speed of the monster

    [Tooltip("Give ALL monsters permanent speed boost multiplier during battle (Default: 1.5)")]
    [SerializeField] private float _walkSpeedPermanentMultiplier = 1.5f; //  + 50% more speed


    [Header("Dash")]

    [Tooltip("Holds the dash cooldown (Player default: 0.8) (Enemy default: 2.5)")]
    [SerializeField] private float _dashCooldown = 2.5f;

    [Tooltip("Holds the dash duration in seconds (Player default: 0.15) (Enemy default: 0.15)")]
    [SerializeField] private float _dashDuration = 0.15f;

    [Tooltip("Holds the distance the monster will travel during the dash (Player default: 2.8) (Enemy default: 10)")]
    [SerializeField] private float _dashRange = 10f;

    [Tooltip("Holds the stamina cost to use dash (Default: 2)")]
    [SerializeField] private float _dashCost = 2f;

    [Tooltip("Holds the stamina slider bar object")]
    [SerializeField] private Slider _staminaBar;

    private bool _canUseDash;    //  When cooldown time runs out, monster can use dash again
    private bool _isDashing; //  Enabled when monster is dashing


    [Header("Other Properties")]

    [Tooltip("Pointer showing direction that attack will go (Player Unit only)")]
    [SerializeField] private Transform _attackPointer;  //  Used only for player unit

    [Header("Other bars")]

    [Tooltip("Holds the health slider bar object")]
    [SerializeField] private Slider _healthBar;

    [Tooltip("Holds the exp slider bar object")]
    [SerializeField] private Slider _ExpBar;

    //  Attack Properties

    private bool _hasSelectedMove = false;   //  Flag for when monster found and selected a move

    private BattleMove _currentMove;  //  current selected move of the monster

    private bool _canAttack;  //  flag for when monster is able to attack again

    private bool _isAttacking;  //  Flag that indicates when monster is using an attack
    
    private bool _isDead;  //  Flag that indicates when monster is dead
    
    private bool _isInKnockbackState;  //  Flag that indicates when monster is taking knockback
    private Vector2 _currentAimDirection = Vector2.right;
    private readonly HashSet<BattleMonster> _switchedInAbilityAppliedMonsters = new HashSet<BattleMonster>();
    private readonly HashSet<BattleMonster> _enemySwitchedAbilityAppliedMonsters = new HashSet<BattleMonster>();

    [Header("Sleep Settings")]

    [Tooltip("Prefab that is spawned when monster is sleeping (spawn sleeping particle system)")]
    [SerializeField] private GameObject _sleepParticleObject;    //  Automatically deleted when monster is awake
    
    private GameObject _sleepParticleObActive;   //  Current sleep particle object that is spawned
    
    private bool _spawnedSleepObject;    //  Flag that indicates when sleep object has spawned
  
    private float _walkAnimationTimer;    //  Current time for walk sprite animation frame
    
    private int _currentAnimationFrame;  //  Current animation frame of walk animation

    Vector2 _startPosition;  //  battle start position

    Rigidbody2D rb; //  Rigid body of monster

    /// <summary>
    /// Get the current battle monster
    /// </summary>
    public BattleMonster CurrentMonster {get => _monster; set => _monster = value; }

    /// <summary>
    /// Get the current stamina value of the monster
    /// </summary>
    public float Stamina { get => _stamina; set => _stamina = value; }

    /// <summary>
    /// Get the current walk speed value of the monster
    /// </summary>
    public float WalkSpeed { get => _walkSpeed; set => _walkSpeed = value; }

    /// <summary>
    /// Get if the monster has finished selecting a move to attack
    /// </summary>
    public bool HasSelectedMove { get => _hasSelectedMove; set => _hasSelectedMove = value; }

    /// <summary>
    /// Get the current selected move of the monster
    /// </summary>
    public BattleMove CurrentMove { get => _currentMove; set => _currentMove = value; }

    /// <summary>
    /// Get if the battle unit is an enemy
    /// </summary>
    public bool IsEnemy => _isEnemy;

    /// <summary>
    /// Get the value of the time the dash will last for in seconds
    /// </summary>
    public float DashDuration => _dashDuration;

    /// <summary>
    /// Get the value of the range of monster will travel during a dash per time interval
    /// </summary>
    /// <returns></returns>
    public float DashRange => _dashRange;

    /// <summary>
    /// Get the duration of dash cooldown
    /// </summary>
    /// <returns></returns>
    public float DashCooldown => _dashCooldown;

    /// <summary>
    /// Get if monster dash is ready to be used
    /// </summary>
    public bool CanUseDash { get => _canUseDash; set => _canUseDash = value; }

    /// <summary>
    /// Set if monster is performing dash
    /// </summary>
    public bool IsDashing { get => _isDashing; set => _isDashing = value; }

    /// <summary>
    /// Get or set when monster is immune to damage
    /// </summary>
    //const<3 the best :) nick to moro <3 const my moro <3
    public bool IsImmune { get => _isImmune; set => _isImmune = value; }

    /// <summary>
    /// True if monster's movements are disabled (cannot move)
    /// </summary>
    public bool DisableMovement { get => _disableMovement; set => _disableMovement = value; }

    /// <summary>
    /// Get attack pointer object (shows direction where attacks will go) (used for player monster only)
    /// </summary>
    public Transform AttackPointer => _attackPointer;
    
    /// <summary>
    /// Get or set if monster cannot perform attack
    /// </summary>
    public bool DisableAttacking { get => _disableAttacking; set => _disableAttacking = value; }
    
    /// <summary>
    /// Get the in-battle health bar of the monster
    /// </summary>
    public GameObject HealthBar => _healthBar.gameObject;
    /// <summary>
    /// Get or set when cooldown has passed and monster can attack again
    /// </summary>
    public bool CanAttack { get => _canAttack; set => _canAttack = value; }

    /// <summary>
    /// Get or set when monster is currently performing an attack
    /// </summary>
    public bool IsAttacking { get => _isAttacking; set => _isAttacking = value;  }

    /// <summary>
    /// Get or set when current monster is dead (health <= 0)
    /// </summary>
    public bool IsDead { get => _isDead; set => _isDead = value; }

    /// <summary>
    /// Get the start position in arena of monster to spawn
    /// </summary>
    public Vector2 StartPosition => _startPosition;

    /// <summary>
    /// Get or set when knockback is currently being applied to the monster
    /// </summary>
    public bool IsInKnockbackState { get => _isInKnockbackState; set => _isInKnockbackState = value; }

    public Vector2 CurrentAimDirection => _currentAimDirection.sqrMagnitude > 0f ? _currentAimDirection.normalized : Vector2.right;

    private void Start()
    {
        if(!rb) rb = GetComponent<Rigidbody2D>();

        _startPosition = transform.position; //  Initialise battle start position (Called once per game load)
        StartBattleUnit();
        ResetBattleUnit();
        UIManager.i.DashOB.SetActive(true); //  Activate Dash UI object that shows cooldown as radial
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private void Update()
    {
        if (_isEnemy) return;

        RotateAttackPointer();
    }

    private void FixedUpdate()
    {
        if (_isEnemy) return;

        if (!GameUtils.i.battleSystem.IsInBattle) return;  // Only process movement while we are actually in-battle or post-battle

        Move();
        DashForward();
    }
    
    /// <summary>
    /// Sets the battle unit ready for battle (resets variables,position,sprites etc)
    /// </summary>
    public void StartBattleUnit()
    {
        ResetPosition();    //  reset position to battle start position
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = _monster.Base.overworldSprites[0];
        spriteRenderer.DOFade(1, 0.4f); //  Apply fading animation to 1 to prevent invisibility of sprite
        spriteRenderer.material = GameUtils.i.defaultMaterial;
        if (_monster != null && _monster.CurrentMoves.Count >= 1) //  set current move selected to first move when battle starts
            _currentMove = _monster?.CurrentMoves[0];
        
        _walkSpeed = _monster.Speed * _walkSpeedPermanentMultiplier;

        _currentAnimationFrame = 0;
        _walkAnimationTimer = _walkSpriteAnimationSpeed;
        _isDashing = false;
        _canUseDash = false;
        _disableMovement = false;

        //  Reset attacking/disability variables to ensure monster does not glitch

        _disableAttacking = true;    //  Set unable to attack
        _isAttacking = false;
        _isInKnockbackState = false;   //  Reset knockback state
        _canAttack = true;

        //add monster to diary data
        
        MonsterDiaryManager.i.AddMonsterToSeen(_monster.Base);
    }

    /// <summary>
    /// Attack pointer follows direction of position (Example use Input.mousePosition to always point at mouse cursor)
    /// </summary>
    public void RotateAttackPointerAroundPosition(Vector3 mouseScreenPosition)
    {
        Camera cam = GameUtils.i.battleSystem.BattleArena?.GetComponentInChildren(typeof(Camera)) as Camera;

        if (cam == null)
            return;

        Vector3 mouseWorld = cam.ScreenToWorldPoint(mouseScreenPosition);
        mouseWorld.z = _attackPointer.position.z;

        Vector2 origin = _attackPointer.position;
        Vector2 direction = (Vector2)mouseWorld - origin;

        SetAimDirection(direction);
    }

    /// <summary>
    /// Attack pointer follows direction of Controller's left joystick
    /// </summary>
    public void RotateAttackPointerAroundController()
    {
        Vector3 input = new Vector3(Input.GetAxis("Right Joystick X"), Input.GetAxis("Right Joystick Y"), 0);
        SetAimDirection(new Vector2(input.x, -input.y));
    }
    

    private void SetAimDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude <= 0.0001f)
            return;

        _currentAimDirection = direction.normalized;

        float angle = Mathf.Atan2(
            _currentAimDirection.y,
            _currentAimDirection.x
        ) * Mathf.Rad2Deg;

        _attackPointer.localRotation = Quaternion.Euler(0f, 0f, angle);
    }

    /// <summary>
    /// Rotates attack pointer to mouse/controller joystick (called per tick)
    /// </summary>
    public void RotateAttackPointer()
    {
        if (GameUtils.i.HasControllerAimInput())  //  Check for active controller aim input
        {
           // if (rb.linearVelocity != Vector2.zero)
            RotateAttackPointerAroundController();
        }
        else
        {
            RotateAttackPointerAroundPosition(Input.mousePosition);
        }
    }

    /// <summary>
    /// Detects if monster is enemy or player and resets its position in battle accordingly
    /// </summary>
    public void ResetPosition()
    {

        if (_isEnemy == false)
            _startPosition = GameUtils.i.battleSystem.PlayerBattleStartPosition;
        else
            _startPosition = GameUtils.i.battleSystem.EnemyBattleStartPosition;

        transform.position = _startPosition;
    }

    /// <summary>
    /// Spawn Sleep particles
    /// </summary>
    void SpawnSleepParticles()
    {
        if (_spawnedSleepObject == true) return;

        _spawnedSleepObject = true;
        _sleepParticleObActive = Instantiate(_sleepParticleObject, new Vector2(transform.position.x, transform.position.y + .5f), Quaternion.identity, transform);
        GetComponent<SpriteRenderer>().sprite = _monster.Base.OverworldSleepSprite;
    }

    /// <summary>
    /// Despawns Sleep particles
    /// </summary>
    void DespawnSleepParticles()
    {
        if (_spawnedSleepObject == false) return;

        _spawnedSleepObject = false;
        Destroy(_sleepParticleObActive);
    }

    /// <summary>
    /// Plays specific direction sprite walk animation depending on direction moving
    /// </summary>
    public void PlayWalkAnimation(float x, float y)
    {
        if (_walkAnimationTimer > 0)
        {
            float angle = (Mathf.Atan2(y, x) * 180) / Mathf.PI;
            if (angle <= 45 && angle >= -45)
                GetComponent<SpriteRenderer>().sprite = _monster.Base.overworldWalkSpritesRight[_currentAnimationFrame];
            else if (angle > 45 && angle < 135)
                GetComponent<SpriteRenderer>().sprite = _monster.Base.overworldWalkSpritesBack[_currentAnimationFrame];
            else if (angle < -45 && angle > -135)
                GetComponent<SpriteRenderer>().sprite = _monster.Base.overworldWalkSpritesFront[_currentAnimationFrame];
            else
                GetComponent<SpriteRenderer>().sprite = _monster.Base.overworldWalkSpritesLeft[_currentAnimationFrame];

            _walkAnimationTimer -= Time.deltaTime;
        }
        else
        {   //  move to next frame when animation timer goes to 0
            _walkAnimationTimer = _walkSpriteAnimationSpeed;
            if (_currentAnimationFrame + 1 < _monster.Base.overworldWalkSpritesFront.Count)
                _currentAnimationFrame++;
            else
                _currentAnimationFrame = 0;
        }
    }

    /// <summary>
    /// Gets the stationary sprite based on a given direction facing
    /// </summary>
    public void GetSpriteFromDirection(float x, float y)    // Method is not used, maybe use it for something
    {
        float angle = (Mathf.Atan2(y, x) * 180) / Mathf.PI;

        if (angle <= 45 && angle >= -45)
            GetComponent<SpriteRenderer>().sprite = _monster.Base.overworldSprites[2];
        else if (angle > 45 && angle < 135)
            GetComponent<SpriteRenderer>().sprite = _monster.Base.overworldSprites[3];
        else if (angle < -45 && angle > -135)
            GetComponent<SpriteRenderer>().sprite = _monster.Base.overworldSprites[0];
        else
            GetComponent<SpriteRenderer>().sprite = _monster.Base.overworldSprites[1];
    }

    /// <summary>
    /// Resets the battle unit to initial state
    /// </summary>
    public void ResetBattleUnit()
    {
        
        if (_isEnemy == true)
        { 
            _monster?.Initialise();   //  Initialise monster when it is enemy as each enemy monster is always a new monster
            _hasSelectedMove = false;
        }
        else // Reset all slider bars to player monster
        {
            //_staminaBar.value = 0;
            _ExpBar.maxValue = (100 * _monster.Level * (1 + (_monster.Level / 100)));
            _ExpBar.value = _monster.Exp;
        }
        _staminaBar.value = 0;
        _disableMovement = false;
        _healthBar.value = 1;
        _isDead = false;
        _stamina = 0;

        //  Reset temporary stat boosts
        _monster.TempAttack = 1;
        _monster.TempMagicAttack = 1;
        _monster.TempDefense = 1;
        _monster.TempSpeed = 1;
        _monster.TempStaminaRegen = 1;

        //  If for some reason sleep particle is still spawned, destroy it
        if (_sleepParticleObActive != null)
            Destroy(_sleepParticleObActive.gameObject);

        //  Update health bar
        updateHealth();
    }

    /// <summary>
    /// Handles behaviour for moving the player unit
    /// </summary>
    void Move()
    {
        if (_isInKnockbackState == true)   //  Check if not in knockback state
            return;

        if (_disableAttacking == true || GameUtils.i.battleSystem.InterruptBattle == true || _disableMovement == true)
        {
            //   If any of the above flags are true, disable movement for monster
            rb.linearVelocity = new Vector2(0,0);
        }
        else
        {
            float x = Input.GetAxis("Horizontal");
            float y = Input.GetAxis("Vertical");
            rb.linearVelocity = new Vector2(x, y).normalized * _walkSpeed;  //  Move the monster

            if (x == 0 && y == 0)
            {
                //  Reset animation frames when monster is not moving
                _currentAnimationFrame = 0;
                _walkAnimationTimer = _walkSpriteAnimationSpeed;
            }
            else
            {
                PlayWalkAnimation(x, y);    //  Play walk animation
            }
            
        }
    }

    /// <summary>
    /// Handles behaviour for player dashing enemy dashing is handled in EnemyBattleAI Script
    /// </summary>
    public void DashForward()
    {
        if (_isEnemy == true)
            return;
        if (_disableMovement == true)
            return;
        if (_disableAttacking == true)
            return;

        if (Input.GetButton("Dash") && _canUseDash == false && _isDashing == false)
        {
            if (_stamina < _dashCost)return;  //  Check if player stamina is bigger than dash stamina cost

            _stamina -= _dashCost;
            _canUseDash = true;
            _isDashing = true;

            //  Set radial fill amount for displaying animation of cooldown time in UI object
            UIManager.i.DashFillOB.GetComponent<Image>().fillAmount = 1;
            UIManager.i.DashFillOB.GetComponent<Image>().DOFillAmount(0, _dashCooldown + _dashDuration);


            transform.DOBlendableMoveBy((Vector3)(CurrentAimDirection * _dashRange), _dashDuration); //  Dash speed behaviour

            StartCoroutine(resetDashCooldown());    //  called to reset dash cooldown
            StartCoroutine(DashEffect(gameObject)); //  called to play dash sprite blur effect
        }
    }
    /// <summary>
    /// Sprite blur dash effect 
    /// </summary>
    public IEnumerator DashEffect(GameObject originalObject)
    {
        yield return new WaitForSeconds(0.01f);
        GameObject virtualEffectObject = Instantiate(_dashEffectObject, transform.position, Quaternion.identity);
        virtualEffectObject.GetComponent<SpriteRenderer>().sprite = originalObject.GetComponent<SpriteRenderer>().sprite;
        virtualEffectObject.GetComponent<SpriteRenderer>().color = Color.white;
        virtualEffectObject.GetComponent<SpriteRenderer>().DOFade(0.4f, 0.1f);
        Destroy(virtualEffectObject, 0.1f);
        if (_isDashing == true)
            StartCoroutine(DashEffect(originalObject));
    }
    /// <summary>
    /// Resets dashing state and resets cooldown for using dash
    /// </summary>
    IEnumerator resetDashCooldown()
    {
        yield return new WaitForSeconds(_dashDuration);
        _isDashing = false;
        yield return new WaitForSeconds(_dashCooldown);
        _canUseDash = false;
    }

    /// <summary>
    /// Handles stamina increase flow and stamina bar
    /// </summary>
    public void IncreaseStamina()
    {
        //if (_isEnemy == false)
        //{
        float perc = _stamina; /// 10;
        _staminaBar.value = perc;
        //}
        if (_stamina < 10)
            _stamina += Mathf.Sqrt(_monster.StaminaRegenSpeed/3500);  //  Increase stamina formulae
    }
    /// <summary>
    /// Update the health bar slider to match current health of the monster
    /// </summary>
    public void updateHealth() => _monster.updateHealth(_healthBar);

    /// <summary>
    /// Apply status effect of the attackedMove to attackedMonster
    /// </summary>
    public void ApplyStatusEffect(BattleMonster attackedMonster, BattleMove attackedMove)   // apply status effect to monster
    {
        if (attackedMonster == null || attackedMove == null || attackedMove.Status == null)
            return;

        foreach (StatusEffect statusEffect in attackedMove.Status)  //  Search and get any status causing conditions in attackedMove
        {
            if (statusEffect == null || statusEffect.Condition == null)
                continue;

            if (Random.Range(0, 100) < statusEffect.Chance) //   successfully apply status effect
            {
                attackedMonster.Status = statusEffect.Condition.StatusEffect;   //  Apply status to monster
                //  spawn virtual indicator that shows that status has been applied
                GameUtils.i.SpawnStatusConditionAppliedIndicator(transform.position, statusEffect.Condition.StatusEffect, 0.5f, 1, Color.cyan, false, "+", "");
                //  check and apply debuffs depending on what status condition monster has gotten
                // Initialise an attacker ctx
                AbilityContext defenderCTX = new AbilityContext()
                {
                    User = this,
                    Target = GetTargetUnit(),
                    Move = attackedMove,
                    AppliedStatus = statusEffect
                };
                
                AbilityContext attackerCTX = new AbilityContext()
                {
                    User = GetTargetUnit(),
                    Target = this,
                    Move = attackedMove,
                    AppliedStatus = statusEffect
                };

                defenderCTX.User._monster.Ability?.OnStatusAppliedSelf(defenderCTX);
                attackerCTX.User._monster.Ability?.OnStatusAppliedEnemy(attackerCTX);
                StartCoroutine(CheckForStatusDebuff(statusEffect));
            }
        }
    }

    //TODO
    /// <summary>
    /// Applys status effects depending on what status condition it is
    /// </summary>
    public IEnumerator CheckForStatusDebuff(StatusEffect status)
    {
        if (status == null || status.Condition == null)
            yield break;

        float duration = Random.Range(status.Condition.DurationLowest, status.Condition.DurationHighest);
        if (status.Condition.StatChangesOpponent != null)
        {
            StartCoroutine(ChangeStatsSelf(status.Condition.StatChangesOpponent));
            StartCoroutine(IncreaseStatsSelf(duration, status.Condition.StatChangesOpponent));
        }
        StartCoroutine(TakeDamagePerSecond(status, duration));

        if (status.Condition.StatusEffect == Status.SLEEP)  //    check if status is sleeping
            SpawnSleepParticles();
        if (status.Condition.DisableMovement == true)   //     check for disable movement
            _disableMovement = true;
        if (status.Condition.DisableAttacking == true)    //    check for disable attack
            _canAttack = false;    


        yield return new WaitForSeconds(duration);  //  Waits for status time to run out and removes condition
        GameUtils.i.SpawnStatusConditionAppliedIndicator(transform.position, status.Condition.StatusEffect, 0.5f, 1, Color.cyan, false, "-", "");   // indicate that status condition is no longer applied
        _monster.Status = Status.NONE;

        if (status.Condition.StatusEffect == Status.SLEEP)//check if status is sleeping
            DespawnSleepParticles();
        if (status.Condition.DisableMovement == true)   //     check for disable movement
            _disableMovement = false;
        if (status.Condition.DisableAttacking == true)  //    check for disable attack
            _canAttack = true;
    }

    /// <summary>
    /// Monster takes damage as percentage of max health per second for specific status conditions
    /// </summary>
    public IEnumerator TakeDamagePerSecond(StatusEffect status,float time)
    {
        if (time >= 1 && status.Condition.HPPerSecond != 0)
        {
            yield return new WaitForSeconds(1);
            time -= 1;
            TakeDamageFromStatusCondition(_monster.MaxHP * (status.Condition.HPPerSecond / 100), status.Condition.StatusEffect);    //  Take damage as percentage per second
            StartCoroutine(TakeDamagePerSecond(status, time));
        }
    }

   /// <summary>
   /// Apply temporary stat boosts to current monster from this monster's move
   /// </summary>
    public IEnumerator ChangeStatsSelf(StatIncreaseEffect statChangesSelf)
    {
        if (statChangesSelf == null || statChangesSelf.Chance <= 0)
            yield break;

        if (!_isDead)
        {
            if (Random.Range(0, 100) <= statChangesSelf.Chance)
            {
                _monster.TempAttack *= statChangesSelf.Attack;
                spawnStatChangeIndicator(statChangesSelf.Attack, "ATK");
                yield return new WaitForSeconds(0.2f);
                _monster.TempMagicAttack *= statChangesSelf.MagicAttack;
                spawnStatChangeIndicator(statChangesSelf.MagicAttack, "MATK");
                yield return new WaitForSeconds(0.2f);
                _monster.TempDefense *= statChangesSelf.Defense;
                spawnStatChangeIndicator(statChangesSelf.Defense, "DEF");
                yield return new WaitForSeconds(0.2f);
                _monster.TempSpeed *= statChangesSelf.Speed;
                spawnStatChangeIndicator(statChangesSelf.Speed, "SPD");
                _walkSpeed = _monster.Speed;
                yield return new WaitForSeconds(0.2f);
                _monster.TempStaminaRegen *= statChangesSelf.StaminaRegen;
                spawnStatChangeIndicator(statChangesSelf.StaminaRegen, "EDR");
            }
        }
    }

    /// <summary>
    /// Apply temporary stats debuffs to current monster after some time (example used to reset buffs after a time passes)
    /// </summary>
    public IEnumerator DecreaseStatsSelf(float time, StatIncreaseEffect move)
    {
        if (move == null)
            yield break;

        if (!_isDead)
        {
            yield return new WaitForSeconds(time);
            _monster.TempAttack /= move.Attack;
            spawnStatChangeIndicatorNegative(move.Attack, "ATK");
            yield return new WaitForSeconds(0.2f);
            _monster.TempMagicAttack /= move.MagicAttack;
            spawnStatChangeIndicatorNegative(move.MagicAttack, "MATK");
            yield return new WaitForSeconds(0.2f);
            _monster.TempDefense /= move.Defense;
            spawnStatChangeIndicatorNegative(move.Defense, "DEF");
            yield return new WaitForSeconds(0.2f);
            _monster.TempSpeed /= move.Speed;
            spawnStatChangeIndicatorNegative(move.Speed, "SPD");
            _walkSpeed = _monster.Speed;
            yield return new WaitForSeconds(0.2f);
            _monster.TempStaminaRegen /= move.StaminaRegen;
            spawnStatChangeIndicatorNegative(move.StaminaRegen, "EDR");
        }
    }

    // NO IDEA WATCH IT LATER
    public IEnumerator IncreaseStatsSelf(float time, StatIncreaseEffect move)
    {
        if (move == null)
            yield break;

        if (!_isDead)
        {
            yield return new WaitForSeconds(time);
            _monster.TempAttack /= move.Attack;
            spawnStatChangeIndicator(move.Attack, "ATK");
            yield return new WaitForSeconds(0.2f);
            _monster.TempMagicAttack /= move.MagicAttack;
            spawnStatChangeIndicator(move.MagicAttack, "MATK");
            yield return new WaitForSeconds(0.2f);
            _monster.TempDefense /= move.Defense;
            spawnStatChangeIndicator(move.Defense, "DEF");
            yield return new WaitForSeconds(0.2f);
            _monster.TempSpeed /= move.Speed;
            spawnStatChangeIndicator(move.Speed, "SPD");
            _walkSpeed = _monster.Speed;
            yield return new WaitForSeconds(0.2f);
            _monster.TempStaminaRegen /= move.StaminaRegen;
            spawnStatChangeIndicator(move.StaminaRegen, "EDR");
        }
    }

    /// <summary>
    /// Apply temporary stat boosts to monster from enemy move
    /// </summary>
    public IEnumerator ChangeStatsFromOpponent(StatIncreaseEffect move)
    {
        if (move == null || move.Chance <= 0)
            yield break;

        if (!_isDead)
        {
            if (Random.Range(0, 100) <= move.Chance)
            {
                _monster.TempAttack *= move.Attack;
                spawnStatChangeIndicator(move.Attack, "ATK");
                yield return new WaitForSeconds(0.2f);
                _monster.TempMagicAttack *= move.MagicAttack;
                spawnStatChangeIndicator(move.MagicAttack, "MATK");
                yield return new WaitForSeconds(0.2f);
                _monster.TempDefense *= move.Defense;
                spawnStatChangeIndicator(move.Defense,"DEF");
                yield return new WaitForSeconds(0.2f);
                _monster.TempSpeed *= move.Speed;
                spawnStatChangeIndicator(move.Speed,"SPD");
                _walkSpeed = _monster.Speed;
                yield return new WaitForSeconds(0.2f);
                _monster.TempStaminaRegen *= move.StaminaRegen;
                spawnStatChangeIndicator(move.StaminaRegen,"EDR");
            }
        }
    }

    /// <summary>
    /// Reset attack state
    /// </summary>
    /// <returns></returns>
    public IEnumerator resetAttackState()
    {
        yield return new WaitForSeconds(0.5f);
        _isAttacking = false;
    }

    /// <summary>
    /// Spanws a virtual indicator on top of monster object that indicates which stat has changed and by how much
    /// </summary>
    void spawnStatChangeIndicator(float stat, string suffix)
    {
        if (stat > 1)
            GameUtils.i.SpawnStatChangeIndicator(transform.position, ((stat - 1) * 100), 0.4f, 1, Color.green, false, "+", suffix);
        else if (stat < 1)
            spawnStatChangeIndicatorNegative(stat, suffix);
    }
    /// <summary>
    /// Spanws a virtual indicator on top of monster object that indicates which stat has changed and by how much for debuffs
    /// </summary>
    void spawnStatChangeIndicatorNegative(float stat, string suffix)
    {
        if (stat != 1)
            GameUtils.i.SpawnStatChangeIndicator(transform.position, ((1 - stat) * 100), 0.4f, 1, Color.red, false, "", suffix);
    }

    /// <summary>
    /// Apply knockback to target caused by damager
    /// </summary>
    public void CauseKnockback(GameObject damager, GameObject target, float knockbackValue, float duration)
    {
        Vector2 direction = (target.transform.position - damager.transform.position).normalized;

        target.GetComponent<Rigidbody2D>().AddForce(direction * knockbackValue, ForceMode2D.Impulse);
        StartCoroutine(ResetKnockbackState(target.GetComponent<Rigidbody2D>(), duration));
    }

    /// <summary>
    /// Removes knockback from a rigid body after a given time
    /// </summary>
    private IEnumerator ResetKnockbackState(Rigidbody2D rb, float time)
    {
        yield return new WaitForSeconds(time);
        OnKnockbackEnd?.Invoke();
        rb.linearVelocity = Vector3.zero;
    }

    /// <summary>
    /// Applys damage to current monster
    /// </summary>
    public void TakeDamage(BattleUnit damager, BattleMove damagerMove, float damage, bool hit/*Not directly used anywhere*/, ElementType attackElement)
    {
        if (damager == null || damagerMove == null || damagerMove.Base == null)
            return;

        if (!_isDead)
        {
            if (!hit)   //  Check if damage is going to hit
                return;
            if (_isImmune == true)   //  check if monster is not immune
                return;

            OnDamageTaken?.Invoke();
            CauseKnockback(damager.gameObject, gameObject, damagerMove.Knockback, damagerMove.KnockbackDuration);

            damage = (damage / _monster.Defense) + 1;
            float type = TypeChart.GetEffectiveness(attackElement, _monster.Base.element) * TypeChart.GetEffectiveness(attackElement, _monster.Base.secondaryElement);    //  Apply element type effectiveness
            damage *= type;
            _monster.HP -= damage;   //  Apply damage
            
            // Initialise a defender ctx
            AbilityContext defenderCTX = new AbilityContext()
            {
                User = this,
                Target = GetTargetUnit(),
                Move = damagerMove,
                Damage = damage
            };
            
            // Initialise an attacker ctx
            AbilityContext attackerCTX = new AbilityContext()
            {
                User = GetTargetUnit(),
                Target = this,
                Move =  damagerMove,
                Damage = damage
            };

            defenderCTX.User._monster.Ability?.OnDamageTaken(defenderCTX);
            attackerCTX.User._monster.Ability?.OnHitTarget(attackerCTX);

            if (GameUtils.i.IsSuperEffective(attackElement, _monster.Base.element) == true)
                GameUtils.i.SpawnDamageIndicator(transform.position, damage, 0.3f, 2, Color.red, true); //  If super effective, Apply red coloured virtual indicator
            else
                GameUtils.i.SpawnDamageIndicator(transform.position, damage, 0.3f, 1.5f, Color.white, false);   //  if not super effective, Apply white coloured virtual indicator

            if (damagerMove.StatChangesOpponent != null)
                StartCoroutine(ChangeStatsFromOpponent(damagerMove.StatChangesOpponent));   //  Apply status effects from damager move

            if (damagerMove.Base.DamageType == MoveType.PHYSICAL) // camera shake if move is physical
                GameUtils.i.ShakeCamera(0.2f, 1f, 2);

            updateHealth(); //  Update health bar

            if (_monster.HP <= 0)    //  Handles Death behaviour
            {
                Dead();
            }
        }
    }
    /// <summary>
    /// Apply damage from status condition to monster
    /// </summary>
    public void TakeDamageFromStatusCondition(float damage1, Status effect)
    {
        if (!_isDead)
        {
            if (_isImmune == true)
                return;
            //OnDamageTaken?.Invoke();
            _monster.HP -= damage1;
            switch (effect)
            {
                case Status.NONE:
                    GameUtils.i.SpawnDamageIndicator(transform.position, damage1, 0.3f, 1.5f, Color.white, false);
                    break;
                case Status.DROWN:
                    GameUtils.i.SpawnDamageIndicator(transform.position, damage1, 0.3f, 1.5f, Color.blue, false);
                    break;
                case Status.BURN:
                    GameUtils.i.SpawnDamageIndicator(transform.position, damage1, 0.3f, 1.5f, Color.red, false);
                    break;
                case Status.SLEEP:
                    GameUtils.i.SpawnDamageIndicator(transform.position, damage1, 0.3f, 1.5f, Color.white, false);
                    break;
                case Status.FREEZE:
                    GameUtils.i.SpawnDamageIndicator(transform.position, damage1, 0.3f, 1.5f, Color.cyan, false);
                    break;
                default:
                    break;
            }
            updateHealth();
            if (_monster.HP <= 0)
            {
                Dead();
            }
        }
    }

    public void Dead()
    {
        _isDead = true;
        playDeathAnimation();
        // Initialise a target ctx
        AbilityContext attacker = new AbilityContext()
        {
            User = GetTargetUnit(),
            Target = this
        };
        
        AbilityContext defender = new AbilityContext()
        {
            User = this,
            Target = GetTargetUnit()    
        };
        
        attacker.User._monster.Ability?.OnEnemyFainted(attacker);
    }

    public void ResetSwitchAbilityApplications()
    {
        _switchedInAbilityAppliedMonsters.Clear();
        _enemySwitchedAbilityAppliedMonsters.Clear();
    }

    public void TriggerSwitchedInAbilityOnce()
    {
        if (_monster == null || _monster.Ability == null)
            return;

        if (_switchedInAbilityAppliedMonsters.Contains(_monster))
            return;

        _switchedInAbilityAppliedMonsters.Add(_monster);

        AbilityContext ctx = new AbilityContext()
        {
            User = this,
            Target = GetTargetUnit()
        };

        _monster.Ability.OnPlayerSwitch(ctx);
    }

    public void TriggerEnemySwitchedAbilityOnce(BattleUnit switchedEnemy)
    {
        if (_monster == null || _monster.Ability == null)
            return;

        if (_enemySwitchedAbilityAppliedMonsters.Contains(_monster))
            return;

        _enemySwitchedAbilityAppliedMonsters.Add(_monster);

        AbilityContext ctx = new AbilityContext()
        {
            User = this,
            Target = switchedEnemy
        };

        _monster.Ability.OnEnemySwitch(ctx);
    }

    public BattleUnit GetTargetUnit()
    {
        if (_isEnemy == true)
            return GameUtils.i.battleSystem.PlayerUnit;
        return GameUtils.i.battleSystem.EnemyUnit;
    }
    
    
    /// <summary>
    /// Play death fading animation for monster sprite
    /// </summary>
    public void playDeathAnimation() => GetComponent<SpriteRenderer>().DOFade(0, 1f);
    /// <summary>
    /// Play spawn fading animation for monster sprite
    /// </summary>
    public void playSpawnAnimation() => GetComponent<SpriteRenderer>().DOFade(1, 1f);
}
