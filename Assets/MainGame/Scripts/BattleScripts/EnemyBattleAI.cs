using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Xml;
using NUnit.Framework;

public enum ENEMYSTAGES
{
    NODANGER,
    NEUTRAL,
    INDANGER,
    RETREAT
}

public class EnemyBattleAI : MonoBehaviour
{
    [Tooltip("Holds the enemy danger states to determine actions and behaviour based on danger score")]
    [SerializeField] private ENEMYSTAGES _enemyStage = ENEMYSTAGES.NODANGER;

    [Tooltip("When enemy reaches this distance from player, the enemy will stop moving")]
    [SerializeField] private float _minDistanceToStop = 1.5f;

    //------------------------------------------------------------------Attacks--------------------------------------------------------------------------------//

    [Header("Attacks")]

    [Tooltip("Cooldown for enabling the monster to attack")]
    [SerializeField] private float _attackCooldown = 0.9f;

    [Tooltip("Time to re-pick a move if move didnt change for that time")]
    [SerializeField] private float _timeForRepickMove = 4;

    [Tooltip("When a move is picked, monster can use it after this duration to prevent using previous move without being selected")]
    [SerializeField] private float _attackChangeCoolDownTimer = 1f;

    [Tooltip("When player is very close range of enemy, force the enemy to attack")]
    [SerializeField] private bool _forceAttackAtCloseRange = false;

    private bool _attackChangeCoolDown;    //  True if attack has changed an is in cooldown
    private bool _performingAttack;

    //---------------------------------------------------------------Attack Ranges-----------------------------------------------------------------------------//

    [Header("Attack Ranges")]

    [Tooltip("Maximum distance between enemy and player than the enemy can perform close ranged moves")]
    [SerializeField] private float _maxCloseRange = 4;  //very close range, e.g scratch

    [Tooltip("Maximum distance between enemy and player than the enemy can perform mid ranged moves")]
    [SerializeField] private float _maxMidRange = 9;    //mid range, e.g tackle

    [Tooltip("Maximum distance between enemy and player than the enemy can perform far ranged moves")]
    [SerializeField] private float _maxFarRange = 13;    //far range, e.g flare or any projectile

    //-----------------------------------------------------------Danger score Settings-------------------------------------------------------------------------//

    [Header("DANGER SCORE SETTINGS")]

    private float _dangerScore;  //    Refered to as DS  -   Current danger score

    [Tooltip("When distance between enemy and player is bigger than or equal to this value, add 0 to danger score")]
    [SerializeField] private float _maxDistanceScore = 12;//score = 0

    //  Danger score added will scale between minDistanceScore and maxDistanceScore depending on current distance between player and enemy

    [Tooltip("When distance between enemy and player is less than or equal to this value, add +100 to danger score")]
    [SerializeField] private float _minDistanceScore = 2;//score = 100


    [Tooltip("When enemy has maxHPDifScore % less current health than player, add +100 to danger score")]
    [SerializeField] private float _maxHPDifScore = 70;//score = 100

    //  Danger score added will scale between minHPDifScore and maxHPDifScore depending on % of current health enemy has less than player

    [Tooltip("When enemy has minHPDifScore % less current health than player, add 0 to danger score")]
    [SerializeField] private float _minHPDifScore = 0;//score = 0


    [Tooltip("When enemy has maxStaminaScore stamina less than player add +100 to danger score")]
    [SerializeField] private float _maxStaminaScore = 7;//score = 100

    //  Danger score added will scale between minStaminaScore and maxStaminaScore depending on how much less stamina the enemy has to player

    [Tooltip("When enemy has minStaminaScore stamina less than player add 0 to danger score")]
    [SerializeField] private float _minStaminaScore = 0;//score = 0

    //--------------------------------------------------------------NODANGER state-------------------------------------------------------------------------------//
    
    [Header("NODANGER")]

    [Tooltip("Multiplier for dash cooldown while monster is in NODANGER state")]
    [SerializeField] float _dashCooldownNoDanger = 0.5f; //  Multiplier

    [Tooltip("When Danger Score is between 0 and this value the enemy will be in NODANGER state")]
    [SerializeField] private float _maxNoDangerDangerScore = 120;

    [UnityEngine.Range(0, 100)]
    [Tooltip("Chance to pick a move that is super effective in NODANGER state")]
    [SerializeField] private int _pickAdvantageElementMoveChanceNoDanger = 70;

    [Tooltip("Wether or not to select the least stamina cost move from current moves in NODANGER state")]
    [SerializeField] private bool _selectLeastStaminaMoveNoDanger = false;

    [Tooltip("Wether or not to be able to select status moves while choosing moves from move pool in NODANGER state")]
    [SerializeField] private bool _includeStatusMovesNoDanger = true;

    //---------------------------------------------------------------NEUTRAL state-------------------------------------------------------------------------------//

    [Header("NEUTRAL")]

    [Tooltip("Multiplier for dash cooldown while monster is in NEUTRAL state")]
    [SerializeField] private float _dashCooldownNeutral = 1f; //  Multiplier

    [Tooltip("When Danger Score is between _noDangerMaxDangerScore and this value the enemy will be in NEUTRAL state")]
    [SerializeField] private float _maxNeutralDangerScore = 230;

    [UnityEngine.Range(0, 100)]
    [Tooltip("Chance to pick a move that is super effective in NEUTRAL state")]
    [SerializeField] private int _pickAdvantageElementMoveChanceNeutral = 70;

    [Tooltip("Wether or not to select the least stamina cost move from current moves in NEUTRAL state")]
    [SerializeField] private bool _selectLeastStaminaMoveNeutral = false;

    [Tooltip("Wether or not to be able to select status moves while choosing moves from move pool in NEUTRAL state")]
    [SerializeField] private bool _includeStatusMovesNeutral = true;

    //--------------------------------------------------------------INDANGER state-------------------------------------------------------------------------------//

    [Header("INDANGER")]

    [Tooltip("Multiplier for dash cooldown while monster is in INDANGER state")]
    [SerializeField] private float _dashCooldownInDanger = 1f; //  Multiplier

    [Tooltip("When Danger Score is between _maxNeutralDangerScore and this value the enemy will be in INDANGER state")]
    [SerializeField] private float _maxInDangerDangerScore = 320;

    [UnityEngine.Range(0, 100)]
    [Tooltip("Chance to pick a move that is super effective in INDANGER state")]
    [SerializeField] private int _pickAdvantageElementMoveChanceInDanger = 70;

    [Tooltip("Wether or not to select the least stamina cost move from current moves in INDANGER state")]
    [SerializeField] private bool _selectLeastStaminaMoveInDanger = false;

    [Tooltip("Wether or not to be able to select status moves while choosing moves from move pool in INDANGER state")]
    [SerializeField] private bool _includeStatusMovesInDanger = false;

    //---------------------------------------------------------------RETREAT state-------------------------------------------------------------------------------//

    [Header("RETREAT")]

    [Tooltip("Multiplier for dash cooldown while monster is in RETREAT state")]
    [SerializeField] private float _dashCooldownRetreat = 0.4f; //  Multiplier

    [Tooltip("When Danger Score is between _maxInDangerDangerScore and this value the enemy will be in RETREAT state")]
    [SerializeField] private float _maxRetreatDangerScore = 700;

    [UnityEngine.Range(0,100)]
    [Tooltip("Chance to pick a move that is super effective in RETREAT state")]
    [SerializeField] private int _pickAdvantageElementMoveChanceRetreat = 50;

    [Tooltip("Wether or not to select the least stamina cost move from current moves in RETREAT state")]
    [SerializeField] private bool _selectLeastStaminaMoveRetreat = true;

    [Tooltip("Wether or not to be able to select status moves while choosing moves from move pool in RETREAT state")]
    [SerializeField] private bool _includeStatusMovesRetreat = false;

    //----------------------------------------------------------------Move Points--------------------------------------------------------------------------------//

    [Header("Move Points")]

    [Tooltip("Maximum duration for resetting the points to move near player")]
    [SerializeField] private float _respawnPointsTimerMaxValue = 1;

    [Tooltip("Minimum distance in X-direction to choose from player")]
    [SerializeField] private int _minRandomPointRangeX = 3;

    [Tooltip("Maximum distance in X-direction to choose from player")]
    [SerializeField] private int _maxRandomPointRangeX = 6;

    [Tooltip("Minimum distance in Y-direction to choose from player")]
    [SerializeField] private int _minRandomPointRangeY = 3;

    [Tooltip("Maximum distance in Y-direction to choose from player")]
    [SerializeField] private int _maxRandomPointRangeY = 6;

    [Tooltip("Minimum distance to move enemy away from player if enemy is trying to move away from player")]
    [SerializeField] private float _minDistanceAway = 2f;

    [Tooltip("Maximum distance to move enemy away from player if enemy is trying to move away from player")]
    [SerializeField] private float _maxDistanceAway = 5f;

    [Tooltip("When distance between player and enemy is less than this value and in some Danger States, enemy will move away otherwise move towards player")]
    [SerializeField] private float _moveAwayFromPlayerMaxDistance = 7;

    private Vector2 _pointToMove;    //  Random point chosen to move to
    private int _randomPointRangeX;  //  random X-direction point chosen
    private int _randomPointRangeY;  //  random Y-direction point chosen
    private float _respawnPointsTimer = 1;   //  Current max duration chosen for this battle for rechoosing a random point
    private float _currentRespawnPointsTimer;    //  current timer to determine when to rechoose a random point

    // Buffer for overlapping to find any colliders
    [Header("Move Points collision Checking")]

    [Tooltip("Radius to overlap and check for collisions at random point chosen before moving the enemy")]
    [SerializeField] private float _findCollisionsRadius = 1f;

    private readonly Collider2D[] _overlapResults = new Collider2D[8];  //  Buffer array for collision checking

    //  Temporary values
    private BattleUnit _enemyBattleUnit;
    private bool _followingPlayer;
    private float _distanceBetween;
    private float _tempDashSpeed;
    private float _dashCoolDownBoost;

    /// <summary>
    /// Get or set if enemy battle monster is performing an attack
    /// </summary>
    public bool IsPerformingAttack { get => _performingAttack; set => _performingAttack = value; }



    private void Start()
    {
        _enemyBattleUnit = GetComponent<BattleUnit>();
        _randomPointRangeX = Random.Range(_minRandomPointRangeX, _maxRandomPointRangeY);   //  Setup the random point in X-direction offsets
        _randomPointRangeY = Random.Range(_minRandomPointRangeY, _maxRandomPointRangeY);   //  Setup the random point in Y-direction offsets
        _respawnPointsTimer = Random.Range(1, _respawnPointsTimerMaxValue);               //  Setup the re-choosing points timer
    }

    private void FixedUpdate()
    {
        StopEnemyMovement();    //  Stops enemy movements if some conditions are met
        if (GameUtils.i.battleSystem.CurrentState is not InBattleState)
            return;
        if (GameUtils.i.battleSystem.CheckForBattleEnd == true)
            return;
        if (GameUtils.i.battleSystem.InterruptBattle == true)
            return;
        if (_enemyBattleUnit.DisableAttacking == true)
            return;

        CalculateDangerScore(); //  Calculate Danger Score based on some events


        if (GameUtils.i.battleSystem.EnemyUnit.IsInKnockbackState == true)
            return;
        //calculate distance between enemy and player
        _distanceBetween = Vector2.Distance(transform.position, GameUtils.i.battleSystem.PlayerUnit.transform.position);

        EnterDangerState(); //  Calculate at which Danger State the enemy is in
        StatesBehaviour();  //  Add behaviour based on which Danger State the enemy is in
        Attack();           //  Handle enemy attacking
    }

    #region Danger State Handling

    //-------------------------------------------------------------------------------------------------------------------------------------
    //                       Logic for each state of the enemy AI
    //-------------------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Chooses enemy behaviour based on which Danger State the enemy is in
    /// </summary>
    private void StatesBehaviour()
    {
        List<MoveRange> range = new List<MoveRange>();  //  Holds the possible ranges of attacks the enemy can choose in each Danger State
        switch (_enemyStage)
        {
            case ENEMYSTAGES.NODANGER:

                //  Add possible ranges of moves the enemy can select
                range.Add(MoveRange.CLOSE);
                range.Add(MoveRange.MID);
                range.Add(MoveRange.FAR);

                _dashCoolDownBoost = _dashCooldownNoDanger;   //  Set the dash cooldown multiplier for NODANGER state

                MoveTowardsPlayer();    //  Gets random points around player and moves towards those points
                SelectMove(_pickAdvantageElementMoveChanceInDanger, _selectLeastStaminaMoveNoDanger, range, _includeStatusMovesNoDanger); //  Choose a current move and select it
                DashForward();          //  Handles dashing
                break;
            case ENEMYSTAGES.NEUTRAL:

                //  Add possible ranges of moves the enemy can select
                range.Clear();
                range.Add(MoveRange.MID);
                range.Add(MoveRange.FAR);

                _dashCoolDownBoost = _dashCooldownNeutral;

                MoveTowardsPlayer();    //  Gets random points around player and moves towards those points
                SelectMove(_pickAdvantageElementMoveChanceNeutral, _selectLeastStaminaMoveNeutral, range, _includeStatusMovesNeutral); //  Choose a current move and select it
                DashForward();          //  Handles dashing
                break;
            case ENEMYSTAGES.INDANGER:

                //  Add possible ranges of moves the enemy can select
                range.Clear();
                range.Add(MoveRange.MID);
                range.Add(MoveRange.FAR);

                _dashCoolDownBoost = _dashCooldownInDanger;

                DetermineMoveDirection();             //  Gets random points AWAY from player and moves towards those points
                SelectMove(_pickAdvantageElementMoveChanceInDanger, _selectLeastStaminaMoveInDanger, range, _includeStatusMovesInDanger); //  Choose a current move and select it
                DashForward();          //  Handles dashing
                break;
            case ENEMYSTAGES.RETREAT:

                //  Add possible ranges of moves the enemy can select
                range.Clear();
                range.Add(MoveRange.MID);
                range.Add(MoveRange.FAR);

                _dashCoolDownBoost = _dashCooldownRetreat;

                DetermineMoveDirection();             //  Gets random points AWAY from player and moves towards those points
                SelectMove(_pickAdvantageElementMoveChanceRetreat, _selectLeastStaminaMoveRetreat, range, _includeStatusMovesRetreat); //  Choose a current move and select it
                DashForward();          //  Handles dashing
                break;
            default:
                break;
        }
    }
    //-------------------------------------------------------------------------------------------------------------------------------------
    //                          Determines when the score is high enough to enter a danger state
    //-------------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Determines what Danger State to enter depending on what the Danger Score is
    /// </summary>
    private void EnterDangerState()
    {
        if (_dangerScore <= _maxNoDangerDangerScore)
            _enemyStage = ENEMYSTAGES.NODANGER;
        else if (_dangerScore > _maxNoDangerDangerScore && _dangerScore <= _maxNeutralDangerScore)
            _enemyStage = ENEMYSTAGES.NEUTRAL;
        else if (_dangerScore > _maxNeutralDangerScore && _dangerScore <= _maxInDangerDangerScore)
            _enemyStage = ENEMYSTAGES.INDANGER;
        else if (_dangerScore > _maxInDangerDangerScore)
            _enemyStage = ENEMYSTAGES.RETREAT;
    }

    //-------------------------------------------------------------------------------------------------------------------------------------
    //  Calculate Danger Score
    //  each sub score stat has values from 0 to 100
    //  Stat 1: Distance between player and enemy
    //  Stat 2: Health Difference between player and enemy
    //  Stat 3: Advantage element between player and enemy
    //  Stat 4: Player stamina value and enemy Stamina value
    //-------------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Calculates Danger Score depending on some events
    /// </summary>
    private void CalculateDangerScore()
    {
        //  Calculate distance danger score (the closer the enemy is to the player the more Danger Score is added)
        float distanceScore = 100 - (((_distanceBetween - _minDistanceScore) / (_maxDistanceScore - _minDistanceScore)) * 100);
        distanceScore = Mathf.Clamp(distanceScore, 0f, 100f);//cap the score from 0 to 100

        //  Calculate percentage of health that enemy has less than player danger score (The more the health difference score is the more Danger Score is added)
        float hpDifScore = (((GameUtils.i.playerUnit.CurrentMonster.HP/ GameUtils.i.playerUnit.CurrentMonster.MaxHP)*100 - (_enemyBattleUnit.CurrentMonster.HP/ _enemyBattleUnit.CurrentMonster.MaxHP)*100 - _minHPDifScore) / (_maxHPDifScore - _minHPDifScore)) * 200;
        hpDifScore = Mathf.Clamp(hpDifScore, 0f, 200f);//cap the score from 0 to 100

        //  Calculate who has the element advantage danger score
        float advantageElement = 0f;
        float type1 = (TypeChart.GetEffectiveness(_enemyBattleUnit.CurrentMonster.Base.element, GameUtils.i.playerUnit.CurrentMonster.Base.element) * TypeChart.GetEffectiveness(_enemyBattleUnit.CurrentMonster.Base.element, GameUtils.i.playerUnit.CurrentMonster.Base.secondaryElement));
        float type2 = (TypeChart.GetEffectiveness(_enemyBattleUnit.CurrentMonster.Base.secondaryElement, GameUtils.i.playerUnit.CurrentMonster.Base.element) * TypeChart.GetEffectiveness(_enemyBattleUnit.CurrentMonster.Base.secondaryElement, GameUtils.i.playerUnit.CurrentMonster.Base.secondaryElement));
        
        if (type1 >= 2 || type2 >= 2)  // if one of the two types are advantage dont change Danger Score
            advantageElement = 0f;
        else if(type1 >= 2 && type2 >= 2)//if both are advantage subtract -100 to Danger Score (Enemy is more safe)
            advantageElement = -100f;
        else if (type1 == 1 || type2 == 1)//if one of them is neutral add 50 to Danger Score
            advantageElement = 50f;
        else if (type1 < 1 || type2 < 1)//one of them is disadvantage add 100 to Danger Score
            advantageElement = 100f;
        else if (type1 < 1 && type2 < 1)//both disadvantage add 200 to Danger Score
            advantageElement = 200f;

        //  Calculate how much less stamina the enemy has to player danger score (The less the stamina difference the more Danger Score is added)
        float staminaDifScore = ((GameUtils.i.playerUnit.CurrentMonster.Stamina - _enemyBattleUnit.CurrentMonster.Stamina - _minStaminaScore) / (_maxStaminaScore - _minStaminaScore)) * 100;
        staminaDifScore = Mathf.Clamp(staminaDifScore, -100f, 100f);//cap the score from 0 to 100

        //  Calculate final danger score
        _dangerScore = distanceScore + hpDifScore + advantageElement + staminaDifScore;
    }

    #endregion

    #region Dashing


    //TODO DASH NOT WORKING 20/4/2024
    /// <summary>
    /// Enemy monster dashes forward if not on cooldown or already dashing
    /// </summary>
    private void DashForward()
    {
        if (_enemyBattleUnit.DisableMovement == true)
            return;
        if (_followingPlayer == false)
            return;
        if (_distanceBetween <= _minDistanceToStop)
            return;
        if (GetComponent<BattleUnit>().IsAttacking == true)
            return;
        if (_enemyBattleUnit.CanUseDash == true)
            return;
        if (_enemyBattleUnit.IsDashing == true)
            return;

        //  set dashing and cannot use dash
        _enemyBattleUnit.IsDashing = true;
        _enemyBattleUnit.CanUseDash = true;

        //  Set dashing effects and disable move collisions (moves will not be able to hit the enemy)
        _tempDashSpeed = GameUtils.i.battleSystem.EnemyUnit.WalkSpeed;
        GameUtils.i.battleSystem.EnemyUnit.gameObject.GetComponent<CapsuleCollider2D>().enabled = false;
        StartCoroutine(GameUtils.i.battleSystem.EnemyUnit.DashEffect(GameUtils.i.battleSystem.EnemyUnit.gameObject));
            
        //  set walk speed of enemy to dashing speed
        GameUtils.i.battleSystem.EnemyUnit.WalkSpeed += _enemyBattleUnit.DashRange;

        //  reset dash cooldown and dash after some duration
        StartCoroutine(ResetDash());
        StartCoroutine(ResetDashCooldown());
    }

    /// <summary>
    /// Stop enemy from dashing after dashSpeed duration
    /// </summary>
    private IEnumerator ResetDash()
    {
        yield return new WaitForSeconds(_enemyBattleUnit.DashDuration);

        _enemyBattleUnit.IsDashing = false;

        GameUtils.i.battleSystem.EnemyUnit.WalkSpeed = _tempDashSpeed;
        GameUtils.i.battleSystem.EnemyUnit.gameObject.GetComponent<CapsuleCollider2D>().enabled = true;

        //StartCoroutine(resetDashCooldown());
    }

    /// <summary>
    /// Reset dash cooldown after dashCooldown * dashCoolDownBoost (Multiplier at any Danger State) seconds
    /// </summary>
    private IEnumerator ResetDashCooldown()
    {
        yield return new WaitForSeconds(_enemyBattleUnit.DashCooldown * _dashCoolDownBoost);
        _enemyBattleUnit.CanUseDash = false;
    }

    #endregion

    #region Enemy movement
    /// <summary>
    /// Stop enemy from moving if some conditions are met
    /// </summary>
    private void StopEnemyMovement()
    {
        if (GameUtils.i.battleSystem.InterruptBattle == true
            || _enemyBattleUnit.DisableAttacking == true
            || (Vector2)transform.position == _pointToMove
            || GetComponent<BattleUnit>().IsAttacking == true
            || _enemyBattleUnit.DisableMovement == true)
        {
            if (GameUtils.i.battleSystem.EnemyUnit.IsInKnockbackState == true)
                return;
            GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        }
    }

    //FOLLOW PLAYER TODO 3/4/2024
    /// <summary>
    /// Move enemy towards a random point chosen around player
    /// </summary>
    private void MoveTowardsPlayer()
    {
        if (_enemyBattleUnit.DisableMovement == true)
            return;
        if (_followingPlayer == false)
            return;
        if (_distanceBetween <= _minDistanceToStop)
            return;
      

        GetRandomPointAroundPlayer(false);
        Vector2 currentPosition = transform.position;
        Vector2 directionOfTravel = _pointToMove - currentPosition;


        // Force an attack from enemy if player is at very close distance from enemy
        if(_forceAttackAtCloseRange == true)
        {
            if (GameUtils.i.isNear(currentPosition, GameUtils.i.battleSystem.PlayerUnit.transform.position, _maxCloseRange))
            {

                List<MoveRange> range = new List<MoveRange>();
                range.Add(MoveRange.CLOSE);
                range.Add(MoveRange.MID);
                range.Add(MoveRange.FAR);

                if (FindNonStatusMoves(FindMovesFromRange(_enemyBattleUnit.CurrentMonster.CurrentMoves, range)).Count >= 1)
                {
                    _enemyBattleUnit.CurrentMove = SearchMoveFromRange(FindNonStatusMoves(_enemyBattleUnit.CurrentMonster.CurrentMoves), range);
                    EnemyAttack();
                }
            }
        }

        //  If enemy is near the random point chosen to move, stop moving
        if (GameUtils.i.isNear(currentPosition, _pointToMove, 0.5f))
        {
            GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        }
        else
        {
            //Move Enemy towards a point near player with its speed;
            GetComponent<Rigidbody2D>().linearVelocity = directionOfTravel.normalized * _enemyBattleUnit.WalkSpeed;
            _enemyBattleUnit.PlayWalkAnimation(directionOfTravel.x, directionOfTravel.y);

        }
    }

    /// <summary>
    /// Determine wether to move away or towards player
    /// </summary>
    private void DetermineMoveDirection()
    {
        if (_distanceBetween <= _moveAwayFromPlayerMaxDistance) 
            MoveAwayFromPlayer();
        else
            MoveTowardsPlayer();
    }

    /// <summary>
    /// Moves enemy away from player
    /// </summary>
    private void MoveAwayFromPlayer()
    {
        if (_enemyBattleUnit.DisableMovement == true)
            return;
        if (_distanceBetween <= _minDistanceToStop)
            return;


        GetRandomPointAroundPlayer(true); //  Get random point in direction away from player
        Vector2 currentPosition = transform.position;
        Vector2 directionOfTravel = _pointToMove - currentPosition;

        // Force an attack from enemy if player is at very close distance from enemy
        if (_forceAttackAtCloseRange == true)
        {
            if (GameUtils.i.isNear(currentPosition, GameUtils.i.battleSystem.PlayerUnit.transform.position, _maxCloseRange))
            {
                List<MoveRange> range = new List<MoveRange>();
                range.Add(MoveRange.CLOSE);
                if (SearchMoveFromRange(_enemyBattleUnit.CurrentMonster.CurrentMoves, range) != null)
                {
                    _enemyBattleUnit.CurrentMove = SearchMoveFromRange(_enemyBattleUnit.CurrentMonster.CurrentMoves, range);
                    EnemyAttack();
                }
            }
        }

        // At RETREAT Danger State, force enemy to dash if ready otherwise stop moving when reaching a point
        if (GameUtils.i.isNear(currentPosition, _pointToMove, 0.5f))
        {
            if (_enemyStage == ENEMYSTAGES.RETREAT)
                DashForward();
            else
                GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        }
        else
        {
            //Move Enemy towards a point away from player direction
            GetComponent<Rigidbody2D>().linearVelocity = directionOfTravel.normalized * _enemyBattleUnit.WalkSpeed;
            _enemyBattleUnit.PlayWalkAnimation(directionOfTravel.x, directionOfTravel.y);

        }
    }

    /// <summary>
    /// Picks a random valid point towards or away the player's direction from enemy and sets it as the enemy's target
    /// Respects a cooldown timer and avoids blocked positions
    /// </summary>
    private void GetRandomPointAroundPlayer(bool awayFromPlayer)
    {
        // Only search for a new point when the timer has expired
        if (_currentRespawnPointsTimer > 0f)
        {
            _currentRespawnPointsTimer -= Time.deltaTime;
            return;
        }

        const int maxAttempts = 10;

        // Center of the random area is the player's position
        Vector2 playerPos = GameUtils.i.battleSystem.PlayerUnit.transform.position;
        Vector2 enemyPos = transform.position;

        Vector2 chosenPos = playerPos;
        bool foundValidPoint = false;

        // Direction pointing directly away from the player
        Vector2 baseDirection = (enemyPos - playerPos).normalized;

        // Non-deprecated "no filtering" contact filter for overlap checks
        ContactFilter2D filter = ContactFilter2D.noFilter;

        // Position chosen to move
        Vector2 candidatePosition;

        // Try several times to find a valid point that doesn't hit a blocking collider
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            // -------------------  Get point towards player direction  -------------------

            if (awayFromPlayer == false)
            {
                // Get a random point within the X/Y ranges around the player
                float x = Random.Range(
                    playerPos.x - _randomPointRangeX,
                    playerPos.x + _randomPointRangeX
                );
                float y = Random.Range(
                    playerPos.y - _randomPointRangeY,
                    playerPos.y + _randomPointRangeY
                );

                candidatePosition = new Vector2(x, y);
            }
            else
            {
                // -------------------  Get point away from player direction  -------------------

                // Random offset added so enemy movement doesn't look robotic
                Vector2 randomOffset = new Vector2(
                    Random.Range(-0.5f, 0.5f),
                    Random.Range(-0.5f, 0.5f)
                );

                // Combine the "away direction" with some randomness
                Vector2 moveDirection = (baseDirection + randomOffset).normalized;

                // Pick a random distance within your configured range
                float distance = Random.Range(_minDistanceAway, _maxDistanceAway);

                // Final candidate position based on direction * distance
                candidatePosition = enemyPos + moveDirection * distance;
            }


                // Non-allocating overlap check around the candidate position
                int hitCount = Physics2D.OverlapCircle(
                    candidatePosition,
                    _findCollisionsRadius,  // same radius field as before
                    filter,
                    _overlapResults        // reused Collider2D[] buffer
                );

            // See if any of the overlaps are blocking colliders
            bool isCollider = false;
            for (int i = 0; i < hitCount; i++)
            {
                Collider2D col = _overlapResults[i];
                if (col != null && col.CompareTag("Collider"))
                {
                    isCollider = true;
                    break;
                }
            }

            // If the point is clear, accept it
            if (!isCollider)
            {
                foundValidPoint = true;
                chosenPos = candidatePosition;
                break;
            }
        }

        // Apply the found point (if any)
        if (foundValidPoint)
        {
            _pointToMove = chosenPos;
            _currentRespawnPointsTimer = _respawnPointsTimer; // reset cooldown
        }
    }

    #endregion

    #region Move selecting and Attacking

    /// <summary>
    /// Reselects a move after timeForRepickMove duration
    /// </summary>
    private IEnumerator ReSelectMove()
    {
        yield return new WaitForSeconds(_timeForRepickMove);
        _enemyBattleUnit.HasSelectedMove = false;
    }


    /// <summary>
    /// Selects the best possible move depending on some critiria
    /// </summary>
    private void SelectMove(int elementChance, bool selectLeastStamina, List<MoveRange> ranges, bool includeStatusMoves)
    {
        if (_attackChangeCoolDown == true)   //  Check if choosing attack is not on cooldown
            return;


        if (_enemyBattleUnit.HasSelectedMove == false)  //  Check if enemy has not already chosen an attack
        {
            _enemyBattleUnit.HasSelectedMove = true;
            _attackChangeCoolDown = true;

            StartCoroutine(AttackPickCoolDown());
            int r = Random.Range(0, 100 + 1);

            List<BattleMove> availableMoves = new List<BattleMove>();
            if (HasElementAdvantageMoves() == true) // if there are element advantage attacking moves against player element typings
            {

                if (r <= elementChance) // chance of selecting element advantage move
                {
                    availableMoves = SearchElementAdvantageMoves(ranges);   // select element advantage attacking moves only
                    DebugLogs.Log(":AI: > Selected Advntage moves");
                }
                else
                {
                    availableMoves = SearchNonElementAdvantageMoves(ranges);    //  select non element advantage moves including status
                    DebugLogs.Log(":AI: > Selected NON Advntage moves");
                }
            }
            else // If there are no advantage element moves
            {
                for (int i = 0; i < _enemyBattleUnit.CurrentMonster.CurrentMoves.Count; i++)    //  select all moves available
                    availableMoves.Add(_enemyBattleUnit.CurrentMonster.CurrentMoves[i]);

                DebugLogs.Log(":AI: > Selected Current moves no advantage found");
            }
            if (selectLeastStamina == true) //   select the least stamina cost attacking move available
            {
                _enemyBattleUnit.CurrentMove = GetLeastStaminaAttackingMove(availableMoves);//pick the least stamina cost move
                DebugLogs.Log(":AI: > Selected Least stamina move");
            }
            else  // when select least stamina cost move is false
            {
                if (FindMovesFromRange(availableMoves, GetRangeFromDistance()).Count >= 1)  //  Check if there are any moves at current range between enemy and player
                {
                    if (includeStatusMoves == false)    //  If it doesnt include status moves, choose only attacking moves from that range
                        _enemyBattleUnit.CurrentMove = EnemyPickRandomMove(FindNonStatusMoves(FindMovesFromRange(availableMoves, GetRangeFromDistance())));
                    else // choose any move from that range
                        _enemyBattleUnit.CurrentMove = EnemyPickRandomMove(FindMovesFromRange(availableMoves, GetRangeFromDistance()));
                    DebugLogs.Log(":AI: > Selected A random move from range");
                }
                else // No moves found at that range, choose any random move
                {
                    if (includeStatusMoves == false)    //  If it doesnt include status moves, choose only random attacking moves
                        _enemyBattleUnit.CurrentMove = EnemyPickRandomMove(FindNonStatusMoves(availableMoves));
                    else
                        _enemyBattleUnit.CurrentMove = EnemyPickRandomMove(availableMoves);  //  Choose ANY move randomly
                    DebugLogs.Log(":AI: > Selected A random move from range NOT FOUND");
                }
            }
            if (_enemyBattleUnit.CurrentMove == null)
                _enemyBattleUnit.CurrentMove = EnemyPickRandomMove(availableMoves);

            StartCoroutine(ReSelectMove());//repick a move after some time if no move was chosen
            _followingPlayer = true; //  Set enemy to chase player
            if (_enemyBattleUnit.CurrentMove != null)
                DebugLogs.Log(":AI: > SELECTED MOVE IS: " + _enemyBattleUnit.CurrentMove.Base.Name);
        }
    }

    /// <summary>
    /// Reset change move cooldown
    /// </summary>
    private IEnumerator AttackPickCoolDown()
    {
        yield return new WaitForSeconds(_attackChangeCoolDownTimer);
        _attackChangeCoolDown = false;
    }

    /// <summary>
    /// Attack at given ranges
    /// </summary>
    private void Attack()
    {
        if (_enemyBattleUnit.CurrentMove == null || _enemyBattleUnit.CurrentMove.Base == null)
            return;

        if (_enemyBattleUnit.CurrentMove.Base.AIRange == MoveRange.CLOSE) //  if selected attack is close range, attack ONLY at close range
        {
            if (_distanceBetween <= _maxCloseRange) EnemyAttack();
        }
        else if (_enemyBattleUnit.CurrentMove.Base.AIRange == MoveRange.MID)  //  if selected attack is mid range, attack ONLY at mid or close range distance
        {
            if (_distanceBetween <= _maxMidRange) EnemyAttack();
        }//if selected attack is far range, attack at any range
        else
        {
            EnemyAttack();
        }
    }

    //  Currently not used anywhere you may use it if found useful
    /// <summary>
    /// Selects a a move of only that range between player and enemy
    /// </summary>
    private void GetAttackFromRange()
    {
        if (_enemyBattleUnit.HasSelectedMove == true)
            return;

        _enemyBattleUnit.HasSelectedMove = true;
        if (_distanceBetween <= _maxCloseRange)   //  enemy is in close range with player, choose only close range attacks
        {
            List<MoveRange> range = new List<MoveRange>();
            range.Add(MoveRange.CLOSE);
            _enemyBattleUnit.CurrentMove = SearchMoveFromRange(_enemyBattleUnit.CurrentMonster.CurrentMoves, range);
        }
        else if (_distanceBetween > _maxCloseRange && _distanceBetween <= _maxMidRange)   //  enemy is in mid range with player, choose only mid range attacks
        {
            List<MoveRange> range = new List<MoveRange>();
            range.Add(MoveRange.MID);
            _enemyBattleUnit.CurrentMove = SearchMoveFromRange(_enemyBattleUnit.CurrentMonster.CurrentMoves, range);
        }
        else if (_distanceBetween > _maxMidRange && _distanceBetween <= _maxFarRange)   //  enemy is in far range with player, choose only far range attacks
        {
            List<MoveRange> range = new List<MoveRange>();
            range.Add(MoveRange.FAR);
            _enemyBattleUnit.CurrentMove = SearchMoveFromRange(_enemyBattleUnit.CurrentMonster.CurrentMoves, range);
        }
        else
        {
            _followingPlayer = true; //  If player and enemy are further than far range, set enemy to chase player
        }
        if (_enemyBattleUnit.CurrentMove == null)   //  If no move was selected choose a random move
            _enemyBattleUnit.CurrentMove = EnemyPickRandomMove(_enemyBattleUnit.CurrentMonster.CurrentMoves);

        DebugLogs.Log(":AI: > SELECTED MOVE IS: " + _enemyBattleUnit.CurrentMove.Base.Name);
    }

    /// <summary>
    /// Handles enemy attacking
    /// </summary>
    private void EnemyAttack()
    {
        if (_enemyBattleUnit.CurrentMove == null || _enemyBattleUnit.CurrentMove.Base == null)
            return;

        if (GameUtils.i.battleSystem.EnemyUnit.IsInKnockbackState == true)
            return;
        if (GetComponent<BattleUnit>().IsAttacking == true)
            return;
        if (GetComponent<BattleUnit>().CanAttack == false)
            return;
        //if (GameUtils.i.battleSystem.PlayerUnit.CanAttack)
        //    return;

        if (_enemyBattleUnit.Stamina >= _enemyBattleUnit.CurrentMove.StaminaCost)
        {
            //enemy unit
            //attackAnimation(EnemyUnit);
            _performingAttack = true;
            GetComponent<BattleUnit>().IsAttacking = true;
            StartCoroutine(AttackCoolDown());
            float dmg = _enemyBattleUnit.CurrentMonster.CalculateMoveStats(_enemyBattleUnit.CurrentMove);
            AbilityContext attackerCTX = new AbilityContext()
            {
                User = _enemyBattleUnit,
                Target = GameUtils.i.playerUnit,
                Move = _enemyBattleUnit.CurrentMove,
                Damage = dmg
            };

            attackerCTX.User.CurrentMonster.Ability?.OnMoveUsed(attackerCTX);
            if (attackerCTX.CancelMove)
            {
                _performingAttack = false;
                GetComponent<BattleUnit>().IsAttacking = false;
                _enemyBattleUnit.HasSelectedMove = false;
                return;
            }

            _enemyBattleUnit.CurrentMonster.TotalAttack = attackerCTX.Damage;
            GameUtils.i.PlayAnimationFromMove(_enemyBattleUnit.CurrentMove, GameUtils.i.battleSystem.MoveAnimationHolder, _enemyBattleUnit, GameUtils.i.battleSystem.PlayerUnit, true);
            //unit.setCanAttack(false);
            _enemyBattleUnit.Stamina -= _enemyBattleUnit.CurrentMove.StaminaCost;
            _enemyBattleUnit.HasSelectedMove = false;

        }
    }

    /// <summary>
    /// Reset attack cooldown after attackCooldown seconds
    /// </summary>
    private IEnumerator AttackCoolDown()
    {
        yield return new WaitForSeconds(_attackCooldown);
        GetComponent<BattleUnit>().IsAttacking = false;
    }

    #endregion

    #region Move searching methods

    /// <summary>
    /// Get move range from distance between player and enemy
    /// </summary>
    public List<MoveRange> GetRangeFromDistance()
    {
        List<MoveRange> range = new List<MoveRange>();

        if (_distanceBetween <= _maxCloseRange)
        {
            range.Add(MoveRange.CLOSE);
            DebugLogs.Log(":AI: > CHOSE A CLOSE RANGE MOVE");
        }
        if (_distanceBetween <= _maxMidRange)
        {
            range.Add(MoveRange.MID);
            DebugLogs.Log(":AI: > CHOSE A MID RANGE MOVE");
        }
        if (_distanceBetween <= _maxFarRange)
        {
            range.Add(MoveRange.FAR);
            DebugLogs.Log(":AI: > CHOSE A FAR RANGE MOVE");
        }
        else
        {
            range.Add(MoveRange.FAR);
            DebugLogs.Log(":AI: > CHOSE A FAR RANGE MOVE (TOO FAR)");
        }

        return range;
    }

    /// <summary>
    /// Searches all moves in moveList that their range is in ranges and returns a random move from those
    /// </summary>
    public BattleMove SearchMoveFromRange(List<BattleMove> moveList, List<MoveRange> ranges)
    {
        bool found = false;
        BattleMove move = EnemyPickRandomMove(moveList);    //  Pick random move
        
        if (moveList.Count >= 1)    //  check if move list is not empty
        {
            int i = 0;
            while (i < moveList.Count && found == false)
            {
                if (ranges.Contains(moveList[i].Base.AIRange) == true)    //  Check ranges contain the range of the current searching move
                {
                    found = true;
                    move = moveList[i]; //  return the move found
                }
                i++;
            }
        }
        return move;
    }

    /// <summary>
    /// Searches all moves in moveList that their range is in ranges and returns a a list of all those moves
    /// </summary>
    public List<BattleMove> FindMovesFromRange(List<BattleMove> moveList, List<MoveRange> ranges)
    {
        
        List<BattleMove> move = new List<BattleMove>();

        if (moveList.Count >= 1)    //  check if move list is not empty
        {
            int i = 0;
            while (i < moveList.Count)
            {
                if (ranges.Contains(moveList[i].Base.AIRange) == true)    //  Check ranges contain the range of the current searching move 
                    move.Add(moveList[i]);  //  Add current searching move to move list that is going to be returned
                i++;
            }
        }
        
        if (move.Count == 0)//if no ranges found add random move
        {
            if (moveList.Count == 0)    //  check if move list is empty
                move.Add(EnemyPickRandomMove(_enemyBattleUnit.CurrentMonster.CurrentMoves));    // pick random move from Current Moves of enemy
            else
                move.Add(EnemyPickRandomMove(moveList));    //  otherwise pick random move from given moveList
        }
        return move;
    }

    /// <summary>
    /// Searches moveList and returns a list of only moves that are NOT status moves
    /// </summary>
    public List<BattleMove> FindNonStatusMoves(List<BattleMove> moveList)
    {
        List<BattleMove> move = new List<BattleMove>();

        if (moveList.Count >= 1)    //  check if move list is not empty
        {
            int i = 0;
            while (i < moveList.Count)
            {
                if (moveList[i].Base.DamageType != MoveType.STATUS)   //  Check if current searching move is not a status move
                    move.Add(moveList[i]);  //  Add current searching move to move list that is going to be returned
                i++;
            }
        }
        if (move.Count == 0)//if no status found add all moves
        {
            if (moveList.Count == 0)    //  check if move list is empty
                move.Add(EnemyPickRandomMove(_enemyBattleUnit.CurrentMonster.CurrentMoves));    // pick random move from Current Moves of enemy
            else
                move.Add(EnemyPickRandomMove(moveList));    //  otherwise pick random move from given moveList
        }
        return move;
    }

    /// <summary>
    /// Returns true if the enemy has any element advantage moves that will be super effective against player element typing
    /// </summary>
    public bool HasElementAdvantageMoves()
    {
        bool found = false;

        if (_enemyBattleUnit.CurrentMonster.CurrentMoves.Count >= 1)    //  check if move list is not empty
        {
            int i = 0;
            while (i < _enemyBattleUnit.CurrentMonster.CurrentMoves.Count && found == false)
            {
                //  Get the typing multiplier of current move against the player monster element
                float type = (TypeChart.GetEffectiveness(_enemyBattleUnit.CurrentMonster.CurrentMoves[i].Base.ElementType, GameUtils.i.playerUnit.CurrentMonster.Base.element) * TypeChart.GetEffectiveness(_enemyBattleUnit.CurrentMonster.CurrentMoves[i].Base.ElementType, GameUtils.i.playerUnit.CurrentMonster.Base.secondaryElement));
                if (type >= 2f && _enemyBattleUnit.CurrentMonster.CurrentMoves[i].Base.DamageType != MoveType.STATUS) //  If multiplier is bigger than 2 which is super effective and is not status move return true
                    found = true;
                i++;
            }
        }
        return found;
    }

    /// <summary>
    /// Searches all advantage element moves of enemy that have a range in ranges and return a list of those moves
    /// </summary>
    public List<BattleMove> SearchElementAdvantageMoves(List<MoveRange> ranges)
    {
        List<BattleMove> move = new List<BattleMove>();
        List<BattleMove> tempMove = new List<BattleMove>();

        if (_enemyBattleUnit.CurrentMonster.CurrentMoves.Count >= 1)    //  check if move list is not empty
        {
            int i = 0;
            while (i < _enemyBattleUnit.CurrentMonster.CurrentMoves.Count)
            {
                //  Get the typing multiplier of current move against the player monster element
                float type = (TypeChart.GetEffectiveness(_enemyBattleUnit.CurrentMonster.CurrentMoves[i].Base.ElementType, GameUtils.i.playerUnit.CurrentMonster.Base.element) * TypeChart.GetEffectiveness(_enemyBattleUnit.CurrentMonster.CurrentMoves[i].Base.ElementType, GameUtils.i.playerUnit.CurrentMonster.Base.secondaryElement));
                if (type >= 2f && _enemyBattleUnit.CurrentMonster.CurrentMoves[i].Power > 0)    //  If multiplier is bigger than 2 which is super effective and move has power, add it to moves going to be returned
                    move.Add(_enemyBattleUnit.CurrentMonster.CurrentMoves[i]);
                i++;
            }

            for (int j = 0; j < FindMovesFromRange(move, ranges).Count; j++)    //  Search all advantage element moves
            {
                tempMove.Add(FindMovesFromRange(move, ranges)[j]);  //  Add to other list all advantage element moves which have range in ranges
            }
            
            if (tempMove.Count > 0) //  Check if there are any advantage element moves which have range in ranges
            {
                move.Clear();
                for (int j = 0; j < tempMove.Count; j++)
                {
                    move.Add(tempMove[j]);  //  Copy them to move list
                }
                
            }
        }
        return move;
    }


    /// <summary>
    /// Searches all NON advantage element moves of enemy that have a range in ranges and return a list of those moves
    /// </summary>
    public List<BattleMove> SearchNonElementAdvantageMoves(List<MoveRange> ranges)
    {
        List<BattleMove> move = new List<BattleMove>();
        List<BattleMove> tempMove = new List<BattleMove>();

        if (_enemyBattleUnit.CurrentMonster.CurrentMoves.Count >= 1)    //  check if move list is not empty
        {
            int i = 0;
            while (i < _enemyBattleUnit.CurrentMonster.CurrentMoves.Count)
            {
                //  Get the typing multiplier of current move against the player monster element
                float type = (TypeChart.GetEffectiveness(_enemyBattleUnit.CurrentMonster.CurrentMoves[i].Base.ElementType, GameUtils.i.playerUnit.CurrentMonster.Base.element) * TypeChart.GetEffectiveness(_enemyBattleUnit.CurrentMonster.CurrentMoves[i].Base.ElementType, GameUtils.i.playerUnit.CurrentMonster.Base.secondaryElement));
                if (type < 2f)    //  If multiplier is less than 2 which is NOT super effective add it to moves returned
                    move.Add(_enemyBattleUnit.CurrentMonster.CurrentMoves[i]);
                i++;
            }

            for (int j = 0; j < FindMovesFromRange(move, ranges).Count; j++)    //  Search all non advantage element moves
            {
                tempMove.Add(FindMovesFromRange(move, ranges)[j]);  //  Add to other list all non advantage element moves which have range in ranges
            }

            if (tempMove.Count > 0) //  Check if there are any non advantage element moves which have range in ranges
            {
                move.Clear();
                for (int j = 0; j < tempMove.Count; j++)
                {
                    move.Add(tempMove[j]); //  Copy them to move list
                }
            }
        }
        return move;
    }

    /// <summary>
    /// Get the least stamina cost move from moveList
    /// </summary>
    public BattleMove GetLeastStaminaMove(List<BattleMove> moveList)
    {
        List<BattleMove> tempList = new List<BattleMove>();
        for (int i = 0; i < moveList.Count; i++)
        {
            tempList.Add(moveList[i]);    //  Copy all moves to tempList
        }
        BattleMove move = tempList[0];

        if (tempList.Count > 1)   //  check if move list is not empty
            SortMovesWithStamina(tempList); //  Sort list in ascending order of stamina cost

        move = tempList[0]; //  Select first element which is the least stamina cost
        return move;
    }

    /// <summary>
    /// Get the least stamina cost attacking move ONLY from moveList
    /// </summary>
    /// <param name="moveList"></param>
    /// <returns></returns>
    public BattleMove GetLeastStaminaAttackingMove(List<BattleMove> moveList)
    {
        List<BattleMove> tempList = new List<BattleMove>();
        for (int i = 0; i < moveList.Count; i++)
        {
            if (moveList[i].Power > 0)
                tempList.Add(moveList[i]);    //  Copy all moves to tempList which have power so attacking move
        }
        if (tempList.Count == 0)
            return GetLeastStaminaMove(moveList);

        if (tempList.Count > 1)   //  check if move list is not empty
            SortMovesWithStamina(tempList); //  Sort list in ascending order of stamina cost

        BattleMove move = tempList[0]; //  Select first element which is the least stamina cost
        return move;
    }

    // Bubble sort for stamina cost ascending order of stamina cost
    /// <summary>
    /// Sorts all moves by stamina cost in ascending order of stamina cost
    /// </summary>
    private void SortMovesWithStamina(List<BattleMove> moveList)
    {
        int n = moveList.Count;

        for (int i = 0; i < n - 1; i++)
        {
            for (int j = 0; j < n - i - 1; j++)
            {
                if (moveList[j].StaminaCost > moveList[j + 1].StaminaCost)
                {
                    // Swap elements
                    BattleMove temp = moveList[j];
                    moveList[j] = moveList[j + 1];
                    moveList[j + 1] = temp;
                }
            }
        }
    }

    //  Method not used, use it if you find it useful somewhere
    /// <summary>
    /// Counts moves available in that range
    /// </summary>
    public int CountMoveFromRange(MoveRange range)
    {
        int count = 0;

        for (int i = 0; i < _enemyBattleUnit.CurrentMonster.CurrentMoves.Count; i++)
        {
            if (_enemyBattleUnit.CurrentMonster.CurrentMoves[i].Base.AIRange == range)    //  Count all moves that have that range
                count++;
        }
        return count;
    }

    /// <summary>
    /// Return a random move from moveList
    /// </summary>
    BattleMove EnemyPickRandomMove(List<BattleMove> moveList)
    {
        if (moveList == null || moveList.Count == 0)
            return null;

        return moveList[Random.Range(0, moveList.Count)];
    }

    #endregion
}
