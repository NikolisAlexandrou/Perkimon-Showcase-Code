using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Creates a new monster with name, sprites, moves, stats, etc
/// 
/// - All overworld sprites need to be present for the monster to be able to exist in the overworld
/// - For only one element, set the secondaryElement to NONE
/// </summary>
[CreateAssetMenu(menuName = "New Monster", fileName = "Monster")]
public class Monster : ScriptableObject
{

    [Header("Properties of Monster")]

    [Tooltip("Actual name of monster.")]
    public string Name;

    [Tooltip("Description of monster, shown in Monster Diary.")]
    [TextArea]
    public string description;

    [Tooltip("Monster first element.")]
    public ElementType element;

    [Tooltip("Monster second element")]
    public ElementType secondaryElement;

    [Tooltip("The monster's ability")]
    public Abilities ability;

    [Tooltip("How much more exp the monster will be awarded, SLOW for less, NORMAL for normal, FAST for more exp.")]
    public EXP_GROUP expGroup;

    [Tooltip("Front view of monster sprite, displayed in UI menus.")]
    public Sprite frontSprite;

    [Tooltip("Monster sprite when it is asleep either in overworld or in battle.")]
    public Sprite OverworldSleepSprite;

    [Header("0 for front, " +
        "1 for left, " +
        "2 for right, " +
        "3 for back")]
    [Tooltip("Monster sprites shown in overworld when it is not moving, add 4 elements for 4 directions.")]
    public List<Sprite> overworldSprites;   //  0 frontSprite| 1 leftSprite| 2 rightSprite|3 BackSprite

    [Tooltip("List containing each frame of monster sprite animation when walking DOWN (loops every time).")]
    public List<Sprite> overworldWalkSpritesFront;

    [Tooltip("List containing each frame of monster sprite animation when walking LEFT (loops every time).")]
    public List<Sprite> overworldWalkSpritesLeft;

    [Tooltip("List containing each frame of monster sprite animation when walking RIGHT (loops every time).")]
    public List<Sprite> overworldWalkSpritesRight;

    [Tooltip("List containing each frame of monster sprite animation when walking UP (loops every time).")]
    public List<Sprite> overworldWalkSpritesBack;

    [Header("Base Stats")]

    [Tooltip("Base physical attack of monster.")]
    public float attack = 5;

    [Tooltip("Base magical attack of monster.")]
    public float magic_attack = 5;

    [Tooltip("Base health of monster.")]
    public int health = 60;

    [Tooltip("Base defense of monster.")]
    public float defense = 20;

    [Tooltip("Base stamina regeneration speed of monster also called Endurance.")]
    [Range(0, 100f)]
    public float staminaRegen;

    [Tooltip("Base walk speed of monster during battle.")]
    [Range(0, 10f)]
    public float speed;

    [Tooltip("Range from -NPRange% to +NPRange% for Nature points (IMPORTANT IF POSSIBLE KEEP 10% AS IT WAS DESIGNED)")]
    public float NPRange = 10;


    [Tooltip("Base catch difficulty (at full health), The higher the more difficult.")]
    public float catchDifficulty; //  chance at full health

    [Tooltip("Mass of monster, visible only in Monster Diary.")]
    public float mass;

    [Header("Moves")]

    [Tooltip("List containing ALL moves the monster can learn and at which level it learns it.")]
    public List<LearnableMove> learnableMoves;

    [Header("Location Found")]

    [Tooltip("A list containing all locations the monster can be found in")]
    public List<Area> areas_found = new List<Area>();

    [Tooltip("A list containing all environment types the monster can be found in")]
    public List<ENVIRONMENT_FOUND> environment_found = new List<ENVIRONMENT_FOUND>();

    [Header("Evolutions")]

    [Tooltip("When monster reaches that level, it will evolve to that monster")]
    public Evolution evolution;
}

// All environment types are listed
public enum ENVIRONMENT_FOUND
{
    LAND,
    CAVE,
    WATER,
    LAVA
}

// All experience group are listed
public enum EXP_GROUP
{
    SLOW,
    NORMAL,
    FAST
}


// An object which contains what level the monster should get to to learn that move
[System.Serializable]
public class LearnableMove
{
    [SerializeField] private MonsterMove moveBase;
    [SerializeField] private int level;

    /// <summary>
    /// Get the base move information
    /// </summary>
    public MonsterMove Base => moveBase;

    /// <summary>
    /// level at which the monster learns a move
    /// </summary>
    public int Level => level;
}

// An object which contains what level the current monster will evolve into the evolution monster
[System.Serializable]
public class Evolution
{
    [SerializeField] Monster monsterEvolution;
    [SerializeField] int level;

    /// <summary>
    /// Monster that this monster will evolve into
    /// </summary>
    public Monster MonsterEvolve => monsterEvolution;

    /// <summary>
    /// Level at which this monster will evolve to that monster
    /// </summary>
    public int Level => level;
}

/// <summary>
/// Contains a method that calculates damage multiplier depending on attack element and defending element
/// </summary>
public class TypeChart
{
    // Gets the effectiveness of attacking element when performing an attack on defending element
    public static float GetEffectiveness(ElementType attacker, ElementType defender)
    {
        if (attacker == null || defender == null)
            return 1f;

        if (attacker.noEffectAgainst.Contains(defender))
            return 0f;

        if (attacker.strongAgainst.Contains(defender))
            return 2f;

        if (attacker.weakAgainst.Contains(defender))
            return 0.5f;

        return 1f; // default normal damage
    }
}