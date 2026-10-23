using UnityEngine;

public class BattleStateMachine
{
    IBattleState _current;  //  Current state playing

    /// <summary>
    /// Current state playing
    /// </summary>
    public IBattleState CurrentState => _current;

    /// <summary>
    /// Exits current state and enters the next state
    /// </summary>
    public void ChangeState(IBattleState next)
    {
        _current?.Exit();
        _current = next;
        _current?.Enter();
    }

    /// <summary>
    /// Plays every frame while in that state
    /// </summary>
    public void Tick() => _current?.Tick();

    /// <summary>
    /// Plays every fixed time frame while in that state
    /// </summary>
    public void FixedTick() => _current?.FixedTick();
}
