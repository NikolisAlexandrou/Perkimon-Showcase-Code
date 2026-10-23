using UnityEngine;

/// <summary>
/// Behaviour for post battle state in the state machines. Handles all post battle
/// behaviours and flow when post battle state is running
/// </summary>
public class PostBattleState : IBattleState
{
    readonly BattleSystem _battle;

    /// <summary>
    /// Setup the post battle state
    /// </summary>
    public PostBattleState(BattleSystem battle) => this._battle = battle;

    /// <summary>
    /// Plays when entering the post battle state
    /// </summary>
    public void Enter()
    {
        // Freeze any normal battle input/logic
        _battle.InterruptBattle = true;

        // Hide party button etc.
        _battle.BattleHud.SetPartyButtonVisible(false);
    }

    /// <summary>
    /// Plays when postbattle state is running
    /// </summary>
    public void Tick()
    {
        // Let BattleSystem orchestrate all end-of-battle flow:
        // - OnBattleEndEvent
        // - AwardExp via OnAwardExpEvent
        // - Learn-move checks
        // - Evolutions
        // - Catching / end battle dialogue
        _battle.PostBattleTick();
    }

    public void FixedTick()
    {
        // No physics/stamina logic needed after battle is over
    }

    /// <summary>
    /// Plays when exiting the post battle state
    /// </summary>
    public void Exit()
    {
        // Nothing special; EndBattle() in BattleSystem moves us back to OVERWORLD
    }
}
