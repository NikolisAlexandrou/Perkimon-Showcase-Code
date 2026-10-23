using UnityEngine;

public interface IBattleState
{
    void Enter();
    void Exit();
    void Tick();
    void FixedTick();

}
