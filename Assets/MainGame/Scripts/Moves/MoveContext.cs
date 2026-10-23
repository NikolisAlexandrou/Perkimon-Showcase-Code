using UnityEngine;

[System.Serializable]
public class MoveContext
{
    public BattleUnit User;
    public BattleUnit Target;
    public BattleMove Move;
    public BattleProjectile Projectile;
    public Vector2 Direction;
    public float Damage;
    public bool EnemyAttacking;
    public bool MoveUsedEffectsApplied;

    public MoveContext(BattleUnit user, BattleUnit target, BattleMove move, bool enemyAttacking)
    {
        User = user;
        Target = target;
        Move = move;
        EnemyAttacking = enemyAttacking;
        Damage = user != null && user.CurrentMonster != null ? user.CurrentMonster.TotalAttack : 0;

        if (user != null && target != null)
            Direction = (target.transform.position - user.transform.position).normalized;
    }
}
