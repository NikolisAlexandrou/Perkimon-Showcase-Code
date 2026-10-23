using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

public enum GameState { OVERWORLD, INBATTLE, FREEZE, POSTBATTLE }

[RequireComponent(typeof(BattleHUD), typeof(BattleInputHandler), typeof(BattleRewardService))]
[RequireComponent(typeof(BattleEvolutionService), typeof(BattlePartyService), typeof(LearnMoveService))]
public class BattleSystem : MonoBehaviour
{
    enum PostBattlePhase
    {
        None,
        ExpAndLevelUp,       // showing EXP UI & level-up animations
        WaitForLevelUpClose, // waiting for player to press Interact
        LearnMoves,          // processing LearnMoveService
        Evolutions,          // processing BattleEvolutionService
        CatchOrDialogue,     // catching UI or end dialogue
        Finished
    }

    [Header("In Battle Events")]

    [Tooltip("Events playing once battle is initialised. (plays just when battle scene is created)")]
    public UnityEvent OnBattleInitialiseEvent;

    [Tooltip("Events playing once battle has started and all monsters are able to attack/move")]
    public UnityEvent OnBattleStartEvent;

    [Tooltip("Events playing once the enemy Trainer switches monster while in battle")]
    public UnityEvent OnTrainerSwitchMonsterEvent;

    [Tooltip("Events playing once the player switches monsters while in battle")]
    public UnityEvent OnPlayerSwitchedMonsterEvent;

    [Tooltip("Events playing when any battle has ended in any result")]
    public UnityEvent OnBattleEndEvent;

    [Tooltip("Events playing when all player's monsters are dead")]
    public UnityEvent OnPlayerLoseEvent;

    [Tooltip("Events playing when exp is awarded to player monsters")]
    public UnityEvent OnAwardExpEvent;

    [Tooltip("Events playing once all of enemy Trainer's monsters are dead")]
    public UnityEvent OnDefeatTrainerEvent;

    [Tooltip("Events playing once enemy monster is dead")]
    public UnityEvent OnDefeatWildMonsterEvent;

    [Header("Battle Handlers")]
    [SerializeField] private BattleHUD _battleHud;   //  Handler used for all battle hud logic
    [SerializeField] private BattleInputHandler _battleInputHandler; //  Handler used for all battle input logic
    [SerializeField] private BattleRewardService _rewardService; //  Handler used for all battle exp rewards logic
    [SerializeField] private BattleEvolutionService _evolutionService;   //  Handler used for monster evolution in battle logic
    [SerializeField] private BattlePartyService _partyService;   //  Handler used for party member switching logic
    [SerializeField] private LearnMoveService _learnMoveService; //  Handler used for learning new moves logic
    [SerializeField] private BattleCameraBehaviour _battleCameraBehaviour;
    private BattleStateMachine _stateMachine;   //  Battle state machine
    private PostBattlePhase _postBattlePhase = PostBattlePhase.None;

    [Header("General Info")]

    [HideInInspector]
    public GameObject BattleArena;  //  The current arena the battle takes place

    [Tooltip("Position where the battle arena will spawn (Usually a child of battleSystem object)")]
    public GameObject BattlePosition;   //  Spawns the arena in this position when battle starts

    [Tooltip("Start position of the player monster in battle")]
    public Vector2 PlayerBattleStartPosition;

    [Tooltip("Start position of the enemy monster in battle")]
    public Vector2 EnemyBattleStartPosition;

    [Tooltip("Drop in the animator for capsules when battle starts (must be same object as this script)")]
    [SerializeField] private Animator _battleOverworldAnimations;

    [Tooltip("The player (NOT MONSTER) that is visible in battle")]
    [SerializeField] private GameObject _playerObject;

    [Tooltip("The UI of the battle that holds all the HUDs (example parent of enemy and player health bars)")]
    [SerializeField] private GameObject _battleUI;

    [Header("Units")]

    [Tooltip("Holds the player battle unit")]
    public BattleUnit PlayerUnit;

    [Tooltip("Holds the enemy battle unit")]
    public BattleUnit EnemyUnit;

    [Header("Capsules")]

    [Tooltip("Move animation handler prefab MUST HAVE so that moves work")]
    public GameObject MoveAnimationHolder;

    [Tooltip("Holds the player catching object that is thrown to spawn monster")]
    [SerializeField] private GameObject _playerCatchingObject;

    [Tooltip("Holds the shadow below the player catching object")]
    [SerializeField] private GameObject _playerCatchingObjectShadow;

    [Tooltip("Holds the enemy catching object that is thrown to spawn monster")]
    [SerializeField] private GameObject _trainerCatchingObject;

    [Tooltip("Holds the shadow below the enemy catching object")]
    [SerializeField] private GameObject _trainerCatchingObjectShadow;

    [Header("Battle start")]

    [Tooltip("Timer for starting battle after monsters are spawned from their catching object (default: 2)")]
    [SerializeField] private float _battleResumeTimer = 2f;

    // state
    private bool _interruptBattle;    //  Freeze battle flag
    private bool _isAnimating;   //  Throwing catching objects out flag
    private bool _battleEnd; //  Battle end flag
    private float _timer;  //  Timer used for starting battle, initialised to battleResumeTimer
    private bool _awardExp;  //  award exp to monster flag


    [Header("Battle Effects")]
    [SerializeField] private float _clashKnockBackStrength = 10f;
    [SerializeField] private float _clashKnockBackTime = 0.35f;
    [SerializeField] private GameObject _dashParticles;

    [Header("Battle Transitions")]

    [Tooltip("Transition animation from overworld to battle (Can put your own transition read documentations for more info)")]
    public GameObject RadialBattleTransition;

    [Tooltip("Transition animation from battle to overworld (Can put your own transition read documentations for more info)")]
    public GameObject BattleEndTransition;

    private Vector2 _battleStartAnimationPosition = new Vector2(0, 0);   //  Position at which battle start animation spanwns in UI (Default 0,0)
    private GameObject _currentBattleStartAnimationObject;  //   currently spawned battle start animation object


    [Header("Trainer Battles")]

    [Tooltip("The Trainer (NOT MONSTER) that is visible in battle")]
    [SerializeField] private GameObject _trainerObject;

    private int _enemyMemberToSwitch;  //  Holds current enemy member position that is going to be switched
    private BattleTrainer _currentTrainer;    //  Holds the current battle trainer controller (Set automatically when trainer battle starts)
    private bool _isTrainerBattle;   //  if battle is trainer battle flag
    private bool _enemyMemberSwitched;   //  member switched positions flag
    private bool _enemySwitchAbilityPending;
    private bool _enemyDefeatHandled;

    [Header("Dialogue")]

    [Tooltip("Dialogue displayed when wild monster is defeated, Displays monster name automatically at start of dialogue so just put ' was defeated' for example only")]
    [TextArea]
    [SerializeField] private List<string> _defeatMonsterText = new List<string>();

    [Header("Overworld Start Battle Settings")]

    [Tooltip("When battle ends and state goes back to overworld, cooldown before allowing another battle to instantiate (IMPORTANT if value is < 0.5 battle will start and end instantly)")]
    [SerializeField] private float _resetBattleStartTimer = 0.5f;

    private bool _battleStartCooldown; //  flag for when battle start is on cooldown
    private bool _deathPartyUIOpened;
    private bool _postBattleFlowCompleted;
    private bool _readyToProcessLearnMove;    //  Flag to process move learning after Level up UI closes

    private GameState _gameState { get; set; } = GameState.OVERWORLD;  //  Holds the current game state

    /// <summary>
    /// Get the battle HUD handler
    /// </summary>
    public BattleHUD BattleHud => _battleHud;

    /// <summary>
    /// Get the input handler
    /// </summary>
    public BattleInputHandler BattleInputHandler => _battleInputHandler;

    /// <summary>
    /// Get the battle reward service
    /// </summary>
    public BattleRewardService BattleRewardService => _rewardService;

    /// <summary>
    /// Get the battle evolution service
    /// </summary>
    public BattleEvolutionService BattleEvolutionService => _evolutionService;

    /// <summary>
    /// Get the battle party service
    /// </summary>
    public BattlePartyService BattlePartyService => _partyService;

    /// <summary>
    /// Get the learn move service
    /// </summary>
    public LearnMoveService LearnMoveService => _learnMoveService;
    
    /// <summary>
    /// Get the dynamic battle camera behaviour
    /// </summary>
    public BattleCameraBehaviour BattleCameraBehaviour => _battleCameraBehaviour;

    /// <summary>
    /// Get current state of game
    /// </summary>
    public IBattleState CurrentState => _stateMachine.CurrentState;

    /// <summary>
    /// Get the party member position that enemy trainer is going to switch with active member
    /// </summary>
    public int EnemyMemberToSwitch => _enemyMemberToSwitch;

    /// <summary>
    /// Get the current trainer player is battling
    /// </summary>
    public BattleTrainer CurrentTrainer { get => _currentTrainer; set => _currentTrainer = value; }

    //------------------------------------------------------------------------------------------------------------------------------
    // (!) Flag to check if in battle USE THIS TO CHECK IF PLAYER IS BATTLING NOT CHECK ONLY IF INBATTLE STATE AS THIS WILL BREAK (!)
    /// <summary>
    /// Return true if player is in battle
    /// </summary>
    public bool IsInBattle =>
        (_gameState == GameState.POSTBATTLE ||
        _gameState == GameState.INBATTLE);

    //------------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Get or set interrupt battle state
    /// </summary>
    public bool InterruptBattle { get => _interruptBattle; set => _interruptBattle = value; } //  Set interrupt to battle
    
    /// <summary>
    /// Get or set if catching objects are playing animations
    /// </summary>
    public bool IsAnimating { get => _isAnimating; set => _isAnimating = value; }
    
    /// <summary>
    /// Return true when battle has ended
    /// </summary>
    public bool CheckForBattleEnd => _battleEnd;

    /// <summary>
    /// Get the clash knockback strength value
    /// </summary>
    public float ClashKnockBackStrength => _clashKnockBackStrength;

    /// <summary>
    /// Get the clash knockback timer value
    /// </summary>
    public float ClashKnockBackTime => _clashKnockBackTime;

    /// <summary>
    /// Get the dash particles object
    /// </summary>
    public GameObject DashParticle => _dashParticles;

    /// <summary>
    /// Get the cooldown for starting another battle from overworld
    /// </summary>
    public bool BattleStartCooldown => _battleStartCooldown;

    /// <summary>
    /// Get the player catching object
    /// </summary>
    /// <returns></returns>
    public GameObject PlayerCatchingObject => _playerCatchingObject;

    private void Awake()
    {
        //  if required sub-system components are not found in editor, add them
        if (!_battleHud) _battleHud = GetComponent<BattleHUD>();
        if (!_battleInputHandler) _battleInputHandler = GetComponent<BattleInputHandler>();
        if (!_rewardService) _rewardService = GetComponent<BattleRewardService>();
        if (!_evolutionService) _evolutionService = GetComponent<BattleEvolutionService>();
        if (!_partyService) _partyService = GetComponent<BattlePartyService>();
        if (!_learnMoveService) _learnMoveService = GetComponent<LearnMoveService>();
        if (!_battleOverworldAnimations) _battleOverworldAnimations = GetComponent<Animator>();

        _stateMachine = new BattleStateMachine();   //  Initialise a new Battle state machine
    }
    //  Plays only once on first start of game.
    void Start()
    {
        SetState(GameState.OVERWORLD);  //  Set initial state to OVERWORLD
        BattleInitialise(); //  Initialise Battle (not playing until state is changed to INBATTLE)
    }
    void Update() => _stateMachine.Tick();   //  Method called every game frame
    void FixedUpdate() => _stateMachine.FixedTick();  //  Method called every fixed time game frame

    #region in battle tick

    public void BattleFixedTick()   //  Method called every fixed time game frame while INBATTLE state
    {
        if (/*_gameState != GameState.INBATTLE ||*/ _battleEnd) return;

        if (!_interruptBattle)
        {
            PlayerUnit?.IncreaseStamina();   //  Increase player stamina values over time
            EnemyUnit?.IncreaseStamina();    //  Increase enemy stamina values over time
        }
    }
    public void BattleTick()    //  Method called every INBATTLE game frame
    {
        // Update HUD only if changed
        _battleHud.RefreshNamesAndLevels(PlayerUnit, EnemyUnit);

        if (_isAnimating) return;

        if (_timer <= 1f)    //  when battle start timer gets to 0, enable all battle behaviours
        {
            BattleActive();
        }
        else
        {   //  Disable all behaviour while countdown to start battle
            _interruptBattle = true;
            _timer -= Time.deltaTime;
            if (_timer <= 1f)
                OnBattleStartEvent?.Invoke();   //  Call start battle event
        }
    }
    public void PostBattleTick()    //  Handles all post battle states
    {
        if (!_battleEnd) return;

        switch (_postBattlePhase)
        {
            case PostBattlePhase.ExpAndLevelUp:
                HandleExpAndLevelUpPhase();
                break;

            case PostBattlePhase.WaitForLevelUpClose:
                HandleWaitForLevelUpClosePhase();
                break;

            case PostBattlePhase.LearnMoves:
                HandleLearnMovesPhase();
                break;

            case PostBattlePhase.Evolutions:
                HandleEvolutionPhase();
                break;

            case PostBattlePhase.CatchOrDialogue:
                HandleCatchOrDialoguePhase();
                break;

            case PostBattlePhase.Finished:
                // nothing left; just waiting for animation to call EndBattle()
                break;
        }
    }

    public void SetState(GameState newState)    //  Sets the state of the game
    {
        _gameState = newState;

        switch (newState)
        {
            case GameState.OVERWORLD:
                _stateMachine.ChangeState(new OverworldState(this));
                break;

            case GameState.INBATTLE:
                _stateMachine.ChangeState(new InBattleState(this));
                break;

            case GameState.FREEZE:
                _stateMachine.ChangeState(new FreezeState(this));
                break;

            case GameState.POSTBATTLE:
                _stateMachine.ChangeState(new PostBattleState(this));
                break;
        }
    }
    public void OnEnterOverworld()  //  Called when game state changes to OVERWORLD state
    {
        _battleUI?.SetActive(false);
        if (BattleArena != null)
        {
            Destroy(BattleArena);
            BattleArena = null; //  Destroy Current arena
        }
    }
    public void OnExitOverworld()   //   Called when game state exits from OVERWORLD state
    {
        // Nothing yet, but you might want something later
    }
    void BattleActive() //  Called every frame the game is INBATTLE state
    {
        if (!_interruptBattle)
        {
            PlayerUnit.DisableAttacking = false;
            EnemyUnit.DisableAttacking = false;
            _battleHud.SetPartyButtonVisible(true);
            _battleHud.ChangeActiveMove(PlayerUnit);

            if (_battleInputHandler.AttackHeld) MoveAttack();
            if (_battleInputHandler.PartyPressed) OnClickPartyButton(false);

            _battleHud.UpdatePartyButtonCooldown(_partyService.CooldownTime, _partyService.CanSwitch);
        }
        else
        {
            PlayerUnit.DisableAttacking = true;
            EnemyUnit.DisableAttacking = true;
            _battleHud.SetPartyButtonVisible(false);
        }

        _partyService.Tick(Time.deltaTime);
        CheckForPlayerSwitchOnDeath();
    }

    #endregion

    #region for awarding exp
    public void DisableLevelUpUI()
    {
        _rewardService.CloseLevelUpUI(OnLevelUpUiClosed);
    }   //  Close Level Up UI
    void OnLevelUpUiClosed()
    {
        // At this point EXP UI is fully closed, now we are allowed to
        // start the learn-move flow in PostBattleTick.
        _readyToProcessLearnMove = true;
    }   //  Plays when level Up UI closes

    /// <summary>
    /// Start awarding exp and enable level Up UI
    /// </summary>
    public void AwardExp()
    {
        _battleHud.SetPartyButtonVisible(false);
        if (EnemyUnit?.CurrentMonster == null) return;

        _rewardService.StartAwardExp(EnemyUnit.CurrentMonster);
    }


    #endregion

    #region for starting battle

    public void BattleInitialise()
    {
        _isAnimating = true; //  Set battle state to animating for player/enemy to throw their monsters out

        // Start animation object
        var startPrefab = GameUtils.i.player.GetComponent<PlayerController>().CurrentArea.BattleStartAnimationObject;
        if (startPrefab != null && _battleUI != null)
        {
            _currentBattleStartAnimationObject = Instantiate(startPrefab, _battleStartAnimationPosition, Quaternion.identity, _battleUI.transform);
            _currentBattleStartAnimationObject.SetActive(true); //  Instantiate the battle scene (The game object containing tilemap of the battle arena)
        }
    }

    /// <summary>
    /// Re-enables ability to go into battle during overworld state (Added cooldown to prevent tested bugs)
    /// </summary>
    /// <returns></returns>
    IEnumerator ResetBattleStartCooldown()
    {
        _battleStartCooldown = true;
        yield return new WaitForSeconds(_resetBattleStartTimer);
        _battleStartCooldown = false;
    }

    /// <summary>
    /// Start a wild monster battle
    /// </summary>
    public void TriggerWildBattle()
    {
        SpawnStartAnim();
        _readyToProcessLearnMove = false;
        SetState(GameState.INBATTLE);
        EnsureBattleArenaForCurrentArea();
        OnBattleInitialiseEvent?.Invoke();
        InitialiseBattleMonster();
        ResetSwitchAbilityApplications();

        PlayerUnit.ResetBattleUnit();
        PlayerUnit.gameObject.SetActive(false);

        EnemyUnit.gameObject.SetActive(true);
        EnemyUnit.ResetBattleUnit();
        EnemyUnit.StartBattleUnit();

        if (_playerObject) _playerObject.SetActive(true);
        _battleUI?.SetActive(true);
        BattleArena?.SetActive(true);
        _battleCameraBehaviour.AssignCamera();
        if (_playerCatchingObject) _playerCatchingObject.SetActive(true);
        if (_trainerObject) _trainerObject.SetActive(false);
        if (_trainerCatchingObject) _trainerCatchingObject.SetActive(false);
        if (_trainerCatchingObjectShadow) _trainerCatchingObjectShadow.SetActive(false);
    }
    /// <summary>
    /// Start a trainer battle
    /// </summary>
    public void TriggerTrainerBattle()
    {
        if (_playerCatchingObject) _playerCatchingObject.SetActive(true);
        if (_trainerCatchingObjectShadow) _trainerCatchingObjectShadow.SetActive(true);

        SpawnStartAnim();
        _readyToProcessLearnMove = false;
        SetState(GameState.INBATTLE);
        _isTrainerBattle = true;
        EnsureBattleArenaForCurrentArea();
        InitialiseBattleMonster();
        OnBattleInitialiseEvent?.Invoke();
        ResetSwitchAbilityApplications();
        


        PlayerUnit.ResetBattleUnit();
        PlayerUnit.gameObject.SetActive(false);
        Debug.Log("INITIALISED TRAINER BATTLE UP UNTIL THIS POINT");

        //TODO
        // This method below has a bug when starting trainer battles
        // for the first time in a scene. If battle is started for second time after a wild battle
        // the EnemyUnit.ResetBattleUnit() works fine. But for the first time, it doesn't reset the enemy unit properly.

        // FOUND THE PROBLEM:

        

        if (_currentTrainer != null && _currentTrainer.TrainerParty.Count > 0)
            EnemyUnit.CurrentMonster = _currentTrainer.TrainerParty[0];
        EnemyUnit.ResetBattleUnit();
        EnemyUnit.StartBattleUnit();
        EnemyUnit.gameObject.SetActive(false);

        if (_playerObject) _playerObject.SetActive(true);
        
        if (_trainerObject)
        {
            _trainerObject.SetActive(true);
            var sr = _trainerObject.GetComponent<SpriteRenderer>();
            if (sr) sr.sprite = _currentTrainer?.InBattleTrainerSprite;
        }

        BattleArena?.SetActive(true);
        _battleUI?.SetActive(true);
        _battleCameraBehaviour.AssignCamera();
        if (_playerCatchingObject) _playerCatchingObject.SetActive(true);

        //changedMove = false;
        AnimateTrainer();
        
    }
    /// <summary>
    /// Initialise UI semi-transition playing when battle starts
    /// </summary>
    void SpawnStartAnim()
    {
        var startPrefab = GameUtils.i.player.GetComponent<PlayerController>().CurrentArea.BattleStartAnimationObject;
        if (startPrefab != null && _battleUI != null)
        {
            _currentBattleStartAnimationObject = Instantiate(startPrefab, _battleStartAnimationPosition, Quaternion.identity, _battleUI.transform);
            // ensure UI position (if rect transform exists)
            var rt = _currentBattleStartAnimationObject.GetComponent<RectTransform>();
            if (rt) rt.localPosition = _battleStartAnimationPosition;
            _currentBattleStartAnimationObject.SetActive(true);
        }
    }
    /// <summary>
    /// Spawn the Battle Arena for this instance of battle (arena is a prefab set on MapArea scriptable object)
    /// </summary>
    void EnsureBattleArenaForCurrentArea()
    {
        var desired = GameUtils.i.player.GetComponent<PlayerController>().CurrentArea.BattleArenaPrefab;
        if (desired == null) return;

        // If already using the same prefab, keep it, else rebuild
        if (BattleArena == null || BattleArena.name.Replace("(Clone)", "") != desired.name)
        {
            if (BattleArena != null) Destroy(BattleArena);
            BattleArena = Instantiate(desired, BattlePosition.transform.position, Quaternion.identity, BattlePosition.transform);
        }
    }

    /// <summary>
    /// Initialise battle monster in battle
    /// </summary>
    public void InitialiseBattleMonster()
    {
        if (PlayerUnit == null) return;
        PlayerUnit.Stamina = 0f;    //  Set stamina to 0
        GameUtils.i.getAliveMonster();  //  Gets the first alive monster in party

        PlayerUnit.updateHealth();  //  Update its health

        _battleHud.InitialiseMoves(PlayerUnit);  //  Initialise battle moves for this monster
        PlayerUnit.StartBattleUnit();   //  Reset its position and set sprites

        if (_playerCatchingObject != null && PlayerUnit.CurrentMonster?.CapsuleCaught != null)
        {
            var sr = _playerCatchingObject.GetComponentInChildren<SpriteRenderer>();
            if (sr) sr.sprite = PlayerUnit.CurrentMonster.CapsuleCaught.Capsule_Sprite;    //  Set the catching object sprite for this monster
        }
    }

    #endregion

    #region Player Attack

    /// <summary>
    /// Returns if player can attack when meeting some conditions
    /// </summary>
    bool CanPlayerAttack()
    {
        if (EnemyUnit == null || PlayerUnit == null) return false;
        if (EnemyUnit.IsDead || PlayerUnit.IsDead) return false;
        if (_interruptBattle) return false;

        var move = PlayerUnit.CurrentMove;
        if (move == null) return false;
        if (PlayerUnit.Stamina < move.StaminaCost) return false;
        if (PlayerUnit.IsAttacking) return false;
        if (PlayerUnit.IsInKnockbackState) return false;

        return true;
    }
    /// <summary>
    /// Forces player monster to attack
    /// </summary>
    public void MoveAttack()
    {
        if (!CanPlayerAttack()) return;

        var move = PlayerUnit.CurrentMove;
        PlayerUnit.IsAttacking = true;
        float damage = PlayerUnit.CurrentMonster.CalculateMoveStats(move);
        // Initialise an attacker ctx
        AbilityContext attackerCTX = new AbilityContext()
        {
            User = PlayerUnit,
            Target = EnemyUnit,
            Move = move,
            Damage = damage
        };

        attackerCTX.User.CurrentMonster.Ability?.OnMoveUsed(attackerCTX);
        if (attackerCTX.CancelMove)
        {
            PlayerUnit.IsAttacking = false;
            return;
        }

        PlayerUnit.CurrentMonster.TotalAttack = attackerCTX.Damage;
        GameUtils.i.PlayAnimationFromMove(move, MoveAnimationHolder, PlayerUnit, EnemyUnit, false);

        PlayerUnit.Stamina -= move.StaminaCost;
        PlayerUnit.StartCoroutine(PlayerUnit.resetAttackState());
    }
    #endregion

    #region Catching object animations (start of battle)

    /// <summary>
    /// Throws catching objects to spawn monsters when switching
    /// </summary>
    public void ThrowCapsules()
    {
        if (_battleOverworldAnimations == null) return;

        _battleOverworldAnimations.SetBool("WildEnemyMonster", true);

        if (!_enemyMemberSwitched)
            ThrowPlayerCapsule();

        // only throw enemy capsule if this isn't a player switch animation
        if (_isTrainerBattle && (_partyService == null || !_partyService.PlayerSwitchInProgress))
            ThrowEnemyCapsule();
    }
    /// <summary>
    /// Play animation of throwing player catching object to spawn monster
    /// </summary>
    public void ThrowPlayerCapsule()
    {
        _battleOverworldAnimations?.SetBool("ThrowPlayerMonster", true);
        AudioSystem.i.PlaySoundClip(AudioSystem.i.ThrowCatchingObjectSound);
    }
    /// <summary>
    /// Play animation of throwing enemy catching object to spawn monster
    /// </summary>
    public void ThrowEnemyCapsule()
    {
        _battleOverworldAnimations?.SetBool("ThrowEnemyMonster", true);
        AudioSystem.i.PlaySoundClip(AudioSystem.i.ThrowCatchingObjectSound);
    }
    /// <summary>
    /// When player catching object spawns the monster, initialise monster
    /// </summary>
    public void OnPlayerCapsuleAnimationEnd()
    {
        //  reset player catching object animation state
        _battleOverworldAnimations?.SetBool("ThrowPlayerMonster", false);
        if (_playerCatchingObject) _playerCatchingObject.SetActive(false);
        if (PlayerUnit != null)
        {
            //  spawn and initialise monster and show health bar
            PlayerUnit.gameObject.SetActive(true);
            var healthbar = PlayerUnit.HealthBar;
            if (healthbar) healthbar.transform.parent.gameObject.SetActive(true);

            InitialiseBattleMonster();

            //  spawn particles of catching object spawning monster
            var particle = PlayerUnit.CurrentMonster?.CapsuleCaught?.Capsule_Particle;
            if (particle != null)
            {
                var particleObject = Instantiate(particle.gameObject);
                particleObject.transform.position = PlayerUnit.transform.position;
            }
        }
        if (_partyService != null && _partyService.PlayerSwitchInProgress)
        {
            PlayerUnit.TriggerSwitchedInAbilityOnce();
            EnemyUnit?.TriggerEnemySwitchedAbilityOnce(PlayerUnit);
        }
        _partyService?.MarkPlayerSwitchFinished();
    }
    /// <summary>
    /// When trainer (enemy) catching object spawns the monster, initialise monster
    /// </summary>
    public void OnEnemyCapsuleAnimationEnd()
    {
        //  reset enemy trainer catching object animation state
        _battleOverworldAnimations?.SetBool("ThrowEnemyMonster", false);
        if (_trainerCatchingObject) _trainerCatchingObject.SetActive(false);

        if (EnemyUnit != null)
        {
            //  spawn and initialise monster and show health bar
            EnemyUnit.gameObject.SetActive(true);
            var healthbar = EnemyUnit.HealthBar;
            if (healthbar) healthbar.transform.parent.gameObject.SetActive(true);

            var particle = EnemyUnit.CurrentMonster?.CapsuleCaught?.Capsule_Particle;
            if (particle != null)
            {
                //  spawn particles of catching object spawning monster
                var particleObject = Instantiate(particle.gameObject);
                particleObject.transform.position = EnemyUnit.transform.position;
            }
        }
        if (_enemySwitchAbilityPending)
        {
            EnemyUnit.TriggerSwitchedInAbilityOnce();
            PlayerUnit?.TriggerEnemySwitchedAbilityOnce(EnemyUnit);
            _enemySwitchAbilityPending = false;
        }
    }

    public void OnWildEnemyAnimationEnd()
    {
        if (!_isTrainerBattle && EnemyUnit != null)
        {
            EnemyUnit.gameObject.SetActive(true);
            var hb = EnemyUnit.HealthBar;
            if (hb) hb.transform.parent.gameObject.SetActive(true);
        }
        _battleOverworldAnimations?.SetBool("WildEnemyMonster", false);
    }
    /// <summary>
    /// Set battle timer to re-enable battle when both player and trainer (if any) catching object animation ends
    /// </summary>
    public void OnBothCapsuleAnimationsEnd()
    {
        _isAnimating = false;
        _timer = _battleResumeTimer;
    }

    #endregion

    #region Post Battle

    /// <summary>
    /// Disables all battle behaviours and sets state to overworld
    /// </summary>
    public void EndBattle()
    {
        PlayerUnit?.StopAllCoroutines();
        EnemyUnit?.StopAllCoroutines();
        if (_currentTrainer != null)
        {
            _currentTrainer.BattleMusic?.Stop();
            _currentTrainer.IsDefeated = true;
            _currentTrainer.ResetBattle();
            _currentTrainer = null;
        }

        _battleHud.SetPartyButtonVisible(false);
        UIManager.i.PartyScene.SetActive(false);
        SetState(GameState.OVERWORLD);
        _postBattleFlowCompleted = false;
        if (_trainerObject) _trainerObject.SetActive(false);
        if (_playerObject) _playerObject.SetActive(false);
        _isTrainerBattle = false;

        StartCoroutine(ResetBattleStartCooldown());
        ResetBattle();
    }

    /// <summary>
    /// Resets states of some battle services, resets variables to default and destroys battle start animations
    /// </summary>
    void ResetBattle()
    {
        if (_currentBattleStartAnimationObject != null)
        {
            Destroy(_currentBattleStartAnimationObject);
            _currentBattleStartAnimationObject = null;
        }

        ResetSwitchBattleVariables();
        if (PlayerUnit) PlayerUnit.transform.position = PlayerBattleStartPosition;

        _partyService?.ResetSwitchState();
        _learnMoveService?.ResetState();
    }
    /// <summary>
    /// Reset variables back to default to ensure next battle will work as intended
    /// </summary>
    void ResetSwitchBattleVariables()
    {

        if (EnemyUnit) EnemyUnit.transform.position = EnemyBattleStartPosition;

        _battleEnd = false;
        _awardExp = false;
        InterruptBattle = false;
        _isAnimating = true;
        _timer = 0f;

        _postBattleFlowCompleted = false;
        _readyToProcessLearnMove = false;
        _enemyDefeatHandled = false;

        _rewardService?.ResetRewards();
        _evolutionService?.ResetQueue();
    }

    private void ResetSwitchAbilityApplications()
    {
        PlayerUnit?.ResetSwitchAbilityApplications();
        EnemyUnit?.ResetSwitchAbilityApplications();
        _enemySwitchAbilityPending = false;
    }

    /// <summary>
    /// Handles post battle states of leveling up UI
    /// </summary>
    void HandleExpAndLevelUpPhase()
    {
        // fire defeat event if needed
        if (PlayerUnit != null && PlayerUnit.IsDead)
            OnBattleEndEvent?.Invoke();

        if (_awardExp)
        {
            OnAwardExpEvent?.Invoke();
            _awardExp = false;
        }

        if (!_rewardService.AllAwardsComplete)
            return;

        // EXP animations done; now player must close the UI
        if (_rewardService.IsLevelUpUIOpen)
        {
            _postBattlePhase = PostBattlePhase.WaitForLevelUpClose;
        }
        else
        {
            // weird edge case: nothing to show, skip straight ahead
            _postBattlePhase = PostBattlePhase.LearnMoves;
        }
    }

    /// <summary>
    /// Sets post battle state to learnMove when levelUp UI is closed and handles logic to close levelUp UI
    /// </summary>
    void HandleWaitForLevelUpClosePhase()
    {
        if (!_rewardService.IsLevelUpUIOpen)
        {
            // CloseLevelUpUI callback already ran -> ready for learn moves
            _postBattlePhase = PostBattlePhase.LearnMoves;
            return;
        }

        if (_battleInputHandler.InteractPressed && _rewardService.CanPressContinue)
            DisableLevelUpUI(); // you can keep the callback to do any flag updates
    }

    /// <summary>
    /// Changes post battle state from learnMove to Evolution if learn move service is done
    /// </summary>
    void HandleLearnMovesPhase()
    {
        if (!_learnMoveService.IsDone)
        {
            _learnMoveService.Process(); //  Start checking if there are any new moves learned
            return;
        }

        _postBattlePhase = PostBattlePhase.Evolutions;  //  Set post battle state to evolutions
    }

    /// <summary>
    /// Repeatitively checks for any evolutions to play animations and if everything is done, move to CatchOrDialogue post battle state
    /// </summary>
    void HandleEvolutionPhase()
    {
        if (_evolutionService.HasPendingEvolutions || _evolutionService.IsAnimating)
        {
            _evolutionService.TryPlayNext(); //  Play next evolution animation if any
            return;
        }

        _postBattlePhase = PostBattlePhase.CatchOrDialogue; //  Set post battle state to CatchOrDialogue
    }

    /// <summary>
    /// Displays dialogue or catching UI and sets post battle state to finished
    /// </summary>
    void HandleCatchOrDialoguePhase()
    {
        CompletePostBattleFlow(); // does / catching / dialogue
        _postBattlePhase = PostBattlePhase.Finished;
    }

    // Handles Evolving monster logic
    // Called when we want to process evolutions *for this battle*
    public void EvolveMonster()
    {
        // If we have an evolution service and it has evolutions to play,
        // or is currently animating, let it handle that.
        if (_evolutionService != null)
        {
            // If still animating or still has queued evolutions, just play them.
            if (_evolutionService.HasPendingEvolutions || _evolutionService.IsAnimating)
            {
                _evolutionService.TryPlayNext();
                return;
            }
        }

        // If we reach here, there are NO evolutions pending.
        // So we can jump straight to end-of-battle flow.
        CompletePostBattleFlow();
    }
    // This is ONLY responsible for the "after evolutions" logic
    // (catching / end-battle dialogue).
    public void CompletePostBattleFlow()
    {
        if (!_isTrainerBattle)   //  If we are in a wild monster battle
        {
            if (!BagManager.i.SearchForCatchingDisksInBag())    //  If player has no catching objects in bag, display end battle dialogue
                DisplayEndBattleDialogue(false);
            else
                CatchingManager.i.InitialiseCatchingSystem();   //  Initialise the catching system
        }
        else
        {
            DisplayEndBattleDialogue(false);
            _isTrainerBattle = false;
        }
    }

    /// <summary>
    /// Display end battle dialogue depending if monster was caught, if it was a trainer battle or if it was a wild battle
    /// </summary>
    public void DisplayEndBattleDialogue(bool caught)
    {
        UnityEvent events = new UnityEvent();
        events.AddListener(PlayBattleEndTransition);

        if (_isTrainerBattle)
        {
            GameUtils.i.dialogueSystem.InitialiseNewTextNPC(
                _currentTrainer.TrainerDefeatDialogue,
                _currentTrainer.GetComponent<NPC_Stats>().NPCName,
                _currentTrainer.GetComponent<NPC_Stats>().NPCDialogueSprite,
                events
            );
        }
        else
        {
            if (!caught)
                GameUtils.i.dialogueSystem.InitialiseNewDefeatedText(EnemyUnit.CurrentMonster.Base.Name, events);
            else
                GameUtils.i.dialogueSystem.InitialiseNewCaughtText(EnemyUnit.CurrentMonster.Base.Name, events);
        }
    }

    #endregion

    #region for party switching
    private void PlayBattleEndTransition() => BattleEndTransition?.SetActive(true); //  Play the battle end transition (example the fading black screen at end to transition from battle to overworld)
    public void AnimateTrainer() { if (_trainerCatchingObject) _trainerCatchingObject.SetActive(true); }
    // Switching / death flow
    /// <summary>
    /// Handles checking for player switching if current monster is dead and handles player Losing when all monsters in party are dead
    /// </summary>
    private void CheckForPlayerSwitchOnDeath()
    {
        // Enemy dead = battle is over (player wins)
        if (EnemyUnit != null && EnemyUnit.IsDead)
        {
            if (!_enemyDefeatHandled)
                OnEnemyDefeated();
        }

        // Player dead = either lose, or force switch if someone alive
        if (PlayerUnit != null && PlayerUnit.IsDead)
        {
            var party = PlayerStats.i.PlayerParty;

            if (!PartyRules.HasAliveMonster(party)) //  All monsters in party are dead so player loses battle
            {
                // No monsters left = battle over (player loses)
                if (!_battleEnd)
                {
                    _battleEnd = true;
                    _postBattlePhase = PostBattlePhase.Finished;
                    SetState(GameState.POSTBATTLE);
                    OnPlayerLoseEvent?.Invoke();
                    //TODO
                    // lose UI is displayed when the OnPlayerLoseEvent fires
                    // display lose dialogue and heal player team
                    
                    List<String> loseText = new List<String>();
                    UnityEvent events = new UnityEvent();
                    events.AddListener(PlayBattleEndTransition);
                    events.AddListener(GameUtils.i.HealPlayerTeam);
                    
                    //events.AddListener(() => _battleHud.EnablePlayerLoseUI(false));
                    //TODO
                    // Player teleports to last position healed and loses 1/3 of his money
                    // FOR POLISHING, add different dialogues for when player dies to wild monster or from trainer

                    GameUtils.i.player.transform.position = GameUtils.i.LastHealedNPCPosition;
                    
                    int amountLost = (int)(PlayerStats.i.Money / 3f);
                    PlayerStats.i.Money -= amountLost;
                    loseText.Add("You lost the battle and dropped " + amountLost + " coins");
                    
                    GameUtils.i.dialogueSystem.InitialiseNewText(loseText, " ", events );
                }
            }
            else
            {
                // Still have alive monsters = open party UI to switch
                if (!_deathPartyUIOpened)
                {
                    _deathPartyUIOpened = true;
                    PlayerUnit.IsImmune = true;
                    StartCoroutine(DisplayPartyUI(false));
                }
            }
        }
    }
    /// <summary>
    /// When enemy is defeated, move to post battle states
    /// </summary>
    void OnEnemyDefeated()
    {
        _enemyDefeatHandled = true;
        _awardExp = true;
        _battleEnd = true;
        _postBattlePhase = PostBattlePhase.ExpAndLevelUp;
        SetState(GameState.POSTBATTLE);
    }
    /// <summary>
    /// Returns the next alive monster in a party
    /// </summary>
    public BattleMonster NextAliveMonster(List<BattleMonster> party, int startPosition, bool forEnemy)
    {
        var idx = PartyRules.GetNextAliveSlot(party, startPosition);
        if (forEnemy) _enemyMemberToSwitch = idx;
        return party[Mathf.Clamp(idx, 0, party.Count - 1)];
    }
    /// <summary>
    /// Handles checking for trainer (enemy) party switching when enemy monster is dead and sets animations and initialises monster
    /// </summary>
    public void CheckForEnemyPartyMemberSwitched()
    {
        if (!_enemyMemberSwitched) return;

        //  set battle to interrupt state
        _interruptBattle = true;

        //  set enemy unit to alive so that next member is alive and not dead
        EnemyUnit.IsDead = false;
        EnemyUnit?.StopAllCoroutines();
        if (EnemyUnit != null) EnemyUnit.gameObject.SetActive(false);

        if (_currentTrainer != null && EnemyUnit != null)
        {
            //  switch party member chosen to first active party member and reset their overworld properties
            BattleMonster currentmon = EnemyUnit.CurrentMonster;

            //  set member chosen to first active member position
            _currentTrainer.TrainerParty[0] = NextAliveMonster(_currentTrainer.TrainerParty, 0, true);
            EnemyUnit.CurrentMonster = _currentTrainer.TrainerParty[0];
            EnemyUnit.ResetBattleUnit();

            //  set first member to last member chosen position
            _currentTrainer.TrainerParty[_enemyMemberToSwitch] = currentmon;
            EnemyUnit.StartBattleUnit();

            if (_trainerCatchingObject) _trainerCatchingObject.SetActive(true);
            EnemyUnit.CurrentMonster.Initialise();

            var sr = _trainerCatchingObject?.GetComponentInChildren<SpriteRenderer>();
            if (sr && EnemyUnit.CurrentMonster.CapsuleCaught != null)
                sr.sprite = EnemyUnit.CurrentMonster.CapsuleCaught.Capsule_Sprite;

            ResetSwitchBattleVariables();
            _enemySwitchAbilityPending = true;
            OnTrainerSwitchMonsterEvent?.Invoke();
            _enemyMemberSwitched = false;
        }
    }

    /// <summary>
    /// Checks if trainer has any monsters left in his party and play
    /// certain events like switching to other monsters/ defeat trainer
    /// or defeated wild monster
    /// </summary>
    public void CheckTrainerMonstersLeft()
    {
        if (_rewardService != null && _rewardService.IsLevelUpUIOpen)
            return;


        //if enemy unit is still alive dont continue
        if (EnemyUnit == null || !EnemyUnit.IsDead) return;

        //if is in trainer battle
        if (_isTrainerBattle)
        {
            //enemy has more than 1 alive monsters left so switch to that monster
            if (PartyRules.HasAliveMonster(_currentTrainer.TrainerParty))
            {
                _enemyMemberSwitched = true;
                CheckForEnemyPartyMemberSwitched();
                _interruptBattle = true;
            }
            else//enemy has no more monsters left
            {
                EvolveMonster();
                OnDefeatTrainerEvent?.Invoke();
            }
        }
        else
        {
            EvolveMonster();
            OnDefeatWildMonsterEvent?.Invoke();
        }
    }

    /// <summary>
    /// Reset battle start timer
    /// </summary>
    public void InitialiseTimer() { _timer = _battleResumeTimer; }


    #endregion

    #region for learn move

    /// <summary>
    /// Switch currently selected move index to current learning move (Add this method in UI on click event)
    /// </summary>
    public void OnClickChangeMoveButton(int movenum)
    {
        _learnMoveService.OnClickChangeMoveButton(movenum, _gameState);
    }

    /// <summary>
    /// On click not learn move in party UI
    /// </summary>
    public void OnClickNotLearnMove() => _learnMoveService.OnClickNotLearnMove();

    /// <summary>
    /// Closes LevelUp UI
    /// </summary>
    public void DisableLearnMoveUI() => _learnMoveService.DisableLearnMoveUI(); 

    #endregion

    #region For party UI (both battle and overworld)

    IEnumerator DisplayPartyUI(bool forceBattleEnd)
    {
        yield return new WaitForSeconds(1.5f);
        PlayerUnit.IsDead = false;

        if (forceBattleEnd)
        {
           _battleEnd = true;
        }
        else
        {
            OnClickPartyButton(true);
            _deathPartyUIOpened = false;
        }
    }

    /// <summary>
    /// Opens party UI in battle
    /// </summary>
    public void OnClickPartyButton(bool ignoreCooldown)
    {
        if (EnemyUnit != null && EnemyUnit.IsDead) return;

        if (_partyService.CanSwitch || ignoreCooldown)
        {
            _interruptBattle = true;
            UIManager.i.EnablePartyMenu(true);
            var partyScript = UIManager.i.PartyScene.GetComponent<PartyScript>();
            partyScript.partyState = PartyState.BATTLE;
            _battleHud.SetPartyButtonVisible(false);
        }
    }

    //TODO ADD IT SOMEWHERE ELSE LIKE IN PARTY SCENE
    /// <summary>
    /// Open party UI while in overworld
    /// </summary>
    public void OnClickPartyButtonOVERWORLD()
    {
        if (PlayerStats.i.PlayerParty.Count <= 0) return;    //  If player has no monsters, dont open UI
        UIManager.i.EnablePartyMenu(true);
        UIManager.i.PartyScene.GetComponent<PartyScript>().partyState = PartyState.OVERWORLD;
    }
    /// <summary>
    /// Opens party UI after interacting a healing item in bag to heal a monster
    /// </summary>
    public void OnClickPartyButtonHEAL(Item item)
    {
        UIManager.i.EnablePartyMenu(true);
        var partyScript = UIManager.i.PartyScene.GetComponent<PartyScript>();
        partyScript.partyState = PartyState.HEAL;
        partyScript.currentUsingItem = item;
        partyScript.healAmount = item.ItemBase.HealAmount;
    }
    /// <summary>
    /// Opens party UI after interacting a move scroll in bag to teach a monster a new move
    /// </summary>
    public void OnClickPartyButtonLearnMove(Item item)
    {
        _learnMoveService.OnClickPartyButtonLearnMove(item);
    }
    public void OnUseScrollToMonster() => _learnMoveService.OnUseScrollToMonster();

    #endregion

    #region Misc
    // Inspector event hooks
    public void AddToSeenMonster()
    {
        if (EnemyUnit?.CurrentMonster?.Base != null)
            MonsterDiaryManager.i.AddMonsterToSeen(EnemyUnit.CurrentMonster.Base);
    }   //  Add monster to seen in monster diary
    public void AddMoney()
    {
        if (_currentTrainer != null)
            PlayerStats.i.Money = PlayerStats.i.Money + _currentTrainer.MoneyReward;
    }   //  Add money to player when defeating a trainer
    public void EnqueueEvolutionCandidate(BattleMonster mon)
    {
        if (mon == null || mon.Base == null) return;
        if (mon.Base.evolution == null) return;

        if (mon.Level == mon.Base.evolution.Level)
            _evolutionService.EnqueueEvolution(mon);
    }   //  Adds monster to evolve in queue to evolve after battle

    #endregion
}
