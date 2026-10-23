using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleProjectile : MonoBehaviour
{

    [Header("General Properties")]

    [Tooltip("Object to spawn when the projectile collides with something (Example: explosion particles)")]
    [SerializeField] private GameObject SplashObject;

    [Header("Projectile Stats")]

    [Tooltip("If true, projectile will be stationary and will not move")]
    private bool _isStationary;

    [Tooltip("Projectile will follow shooter position")]
    private bool _isFollowingShooter;

    [Tooltip("If true, this projectile cannot deal any damage")]
    private bool _isCosmetic;

    [Tooltip("If true, this projectile can cause status conditions")]
    [SerializeField] private bool _canCauseStatus;

    [Tooltip("If true, the projectile will play rotating animation")]
    private bool _isRotating;

    [Tooltip("How fast the projectile will rotate")]
    [SerializeField] private float _rotateSpeed;

    [Tooltip("If true, the projectile will get destroyed when it collides with something")]
    private bool _destroyOnImpact;

    private bool _followTarget;  //  NOT IMPLEMENTED YET
    private float _followTargetRange;  //  NOT IMPLEMENTED YET

    private bool _alreadyHit;    //  prevents projectile from hitting target multiple times
    private bool _spawnedSplash; //  checks if splash has already been spawned to prevent double spawning

    private Rigidbody2D rb;
    private BattleMove move;    //  The move actual move that is this projectile
    private BattleUnit shooterUnit;
    private EnemyBattleAI shooterAI;

    public GameObject Shooter { get; set; }
    public MoveAnimationHolder MoveAnimator { get; set; }
    public MoveContext Context { get; set; }
    public float Damage { get; set; }
    public float Speed { get; set; }
    public float Lifetime { get; set; }
    public bool CanCauseStatus { get => _canCauseStatus; set => _canCauseStatus = value; }
    public bool IsCosmetic { get => _isCosmetic; set => _isCosmetic = value; }
    public bool IsFollowingShooter { get => _isFollowingShooter; set => _isFollowingShooter = value; }
    public bool IsStationary { get => _isStationary; set => _isStationary = value; }
    public bool IsRotating { get => _isRotating; set => _isRotating = value; }
    public bool DestroyOnImpact { get => _destroyOnImpact; set => _destroyOnImpact = value; }

    public void ApplyMoveSettings(BattleMove battleMove)
    {
        if (battleMove == null || battleMove.Base == null || battleMove.Base.CastPattern == MoveCastPattern.LegacyAuto)
            return;

        _isStationary = battleMove.Base.IsStationary;
        _isFollowingShooter = battleMove.Base.IsFollowingShooter;
        _isCosmetic = battleMove.Base.IsCosmetic;
        _isRotating = battleMove.Base.IsRotating;
        _destroyOnImpact = battleMove.Base.DestroyOnImpact;

        if (battleMove.Status != null && battleMove.Status.Count > 0)
            _canCauseStatus = true;
    }

    //  When projectile is destroyed:
    //  -   spawn splash object
    //  -   play destroy sound
    //  -   set performing attack to enemy AI to false
    private void OnDestroy()
    {
        if (SplashObject != null && _spawnedSplash == false)
        {
            GameObject ob = Instantiate(SplashObject, transform.position, Quaternion.identity);
            ob.transform.localScale = new Vector3(gameObject.transform.localScale.y, gameObject.transform.localScale.y, gameObject.transform.localScale.z);
        }
        //for destory on impact sound effect
        if (move != null && move.Base.DestroySoundEffect.Count > 0)
            AudioSystem.i.PlaySoundClip(AudioSystem.i.PlayRandomSound(move.Base.DestroySoundEffect), AudioMaster.BATTLE);
        //reset attack state of enemy when attack ends
        if (shooterUnit != null && shooterUnit.IsEnemy == true && shooterAI != null)
            shooterAI.IsPerformingAttack = false;
    }

    //  When projectile spawns
    //  -   Add force to projectile so that it moves
    //  -   Destroy the object after lifetime passes
    private void Start()
    {
       
        rb = GetComponent<Rigidbody2D>();
        if (Shooter == null && Context != null && Context.User != null)
            Shooter = Context.User.gameObject;

        if (Shooter == null)
        {
            Destroy(gameObject);
            return;
        }

        shooterUnit = Shooter.GetComponent<BattleUnit>();
        shooterAI = Shooter.GetComponent<EnemyBattleAI>();
        if (shooterUnit == null)
        {
            Destroy(gameObject);
            return;
        }

        move = Context != null && Context.Move != null ? Context.Move : shooterUnit.CurrentMove;
        if (move == null || move.Base == null)
        {
            Destroy(gameObject);
            return;
        }

        ApplyMoveSettings(move);

        if (_isStationary == false && _isFollowingShooter == false)
            rb.AddForce(transform.right * Speed, ForceMode2D.Impulse);
      
        Destroy(gameObject, Lifetime);
    }


    //  At each frame
    //  -   Rotate projectile if enabled
    //  -   Follows shooter if enabled
    private void FixedUpdate()
    {
        Rotate();
        if (_isFollowingShooter == true)
            transform.position = Shooter.transform.position;
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        //  If shooter is dead disable collisions
        if (shooterUnit == null || shooterUnit.IsDead)
            return;

        //  If it is cosmetic do not apply damage logic
        if (_isCosmetic == true) return;

        //  Spawn splash object when projectile hits a collider
        SpawnSpashObject(collision);

        //  check if defender monster is not shooter or if it has already been hit by this attack
        BattleUnit targetUnit = null;
        bool hitBattleUnit = collision.TryGetComponent(out targetUnit);
        if (hitBattleUnit && collision.gameObject != Shooter && _alreadyHit == false)
        {
            //  check if the defender monster is dashing to avoid attack
            if (targetUnit.IsDashing)   return;

            _alreadyHit = true;
            if (GetMoveClash(targetUnit) == false)
            {
                CauseStatusCondition(collision);
                targetUnit.TakeDamage(shooterUnit, move, Damage, true, move.Base.ElementType);
                if (move.StatChangesSelf != null)
                    shooterUnit.StartCoroutine(shooterUnit.ChangeStatsSelf(move.StatChangesSelf));
                TriggerMoveHitEffects(targetUnit);
                MoveAnimationManager.i.StartCoroutine(MoveAnimationManager.i.playHurtAnimationMaterial(collision.GetComponent<SpriteRenderer>(), 0.2f));
                Instantiate(GameUtils.i.getHitParticle(), collision.transform.position, Quaternion.identity);
            }
            else
            {
                //  if both moves are the same and are physical, cause Knockback to both monsters
                for (int i = 0; i < GameUtils.i.getClashParticles().Count; i++)
                    Instantiate(GameUtils.i.getClashParticles()[i], collision.transform.position, transform.rotation);

                GameUtils.i.ShakeCamera(0.5f, 3f, 10, true);
                targetUnit.OnDamageTaken?.Invoke();
                shooterUnit.OnDamageTaken?.Invoke();
                //  Apply knockback
                targetUnit.CauseKnockback(Shooter, collision.gameObject, GameUtils.i.battleSystem.ClashKnockBackStrength, GameUtils.i.battleSystem.ClashKnockBackTime);
                shooterUnit.CauseKnockback(collision.gameObject, Shooter, GameUtils.i.battleSystem.ClashKnockBackStrength, GameUtils.i.battleSystem.ClashKnockBackTime);
            }

            //  for hit enemy sound effect
            if (move.Base.HitSoundEffect.Count > 0)
                AudioSystem.i.PlaySoundClip(AudioSystem.i.PlayRandomSound(move.Base.HitSoundEffect), AudioMaster.BATTLE);
        }
        //  winning attack
        BattleProjectile collidedProjectile = null;
        bool hitProjectile = collision.TryGetComponent(out collidedProjectile);
        if (hitProjectile && collidedProjectile.Shooter != Shooter)
            DestroyLosingClashMoveByType(collidedProjectile);
        
        //  destroy on impact
        if (_destroyOnImpact == true && collision.gameObject != Shooter && !hitProjectile)
            Destroy(gameObject);
    }

    private void TriggerMoveHitEffects(BattleUnit target)
    {
        if (move == null || move.Base == null || move.Base.onMoveHitEffects == null)
            return;

        MoveContext context = Context ?? new MoveContext(shooterUnit, target, move, shooterUnit.IsEnemy);
        context.User = shooterUnit;
        context.Target = target;
        context.Move = move;
        context.Projectile = this;
        context.Damage = Damage;

        for (int i = 0; i < move.Base.onMoveHitEffects.Count; i++)
        {
            if (move.Base.onMoveHitEffects[i] != null)
                move.Base.onMoveHitEffects[i].OnMoveHitTarget(context);
        }
    }

    //  Applys status condition to monster hit if the move can cause status conditions
    //  and if the monster hit does not already have an active status condition
    private void CauseStatusCondition(Collider2D collision)
    {
        //  get if move can cause status condition
        if (_canCauseStatus == true)
        {
            if (move == null || move.Status == null)
                return;

            //  check if monster hit is actually a monster and is not the shooter
                BattleUnit targetUnit;
                if (collision.TryGetComponent(out targetUnit) && collision.gameObject != Shooter)
                {
                    //  check if monster already has status, if yes, dont apply other status
                    if (targetUnit.CurrentMonster.Status != Status.NONE) return;

                    targetUnit.ApplyStatusEffect(targetUnit.CurrentMonster, move);
                }
            }
        }

    //  Spawns splash particles object when the projectile hits a collider
    private void SpawnSpashObject(Collider2D collision)
    {
        if (SplashObject != null && collision.gameObject != Shooter)
        {
            _spawnedSplash = true;
            Bounds thisBounds = GetBounds(gameObject);
            Bounds otherBounds = collision.bounds;

            Vector2 contactPoint = GetContactPoint(thisBounds, otherBounds);

            GameObject ob = Instantiate(SplashObject, contactPoint, Quaternion.identity);
            ob.transform.localScale = new Vector3(gameObject.transform.localScale.y, gameObject.transform.localScale.y, gameObject.transform.localScale.z);
        }
    }


    //  Add rotation to projectile if enabled
    private void Rotate()
    {
        if (_isRotating == false) return;

        gameObject.transform.eulerAngles += new Vector3(0, 0, _rotateSpeed * Time.deltaTime);
    }


    //  if enemy and player attack has same type but different power moves, destroy move depending on stronger move
    private void DestroyLosingClashMoveByType(BattleProjectile collidedObject)
    {
        if (shooterUnit != null && shooterUnit.IsEnemy == true)
        {
            if (move.Base.ElementType.name == collidedObject.move.Base.ElementType.name)
            {
                if (move.Base.Power > collidedObject.move.Base.Power)
                {
                    Destroy(collidedObject.gameObject);
                }
                else if (move.Base.Power < collidedObject.move.Base.Power)
                {
                    Destroy(gameObject);
                }
                else
                {
                    Destroy(gameObject);
                    Destroy(collidedObject.gameObject);
                }
            }
        }
    }
    private Vector2 GetContactPoint(Bounds a, Bounds b)
    {
        float x = Mathf.Clamp(b.center.x, a.min.x, a.max.x);
        float y = Mathf.Clamp(b.center.y, a.min.y, a.max.y);
        return new Vector2(x, y);
    }

    private Bounds GetBounds(GameObject ob)
    {
        Bounds bounds = new Bounds();
        if (ob.GetComponent<CapsuleCollider2D>() != null)
            bounds = GetComponent<CapsuleCollider2D>().bounds;
        if (ob.GetComponent<CircleCollider2D>() != null)
            bounds = GetComponent<CircleCollider2D>().bounds;
        return bounds;
    }
    //TODO not working, enemy attacking check is true until next attack is played not until attack ends
    private bool GetMoveClash(BattleUnit defender)
    {
        bool clash = false;
        if (shooterUnit != null && shooterUnit.IsEnemy == true)
        {
            if (defender.IsAttacking == true && shooterAI != null && shooterAI.IsPerformingAttack == true)
            {
                if (shooterUnit.CurrentMove.Base.Name == defender.CurrentMove.Base.Name)
                {
                    if (shooterUnit.CurrentMove.Base.DamageType == MoveType.PHYSICAL && defender.CurrentMove.Base.DamageType == MoveType.PHYSICAL)
                        clash = true;
                }
            }
        }
        return clash;
    }
}
