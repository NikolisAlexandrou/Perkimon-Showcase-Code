using UnityEngine;

[CreateAssetMenu(fileName = "StatusCondition", menuName = "New Status Condition")]
public class StatusCondition : ScriptableObject
{
    [Tooltip("Status effect type that will be applied")]
    [SerializeField] private Status status;

    [Tooltip("Stat changes to monster that has this status effect")]
    [SerializeField] private StatIncreaseEffect statChangesOpponent;

    [Tooltip("The least amount of time the status effect is applied before clearing")]
    [SerializeField] private float durationLowest;

    [Tooltip("The maximum amount of time the status effect is applied before clearing")]
    [SerializeField] private float durationHighest;

    [Tooltip("The damage to target per second")]
    [SerializeField] private float healthPerSecond;

    [Tooltip("Disables movement of monster this status is applied to (MONSTER CAN STILL PERFORM ATTACKS NORMALY if disableAttacking is not checked)")]
    [SerializeField] private bool disableMovement;

    [Tooltip("Disables the ability for monster to attack when this status effect is applied")]
    [SerializeField] private bool disableAttacking;

    /// <summary>
    /// Get the status effect applied
    /// </summary>
    public Status StatusEffect => status;

    /// <summary>
    /// Get the stat changes affecting the attacked monster
    /// </summary>
    public StatIncreaseEffect StatChangesOpponent => statChangesOpponent;

    /// <summary>
    /// Get the lowest duration in seconds the effect will be applied before disappearing
    /// </summary>
    public float DurationLowest => durationLowest;

    /// <summary>
    /// Get the highest duration in seconds the effect will be applied before disappearing
    /// </summary>
    public float DurationHighest => durationHighest;

    /// <summary>
    /// Get how much health per second damage the attacked monster will take when the effect is active
    /// </summary>
    public float HPPerSecond => healthPerSecond;

    /// <summary>
    /// Get if the attacked monster movement is disabled when effect is active
    /// </summary>
    public bool DisableMovement => disableMovement;

    /// <summary>
    /// Get if the attacked monster's attack is disabled when effect is active
    /// </summary>
    public bool DisableAttacking => disableAttacking;
}
