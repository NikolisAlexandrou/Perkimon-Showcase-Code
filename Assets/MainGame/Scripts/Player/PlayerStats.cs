using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats i;

    [Header("Player Party")]

    [Tooltip("Stores the monsters party members of the player")]
    [SerializeField] List<BattleMonster> _party;

    [Tooltip("Maximum amount of monsters the player's party can have at once (DEFAULT: 5 CHANGE AT YOUR OWN RISK)")]
    [SerializeField] int _maxPartyMembers = 5;

    [Header("Player Profile Stats")]

    [Tooltip("The name of the player throughout the game")]
    [SerializeField] string _playerName;

    [Tooltip("Amount of money the player is currently holding")]
    [SerializeField] int _money;

    [Tooltip("Amount of ALL monsters owned (includes duplicates)")]
    [SerializeField] int _monstersOwned;

    [Header("Tournament Stats")]

    [Tooltip("Number of tournaments the player has won")]
    [SerializeField] int _tournamentsWon;

    [Tooltip("Number of tournaments the player has lost")]
    [SerializeField] int _tournamentsLost;

    [Tooltip("Current tournament rank name of the player")]
    [SerializeField] string _tournamentRank;

    [Header("Battle Tower")]

    [Tooltip("Total number of trainers defeated in the battle tower")]
    [SerializeField] int _battleTowerTrainersDefeated;

    [Tooltip("Total number of boss trainers defeated in the battle tower")]
    [SerializeField] int _battleTowerBossesDefeated;

    [Tooltip("Current floor number of current battle tower run of player")]
    [SerializeField] int _battleTowerCurrentFloor;

    [Tooltip("The highest floor reached during the last battle tower run")]
    [SerializeField] int _battleTowerLastHighestFloorReached;

    [Tooltip("Lifetime highest floor reached by the player in the battle tower")]
    [SerializeField] int _battleTowerHighestFloorReached;

    //---------------------------------------------------------------------------------------------------------------------------
    //  0 indicates not defeated / 1 indicated defeated
    //  - Add a list of scriptable object called trial masters
    //  - Add another list that will store which trial masters has been defeated
    //---------------------------------------------------------------------------------------------------------------------------
    [Header("Trial Masters Defeated")]

    [Tooltip("Total number of boss trainers defeated")]
    [SerializeField] int _bossTrainersDefeated;

    [Tooltip("Returns true if Mouxtaris was defeated")]
    [SerializeField] bool _isMouxtarisDefeated;

    [Tooltip("Returns true if Geros was defeated")]
    [SerializeField] bool _isGerosDefeated;

    [Tooltip("Returns true if Nikos was defeated")]
    [SerializeField] bool _isNikosDefeated;

    [Header("Player Level")]

    [Tooltip("Get the current exp the player has")]
    [SerializeField] float _playerExp;

    [Tooltip("Get the current level of the player")]
    [SerializeField] int _playerLevel; //max 10, represented as stars in profile menu

    [Tooltip("Each index represents the level of player and each value in that index represents exp needed for level up")]
    [SerializeField] List<int> _expNeededForLevelUps = new List<int>();

    [Header("Skills Learned")] [SerializeField]
    private bool _hasSwimSkill = true;

    [Header("Story Stats")]

    [Tooltip("Stores the story cutscene number the player has currently finished")]
    [SerializeField] int _storyNumber;

    [Tooltip("Total number of sidequests completed by the player")]
    [SerializeField] int _sideQuestsCompleted;

    private void Awake()
    {
        if (i != null)
        {
            Destroy(i);
        }
        i = this;
    }

    /// <summary>
    /// Get the number of boss trainers defeated
    /// </summary>
    public int BossTrainersDefeated => _bossTrainersDefeated;
    public bool IsMouxtarisDefeated => _isMouxtarisDefeated;
    public bool IsGerosDefeated => _isGerosDefeated;
    public bool IsNikosDefeated => _isNikosDefeated;

    /// <summary>
    /// Get the highest floor reached during last battle tower run
    /// </summary>
    public int BattletowerLastHighestFloorReached => _battleTowerLastHighestFloorReached;

    /// <summary>
    /// Get the lifetime highest floor reached by player
    /// </summary>
    public int BattletowerHighestFloorReached => _battleTowerHighestFloorReached;

    /// <summary>
    /// Get the current floor number the player is currently in
    /// </summary>
    public int BattletowerCurrentFloor => _battleTowerCurrentFloor;

    /// <summary>
    /// Get the total number of trainers the player has defeated in all battle tower runs
    /// </summary>
    public int BattletowerTrainersDefeated => _battleTowerTrainersDefeated;

    /// <summary>
    /// Get the total number of boss trainers the player has defeated in all battle tower runs
    /// </summary>
    public int BattletowerBossesDefeated => _battleTowerBossesDefeated;

    /// <summary>
    /// Get the total number of lifetime tournaments won
    /// </summary>
    public int TournamentsWon => _tournamentsWon;

    /// <summary>
    /// Get the total number of lifetime tournaments lost
    /// </summary>
    public int TournamentsLost => _tournamentsLost;

    /// <summary>
    /// Get the current player tournament rank name
    /// </summary>
    public string TournamentRank => _tournamentRank;

    /// <summary>
    /// Get the total number of monsters owned (Including duplicates)
    /// </summary>
    public int MonstersOwned => _monstersOwned;

    /// <summary>
    /// Get the name of the player
    /// </summary>
    public string PlayerName => _playerName;

    /// <summary>
    /// Get the total number of sidequests completed
    /// </summary>
    public int OtherQuestsCompleted => _sideQuestsCompleted;

    /// <summary>
    /// Get the player's monsters party list 
    /// </summary>
    public List<BattleMonster> PlayerParty => _party;

    /// <summary>
    /// Get the max number of allowed party members
    /// </summary>
    public int maxPartyMembers => _maxPartyMembers;

    /// <summary>
    /// Get the player level
    /// </summary>
    public int PlayerLevel => _playerLevel;
    
    public bool HasSwimSkill => _hasSwimSkill;

    /// <summary>
    /// Get the Story number cutscene the player has finished
    /// </summary>
    public int StoryNumber { get => _storyNumber; set => _storyNumber = value; }

    /// <summary>
    /// Get the amount of money the player has
    /// </summary>
    public int Money { get => _money; set => _money = value; }

    /// <summary>
    /// Get the player exp and checks for level up
    /// </summary>
    public float PlayerExp
    {
        get => _playerExp;
        set
        {
            _playerExp = value;
            StartCoroutine(CheckPlayerLevelUp());
        }
    }

    /// <summary>
    /// Removes a specified monster from player party
    /// </summary>
    /// <param name="mon"></param>
    public void RemoveMonsterFromParty(BattleMonster mon) => _party.Remove(mon);

    /// <summary>
    /// Adds and initialises a specified monster to party if the party is not full
    /// </summary>
    /// <param name="mon"></param>
    public void AddMonsterToParty(BattleMonster mon)
    {
        if (_party.Count < maxPartyMembers)
        {
            mon.Initialise();
            _party.Add(mon);
        }
    }

    /// <summary>
    /// Adds and does not initialise a monster to player party with custom health value
    /// </summary>
    /// <param name="mon"></param>
    /// <param name="hp"></param>
    public void AddUninitialisedMonsterToParty(BattleMonster mon, float hp)
    {
        if (_party.Count < maxPartyMembers)
        {
            mon.HP = hp;
            _party.Add(mon);
        }
    }


    //------------------------------------------------------------------------------------------------------------------
    //  Player Level 
    //------------------------------------------------------------------------------------------------------------------
    //  HOW TO GAIN PLAYER EXP
    // - Catch perkimon: 1* (scale with level, max 20 exp) exp
    // - defeat wild perkimon: 1*(evolution stage) exp
    // - register perkimon to diary: 2 exp
    // - defeat trainers: 2*(party count) exp
    // - defeat boss trainers: 4*(party count) exp
    // - sidequests: rewards accordingly
    //------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// once level up occures exp goes to 0 so in each case is from 0 - maxlvl
    /// Gets the exp needed to level up from expNeededForLevelUps list with playerLevel
    /// indicating the position to search in list
    /// </summary>
    /// <returns></returns>
    private int CheckCurrentLevelMaxExpNeeded()
    {
        if (_expNeededForLevelUps.Count > _playerLevel)
            return _expNeededForLevelUps[PlayerLevel];
        else
            return 1000000;
    }

    /// <summary>
    /// Checks for level up with exp needed and if there is enough exp, level up and recheck
    /// for level up if there are still enough exp
    /// </summary>
    /// <returns></returns>
    private IEnumerator CheckPlayerLevelUp()
    {
        float max = CheckCurrentLevelMaxExpNeeded();
        // check if current exp is more than max exp needed to level up
        if (_playerExp >= max)
        {
            yield return new WaitForSeconds(.1f);
            // remove max exp from current exp, add a level to player and 
            _playerExp -= max;
            _playerLevel += 1;
            //_playerExp = 0;
            // rechecks for level up if enough exp are still there
            StartCoroutine(CheckPlayerLevelUp());
        }
    }
}
