using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class BattleMonster
{
    [SerializeField] private Monster _monster;  //  Monster base
    [SerializeField] private int _Level = 1;    //  Monster level, always initialised to 1
    [SerializeField] private Capsule _capsule;  //  Catching object that monster was caught (always add one to avoid errors)
    [SerializeField] private List<BattleMove> currentMoves = new List<BattleMove>();  //  the current moves the monster has equiped (max 4 moves)
    [SerializeField] private Abilities ability; // the current ability of the monster
    
    // Runtime state
    public Status Status { get;  set; } = Status.NONE;  //  Status conditions currently applied to this monster
    public float HP { get;  set; }  //  Current health value of current monster
    public float Stamina { get;  set; } //  Current stamina value of current monster
    public float Exp { get;  set; } //  Current Experience points of current monster
    public float TotalAttack { get;  set; } //  Current calculated total attack damage of current monster
    // Move collections
    public List<BattleMove> Moves { get; set; } = new List<BattleMove>();   //  list of all moves the monster can learn
    public Queue<BattleMove> CurrentLearningMoves { get; } = new Queue<BattleMove>();   //  when monster tries to learn a move that move is added to this queue to handle multiple learning moves at once

    // Temporary battle multipliers
    public float TempAttack { get; set; } = 1f;
    public float TempMagicAttack { get; set; } = 1f;
    public float TempDefense { get; set; } = 1f;
    public float TempStaminaRegen { get; set; } = 1f;
    public float TempSpeed { get; set; } = 1f;

    // Nature Points (percent bonuses, e.g., -10%..+10%)
    public float NPAttack { get; set; } //  permanent multiplier for this monster's physical attack
    public float NPMagicAttack { get; set; } //  permanent multiplier for this monster's magic attack
    public float NPDefense { get; set; } //  permanent multiplier for this monster's defense
    public float NPStaminaRegen { get; set; } //  permanent multiplier for this monster's endurance
    public float NPSpeed { get; set; } //  permanent multiplier for this monster's speed
    public bool HasAssignedNP { get; set; } //  this is checked at start to prevent reassigning when loading from save file

    // --- Accessors ---
    public Monster Base => _monster;
    public Capsule CapsuleCaught => _capsule;
    public Abilities Ability => ability;

    /// <summary>
    /// Current moves equipped (max 4)
    /// </summary>
    public List<BattleMove> CurrentMoves => currentMoves;

    /// <summary>
    /// Sets the current monster data to newMonster data
    /// </summary>
    public void SetMonster(Monster newMonster) => _monster = newMonster;

    /// <summary>
    /// Get current level of the monster
    /// </summary>
    public int Level
    {
        get => _Level;
        set => _Level = Mathf.Max(1, value);
    }

    /// <summary>
    /// Sets the current moves equipped to a list of moves
    /// </summary>
    public void SetCurrentMoves(List<BattleMove> newMoves) => currentMoves = newMoves;

    /// <summary>
    /// Adds a move to current moves equipped list
    /// </summary>
    public void AddCurrentMoves(BattleMove newMove) => currentMoves.Add(newMove);

    // --- Derived stats ---

    /// <summary>
    /// Scales physical attack stat based on Nature Point attack, base attack, temporary attack and level of monster
    /// </summary>
    public float PhysicalAttack => 
        ((Base.attack + (Base.attack * (NPAttack / 100f))) * TempAttack) * ((float)Level / 60f);

    /// <summary>
    /// Scales magical attack stat based on Nature Point magic attack, base magic attack, temporary magic attack and level of monster
    /// </summary>
    public float MagicalAttack =>
        ((Base.magic_attack + (Base.magic_attack * (NPMagicAttack / 100f))) * TempMagicAttack) * ((float)Level / 60f);

    /// <summary>
    /// scales monster's maximum health based on level and base health
    /// </summary>
    public float MaxHP => Base.health * (1f + (float)Level / 60f);

    /// <summary>
    /// scales speed of monster in battle scenes based on base speed, Nature Point speed and temp Speed
    /// </summary>
    public float Speed =>
        ((Base.speed + (Base.speed * (NPSpeed / 100f))) * TempSpeed);

    /// <summary>
    /// Default value for max stamina is 10
    /// </summary>
    public float MaxStamina => 10f;

    /// <summary>
    /// Adds monster level to difficulty to make it more difficult for higher level monsters to be caught
    /// </summary>
    public float CatchDifficulty => Base.catchDifficulty + Level;

    /// <summary>
    /// Scales defense stat based on Nature Point defense, base defense, temporary defense and level of monster
    /// </summary>
    public float Defense =>
        ((Base.defense + (Base.defense * (NPDefense / 100f))) * TempDefense) * ((float)Level / 60f);

    /// <summary>
    /// Scales endurance stat based on Nature Point endurance, base endurance, temporary endurance and level of monster
    /// </summary>
    public float StaminaRegenSpeed =>
        ((Base.staminaRegen + (Base.staminaRegen * (NPStaminaRegen / 100f))) * TempStaminaRegen) * (1f + (float)Level / 60f);

    /// <summary>
    /// Returns the status effect currently applied to monster
    /// </summary>
    public Status StatusEffect => Status;

    /// <summary>
    /// Set Health of monster to a value
    /// </summary>
    public void SetHP(float newHp) => HP = Mathf.Clamp(newHp, 0f, MaxHP);
    /// <summary>
    /// Add Health points to current health of monster
    /// </summary>
    public void AddHP(float delta) => SetHP(HP + delta);

    /// <summary>
    /// Set stamina to new value of monster
    /// </summary>
    public void SetStamina(float newStamina) => Stamina = Mathf.Clamp(newStamina, 0f, MaxStamina);
    /// <summary>
    /// Add stamina value to current stamina of monster
    /// </summary>
    public void AddStamina(float delta) => SetStamina(Stamina + delta);

    /// <summary>
    /// Set the level of the monster
    /// </summary>
    public void SetLevel(int lvl) => Level = lvl;

    // --- Lifecycle ---
    /// <summary>
    /// Initialises all data for Battle Monster
    /// </summary>
    public void Initialise()
    {
        // (1) Build/refresh full move list for this level
        InitialiseLearnableMoves(Moves, Base.learnableMoves);

        // (2) Keep editor preset moves when they exist; otherwise auto-fill from learnable moves.
        if (currentMoves.Count > 0)
        {
            for (int i = currentMoves.Count - 1; i >= 0; i--)
            {
                if (currentMoves[i] == null || currentMoves[i].Base == null)
                    currentMoves.RemoveAt(i);
            }

            if (currentMoves.Count > 4)
                currentMoves.RemoveRange(4, currentMoves.Count - 4);

            for (int i = 0; i < currentMoves.Count; i++)
                currentMoves[i].Initialise();
        }
        else
        {
            for (int i = 0; i < Mathf.Min(4, Moves.Count); i++)
            {
                var move = Moves[i];
                move.Initialise();
                currentMoves.Add(move);
            }
        }

        // (3) Initialise Base stats
        HP = MaxHP;
        Stamina = 0f;
        Status = Status.NONE;

        // (4) Temp multipliers are already defaulted to 1f, but reset for safety
        TempAttack = TempMagicAttack = TempDefense = TempStaminaRegen = TempSpeed = 1f;

        // (5) Roll NPs once
        InitialiseNaturePoints();
    }

    /// <summary>
    /// Initialise nature points once per monster creation
    /// </summary>
    public void InitialiseNaturePoints()
    {
        if (HasAssignedNP) return;

        // Nature Points can be from -NPRange% to NPRange%  Default: -10% to 10%
        NPAttack = Random.Range(-_monster.NPRange, _monster.NPRange + 1);
        NPMagicAttack = Random.Range(-_monster.NPRange, _monster.NPRange + 1);
        NPDefense = Random.Range(-_monster.NPRange, _monster.NPRange + 1);
        NPStaminaRegen = Random.Range(-_monster.NPRange, _monster.NPRange + 1);
        NPSpeed = Random.Range(-_monster.NPRange, _monster.NPRange + 1);
        HasAssignedNP = true;
    }

    // If monster is higher level than moves in baseMoves, add those moves in movesArray which are the current learned moves (NOT EQUIPPED MOVES)
    /// <summary>
    /// Initialise list of all moves the monster is able to learn at it's current level
    /// </summary>
    public void InitialiseLearnableMoves(List<BattleMove> movesArray, List<LearnableMove> baseMoves)
    {
        movesArray.Clear();

        foreach (var lm in baseMoves)
        {
            if (lm.Level <= Level)
            {
                var bm = new BattleMove(lm.Base);
                bm.Initialise();
                movesArray.Add(bm);
            }
        }
    }


    //  Selects physical or magical attack depending of move type
    //  Adds a small boost if element of monster matches element of move
    /// <summary>
    /// Calculate total attack with move stats
    /// </summary>
    public float CalculateMoveStats(BattleMove move)
    {
        TotalAttack = 0f;

        if (move == null || move.Base == null || Base == null)
            return TotalAttack;

        float atk = 0f;
        switch (move.Base.DamageType)
        {
            case MoveType.PHYSICAL:
                atk = PhysicalAttack;
                break;
            case MoveType.MAGICAL:
                atk = MagicalAttack;
                break;
            case MoveType.STATUS:
                return TotalAttack;
        }

        if (HasSameElement(move.Base.ElementType, Base.element) || HasSameElement(move.Base.ElementType, Base.secondaryElement))
            atk *= 1.05f;  //  Adds a 5% boost

        TotalAttack = move.Power * atk;
        return TotalAttack;
    }

    private bool HasSameElement(ElementType first, ElementType second)
    {
        if (first == null || second == null)
            return false;

        return first == second || first.displayName == second.displayName;
    }

    // --- EXP / Leveling ---
    /// <summary>
    /// Get exp needed to level up
    /// </summary>
    private float ExpToNextLevel()  //  Calculates exp needed for next level up
    {
        float L = Level;
        float baseNeed = 100f * L * (1f + (L / 100f));
        return baseNeed * getExpGroupMultiplier();
    }

    /// <summary>
    /// Add exp to monster
    /// </summary>
    public float GainEXP(float newexp)  //  Adds exp to monster
    {
        Exp += Mathf.Max(0f, newexp);
        return Exp;
    }

    /// <summary>
    /// Get multiplier for how much exp to add to monster depending on exp group
    /// </summary>
    public float getExpGroupMultiplier()
    {
        switch (_monster.expGroup)
        {
            case EXP_GROUP.SLOW: return 1.15f;  //  needs 15% more exp to level up
            case EXP_GROUP.FAST: return 0.90f;  //  needs 10% less exp to level up
            case EXP_GROUP.NORMAL:  //  normal amount of exp
            default: return 1f;
        }
    }

    //  Checks for level up when gaining exp
    /// <summary>
    /// Checks if monster has enough exp to level up and levels up if true
    /// </summary>
    public void CheckForLevelUP()
    {
        // Loop allows multiple level-ups at once.
        while (Exp >= ExpToNextLevel())
        {
            Exp -= ExpToNextLevel();
            Level += 1;
        }
        if (Exp < 0f) Exp = 0f;
    }

    // --- Move queries ---
    /// <summary>
    /// Returns true if monster is able to learn that move at that level
    /// </summary>
    public bool SearchLearnedMove(BattleMove move) //  searches if monster has this move in its moves learned (ALL moves not only current moves equipped)
    {
        for (int i = 0; i < Moves.Count; i++)
        {
            if (ReferenceEquals(Moves[i].Base, move.Base) || Moves[i].Base.name == move.Base.name)
                return true;
        }
        return false;
    }

    // Searches if monster has this move in its currently equipped moves ONLY
    /// <summary>
    /// Returns true if monster has this move in its currently equipped moves
    /// </summary>
    public bool SearchCurrentMove(BattleMove move)
    {
        for (int i = 0; i < currentMoves.Count; i++)
        {
            if (ReferenceEquals(currentMoves[i].Base, move.Base) || currentMoves[i].Base.name == move.Base.name)
                return true;
        }
        return false;
    }

    //  If applicable teach new move to monster
    //  if current moves are less than 4, add move to current moves
    //  if bigger than 4 display UI to player to select a move to change for this move
    /// <summary>
    /// Learns new move to monster (display learn move UI if current moves are bigger or equal to 4)
    /// </summary>
    public bool LearnNewMove()
    {
        bool promptPlayerChoice = false;

        for (int i = 0; i < Base.learnableMoves.Count; i++)
        {
            var lm = Base.learnableMoves[i];
            if (Level < lm.Level) continue;

            var mv = new BattleMove(lm.Base);
            mv.Initialise();

            if (!SearchLearnedMove(mv))
            {
                Moves.Add(mv);

                if (currentMoves.Count < 4)
                    currentMoves.Add(mv);
                else
                {
                    CurrentLearningMoves.Enqueue(mv);
                    promptPlayerChoice = true;
                }
            }
        }

        return promptPlayerChoice;
    }

    // --- UI helpers ---
    /// <summary>
    /// Updates value of slider of health bar depending on current health and max health, also adds colors
    /// </summary>
    public void updateHealth(Slider healthBar) 
    {
        // clamp first so bar never goes out of range
        HP = Mathf.Clamp(HP, 0f, MaxHP);
        float perc = MaxHP > 0f ? HP / MaxHP : 0f;
        healthBar.value = perc;

        // Avoid repeated GetComponent calls in hot paths
        var img = healthBar.fillRect != null ? healthBar.fillRect.gameObject.GetComponent<Image>() : null;
        if (img != null)
        {
            if (perc <= 0.25f) img.color = GameUtils.i.healthBarQuarterHPColor;
            else if (perc <= 0.5f) img.color = GameUtils.i.healthBarHalfHPColor;
            else img.color = GameUtils.i.healthBarFullHPColor;
        }
    }

    /// <summary>
    /// Updates value of slider for exp bar to display current exp
    /// </summary>
    public void updateExpBar(Slider expBar)
    {
        float need = ExpToNextLevel();
        expBar.value = need > 0f ? Mathf.Clamp01(Exp / need) : 0f;
    }
}
