using UnityEngine;

public abstract class Abilities : ScriptableObject
{
    public string Name;
    
    [TextArea]
    public string Description;
    
    
    public virtual void OnBattleStart(AbilityContext ctx) { }
    public virtual void OnEnemySwitch(AbilityContext ctx) { }
    public virtual void OnPlayerSwitch(AbilityContext ctx) { }
    public virtual void OnMoveUsed(AbilityContext ctx) { }
    public virtual void OnHitTarget(AbilityContext ctx) { }
    public virtual void OnDamageTaken(AbilityContext ctx) { }
    public virtual void OnStatusAppliedSelf(AbilityContext ctx) { }
    public virtual void OnStatusAppliedEnemy(AbilityContext ctx) { }
    public virtual void OnEnemyFainted(AbilityContext ctx) { }
    public virtual void OnTick(AbilityContext ctx) { }
}
