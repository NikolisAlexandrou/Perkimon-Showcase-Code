using UnityEngine;

/// <summary>
/// Behaviour for overworld state machine, handles behaviours for when game state enters
/// or exits overworld state.
/// </summary>
public class OverworldState : IBattleState
{
    readonly BattleSystem _battle;

    /// <summary>
    /// Setup the Overworld state
    /// </summary>
    public OverworldState(BattleSystem battle) => _battle = battle;

    public void Enter() => _battle.OnEnterOverworld();// what you now do when entering OVERWORLD

    public void Tick()
    {
        // Overworld-specific per-frame logic could go here later.
        // For now, nothing.
    }

    public void FixedTick()
    {
        // No physics needed in overworld from BattleSystem’s side.
    }

    public void Exit() => _battle.OnExitOverworld();
}
