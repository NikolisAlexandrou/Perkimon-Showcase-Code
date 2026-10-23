using UnityEngine;

/// <summary>
/// Behaviour for freeze state machine, the game will freeze, there are no updates, and if in battle,
/// battle gets interrupted and every moving object freezes.
/// </summary>
public class FreezeState : IBattleState
{
    
    readonly BattleSystem _battle;

    /// <summary>
    /// Setup the freeze state
    /// </summary>
    public FreezeState(BattleSystem battle) => _battle = battle;

    public void Enter() => _battle.InterruptBattle = true; // Freeze can mean: stop stamina, stop inputs, etc.

    public void Exit() => _battle.InterruptBattle = false; // When unfreezing, allow battle again if still in battle.

    public void Tick() { }  // Intentionally empty – no updates when frozen.

    public void FixedTick() { } // No stamina gain, no physics from battle.
}
