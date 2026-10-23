using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class BattleMove
{
    [SerializeField] private MonsterMove _move; // Current instance of a move
    public List<StatusEffect> Status { get; set; }  //  Status condition this move can apply to opponent

    public StatIncreaseEffect StatChangesOpponent { get; set; } //  Stats changes applied to target that this move hit
    public StatIncreaseEffect StatChangesSelf { get; set; } //  Stats changes applied to current monster that used this move
    public int Power { get; set; }  //  The power of this move which scales with monster attack/magic attack
    public float StaminaCost { get; set; }  //  The stamina cost to perform the move
    public float Knockback { get; set; }    //  Amount of knockback applied to enemy when hit
    public float KnockbackDuration { get; set; }    //  duration of knockback applied to enemy hit

    /// <summary>
    /// Set the battle move to a new move
    /// </summary>
    public BattleMove(MonsterMove newMove) => _move = newMove;    

    /// <summary>
    /// Get the base data of the move
    /// </summary>
    public MonsterMove Base => _move;


    /// <summary>
    /// Initialisation of stats for this instance of move
    /// </summary>
    public void Initialise()
    {
        if (Base == null)
            return;

        Power = Base.Power;
        StaminaCost = Base.StaminaCost;
        Status = Base.StatusEffects ?? new List<StatusEffect>();
        Knockback = Base.Knockback;
        KnockbackDuration = Base.KnockbackDuration;
        StatChangesOpponent = Base.StatChangesOpponent;
        StatChangesSelf = Base.StatChangesSelf;
    }
}
