using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using UnityEngine.Events;

//public enum GameState { OVERWORLD, INBATTLE, FREEZE }

public class BattleSystemImproved : MonoBehaviour
{
    [Header("In Battle Events")]

    [Tooltip("Events playing once battle is initialised. (plays just when battle scene is created)")]
    public UnityEvent OnBattleInitialiseEvent;

    [Tooltip("Events playing once battle has started and all monsters are able to attack/move")]
    public UnityEvent OnBattleStartEvent;

    [Tooltip("Events playing once the enemy Trainer switches monster while in battle")]
    public UnityEvent OnTrainerSwitchMonsterEvent;

    [Tooltip("Events playing once the player switches monsters while in battle")]
    public UnityEvent OnPlayerSwitchedMonsterEvent;

    [Tooltip("Events playing when all player's monsters are dead")]
    public UnityEvent OnBattleEndEvent;

    [Tooltip("Events playing when exp is awarded to player monsters")]
    public UnityEvent OnAwardExpEvent;

    [Tooltip("Events playing once all of enemy Trainer's monsters are dead")]
    public UnityEvent OnDefeatTrainerEvent;

    [Tooltip("Events playing once enemy monster is dead")]
    public UnityEvent OnDefeatWildMonsterEvent;

    [Header("General Info")]
    [SerializeField] Animator battleAnimations;
    public GameObject BattleArena;
    public GameObject BattlePosition;
    public GameObject BattleScenePrefab; // (kept, not used directly)
    public Vector2 PlayerBattleStartPos;
    public Vector2 EnemyBattleStartPos;
    [SerializeField] GameObject playerObject;

    public GameState _gameState { get; set; }

    [SerializeField] EvolutionAnimation evolutionHandler;
    public GameObject BattleScene;
    public GameObject RadialBattleTransition;
    public GameObject BattleEndTransition;
    [SerializeField] GameObject partyButton;

    [Header("HUD")]
    [SerializeField] TextMeshProUGUI PlayerMonsterName;
    [SerializeField] TextMeshProUGUI PlayerMonsterLevel;
    [SerializeField] TextMeshProUGUI EnemyMonsterName;
    [SerializeField] TextMeshProUGUI EnemyMonsterLevel;

    [Header("Units")]
    public BattleUnit PlayerUnit;
    public BattleUnit EnemyUnit;

    [Header("Capsules")]
    [SerializeField] GameObject capsule;
    [SerializeField] GameObject capsuleShadow;
    [SerializeField] float capsuleTimer;
    [SerializeField] GameObject trainer_capsule;
    [SerializeField] GameObject trainer_capsuleShadow;

    [Header("Battle start")]
    [SerializeField] float startTimer = 2f;
    [SerializeField] List<GameObject> movesUI;
    public GameObject moveAnimationHolder;

    // state
    public bool interruptBattle;
    bool isAnimating;
    bool battleEnd;
    float timer { get; set; }
    bool alreadyInitialisedMoves { get; set; }
    bool awardExp;

    // track awards in an array (index: 0..4)
    readonly bool[] awarded = new bool[5];

    [Header("Battle Effects")]
    [SerializeField] float clashKBStrength = 10f;
    [SerializeField] float clashKBTime = 0.35f;
    [SerializeField] GameObject dashParticles;

    [Header("Animations")]
    [SerializeField] Vector2 BattleStartAnimationPosition;
    [SerializeField] GameObject BattleStartAnimationOB;
    [SerializeField] GameObject UIArrow;

    [Header("Party")]
    [SerializeField] float partyswitchCooldown = 4f;
    public bool partyMemberSwitched = false;
    public bool canSwitch = true;
    float partyTimer;

    [Header("Trainer Battles")]
    public static int enemyMemberToSwitch;
    public BattleTrainer currentTrainer;
    [SerializeField] bool isTrainerBattle;
    [SerializeField] GameObject trainerObject;
    bool enemyMemberSwitched;
    bool playerSwitchAbilityPending;
    bool enemySwitchAbilityPending;

    [Header("After Battle")]
    [SerializeField] GameObject LevelUPUI;
    [SerializeField] TextMeshProUGUI Monster1Name;
    [SerializeField] TextMeshProUGUI Monster1Level;
    [SerializeField] Image Monster1Sprite;
    [SerializeField] Slider Monster1ExpBar;
    [SerializeField] GameObject Monster1LvlUpIndicator;

    [SerializeField] TextMeshProUGUI Monster2Name;
    [SerializeField] TextMeshProUGUI Monster2Level;
    [SerializeField] Image Monster2Sprite;
    [SerializeField] Slider Monster2ExpBar;
    [SerializeField] GameObject Monster2LvlUpIndicator;

    [SerializeField] TextMeshProUGUI Monster3Name;
    [SerializeField] TextMeshProUGUI Monster3Level;
    [SerializeField] Image Monster3Sprite;
    [SerializeField] Slider Monster3ExpBar;
    [SerializeField] GameObject Monster3LvlUpIndicator;

    [SerializeField] TextMeshProUGUI Monster4Name;
    [SerializeField] TextMeshProUGUI Monster4Level;
    [SerializeField] Image Monster4Sprite;
    [SerializeField] Slider Monster4ExpBar;
    [SerializeField] GameObject Monster4LvlUpIndicator;

    [SerializeField] TextMeshProUGUI Monster5Name;
    [SerializeField] TextMeshProUGUI Monster5Level;
    [SerializeField] Image Monster5Sprite;
    [SerializeField] Slider Monster5ExpBar;
    [SerializeField] GameObject Monster5LvlUpIndicator;

    [SerializeField] GameObject LvlUpGameObject; // (kept)
    [SerializeField] TextMeshProUGUI LevelUpTextContinue;

    bool canPressLevelUpUIButton { get; set; }
    bool battleStartCooldown { get; set; }
    [SerializeField] float resetBattleStartTimer = 0.5f;

    [Header("Learn Move UI")]
    [SerializeField] GameObject LearnMoveUI;
    [SerializeField] TextMeshProUGUI LearnMoveMonsterName;
    [SerializeField] TextMeshProUGUI LearnMoveMonsterLvl;
    [SerializeField] Image LearnMoveMonsterElement1;
    [SerializeField] Image LearnMoveMonsterElement2;
    [SerializeField] TextMeshProUGUI LearnMove1Name;
    [SerializeField] TextMeshProUGUI LearnMove2Name;
    [SerializeField] TextMeshProUGUI LearnMove3Name;
    [SerializeField] TextMeshProUGUI LearnMove4Name;
    [SerializeField] TextMeshProUGUI LearningMoveName;
    [SerializeField] TextMeshProUGUI LearnMove1cost;
    [SerializeField] TextMeshProUGUI LearnMove2cost;
    [SerializeField] TextMeshProUGUI LearnMove3cost;
    [SerializeField] TextMeshProUGUI LearnMove4cost;
    [SerializeField] Image LearnMove1element;
    [SerializeField] Image LearnMove2element;
    [SerializeField] Image LearnMove3element;
    [SerializeField] Image LearnMove4element;
    [SerializeField] TextMeshProUGUI LearningMovecost;
    [SerializeField] Image LearningMoveElement;
    [SerializeField] Image LearningMonSprite;
    public BattleMove LearningMove;
    public Queue<BattleMonster> currentEvolvingMonsters = new Queue<BattleMonster>();
    [SerializeField] BattleMonster currentLearningMonster;

    bool changedMove;
    bool openedUI;
    bool triggerOnce;

    [Header("Dialogue")]
    [TextArea]
    [SerializeField] List<string> defeatMonsterText = new List<string>();

    // HUD cache to avoid per-frame updates
    string _cachedPlayerName, _cachedEnemyName;
    int _cachedPlayerLevel, _cachedEnemyLevel;

    void Start() => BattleInitialise();

    public void BattleInitialise()
    {
        isAnimating = true;

        // Start animation object
        var startPrefab = GameUtils.i.player.GetComponent<PlayerController>().CurrentArea.BattleStartAnimationObject;
        if (startPrefab != null && BattleScene != null)
        {
            BattleStartAnimationOB = Instantiate(startPrefab, BattleStartAnimationPosition, Quaternion.identity, BattleScene.transform);
            BattleStartAnimationOB.SetActive(true);
        }
    }

    void Update()
    {
        switch (_gameState)
        {
            case GameState.OVERWORLD:
                BattleScene?.SetActive(false);
                if (BattleArena != null)
                {
                    Destroy(BattleArena);
                    BattleArena = null;
                }
                break;

            case GameState.INBATTLE:
                Battle();
                break;

            case GameState.FREEZE:
                // intentionally empty
                break;
        }
    }

    public void InterruptBattle(bool val) => interruptBattle = val;
    public float GetClashKBStrength() => clashKBStrength;
    public float GetClashKBTime() => clashKBTime;
    public GameObject GetDashParticle() => dashParticles;

    // Inspector event hooks
    public void AddToSeenMonster()
    {
        if (EnemyUnit?.CurrentMonster?.Base != null)
            MonsterDiaryManager.i.AddMonsterToSeen(EnemyUnit.CurrentMonster.Base);
    }
    public void AddMoney()
    {
        if (currentTrainer != null)
            PlayerStats.i.Money = PlayerStats.i.Money + currentTrainer.MoneyReward;
    }

    void Battle()
    {
        // Update HUD only if changed
        RefreshHudOncePerChange();

        BattleEnd();

        if (isAnimating) return;

        if (timer <= 1f)
        {
            BattleActive();
        }
        else
        {
            interruptBattle = true;
            timer -= Time.deltaTime;
            if (timer <= 1f)
                OnBattleStartEvent?.Invoke();
        }
    }

    void FixedUpdate()
    {
        if (_gameState != GameState.INBATTLE || battleEnd) return;

        if (!interruptBattle)
        {
            PlayerUnit?.IncreaseStamina();
            EnemyUnit?.IncreaseStamina();
        }
    }

    void BattleActive()
    {
        if (battleEnd) // check end first
        {
            if (Input.GetButtonDown("Interact") && canPressLevelUpUIButton)
                DisableLevelUpUI();
            return;
        }

        if (!interruptBattle)
        {
            PlayerUnit.DisableAttacking = false;
            EnemyUnit.DisableAttacking = false;

            partyButton?.SetActive(true);

            ChangeActiveMove();

            if (Input.GetButton("Attack")) MoveAttack();
            if (Input.GetButton("PartyButton")) OnClickPartyButton(false);

            partyCooldown();
        }
        else
        {
            PlayerUnit.DisableAttacking = true;
            EnemyUnit.DisableAttacking = true;
            partyButton?.SetActive(false);
        }

        CheckForPartyMemberSwitched();
        checkForPlayerSwitchOnDeath();

        // mark exp award if enemy died (player alive)
        if (EnemyUnit != null && EnemyUnit.IsDead && PlayerUnit != null && !PlayerUnit.IsDead)
            awardExp = true;
    }

    void RefreshHudOncePerChange()
    {
        if (EnemyUnit?.CurrentMonster != null)
        {
            var name = EnemyUnit.CurrentMonster.Base?.Name ?? "";
            var lvl = EnemyUnit.CurrentMonster.Level;

            if (_cachedEnemyName != name) { _cachedEnemyName = name; EnemyMonsterName.text = name; }
            if (_cachedEnemyLevel != lvl) { _cachedEnemyLevel = lvl; EnemyMonsterLevel.text = "Lvl: " + lvl; }
        }

        if (PlayerUnit?.CurrentMonster != null)
        {
            var name = PlayerUnit.CurrentMonster.Base?.Name ?? "";
            var lvl = PlayerUnit.CurrentMonster.Level;

            if (_cachedPlayerName != name) { _cachedPlayerName = name; PlayerMonsterName.text = name; }
            if (_cachedPlayerLevel != lvl) { _cachedPlayerLevel = lvl; PlayerMonsterLevel.text = "Lvl: " + lvl; }
        }
    }

    // end-of-battle orchestration
    void BattleEnd()
    {
        if (!battleEnd) return;

        if (PlayerUnit != null && PlayerUnit.IsDead)
            OnBattleEndEvent?.Invoke();

        if (awardExp)
        {
            OnAwardExpEvent?.Invoke();
            awardExp = false;
        }

        // all awards done?
        if (AllAwardsComplete())
        {
            CheckForLearningMoves();
            if (!changedMove) return;

            bool noMovesToLearn = TrueIfNoPendingLearnAcrossParty();
            if (noMovesToLearn)
            {
                canPressLevelUpUIButton = true;
                LevelUpTextContinue?.gameObject.SetActive(true);
            }
        }
    }

    bool AllAwardsComplete()
    {
        for (int i = 0; i < awarded.Length; i++)
            if (!awarded[i]) return false;
        return true;
    }

    bool TrueIfNoPendingLearnAcrossParty()
    {
        // checks members 0..4 if present
        var party = PlayerStats.i.PlayerParty;
        for (int i = 0; i < 5; i++)
        {
            if (i < party.Count)
            {
                if (party[i].CurrentLearningMoves.Count > 0)
                    return false;
            }
        }
        return true;
    }

    public void DisableLevelUpUI() => StartCoroutine(closeLevelUpUI());

    IEnumerator closeLevelUpUI()
    {
        if (LevelUPUI != null)
            LevelUPUI.GetComponent<Animator>().SetTrigger("CloseMenu");

        canPressLevelUpUIButton = false;
        if (LevelUpTextContinue != null) LevelUpTextContinue.gameObject.SetActive(false);
        alreadyInitialisedMoves = false;
        InitialiseMoves();

        yield return new WaitForSeconds(0.2f);

        // hide indicators
        if (Monster1LvlUpIndicator) Monster1LvlUpIndicator.SetActive(false);
        if (Monster2LvlUpIndicator) Monster2LvlUpIndicator.SetActive(false);
        if (Monster3LvlUpIndicator) Monster3LvlUpIndicator.SetActive(false);
        if (Monster4LvlUpIndicator) Monster4LvlUpIndicator.SetActive(false);
        if (Monster5LvlUpIndicator) Monster5LvlUpIndicator.SetActive(false);

        if (LevelUPUI) LevelUPUI.SetActive(false);

        // reset award trackers (except index 0 which is awarded by first call)
        for (int i = 1; i < awarded.Length; i++) awarded[i] = false;

        canPressLevelUpUIButton = false;
        CheckTrainerMonstersLeft();
    }

    public void AwardExp()
    {
        partyButton?.SetActive(false);
        LevelUPUI?.SetActive(true);

        var party = PlayerStats.i.PlayerParty;
        if (party.Count == 0) return;

        // main (member 1)
        party[0].GainEXP((EnemyUnit.CurrentMonster.MaxHP) * EnemyUnit.CurrentMonster.Level);
        StartCoroutine(CheckForLevelUP(party[0], Monster1ExpBar, Monster1Level, Monster1LvlUpIndicator, 1));
        Monster1Name.text = party[0].Base.Name;
        Monster1Sprite.sprite = party[0].Base.frontSprite;

        // the rest: 2..5
        AwardEXP(2, Monster2ExpBar, Monster2Level, Monster2Name, Monster2Sprite, Monster2LvlUpIndicator, ref awarded[1]);
        AwardEXP(3, Monster3ExpBar, Monster3Level, Monster3Name, Monster3Sprite, Monster3LvlUpIndicator, ref awarded[2]);
        AwardEXP(4, Monster4ExpBar, Monster4Level, Monster4Name, Monster4Sprite, Monster4LvlUpIndicator, ref awarded[3]);
        AwardEXP(5, Monster5ExpBar, Monster5Level, Monster5Name, Monster5Sprite, Monster5LvlUpIndicator, ref awarded[4]);
    }

    public void EndBattle()
    {
        PlayerUnit?.StopAllCoroutines();
        EnemyUnit?.StopAllCoroutines();
        currentTrainer?.BattleMusic.Stop();
        currentTrainer.IsDefeated = true;
        currentTrainer?.ResetBattle();
        currentTrainer = null;

        partyButton?.SetActive(false);
        UIManager.i.PartyScene.SetActive(false);
        _gameState = GameState.OVERWORLD;
        triggerOnce = false;
        if (trainerObject) trainerObject.SetActive(false);
        if (playerObject) playerObject.SetActive(false);
        isTrainerBattle = false;

        StartCoroutine(ResetBattleStartCooldown());
        ResetBattle();
    }

    IEnumerator ResetBattleStartCooldown()
    {
        battleStartCooldown = true;
        yield return new WaitForSeconds(resetBattleStartTimer);
        battleStartCooldown = false;
    }

    public bool GetBattleStartCooldown() => battleStartCooldown;

    public void TriggerWildBattle()
    {
        SpawnStartAnim();

        _gameState = GameState.INBATTLE;
        OnBattleInitialiseEvent?.Invoke();

        PlayerUnit.ResetBattleUnit();
        PlayerUnit.gameObject.SetActive(false);

        EnemyUnit.gameObject.SetActive(true);
        EnemyUnit.ResetBattleUnit();
        EnemyUnit.StartBattleUnit();

        InitialiseBattleMonster();
        ResetSwitchAbilityApplications();

        EnsureBattleArenaForCurrentArea();

        if (playerObject) playerObject.SetActive(true);
        BattleScene?.SetActive(true);
        BattleArena?.SetActive(true);

        if (capsule) capsule.SetActive(true);
        if (trainerObject) trainerObject.SetActive(false);
        if (trainer_capsule) trainer_capsule.SetActive(false);
        if (trainer_capsuleShadow) trainer_capsuleShadow.SetActive(false);

        changedMove = false;
    }

    public void TriggerTrainerBattle()
    {
        if (trainer_capsule) trainer_capsule.SetActive(true);
        if (trainer_capsuleShadow) trainer_capsuleShadow.SetActive(true);

        SpawnStartAnim();

        _gameState = GameState.INBATTLE;
        isTrainerBattle = true;

        PlayerUnit.ResetBattleUnit();
        PlayerUnit.gameObject.SetActive(false);

        EnemyUnit.ResetBattleUnit();
        if (currentTrainer != null && currentTrainer.TrainerParty.Count > 0)
            EnemyUnit.CurrentMonster = currentTrainer.TrainerParty[0];
        EnemyUnit.StartBattleUnit();
        EnemyUnit.gameObject.SetActive(false);

        InitialiseBattleMonster();
        ResetSwitchAbilityApplications();
        EnsureBattleArenaForCurrentArea();

        if (playerObject) playerObject.SetActive(true);
        if (trainerObject)
        {
            trainerObject.SetActive(true);
            var sr = trainerObject.GetComponent<SpriteRenderer>();
            if (sr) sr.sprite = currentTrainer?.InBattleTrainerSprite;
        }

        BattleArena?.SetActive(true);
        BattleScene?.SetActive(true);
        if (capsule) capsule.SetActive(true);

        changedMove = false;
        AnimateTrainer();
        OnBattleInitialiseEvent?.Invoke();
    }

    void SpawnStartAnim()
    {
        var startPrefab = GameUtils.i.player.GetComponent<PlayerController>().CurrentArea.BattleStartAnimationObject;
        if (startPrefab != null && BattleScene != null)
        {
            BattleStartAnimationOB = Instantiate(startPrefab, BattleStartAnimationPosition, Quaternion.identity, BattleScene.transform);
            // ensure UI position (if rect transform exists)
            var rt = BattleStartAnimationOB.GetComponent<RectTransform>();
            if (rt) rt.localPosition = BattleStartAnimationPosition;
            BattleStartAnimationOB.SetActive(true);
        }
    }

    void EnsureBattleArenaForCurrentArea()
    {
        var desired = GameUtils.i.player.GetComponent<PlayerController>().CurrentArea.BattleArenaPrefab;
        if (desired == null) return;

        // If already using the same prefab, keep it; else rebuild
        if (BattleArena == null || BattleArena.name.Replace("(Clone)", "") != desired.name)
        {
            if (BattleArena != null) Destroy(BattleArena);
            BattleArena = Instantiate(desired, BattlePosition.transform.position, Quaternion.identity, BattlePosition.transform);
        }
    }

    void ResetBattle()
    {
        if (BattleStartAnimationOB != null)
        {
            Destroy(BattleStartAnimationOB);
            BattleStartAnimationOB = null;
        }

        ResetSwitchBattleVariables();
        if (PlayerUnit) PlayerUnit.transform.position = PlayerBattleStartPos;

        canSwitch = true;
        partyMemberSwitched = false;
        partyTimer = 0f;
    }

    void ResetSwitchBattleVariables()
    {
        if (EnemyUnit) EnemyUnit.transform.position = EnemyBattleStartPos;
        battleEnd = false;
        awardExp = false;
        for (int i = 0; i < awarded.Length; i++) awarded[i] = false;

        InterruptBattle(false);
        isAnimating = true;
        timer = 0f;
        alreadyInitialisedMoves = false;

        if (LevelUPUI) LevelUPUI.SetActive(false);
    }

    void ResetSwitchAbilityApplications()
    {
        PlayerUnit?.ResetSwitchAbilityApplications();
        EnemyUnit?.ResetSwitchAbilityApplications();
        playerSwitchAbilityPending = false;
        enemySwitchAbilityPending = false;
    }

    public bool checkForBattleEnd() => battleEnd;

    // Player monster spawn
    public void InitialiseBattleMonster()
    {
        if (PlayerUnit == null) return;

        PlayerUnit.Stamina = 0f;
        GameUtils.i.getAliveMonster(); // side-effects retained
        alreadyInitialisedMoves = false;

        PlayerUnit.updateHealth();

        InitialiseMoves();
        PlayerUnit.StartBattleUnit();

        if (capsule != null && PlayerUnit.CurrentMonster?.CapsuleCaught != null)
        {
            var sr = capsule.GetComponentInChildren<SpriteRenderer>();
            if (sr) sr.sprite = PlayerUnit.CurrentMonster.CapsuleCaught.Capsule_Sprite;
        }
    }

    public void MoveAttack()
    {
        if (EnemyUnit == null || PlayerUnit == null) return;
        if (EnemyUnit.IsDead|| PlayerUnit.IsDead) return;
        if (interruptBattle) return;

        var move = PlayerUnit.CurrentMove;
        if (move == null) return;

        if (PlayerUnit.Stamina < move.StaminaCost) return;
        if (PlayerUnit.IsAttacking) return;
        if (PlayerUnit.IsInKnockbackState) return;

        PlayerUnit.IsAttacking = true;
        float damage = PlayerUnit.CurrentMonster.CalculateMoveStats(move);
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
        GameUtils.i.PlayAnimationFromMove(move, moveAnimationHolder, PlayerUnit, EnemyUnit, false);

        PlayerUnit.Stamina -= move.StaminaCost;
        PlayerUnit.StartCoroutine(PlayerUnit.resetAttackState());
    }

    // Animations (unchanged behavior, with guards)
    public void ThrowCapsules()
    {
        if (battleAnimations == null) return;

        battleAnimations.SetBool("WildEnemyMonster", true);
        if (!enemyMemberSwitched) ThrowPlayerCapsule();
        if (isTrainerBattle && !partyMemberSwitched) ThrowEnemyCapsule();
    }
    public void ThrowPlayerCapsule()
    {
        battleAnimations?.SetBool("ThrowPlayerMonster", true);
        AudioSystem.i.PlaySoundClip(AudioSystem.i.ThrowCatchingObjectSound);
    }
    public void ThrowEnemyCapsule()
    {
        battleAnimations?.SetBool("ThrowEnemyMonster", true);
        AudioSystem.i.PlaySoundClip(AudioSystem.i.ThrowCatchingObjectSound);
    }
    public void OnPlayerCapsuleAnimationEnd()
    {
        battleAnimations?.SetBool("ThrowPlayerMonster", false);
        if (capsule) capsule.SetActive(false);
        if (PlayerUnit != null)
        {
            PlayerUnit.gameObject.SetActive(true);
            var hb = PlayerUnit.HealthBar;
            if (hb) hb.transform.parent.gameObject.SetActive(true);

            InitialiseBattleMonster();
            if (playerSwitchAbilityPending)
            {
                PlayerUnit.TriggerSwitchedInAbilityOnce();
                EnemyUnit?.TriggerEnemySwitchedAbilityOnce(PlayerUnit);
                playerSwitchAbilityPending = false;
            }

            var particle = PlayerUnit.CurrentMonster?.CapsuleCaught?.Capsule_Particle;
            if (particle != null)
            {
                var ob = Instantiate(particle.gameObject);
                ob.transform.position = PlayerUnit.transform.position;
            }
        }
    }
    public void OnEnemyCapsuleAnimationEnd()
    {
        battleAnimations?.SetBool("ThrowEnemyMonster", false);
        if (trainer_capsule) trainer_capsule.SetActive(false);

        if (EnemyUnit != null)
        {
            EnemyUnit.gameObject.SetActive(true);
            var hb = EnemyUnit.HealthBar;
            if (hb) hb.transform.parent.gameObject.SetActive(true);

            var particle = EnemyUnit.CurrentMonster?.CapsuleCaught?.Capsule_Particle;
            if (particle != null)
            {
                var ob = Instantiate(particle.gameObject);
                ob.transform.position = EnemyUnit.transform.position;
            }
            if (enemySwitchAbilityPending)
            {
                EnemyUnit.TriggerSwitchedInAbilityOnce();
                PlayerUnit?.TriggerEnemySwitchedAbilityOnce(EnemyUnit);
                enemySwitchAbilityPending = false;
            }
        }
    }
    public void OnWildEnemyAnimationEnd()
    {
        if (!isTrainerBattle && EnemyUnit != null)
        {
            EnemyUnit.gameObject.SetActive(true);
            var hb = EnemyUnit.HealthBar;
            if (hb) hb.transform.parent.gameObject.SetActive(true);
        }
        battleAnimations?.SetBool("WildEnemyMonster", false);
    }
    public void OnBothCapsuleAnimationsEnd()
    {
        isAnimating = false;
        timer = startTimer;
    }
    void playBattleEndTransition() => BattleEndTransition?.SetActive(true);
    public void AnimateTrainer() { if (trainer_capsule) trainer_capsule.SetActive(true); }
    public void attackAnimation(BattleUnit ob)
    {
        if (ob != null) ob.transform.DOJump(ob.StartPosition, 15, 1, .2f);
    }

    // Switching / death flow
    void checkForPlayerSwitchOnDeath()
    {
        if (EnemyUnit != null && EnemyUnit.IsDead)
        {
            battleEnd = true;
        }

        if (PlayerUnit != null && PlayerUnit.IsDead)
        {
            if (!SearchForAliveMonsterInParty(PlayerStats.i.PlayerParty))
            {
                // No monsters left
                battleEnd = true;
            }
            else
            {
                if (!openedUI)
                {
                    openedUI = true;
                    PlayerUnit.IsImmune = true;
                    StartCoroutine(DisplayPartyUI(false));
                }
            }
        }
    }

    public bool SearchForAliveMonsterInParty(List<BattleMonster> party)
    {
        for (int i = 0; i < party.Count; i++)
            if (party[i].HP > 0) return true;
        return false;
    }

    public int CountDeadMonsterInParty(List<BattleMonster> party)
    {
        int count = 0;
        for (int i = 0; i < party.Count; i++)
            if (party[i].HP <= 0) count++;
        return count;
    }

    public static int GetNextAliveMonsterSlot(List<BattleMonster> party, int startPos)
    {
        for (int i = Mathf.Max(0, startPos); i < party.Count; i++)
            if (party[i].HP > 0) return i;
        return 0;
    }

    public static BattleMonster GetNextAliveMonster(List<BattleMonster> party, int startPos, bool forEnemy)
    {
        var idx = GetNextAliveMonsterSlot(party, startPos);
        if (forEnemy) enemyMemberToSwitch = idx;
        return party[Mathf.Clamp(idx, 0, party.Count - 1)];
    }

    public void CheckForPartyMemberSwitched()
    {
        if (!partyMemberSwitched) return;

        partyTimer = partyswitchCooldown + 1f;
        canSwitch = false;

        PlayerUnit.IsDead = false;
        PlayerUnit?.StopAllCoroutines();
        if (PlayerUnit != null) PlayerUnit.gameObject.SetActive(false);

        if (capsule) capsule.SetActive(true);
        isAnimating = true;

        foreach (var item in UIManager.i.PartyScene.GetComponent<PartyScript>().members)
            item.GetComponent<PartyMember>().InitialiseMoves();

        UIManager.i.EnablePartyMenu(false);
        playerSwitchAbilityPending = true;
        OnPlayerSwitchedMonsterEvent?.Invoke();
        partyMemberSwitched = false;
        PlayerUnit.IsImmune = false;
    }

    public void CheckForEnemyPartyMemberSwitched()
    {
        if (!enemyMemberSwitched) return;

        interruptBattle = true;
        EnemyUnit.IsDead = false;
        EnemyUnit?.StopAllCoroutines();
        if (EnemyUnit != null) EnemyUnit.gameObject.SetActive(false);

        if (currentTrainer != null && EnemyUnit != null)
        {
            BattleMonster currentmon = EnemyUnit.CurrentMonster;
            currentTrainer.TrainerParty[0] = GetNextAliveMonster(currentTrainer.TrainerParty, 0, true);
            EnemyUnit.CurrentMonster = currentTrainer.TrainerParty[0];
            EnemyUnit.ResetBattleUnit();
            currentTrainer.TrainerParty[enemyMemberToSwitch] = currentmon;
            EnemyUnit.StartBattleUnit();

            if (trainer_capsule) trainer_capsule.SetActive(true);
            EnemyUnit.CurrentMonster.Initialise();

            var sr = trainer_capsule?.GetComponentInChildren<SpriteRenderer>();
            if (sr && EnemyUnit.CurrentMonster.CapsuleCaught != null)
                sr.sprite = EnemyUnit.CurrentMonster.CapsuleCaught.Capsule_Sprite;

            ResetSwitchBattleVariables();
            enemySwitchAbilityPending = true;
            OnTrainerSwitchMonsterEvent?.Invoke();
            enemyMemberSwitched = false;
        }
    }


    //Checks if trainer has any monsters left in his party and play
    //certain events like switching to other monsters/ defeat trainer
    //or defeated wild monster
    public void CheckTrainerMonstersLeft()
    {
        //if enemy unit is still alive dont continue
        if (EnemyUnit == null || !EnemyUnit.IsDead) return;

        //if is in trainer battle
        if (isTrainerBattle)
        {
            //enemy has more than 1 alive monsters left so switch to that monster
            if (SearchForAliveMonsterInParty(currentTrainer.TrainerParty))
            {
                enemyMemberSwitched = true;
                CheckForEnemyPartyMemberSwitched();
                interruptBattle = true;
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

    // Evolution / learning flow
    public bool CheckForEvolvingMonsters() => currentEvolvingMonsters.Count > 0;

    public void CheckForLearningMoves()
    {
        changedMove = false;
        var party = PlayerStats.i.PlayerParty;

        // Check each party member in order; show first learning UI found, else mark changedMove
        for (int i = 0; i < Mathf.Min(5, party.Count); i++)
        {
            var mon = party[i];
            if (mon == null) continue;

            // Queue up any new move(s)
            bool queued = mon.LearnNewMove();
            if (queued || mon.CurrentLearningMoves.Count > 0)
            {
                LearnNewMove(mon, mon.CurrentLearningMoves.Peek());
                return; // we open UI for the first case found
            }
        }

        // no learning UI opened
        changedMove = true;
    }

    public void LearnNewMove(BattleMonster mon, BattleMove move)
    {
        if (!mon.SearchCurrentMove(move))
        {
            if (mon.CurrentMoves.Count >= 4)
            {
                currentLearningMonster = mon;
                LearningMove = move;
                EnableLearningMoveUI();
            }
            else
            {
                mon.AddCurrentMoves(move);
                GameUtils.i.dialogueSystem.InitialiseNewLearnMove(move.Base.Name, mon.Base.Name, OnUseScrollToMonster);
            }
        }
    }

    //checks if there are current learning moves in queue and if there are
    //initialise learn move UI to learn move to current learning monster
    public void CheckInQueueForMoves()
    {
        if (currentLearningMonster != null && currentLearningMonster.CurrentLearningMoves.Count > 0)
        {
            LearningMove = currentLearningMonster.CurrentLearningMoves.Peek();
            EnableLearningMoveUI();
        }
    }


    //Handles Evolving monster logic
    public void EvolveMonster()
    {
        if (CheckForEvolvingMonsters())//checks if there is a monster ready to evolve
        {
           // if (!evolutionHandler.IsAnimating())//initialise new monster evolve animation when not current evolve active
             //   evolutionHandler.InitialiseAnimation(currentEvolvingMonsters.Peek());
           // return;
        }

        if (!changedMove) return;

        //triggers animation only once
        if (!triggerOnce)
        {
            triggerOnce = true;

            //check if battle is not a trainer battle
            if (!isTrainerBattle)
            {
                if (!BagManager.i.SearchForCatchingDisksInBag())//no catching disk available, move to end battle dialogue
                    DisplayEndBattleDialogue(false);
                else
                    CatchingManager.i.InitialiseCatchingSystem();//catching disk available, initialise catching system
            }
            else // if it is a trainer battle display end battle dialogue
            {
                DisplayEndBattleDialogue(false);
                isTrainerBattle = false;
            }
        }
    }

    public void DisplayEndBattleDialogue(bool caught)
    {
        UnityEvent events = new UnityEvent();
        events.AddListener(playBattleEndTransition);

        if (isTrainerBattle)
        {
            GameUtils.i.dialogueSystem.InitialiseNewTextNPC(
                currentTrainer.TrainerDefeatDialogue,
                currentTrainer.GetComponent<NPC_Stats>().NPCName,
                currentTrainer.GetComponent<NPC_Stats>().NPCDialogueSprite,
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

    void AwardEXP(int memberNum, Slider MonsterExpBar, TextMeshProUGUI MonsterLevel, TextMeshProUGUI MonsterName, Image MonsterSprite, GameObject lvlUpIndicator, ref bool awardedVar)
    {
        var party = PlayerStats.i.PlayerParty;
        if (party.Count >= memberNum)
        {
            MonsterName.transform.parent.gameObject.SetActive(true);
            var mon = party[memberNum - 1];
            mon.GainEXP(((EnemyUnit.CurrentMonster.MaxHP) * EnemyUnit.CurrentMonster.Level) / 5f);
            StartCoroutine(CheckForLevelUP(mon, MonsterExpBar, MonsterLevel, lvlUpIndicator, memberNum));
            MonsterName.text = mon.Base.Name;
            MonsterSprite.sprite = mon.Base.frontSprite;
        }
        else
        {
            MonsterName.transform.parent.gameObject.SetActive(false);
            awardedVar = true;
        }
    }

    public IEnumerator CheckForLevelUP(BattleMonster mon, Slider expBar, TextMeshProUGUI level, GameObject LvlUpIndicator, int awardedIndex)
    {
        if (mon == null) yield break;

        level.text = "Level: " + mon.Level;

        // Use mon.ExpToNextLevel() if available; otherwise keep the original formula as float math
        float need = (100f * mon.Level * (1f + (mon.Level / 100f))) * mon.getExpGroupMultiplier();
        expBar.maxValue = need;

        expBar.DOValue(Mathf.Min(mon.Exp, need), 1f);

        if (mon.Exp >= need)
        {
            yield return new WaitForSeconds(1f);
            mon.Exp -= need;
            mon.Level += 1;
            LvlUpIndicator.SetActive(true);

            // enqueue evolution check
            if (mon.Level == mon.Base.evolution.Level)
                currentEvolvingMonsters.Enqueue(mon);

            expBar.value = 0f;
            StartCoroutine(CheckForLevelUP(mon, expBar, level, LvlUpIndicator, awardedIndex));
        }
        else
        {
            yield return new WaitForSeconds(1f);

            // mark award complete for this slot (1..5 maps to 0..4)
            int idx = Mathf.Clamp(awardedIndex - 1, 0, awarded.Length - 1);
            awarded[idx] = true;
        }
    }

    public void InitialiseMoves()
    {
        if (alreadyInitialisedMoves || PlayerUnit?.CurrentMonster == null) return;

        // disable all
        for (int j = 0; j < movesUI.Count; j++)
            movesUI[j].SetActive(false);

        // enable up to 4
        int show = Mathf.Min(4, Mathf.Min(movesUI.Count, PlayerUnit.CurrentMonster.CurrentMoves.Count));
        for (int i = 0; i < show; i++)
        {
            var cell = movesUI[i];
            cell.SetActive(true);
            var init = cell.GetComponent<MoveInitialiser>();
            if (init != null)
            {
                init.Move = PlayerUnit.CurrentMonster.CurrentMoves[i];
                init.InitialiseMove();
            }
        }

        SelectMove(0);
        alreadyInitialisedMoves = true;
    }

    int currentMoveNumber = 0;
    public int GetCurrentMoveNumber() => currentMoveNumber;

    public void ChangeActiveMove()
    {
        bool changed = false;

        if (Input.mouseScrollDelta.y < 0 || Input.GetButtonDown("ChangeMoveDown"))
        {
            if (PlayerUnit?.CurrentMonster == null) return;
            currentMoveNumber = (currentMoveNumber + 1) % Mathf.Max(1, PlayerUnit.CurrentMonster.CurrentMoves.Count);
            changed = true;
        }
        else if (Input.mouseScrollDelta.y > 0 || Input.GetButtonDown("ChangeMoveUp"))
        {
            if (PlayerUnit?.CurrentMonster == null) return;
            currentMoveNumber = (currentMoveNumber - 1 + PlayerUnit.CurrentMonster.CurrentMoves.Count) % Mathf.Max(1, PlayerUnit.CurrentMonster.CurrentMoves.Count);
            changed = true;
        }

        if (Input.GetKey(KeyCode.Alpha1)) { currentMoveNumber = 0; changed = true; }
        else if (Input.GetKey(KeyCode.Alpha2)) { currentMoveNumber = 1; changed = true; }
        else if (Input.GetKey(KeyCode.Alpha3)) { currentMoveNumber = 2; changed = true; }
        else if (Input.GetKey(KeyCode.Alpha4)) { currentMoveNumber = 3; changed = true; }

        if (changed) SelectMove(currentMoveNumber);
    }

    void SelectMove(int number)
    {
        if (PlayerUnit?.CurrentMonster == null) return;
        if (number < 0 || number >= PlayerUnit.CurrentMonster.CurrentMoves.Count) return;

        PlayerUnit.CurrentMove = PlayerUnit.CurrentMonster.CurrentMoves[number];

        // reset label colors safely
        for (int i = 0; i < Mathf.Min(4, movesUI.Count); i++)
        {
            var txt = movesUI[i].GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null) txt.DOColor(Color.white, 0.2f);
        }

        // highlight selected
        if (number < movesUI.Count)
        {
            var txt = movesUI[number].GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null) txt.DOColor(Color.green, 0.2f);

            if (UIArrow != null)
            {
                UIArrow.transform.SetParent(movesUI[number].transform, worldPositionStays: true);
                var pos = UIArrow.transform.position;
                UIArrow.transform.position = new Vector2(pos.x, movesUI[number].transform.position.y);
            }
        }

        currentMoveNumber = number;
    }

    // UI timers
    public void InitialiseTimer(int time) { timer = time; }
    public void InitialiseTimer() { timer = startTimer; }

    IEnumerator DisplayPartyUI(bool forceBattleEnd)
    {
        yield return new WaitForSeconds(1.5f);
        PlayerUnit.IsDead = false;

        if (forceBattleEnd)
        {
            battleEnd = true;
        }
        else
        {
            OnClickPartyButton(true);
            openedUI = false;
        }
    }

    void EnableLearnMove(int movepos, TextMeshProUGUI name, TextMeshProUGUI cost, Image element)
    {
        if (currentLearningMonster == null || name == null || cost == null || element == null) return;

        if (movepos <= currentLearningMonster.CurrentMoves.Count - 1)
        {
            var mv = currentLearningMonster.CurrentMoves[movepos];
            name.text = mv.Base.Name;
            cost.text = mv.StaminaCost.ToString();
            element.sprite = mv.Base.ElementType.icon;
        }
        else
        {
            name.text = " ";
            cost.text = " ";
            element.sprite = null;
        }
    }

    public void EnableLearningMoveUI()
    {
        if (currentLearningMonster == null || LearningMove == null) return;

        LearningMonSprite.sprite = currentLearningMonster.Base.frontSprite;
        LearnMoveMonsterName.text = currentLearningMonster.Base.Name;
        LearnMoveMonsterLvl.text = currentLearningMonster.Level.ToString();
        LearnMoveMonsterElement1.sprite = currentLearningMonster.Base.element.icon;
        LearnMoveMonsterElement2.sprite = currentLearningMonster.Base.secondaryElement.icon;
        LearnMoveUI.SetActive(true);

        EnableLearnMove(0, LearnMove1Name, LearnMove1cost, LearnMove1element);
        EnableLearnMove(1, LearnMove2Name, LearnMove2cost, LearnMove2element);
        EnableLearnMove(2, LearnMove3Name, LearnMove3cost, LearnMove3element);
        EnableLearnMove(3, LearnMove4Name, LearnMove4cost, LearnMove4element);

        LearningMoveName.text = LearningMove.Base.Name;
        LearningMoveElement.sprite = LearningMove.Base.ElementType.icon;
        LearningMovecost.text = LearningMove.StaminaCost.ToString();
    }

    public void OnClickChangeMoveButton(int movenum)
    {
        if (currentLearningMonster == null || LearningMove == null) return;
        if (movenum < 0 || movenum >= currentLearningMonster.CurrentMoves.Count) return;

        currentLearningMonster.CurrentMoves[movenum] = LearningMove;
        LearnMoveUI.SetActive(false);

        if (_gameState != GameState.FREEZE)
        {
            if (currentLearningMonster.CurrentLearningMoves.Count > 0)
                currentLearningMonster.CurrentLearningMoves.Dequeue();

            CheckInQueueForMoves();
        }
        else
        {
            OnUseScrollToMonster();
        }
    }

    public void OnUseScrollToMonster()
    {
        var party = getPartyScene().GetComponent<PartyScript>();
        BagManager.i.RemoveItemFromBag(party.currentUsingItem, 1);
        StartCoroutine(BagManager.i.ClosePartyWhenEmpty(party.currentUsingItem));
    }

    public void OnClickNotLearnMove()
    {
        if (currentLearningMonster != null && currentLearningMonster.CurrentLearningMoves.Count >= 1)
            currentLearningMonster.CurrentLearningMoves.Dequeue();

        LearnMoveUI.SetActive(false);
        CheckInQueueForMoves();

        var ps = UIManager.i.PartyScene.GetComponent<PartyScript>();
        if (ps.partyState == PartyState.LEARNMOVE)
            StartCoroutine(ps.closeUI());
    }

    public void DisableLearnMoveUI() => LearnMoveUI.SetActive(false);

    // Party
    public void OnClickPartyButton(bool ignoreCooldown)
    {
        if (EnemyUnit != null && EnemyUnit.IsDead) return;

        if (canSwitch || ignoreCooldown)
        {
            interruptBattle = true;
            UIManager.i.EnablePartyMenu(true);
            var ps = getPartyScene().GetComponent<PartyScript>();
            ps.partyState = PartyState.BATTLE;
            partyButton?.SetActive(false);
        }
    }

    public void OnClickPartyButtonOVERWORLD()
    {
        if (PlayerStats.i.PlayerParty.Count <= 0) return;
        UIManager.i.EnablePartyMenu(true);
        getPartyScene().GetComponent<PartyScript>().partyState = PartyState.OVERWORLD;
    }

    public void OnClickPartyButtonHEAL(Item item)
    {
        UIManager.i.EnablePartyMenu(true);
        var ps = getPartyScene().GetComponent<PartyScript>();
        ps.partyState = PartyState.HEAL;
        ps.currentUsingItem = item;
        ps.healAmount = item.ItemBase.HealAmount;
    }

    public void OnClickPartyButtonLearnMove(Item item)
    {
        UIManager.i.EnablePartyMenu(true);
        var ps = getPartyScene().GetComponent<PartyScript>();
        ps.partyState = PartyState.LEARNMOVE;
        ps.currentUsingItem = item;
        if (item?.ItemBase?.TeachableMove?.Base == null)
            return;

        LearningMove = new BattleMove(item.ItemBase.TeachableMove.Base);
        LearningMove.Initialise();
    }

    public GameObject getPartyScene() => UIManager.i.PartyScene;

    public void partyCooldown()
    {
        if (canSwitch) return;

        if (partyTimer > 1f)
        {
            partyTimer -= Time.deltaTime;
            var label = partyButton?.GetComponentInChildren<TextMeshProUGUI>();
            if (label) label.text = ((int)partyTimer).ToString();
        }
        else
        {
            canSwitch = true;
            var label = partyButton?.GetComponentInChildren<TextMeshProUGUI>();
            if (label) label.text = "";
        }
    }
}
