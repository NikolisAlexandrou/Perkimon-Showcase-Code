using System;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ------------------------------------------------------------------------------------------------
/// Level Scalability
/// ------------------------------------------------------------------------------------------------
///
/// - There is a starting area where the player spawns and levels will scale accordingly
/// - There is a Threat Level in which all levels will scale using that value:
///     1. Perkimon Defeated: +1 threat level
///     2. Trainers Defeated: +2 threat level
///     3. Miniboss Defeated: +8 Threat level
///
/// - Threat levels will have boundaries for tiering up
///     Threat Levels 0-7: Tier I
///     Threat Levels 8-15: Tier II
///     Threat Levels 16-23: Tier III
///     Threat Levels 24-31: Tier IV
///     Threat Levels 32-39: Tier V
///
/// TODO NOT DONE YET FOR 15/09/2026
/// 
/// Add wild perkimon levels being scalable
/// Add trainers level being scalable
/// Add Boss levels being scalable
/// 
/// </summary>

/// <summary>
/// Handles progression and difficulty scaling for the current run.
/// </summary>
public class RunProperties : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private ProceduralWorldGenerator _proceduralWorldGenerator;
    
    [Header("Threat Values")]

    [Tooltip("Threat gained after defeating a wild Perkimon")]
    [SerializeField] private int _wildMonsterThreatLevel = 1;

    [Tooltip("Threat gained after defeating a trainer")]
    [SerializeField] private int _trainerThreatLevel = 2;

    [Tooltip("Threat gained after defeating a miniboss / Trial Leader")]
    [SerializeField] private int _minibossThreatLevel = 8;


    [Header("Tier Groups")]
    [SerializeField] private List<TierGroup> _tierGroups = new();
    
    [Header("Miniboss Scaling")]
    [SerializeField] private List<MinibossTier> _minibossTiers = new();


    [Header("Run Progress")]

    [SerializeField] private int _currentThreatLevel;

    [SerializeField] private int _minibossesDefeated;

    [SerializeField] private int _minibossesRequiredForFinalArea = 3;


    public int CurrentThreatLevel => _currentThreatLevel;

    public int MinibossesDefeated => _minibossesDefeated;

    public bool FinalAreaUnlocked => _minibossesDefeated >= _minibossesRequiredForFinalArea;
    
    public event Action<TierGroup> OnTierChanged;
    
    public event Action<int> OnThreatChanged;
    
    public event Action<int> OnMinibossDefeated;
    
    public event Action OnFinalAreaUnlocked;

    private TierGroup _currentTier;

    public TierGroup CurrentTier => _currentTier;


    private void Awake()
    {
        UpdateCurrentTier();
    }


    /// <summary>
    /// Adds threat to the current run.
    /// </summary>
    public void AddThreat(int amount)
    {
        if (amount <= 0)
            return;

        _currentThreatLevel += amount;
        
        OnThreatChanged?.Invoke(_currentThreatLevel);

        UpdateCurrentTier();
    }


    /// <summary>
    /// Called when a wild Perkimon is defeated.
    /// </summary>
    public void WildPerkimonDefeated()
    {
        AddThreat(_wildMonsterThreatLevel);
    }


    /// <summary>
    /// Called when a trainer is defeated.
    /// </summary>
    public void TrainerDefeated()
    {
        AddThreat(_trainerThreatLevel);
    }


    /// <summary>
    /// Called when a miniboss is defeated.
    /// </summary>
    public void MinibossDefeated()
    {
        bool wasUnlocked = FinalAreaUnlocked;

        _minibossesDefeated++;

        OnMinibossDefeated?.Invoke(_minibossesDefeated);

        AddThreat(_minibossThreatLevel);

        if (!wasUnlocked && FinalAreaUnlocked)
        {
            OnFinalAreaUnlocked?.Invoke();
        }
    }

    /// <summary>
    /// Finds the tier corresponding to the current threat level.
    /// </summary>
    private void UpdateCurrentTier()
    {
        TierGroup newTier = GetTier(_currentThreatLevel);

        if (newTier == null)
            return;

        if (newTier == _currentTier)
            return;

        _currentTier = newTier;

        OnTierChanged?.Invoke(_currentTier);
    }


    /// <summary>
    /// Returns the tier corresponding to the supplied threat level.
    /// </summary>
    private TierGroup GetTier(int threatLevel)
    {
        foreach (TierGroup tier in _tierGroups)
        {
            if (threatLevel >= tier.MinThreat && threatLevel <= tier.MaxThreat)
            {
                return tier;
            }
        }

        // If threat exceeds every tier,
        // use the final tier.
        if (_tierGroups.Count > 0)
            return _tierGroups[^1];

        return null;
    }


    /// <summary>
    /// Resets all properties when starting a new run.
    /// </summary>
    public void ResetRun()
    {
        _currentThreatLevel = 0;
        _minibossesDefeated = 0;

        UpdateCurrentTier();
    }
    
    /// <summary>
    /// Get the level of the wild perkimon
    /// </summary>
    /// <returns></returns>
    public int GetRandomWildLevel()
    {
        if (_currentTier == null)
            return 1;

        return UnityEngine.Random.Range(_currentTier.MinWildLevel, _currentTier.MaxWildLevel + 1);
    }

    /// <summary>
    ///  Get the level of the trainer
    /// </summary>
    /// <returns></returns>
    public int GetRandomTrainerLevel()
    {
        if (_currentTier == null)
            return 1;

        return UnityEngine.Random.Range(_currentTier.MinTrainerLevel, _currentTier.MaxTrainerLevel + 1);
    }

    public int GetRandomMinibossLevel()
    {
        int index = Mathf.Clamp(_minibossesDefeated, 0, _minibossTiers.Count - 1);

        MinibossTier tier = _minibossTiers[index];
        return UnityEngine.Random.Range(tier.MinLevel, tier.MaxLevel + 1);
    }
}

[System.Serializable]
public class TierGroup
{
    [SerializeField] private string _tierName;

    [Header("Threat Range")]
    [SerializeField] private int _minThreat;
    [SerializeField] private int _maxThreat;

    [Header("Level Range")]
    [SerializeField] private int _minWildLevel;
    [SerializeField] private int _maxWildLevel;

    [SerializeField] private int _minTrainerLevel;
    [SerializeField] private int _maxTrainerLevel;

    [SerializeField] private int _minMinibossLevel;
    [SerializeField] private int _maxMinibossLevel;

    public string TierName => _tierName;

    public int MinThreat => _minThreat;
    public int MaxThreat => _maxThreat;

    public int MinWildLevel => _minWildLevel;
    public int MaxWildLevel => _maxWildLevel;

    public int MinTrainerLevel => _minTrainerLevel;
    public int MaxTrainerLevel => _maxTrainerLevel;

    public int MinMinibossLevel => _minMinibossLevel;
    public int MaxMinibossLevel => _maxMinibossLevel;
}

[System.Serializable]
public class MinibossTier
{
    [SerializeField] private int _minLevel;
    [SerializeField] private int _maxLevel;

    public int MinLevel => _minLevel;
    public int MaxLevel => _maxLevel;
}

