using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Linq;
public class MoveAnimationManager : MonoBehaviour
{
    public static MoveAnimationManager i;
    [SerializeField] private Material _damageShaderMat;

    private void Awake()
    {
        if (i != null)
        {
            Destroy(i);
        }
        i = this;
    }

    /*private float RotateObToMouse()
    {
        if (GameUtils.i.HasControllerAimInput())
        {
            
            BattleUnit playerUnit = GameUtils.i.battleSystem.PlayerUnit;
            if (playerUnit != null && playerUnit.AttackPointer != null)
                return playerUnit.AttackPointer.transform.eulerAngles.z;
        }

        
        Vector3 mousePos = Input.mousePosition;
        Vector3 playerPos = GameUtils.i.playerUnit != null
            ? GameUtils.i.playerUnit.transform.position
            : GameUtils.i.transform.position;
        Vector3 screenPlayerPos = GameUtils.i.MainCamera.WorldToScreenPoint(playerPos);
        mousePos.x -= screenPlayerPos.x;
        mousePos.y -= screenPlayerPos.y;
        float angle = Mathf.Atan2(mousePos.y, mousePos.x) * Mathf.Rad2Deg;
        Debug.Log("Mouse rotating with Mouse: " + angle);
        return angle;
    }*/
    
    private float RotateObToMouse()
    {
        Vector2 direction = GetPlayerAimDirection();
        return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    }

    private Vector2 GetPlayerAimDirection()
    {
        if (GameUtils.i.playerUnit != null)
            return GameUtils.i.playerUnit.CurrentAimDirection;

        return Vector2.right;
    }

    private float RotateObToOtherOb(GameObject damager, GameObject target)
    {
        Vector3 mousePos = target.transform.position;
        Vector3 playerPos = damager.transform.position;
        Vector3 armPos = playerPos;
        mousePos.x = mousePos.x - armPos.x;
        mousePos.y = mousePos.y - armPos.y;
        float angle = Mathf.Atan2(mousePos.y, mousePos.x) * Mathf.Rad2Deg;
        return angle;
    }
    private Quaternion RotateQuaternion(GameObject damager, GameObject target)
    {
        Vector2 direction = target.transform.position - damager.transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        return targetRotation;
    }
    private void SpawnDashParticle(Vector3 target, GameObject targetOb, Quaternion rotation, bool onlyEnemy)
    {
        GameObject ob = Instantiate(GameUtils.i.battleSystem.DashParticle, target, rotation);
        if (onlyEnemy == true)
        {
            ob.transform.rotation = Quaternion.Euler(0, ob.transform.rotation.y, RotateObToOtherOb(ob, targetOb));
        }
        Destroy(ob, 1f);
    }

    public Vector3 DirectionFrom(Vector3 From, Vector3 To)
    {
        Vector3 relativePos = To - From;
        return relativePos;
    }

    private float hyp(float v1, float v2)
    {
        return Mathf.Sqrt(Mathf.Pow(v1, 2) + Mathf.Pow(v2, 2));
    }

    public IEnumerator playHurtAnimation(SpriteRenderer ob, float timer, Color color)
    {
        yield return new WaitForSeconds(timer);
        var sequence = DOTween.Sequence();
        sequence.Append(ob.DOColor(color, 0.15f));
        sequence.Append(ob.DOColor(Color.white, 0.15f));
        //ob.gameObject.transform.DOShakePosition(0.1f, .5f);
    }
    public IEnumerator playHurtAnimationMaterial(SpriteRenderer ob, float resetTimer)
    {
        
        if (ob.material != _damageShaderMat)
        {
            Material oldMat = GameUtils.i.defaultMaterial;
            ob.material = _damageShaderMat;
            yield return new WaitForSeconds(resetTimer);
            ob.material = oldMat;

        }
    }
    public void PlaySoundEffect(List<AudioClip> SoundEffect)
    {
        if (SoundEffect.Count > 0)
        {
            AudioSystem.i.PlaySoundClip(SoundEffect[Random.Range(0, SoundEffect.Count)], AudioMaster.UI);
        }
    }

    private BattleProjectile ConfigureProjectile(GameObject ob, MoveAnimationHolder moveHolder)
    {
        BattleProjectile projectile = ob.GetComponent<BattleProjectile>();
        if (projectile == null)
            return null;

        projectile.MoveAnimator = moveHolder;
        projectile.Context = moveHolder.Context;
        projectile.ApplyMoveSettings(moveHolder.CurrentMove);

        if (moveHolder.Context != null)
        {
            moveHolder.Context.Projectile = projectile;
            RunMoveUsedEffects(moveHolder.Context);
        }

        return projectile;
    }

    private void RunMoveUsedEffects(MoveContext context)
    {
        if (context.MoveUsedEffectsApplied || context.Move == null || context.Move.Base == null || context.Move.Base.onMoveUsedEffects == null)
            return;

        context.MoveUsedEffectsApplied = true;

        for (int i = 0; i < context.Move.Base.onMoveUsedEffects.Count; i++)
        {
            if (context.Move.Base.onMoveUsedEffects[i] != null)
                context.Move.Base.onMoveUsedEffects[i].OnMoveUsed(context);
        }
    }

    public void PlayShootAnimation(MoveAnimationHolder move, bool statChangeMove)
    {
        if (statChangeMove == false)
        {
            StartCoroutine(PlayShootMultipleParticlesAnimation(move));
        }
        else
        {
            PlayStatsChangeMoveAnimation(move);
        }
    }


    //-------------------------------------------------------------------------------------------------------
    // Shoot multiple particles -> monster moves scriptable objects configure amount, frequency, object, speed
    //-------------------------------------------------------------------------------------------------------
    public IEnumerator PlayShootMultipleParticlesAnimation(MoveAnimationHolder moveHolder)
    {

        if (moveHolder.ProjectileCount < moveHolder.CurrentMove.Base.MoveProjectileCount)
        {
            yield return new WaitForSeconds(moveHolder.CurrentMove.Base.MoveProjectileFrequency);

            GameObject ob;
            if (moveHolder.PreviousRotation == Quaternion.Euler(0, 0, 0))
            {
                moveHolder.PreviousRotation = Quaternion.Euler(0, 0, RotateObToMouse());
            }
            if (moveHolder.EnemyAttacking == false)
            {
                ob = Instantiate(moveHolder.CurrentMove.Base.MoveObject, GameUtils.i.battleSystem.PlayerUnit.transform.position, moveHolder.PreviousRotation, transform);
                ob.GetComponent<BattleProjectile>().Shooter = GameUtils.i.battleSystem.PlayerUnit.gameObject;
                ob.GetComponent<BattleProjectile>().Damage = GameUtils.i.battleSystem.PlayerUnit.CurrentMonster.TotalAttack;
                moveHolder.PreviousRotation = ob.transform.rotation;

            }
            else
            {
                ob = Instantiate(moveHolder.CurrentMove.Base.MoveObject, GameUtils.i.battleSystem.EnemyUnit.transform.position, RotateQuaternion(GameUtils.i.battleSystem.PlayerUnit.gameObject, moveHolder.EnemyPosition), transform);
                ob.transform.rotation = Quaternion.Euler(0, 0, RotateObToOtherOb(ob, GameUtils.i.battleSystem.PlayerUnit.gameObject));
                ob.GetComponent<BattleProjectile>().Shooter = GameUtils.i.battleSystem.EnemyUnit.gameObject;
                ob.GetComponent<BattleProjectile>().Damage = GameUtils.i.battleSystem.EnemyUnit.CurrentMonster.TotalAttack;
            }

            // Get if the move projectile is rotating
            for (int i = 0; i < ob.transform.childCount; i++)
            {
                ob.transform.GetChild(i).transform.rotation = Quaternion.Euler(0, 0, 0);
            }

            PlaySoundEffect(moveHolder.ShootSoundEffect);
            moveHolder.PreviousRotation = ob.transform.rotation;
            //AudioSystem.i.playRandomSound(AudioSystem.i.battle_fire_shoot, ob.transform, 1.5f);

            // Get if the move can cause status conditions
            if (moveHolder.CurrentMove.Status != null && moveHolder.CurrentMove.Status.Count > 0)
            {
                ob.GetComponent<BattleProjectile>().CanCauseStatus = true;
            }

            ConfigureProjectile(ob, moveHolder);
            ob.GetComponent<BattleProjectile>().Speed = moveHolder.CurrentMove.Base.MoveProjectileSpeed;
            ob.GetComponent<BattleProjectile>().Lifetime = 20;
            
            Destroy(ob, GetProjectileLifetime(moveHolder.CurrentMove));
            moveHolder.ProjectileCount++;
            StartCoroutine(PlayShootMultipleParticlesAnimation(moveHolder));

        }
        else
        {
            moveHolder.PreviousRotation = Quaternion.Euler(0, 0, 0);
            Destroy(moveHolder.gameObject, 5f);
        }
    }
    public void PlayShootSingleParticleAnimation(MoveAnimationHolder moveHolder)
    {
        GameObject ob;
        if (moveHolder.EnemyAttacking == false)
        {
            ob = Instantiate(moveHolder.CurrentMove.Base.MoveObject, GameUtils.i.battleSystem.PlayerUnit.transform.position, Quaternion.Euler(0, 0, RotateObToMouse()), transform);
            ob.GetComponent<BattleProjectile>().Shooter = GameUtils.i.battleSystem.PlayerUnit.gameObject;
            ob.GetComponent<BattleProjectile>().Damage = GameUtils.i.battleSystem.PlayerUnit.CurrentMonster.TotalAttack;

        }
        else
        {
            ob = Instantiate(moveHolder.CurrentMove.Base.MoveObject, GameUtils.i.battleSystem.EnemyUnit.transform.position, RotateQuaternion(GameUtils.i.battleSystem.PlayerUnit.gameObject, moveHolder.EnemyPosition), transform);
            ob.transform.rotation = Quaternion.Euler(0, 0, RotateObToOtherOb(ob, GameUtils.i.battleSystem.PlayerUnit.gameObject));
            ob.GetComponent<BattleProjectile>().Shooter = GameUtils.i.battleSystem.EnemyUnit.gameObject;
            ob.GetComponent<BattleProjectile>().Damage = GameUtils.i.battleSystem.EnemyUnit.CurrentMonster.TotalAttack;
        }

        PlaySoundEffect(moveHolder.ShootSoundEffect);
        ConfigureProjectile(ob, moveHolder);
        ob.GetComponent<BattleProjectile>().Speed = moveHolder.CurrentMove.Base.MoveProjectileSpeed;
        ob.GetComponent<BattleProjectile>().Lifetime = 20;
        Destroy(moveHolder.gameObject, 5f);
    }
    public void PlayShootSingleParticleNotRotateAnimation(MoveAnimationHolder moveHolder)
    {
        GameObject ob;
        if (moveHolder.EnemyAttacking == false)
        {
            ob = Instantiate(moveHolder.CurrentMove.Base.MoveObject, GameUtils.i.battleSystem.PlayerUnit.transform.position, Quaternion.Euler(0, 0, RotateObToMouse()), transform);
            ob.GetComponent<BattleProjectile>().Shooter = GameUtils.i.battleSystem.PlayerUnit.gameObject;
            ob.GetComponent<BattleProjectile>().Damage = GameUtils.i.battleSystem.PlayerUnit.CurrentMonster.TotalAttack;

        }
        else
        {
            ob = Instantiate(moveHolder.CurrentMove.Base.MoveObject, GameUtils.i.battleSystem.EnemyUnit.transform.position, RotateQuaternion(GameUtils.i.battleSystem.PlayerUnit.gameObject, moveHolder.EnemyPosition), transform);
            ob.transform.rotation = Quaternion.Euler(0, 0, RotateObToOtherOb(ob, GameUtils.i.battleSystem.PlayerUnit.gameObject));
            ob.GetComponent<BattleProjectile>().Shooter = GameUtils.i.battleSystem.EnemyUnit.gameObject;
            ob.GetComponent<BattleProjectile>().Damage = GameUtils.i.battleSystem.EnemyUnit.CurrentMonster.TotalAttack;
        }
        for (int i = 0; i < ob.transform.childCount; i++)
        {
            ob.transform.GetChild(i).transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        PlaySoundEffect(moveHolder.ShootSoundEffect);
        ConfigureProjectile(ob, moveHolder);
        ob.GetComponent<BattleProjectile>().Speed = moveHolder.CurrentMove.Base.MoveProjectileSpeed;
        ob.GetComponent<BattleProjectile>().Lifetime = 20;
        Destroy(moveHolder.gameObject, 5f);
    }
    public void PlayShootSingleStatusParticleAnimation(MoveAnimationHolder moveHolder)
    {

        GameObject ob;
        if (moveHolder.EnemyAttacking == false)
        {
            ob = Instantiate(moveHolder.CurrentMove.Base.MoveObject, GameUtils.i.battleSystem.PlayerUnit.transform.position, Quaternion.Euler(0, 0, RotateObToMouse()), transform);
            ob.GetComponent<BattleProjectile>().Shooter = GameUtils.i.battleSystem.PlayerUnit.gameObject;
        }
        else
        {
            ob = Instantiate(moveHolder.CurrentMove.Base.MoveObject, GameUtils.i.battleSystem.EnemyUnit.transform.position, RotateQuaternion(GameUtils.i.battleSystem.PlayerUnit.gameObject, moveHolder.EnemyPosition), transform);
            ob.transform.rotation = Quaternion.Euler(0, 0, RotateObToOtherOb(ob, GameUtils.i.battleSystem.PlayerUnit.gameObject));
            ob.GetComponent<BattleProjectile>().Shooter = GameUtils.i.battleSystem.EnemyUnit.gameObject;
        }
        PlaySoundEffect(moveHolder.ShootSoundEffect);
        ob.GetComponent<BattleProjectile>().Damage = 0;
        ob.GetComponent<BattleProjectile>().CanCauseStatus = true;
        ConfigureProjectile(ob, moveHolder);
        ob.GetComponent<BattleProjectile>().Speed = moveHolder.CurrentMove.Base.MoveProjectileSpeed;
        ob.GetComponent<BattleProjectile>().Lifetime = 20;
        //Destroy(ob, RockThrowSpeed);
        Destroy(moveHolder.gameObject, 5f);
    }
    
   public IEnumerator PlayStatusMoveAnimation(MoveAnimationHolder moveHolder)
{
    if (moveHolder == null ||
        moveHolder.CurrentMove == null ||
        moveHolder.CurrentMove.Base == null ||
        moveHolder.CurrentMove.Base.MoveObject == null)
    {
        yield break;
    }

    GameObject ob = Instantiate(
        moveHolder.CurrentMove.Base.MoveObject,
        moveHolder.PlayerPosition,
        Quaternion.identity,
        transform
    );

    BattleProjectile projectile = ob.GetComponent<BattleProjectile>();
    SpriteRenderer spriteRenderer = ob.GetComponent<SpriteRenderer>();

    if (projectile == null)
    {
        Debug.LogError(
            $"{ob.name} does not contain a BattleProjectile component."
        );

        Destroy(ob);
        yield break;
    }

    float time = moveHolder.CurrentMove.Base.StatChangesSelf.Time;

    if (!moveHolder.EnemyAttacking)
    {
        var playerUnit = GameUtils.i.battleSystem.PlayerUnit;

        playerUnit.StartCoroutine(
            playerUnit.ChangeStatsSelf(
                playerUnit.CurrentMove.StatChangesSelf
            )
        );

        playerUnit.StartCoroutine(
            playerUnit.DecreaseStatsSelf(
                time,
                playerUnit.CurrentMove.StatChangesSelf
            )
        );

        projectile.Shooter = playerUnit.gameObject;
        projectile.Damage = playerUnit.CurrentMonster.TotalAttack;
    }
    else
    {
        var enemyUnit = GameUtils.i.battleSystem.EnemyUnit;

        enemyUnit.StartCoroutine(
            enemyUnit.ChangeStatsSelf(
                enemyUnit.CurrentMove.StatChangesSelf
            )
        );

        enemyUnit.StartCoroutine(
            enemyUnit.DecreaseStatsSelf(
                time,
                enemyUnit.CurrentMove.StatChangesSelf
            )
        );

        projectile.Shooter = enemyUnit.gameObject;
        projectile.Damage = enemyUnit.CurrentMonster.TotalAttack;
    }

    projectile.MoveAnimator = moveHolder;
    projectile.Speed = 0f;
    projectile.IsCosmetic = true;

    PlaySoundEffect(moveHolder.ShootSoundEffect);

    // Prevent a negative waiting time when time is less than 1.
    float visibleDuration = Mathf.Max(0f, time - 1f);

    yield return new WaitForSeconds(visibleDuration);

    // The projectile may have been destroyed during the wait.
    if (ob == null)
    {
        if (moveHolder != null)
            Destroy(moveHolder.gameObject);

        yield break;
    }

    if (spriteRenderer != null)
    {
        spriteRenderer
            .DOFade(0f, 1f)
            .SetEase(Ease.Linear);
    }

    yield return new WaitForSeconds(1f);

    // It may also have been destroyed during the fade.
    if (ob != null)
        Destroy(ob);

    if (moveHolder != null)
        Destroy(moveHolder.gameObject);
}
    
    public IEnumerator PlayStatsChangeMoveAnimation(MoveAnimationHolder moveHolder)
    {
        GameObject ob = Instantiate(moveHolder.CurrentMove.Base.MoveObject, moveHolder.PlayerPosition, Quaternion.identity, transform);
        BattleProjectile bProj = ob.GetComponent<BattleProjectile>();
        if (moveHolder.CurrentMove.StatChangesSelf == null)
        {
            Destroy(ob);
            Destroy(moveHolder.gameObject);
            yield break;
        }

        float time = GetSelfAuraDuration(moveHolder.CurrentMove);
        BattleUnit auraUser = null;
        if (moveHolder.EnemyAttacking == false)
        {
            auraUser = GameUtils.i.playerUnit;
            GameUtils.i.playerUnit.StartCoroutine(GameUtils.i.playerUnit.ChangeStatsSelf(GameUtils.i.playerUnit.CurrentMove.StatChangesSelf));
            GameUtils.i.playerUnit.StartCoroutine(GameUtils.i.playerUnit.DecreaseStatsSelf(time, GameUtils.i.playerUnit.CurrentMove.StatChangesSelf));
            bProj.Shooter = GameUtils.i.battleSystem.PlayerUnit.gameObject;
            bProj.Damage = GameUtils.i.battleSystem.PlayerUnit.CurrentMonster.TotalAttack;
        }
        else
        {
            auraUser = GameUtils.i.battleSystem.EnemyUnit;
            GameUtils.i.battleSystem.EnemyUnit.StartCoroutine(GameUtils.i.battleSystem.EnemyUnit.ChangeStatsSelf(GameUtils.i.battleSystem.EnemyUnit.CurrentMove.StatChangesSelf));
            GameUtils.i.battleSystem.EnemyUnit.StartCoroutine(GameUtils.i.battleSystem.EnemyUnit.DecreaseStatsSelf(time, GameUtils.i.battleSystem.EnemyUnit.CurrentMove.StatChangesSelf));
            bProj.Shooter = GameUtils.i.battleSystem.EnemyUnit.gameObject;
            bProj.Damage = GameUtils.i.battleSystem.EnemyUnit.CurrentMonster.TotalAttack;
        }
        PlaySoundEffect(moveHolder.ShootSoundEffect);
        ConfigureProjectile(ob, moveHolder);
        bProj.Speed = 0;
        bProj.IsCosmetic = true;
        yield return new WaitForSeconds(Mathf.Max(0f, time - 1));
        ob?.GetComponent<SpriteRenderer>()?.DOFade(0, 1);
        yield return new WaitForSeconds(1);
        Destroy(ob, 1);
        Destroy(moveHolder.gameObject, 1);
    }

    private float GetSelfAuraDuration(BattleMove move)
    {
        if (move == null || move.Base == null)
            return 1f;

        if (move.StatChangesSelf != null && move.StatChangesSelf.Time > 0f)
            return move.StatChangesSelf.Time;

        if (move.Base.MoveProjectileRange > 0f)
            return move.Base.MoveProjectileRange;

        return 1f;
    }
    public IEnumerator PlayStrikeForwardAnimation(MoveAnimationHolder moveHolder, float lifetime)
    {
        GameObject ob;
        if (moveHolder.EnemyAttacking == false)
        {
            GameUtils.i.battleSystem.PlayerUnit.transform.DOBlendableMoveBy((Vector3)(GetPlayerAimDirection() * moveHolder.CurrentMove.Base.MoveProjectileRange), moveHolder.CurrentMove.Base.MoveProjectileSpeed);
            ob = Instantiate(moveHolder.CurrentMove.Base.MoveObject, moveHolder.PlayerPosition, Quaternion.Euler(0, 0, RotateObToMouse()), transform);
            //ob.transform.rotation = Quaternion.Euler(0, 0, RotateObToOtherOb(ob, GameUtils.i.battleSystem.PlayerUnit.gameObject));
            ob.GetComponent<BattleProjectile>().Shooter = GameUtils.i.battleSystem.PlayerUnit.gameObject;
            ob.GetComponent<BattleProjectile>().IsFollowingShooter = true;
            ob.GetComponent<BattleProjectile>().Damage = GameUtils.i.battleSystem.PlayerUnit.CurrentMonster.TotalAttack;
            SpawnDashParticle(moveHolder.PlayerPosition, GameUtils.i.battleSystem.PlayerUnit.gameObject, Quaternion.Euler(0, 0, RotateObToMouse()), false);
        }
        else
        {

            ob = Instantiate(moveHolder.CurrentMove.Base.MoveObject, GameUtils.i.battleSystem.EnemyUnit.transform.position, RotateQuaternion(GameUtils.i.battleSystem.PlayerUnit.gameObject, moveHolder.EnemyPosition), transform);
            ob.GetComponent<BattleProjectile>().Shooter = GameUtils.i.battleSystem.EnemyUnit.gameObject;
            ob.GetComponent<BattleProjectile>().IsFollowingShooter = true;
            ob.transform.rotation = Quaternion.Euler(0, 0, RotateObToOtherOb(ob, GameUtils.i.battleSystem.PlayerUnit.gameObject));
            GameUtils.i.battleSystem.EnemyUnit.gameObject.GetComponent<Rigidbody2D>().DOMove(GameUtils.i.battleSystem.PlayerUnit.transform.position, moveHolder.CurrentMove.Base.MoveProjectileSpeed);
            ob.GetComponent<BattleProjectile>().Damage = GameUtils.i.battleSystem.EnemyUnit.CurrentMonster.TotalAttack;
            SpawnDashParticle(GameUtils.i.battleSystem.EnemyUnit.transform.position, GameUtils.i.battleSystem.EnemyUnit.gameObject, RotateQuaternion(GameUtils.i.battleSystem.PlayerUnit.gameObject, moveHolder.EnemyPosition), true);
        }
        PlaySoundEffect(moveHolder.ShootSoundEffect);
        ConfigureProjectile(ob, moveHolder);
        ob.GetComponent<BattleProjectile>().Lifetime = lifetime;
        Destroy(ob, lifetime);
        if (moveHolder.EnemyAttacking == true)
        {
            GameUtils.i.battleSystem.EnemyUnit.gameObject.GetComponent<CapsuleCollider2D>().enabled = false;
            yield return new WaitForSeconds(lifetime);
            GameUtils.i.battleSystem.EnemyUnit.gameObject.GetComponent<CapsuleCollider2D>().enabled = true;
        }
        Destroy(moveHolder.gameObject, 5f);
    }
    public IEnumerator PlayStrikeDoubleAnimation(MoveAnimationHolder moveHolder)
    {
        GameObject ob;
        if (moveHolder.EnemyAttacking == false)
        {
            GameUtils.i.battleSystem.PlayerUnit.transform.DOPunchPosition((Vector3)(GetPlayerAimDirection() * moveHolder.CurrentMove.Base.MoveProjectileRange), moveHolder.CurrentMove.Base.MoveProjectileSpeed);
            ob = Instantiate(moveHolder.CurrentMove.Base.MoveObject, moveHolder.PlayerPosition, Quaternion.Euler(0, 0, RotateObToMouse()), transform);
            ob.transform.localScale = new Vector2(1.1f, 1.1f);
            ob.GetComponent<BattleProjectile>().Shooter = GameUtils.i.battleSystem.PlayerUnit.gameObject;
            ob.GetComponent<BattleProjectile>().Damage = GameUtils.i.battleSystem.PlayerUnit.CurrentMonster.TotalAttack;
        }
        else
        {
            ob = Instantiate(moveHolder.CurrentMove.Base.MoveObject, GameUtils.i.battleSystem.EnemyUnit.transform.position, RotateQuaternion(GameUtils.i.battleSystem.PlayerUnit.gameObject, moveHolder.EnemyPosition), transform);
            ob.GetComponent<BattleProjectile>().Shooter = GameUtils.i.battleSystem.EnemyUnit.gameObject;
            GameUtils.i.battleSystem.EnemyUnit.gameObject.GetComponent<Rigidbody2D>().DOMove(GameUtils.i.battleSystem.PlayerUnit.transform.position, moveHolder.CurrentMove.Base.MoveProjectileSpeed);
            ob.GetComponent<BattleProjectile>().Damage = GameUtils.i.battleSystem.EnemyUnit.CurrentMonster.TotalAttack;
        }
        PlaySoundEffect(moveHolder.ShootSoundEffect);
        ob.GetComponent<BattleProjectile>().IsFollowingShooter = true;
        ConfigureProjectile(ob, moveHolder);
        ob.GetComponent<BattleProjectile>().Lifetime = moveHolder.CurrentMove.Base.MoveProjectileSpeed;
        ob.GetComponent<SpriteRenderer>().DOFade(0.2f, moveHolder.CurrentMove.Base.MoveProjectileSpeed);
        Destroy(ob, moveHolder.CurrentMove.Base.MoveProjectileSpeed);
        if (moveHolder.EnemyAttacking == true)
        {
            GameUtils.i.battleSystem.EnemyUnit.gameObject.GetComponent<CapsuleCollider2D>().enabled = false;
            yield return new WaitForSeconds(moveHolder.CurrentMove.Base.MoveProjectileSpeed);
            GameUtils.i.battleSystem.EnemyUnit.gameObject.GetComponent<CapsuleCollider2D>().enabled = true;
        }
        Destroy(moveHolder.gameObject, 5f);

    }

    //TODO NOT WORKING
    //----------------------------------------------------------------------------------------------------------------
    //moveRange variable of move is used as Laser Length
    //moveSpeed variable of move is used as duration of laser
    //----------------------------------------------------------------------------------------------------------------
    public void PlayShootLaserAnimation(MoveAnimationHolder moveHolder)
    {

        GameObject ob;
        if (moveHolder.EnemyAttacking == false)
        {
            ob = Instantiate(moveHolder.CurrentMove.Base.MoveObject, GameUtils.i.battleSystem.PlayerUnit.transform.position, Quaternion.Euler(0, 0, RotateObToMouse()), transform);
            ob.GetComponent<BattleProjectile>().Shooter = GameUtils.i.battleSystem.PlayerUnit.gameObject;
            ob.GetComponent<BattleProjectile>().Damage = GameUtils.i.battleSystem.PlayerUnit.CurrentMonster.TotalAttack;

        }
        else
        {
            ob = Instantiate(moveHolder.CurrentMove.Base.MoveObject, GameUtils.i.battleSystem.EnemyUnit.transform.position, RotateQuaternion(GameUtils.i.battleSystem.PlayerUnit.gameObject, moveHolder.EnemyPosition), transform);
            ob.transform.rotation = Quaternion.Euler(0, 0, RotateObToOtherOb(ob, GameUtils.i.battleSystem.PlayerUnit.gameObject));
            ob.GetComponent<BattleProjectile>().Shooter = GameUtils.i.battleSystem.EnemyUnit.gameObject;
            ob.GetComponent<BattleProjectile>().Damage = GameUtils.i.battleSystem.EnemyUnit.CurrentMonster.TotalAttack;
        }
       
       
        ob.transform.DOScaleX(moveHolder.CurrentMove.Base.MoveProjectileRange, moveHolder.CurrentMove.Base.MoveProjectileSpeed);
        Vector2 size = ob.GetComponent<SpriteRenderer>().size;
        size.x = moveHolder.CurrentMove.Base.MoveProjectileRange; // This repeats the texture instead of stretching
        ob.GetComponent<SpriteRenderer>().size = size;

        PlaySoundEffect(moveHolder.ShootSoundEffect);
        ConfigureProjectile(ob, moveHolder);
        ob.GetComponent<BattleProjectile>().Speed = 0;
        ob.GetComponent<BattleProjectile>().Lifetime = 20;
        Destroy(moveHolder.gameObject, 5f);
    }

    private float GetProjectileLifetime(BattleMove move)
    {
        if (move == null || move.Base == null)
            return 20f;

        float speed = move.Base.MoveProjectileSpeed;
        float range = move.Base.MoveProjectileRange;
        if (speed <= 0f || range <= 0f)
            return 20f;

        return range / speed;
    }

   
}
