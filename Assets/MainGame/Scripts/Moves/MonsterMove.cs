using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "New move", fileName = "newmove")]
public class MonsterMove : ScriptableObject
{
    [Header("Properties of a Move")]

    [Tooltip("Actual name of move")]
    public string Name;

    [Tooltip("Description of move, displayed when hovering in party UI")]
    [TextArea]
    public string Description;

    [Tooltip("Element of move")]
    public ElementType ElementType;

    [Tooltip("Type of move, Physical uses attack of monster, Magical uses magic attack and status does no damage only for effects")]
    public MoveType DamageType;

    [Tooltip("Used for enemy AI to select a move depending on distance between player and ensure it will reach the player")]
    public MoveRange AIRange;

    [Header("Move Stats")]

    [Tooltip("The power of a move which is used in attack damage calculation")]
    public int Power = 50;

    [Tooltip("Stamina cost of the move in battle")]
    [Range(0,10f)]
    public float StaminaCost;

    [Tooltip("Amount of knockback this move applies directly when it hits a target")]
    public float Knockback = 4;

    [Tooltip("Amount of time this move's direct knockback is applied to the target")]
    public float KnockbackDuration = 0.1f;

    [Tooltip("List of status effects the move can cause to the enemy and their chances of causing per hit")]
    public List<StatusEffect> StatusEffects;

    [Tooltip("chance to apply multipliers to stats when opponent gets hit with current move")]
    //references: 0.5 = half the stat
    //             1  = no multiplier
    //             2 = double the stat
    public StatIncreaseEffect StatChangesOpponent;

    [Tooltip("chance to apply multipliers to stats on user when performing the current move")]
    //references: 0.5 = half the stat
    //             1  = no multiplier
    //             2 = double the stat
    public StatIncreaseEffect StatChangesSelf;

    [Header("Sound Effects")]

    [Tooltip("Play sounds when performing the move")]
    public List<AudioClip> ShootSoundEffect = new List<AudioClip>();

    [Tooltip("Play sounds when move hits an enemy")]
    public List<AudioClip> HitSoundEffect = new List<AudioClip>();

    [Tooltip("Play sounds when move get destroyed naturaly (collides or destroys itself)")]
    public List<AudioClip> DestroySoundEffect = new List<AudioClip>();

    [Header("Move Effects")]

    [Tooltip("List of effects that will get triggered when move is performed by monster")]
    public List<MoveEffect> onMoveUsedEffects;

    [Tooltip("List of effects that will get triggered when move is hits a target")]
    public List<MoveEffect> onMoveHitEffects;


    [Header("Move Animations")]

    [Tooltip("How this move should be performed in real-time battle. Legacy Auto keeps existing name-based behavior until the asset is configured.")]
    public MoveCastPattern CastPattern = MoveCastPattern.LegacyAuto;

    [Tooltip("Object prefab that the move will spawn (example: used for shooting projectiles)")]
    public GameObject MoveObject;   //  Prefab used that will get spawned when monster performs current move e.g. flame projectiles moving to direction of mouse dealing damage

    [Tooltip("Amount of move objects to spawn when performing move a single time")]
    public int MoveProjectileCount = 1;   //  For example when set to 3, spawns 3 flame objects when move is performed once meaning it is clicked once by player

    [Tooltip("time in seconds between the next spawn of each move object if count is bigger than 1")]
    public float MoveProjectileFrequency = 0; //  For example when set to 1 and count is 3 when move is performed, it will spawn 1 object, after 1 second another and repeat 3 times

    [Tooltip("Speed of move object travelling in direction shot")]
    public float MoveProjectileSpeed = 10;

    [Tooltip("Distance in units for move object shot to get destroyed by itself naturally")]
    public float MoveProjectileRange = 5;

    public bool PlayStatusAnimation;

    public bool IsRotating;

    public bool IsCosmetic;

    public bool IsFollowingShooter;

    public bool IsStationary;

    public bool DestroyOnImpact;

    public bool IsLaser;

    public bool IsDash;

    private void OnValidate()
    {
        if (MoveProjectileCount < 1)
            MoveProjectileCount = 1;

        if (MoveProjectileSpeed < 0)
            MoveProjectileSpeed = 0;

        if (MoveProjectileRange < 0)
            MoveProjectileRange = 0;

        if (StaminaCost < 0)
            StaminaCost = 0;

        if (Knockback < 0)
            Knockback = 0;

        if (KnockbackDuration < 0)
            KnockbackDuration = 0;

        if (MoveObject == null && CastPattern != MoveCastPattern.SelfAura)
            Debug.LogWarning($"{name} has no MoveObject assigned.", this);

        int statusEffectCount = StatusEffects == null ? 0 : StatusEffects.Count;
        int onUsedEffectCount = onMoveUsedEffects == null ? 0 : onMoveUsedEffects.Count;
        int onHitEffectCount = onMoveHitEffects == null ? 0 : onMoveHitEffects.Count;

        if (DamageType == MoveType.STATUS && statusEffectCount == 0 && onUsedEffectCount == 0 && onHitEffectCount == 0)
            Debug.LogWarning($"{name} is a status move with no status effects or custom move effects.", this);

        if (IsLaser && IsDash)
            Debug.LogWarning($"{name} is marked as both laser and dash. Prefer one cast pattern per move.", this);
    }
}

public enum MoveCastPattern
{
    LegacyAuto,
    SingleProjectile,
    BurstProjectile,
    SingleProjectileNoChildRotation,
    StatusProjectile,
    DashStrike,
    DoubleDashStrike,
    Laser,
    SelfAura
}

/// <summary>
/// The status effects that a move can deal to a monster
/// </summary>
public enum Status
{
    NONE,
    DROWN,
    BURN,
    SLEEP,
    FREEZE
}

/// <summary>
/// Type of move to know which attack stat to use
/// </summary>
public enum MoveType { PHYSICAL, MAGICAL, STATUS }

/// <summary>
/// Distance the move can go to ensure a hit to enemy (Used for enemy AI)
/// </summary>
public enum MoveRange { CLOSE, MID, FAR }

/// <summary>
/// An object of a status effect which contains the chance of that status to apply to a target
/// </summary>
[System.Serializable]
public class StatusEffect
{
    [SerializeField] private StatusCondition _status;
    [SerializeField] private int _chance;

    public StatusCondition Condition => _status;

    public int Chance => _chance;
}

/// <summary>
/// An object of stats changes to target 
/// </summary>
[System.Serializable]
public class StatIncreaseEffect
{
    [SerializeField] private float _attack = 1;
    [SerializeField] private float _magicAttack = 1;
    [SerializeField] private float _defense = 1;
    [SerializeField] private float _speed = 1;
    [SerializeField] private float _staminaRegen = 1;
    [SerializeField] private int _chance;    //  chance of this stats changed to get applied.
    [SerializeField] private float _time;    //  Duration the stats will change in seconds, after that duration stats go back to normal
    
    public float Attack => _attack;

    public float MagicAttack => _magicAttack;

    public float Defense => _defense;

    public float Speed => _speed;

    public float StaminaRegen => _staminaRegen;

    public float Time => _time;

    public int Chance => _chance;
}
