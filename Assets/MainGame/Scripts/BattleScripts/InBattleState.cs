using UnityEngine;

/// <summary>
/// Contains behaviours while in the battle state of the battle system state machine 
/// when entering state, game unfreezes and timer is initialised.
/// While game runs, handle battle running behaviours
/// </summary>
public class InBattleState : IBattleState
{

    readonly BattleSystem _battle;

    public InBattleState(BattleSystem battle) => _battle = battle;

    public void Enter()
    {
        // what you now do when entering INBATTLE
        _battle.InitialiseTimer();
        _battle.InterruptBattle = true;
    }

    public void Tick() => _battle.BattleTick(); // plays all methods while in battle that need updating every frame

    public void FixedTick() => _battle.BattleFixedTick();   //  Plays all methods while in battle that need fixed frame updating

    public void Exit() { }
}
