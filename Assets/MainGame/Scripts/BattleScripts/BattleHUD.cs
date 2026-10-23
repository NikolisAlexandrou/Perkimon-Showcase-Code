using System;
using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Handles HUD responsibilities:
/// - Player/enemy name & level
/// - Move buttons (text + highlight + arrow)
/// - Party button visibility + cooldown text
/// 
/// BattleSystem calls these methods but doesn't touch TMP or GameObjects directly.
/// </summary>
public class BattleHUD : MonoBehaviour
{
    [Header("Battle System")]
    [SerializeField] private BattleSystem _battleSystem;

    [Header("Name & Level")]

    [Tooltip("Holds the player's monster name text UI")]
    [SerializeField] private TextMeshProUGUI _playerMonsterName;

    [Tooltip("Holds the player's monster level text UI")]
    [SerializeField] private TextMeshProUGUI _playerMonsterLevel;

    [Tooltip("Holds the enemy's monster name text UI")]
    [SerializeField] private TextMeshProUGUI _enemyMonsterName;

    [Tooltip("Holds the enemy's monster level text UI")]
    [SerializeField] private TextMeshProUGUI _enemyMonsterLevel;

    [Header("Moves")]

    [Tooltip("Holds the 4 moves UI objects that will display current equipped moves of current player monster")]
    [SerializeField] private List<GameObject> _movesUIObjects;

    [Tooltip("UI arrow image that shows which move is currently selected by player NEEDS TO BE A CHILD OF ")]
    [SerializeField] private GameObject _uiArrow;

    [Header("Party Button")]

    [Tooltip("Holds the party button that shows party UI and allow player to switch monsters")]
    [SerializeField] private GameObject _partyButton;
    
    [Header("Player Lose")]
    
    [Tooltip("The UI to display when player loses")]
    [SerializeField] private GameObject _playerLoseUI;

    [Tooltip("Text displayed when the player loses")]
    [SerializeField] private List<String> _playerLoseText;

    private string _cachedPlayerName;
    private int _cachedPlayerLevel;
    private string _cachedEnemyName;
    private int _cachedEnemyLevel;

    private int currentMoveNumber = 0;

    /// <summary>
    /// Get all moves in battle UI list
    /// </summary>
    public IReadOnlyList<GameObject> MovesUIObjects => _movesUIObjects;
    
    /// <summary>
    /// Text displayed when the player loses
    /// </summary>
    public List<String> PlayerLoseText => _playerLoseText;

    /// <summary>
    /// Enable the lose UI (connected with PlayerLost event in BattleSystem through inspector)
    /// </summary>
    /// <param name="enable"></param>
    public void EnablePlayerLoseUI(bool enable) => _playerLoseUI.SetActive(enable);

    // ------------------ NAMES / LEVELS ------------------

    /// <summary>
    /// Updates names and level details in battle UI of both current battle monsters
    /// </summary>
    public void RefreshNamesAndLevels(BattleUnit playerUnit, BattleUnit enemyUnit)
    {
        if (enemyUnit?.CurrentMonster != null)
        {
            var name = enemyUnit.CurrentMonster.Base?.Name ?? "";
            var lvl = enemyUnit.CurrentMonster.Level;

            if (_cachedEnemyName != name)
            {
                _cachedEnemyName = name;
                _enemyMonsterName.text = name;   //  Set UI name for enemy
            }

            if (_cachedEnemyLevel != lvl)
            {
                _cachedEnemyLevel = lvl;
                _enemyMonsterLevel.text = "Lvl: " + lvl; //  Set UI level for enemy
            }
        }

        if (playerUnit?.CurrentMonster != null)
        {
            var name = playerUnit.CurrentMonster.Base?.Name ?? "";
            var lvl = playerUnit.CurrentMonster.Level;

            if (_cachedPlayerName != name)
            {
                _cachedPlayerName = name;
                _playerMonsterName.text = name;  //  Set UI name for player
            }

            if (_cachedPlayerLevel != lvl)
            {
                _cachedPlayerLevel = lvl;
                _playerMonsterLevel.text = "Lvl: " + lvl;    //  Set UI level for player
            }
        }
    }

    // ------------------ MOVES ------------------

    /// <summary>
    /// Sets up the current equipped moves details in each UI move object based on the monster's current moves.
    /// </summary>
    public void InitialiseMoves(BattleUnit monster)
    {
        if ( monster?.CurrentMonster == null) return;   //  Checks if moves are not already initialised

        // Disable all
        for (int j = 0; j < _movesUIObjects.Count; j++)
            _movesUIObjects[j].SetActive(false);

        // Enable up to 4 moves
        int show = Mathf.Min(4, Mathf.Min(_movesUIObjects.Count, monster.CurrentMonster.CurrentMoves.Count));   //  Gets total number of currently equipped moves MAX 4
        for (int i = 0; i < show; i++)
        {
            var cell = _movesUIObjects[i];   //  Enable the same number of moves in UI as current moves of monster
            cell.SetActive(true);
            var init = cell.GetComponent<MoveInitialiser>();    //  This script holds all details about UI move e.g. name, stamina cost
            if (init != null)
            {
                init.Move = monster.CurrentMonster.CurrentMoves[i];  //  Assign the current move to UI move object so that it is able to get details and initialise it to texts UI
                init.InitialiseMove();  //  Sets the name and stamina cost values of move to the UI
            }
        }
        SelectMove(monster, 0); //  Selects the first current move
    }
    /// <summary>
    /// Highlights the selected move and positions the arrow next to it.
    /// </summary>
    public void SelectMove(BattleUnit monster, int index)
    {
        if (monster == null) return;
        if (index < 0 || index >= monster.CurrentMonster.CurrentMoves.Count) return;   //  Checks if move is actually in the range of index (0-3)

        monster.CurrentMove = monster.CurrentMonster.CurrentMoves[index];

        // Reset all labels to white
        for (int i = 0; i < Mathf.Min(4, _movesUIObjects.Count); i++)
        {
            var txt = _movesUIObjects[i].GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null) txt.DOColor(Color.white, 0.2f);
        }

        // Highlight selected index
        if (index < _movesUIObjects.Count)
        {
            var cell = _movesUIObjects[index];
            var txt = cell.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null) txt.DOColor(Color.green, 0.2f);

            if (_uiArrow != null)    //  Put the UI arrow right next to the move UI object that is selected
            {
                _uiArrow.transform.SetParent(cell.transform, worldPositionStays: true);
                var pos = _uiArrow.transform.position;
                _uiArrow.transform.position = new Vector2(pos.x, cell.transform.position.y);
            }
        }

        currentMoveNumber = index;
    }


    /// <summary>
    /// Select move and update its UI indication using scroll wheel or number keys
    /// </summary>
    /// <param name="monsterUnit"></param>
    public void ChangeActiveMove(BattleUnit monsterUnit)
    {
        bool changed = false;

        if (_battleSystem.BattleInputHandler.MoveScroll < 0 || Input.GetButtonDown("ChangeMoveDown"))
        {
            if (monsterUnit?.CurrentMonster == null) return;
            currentMoveNumber = (currentMoveNumber + 1) % Mathf.Max(1, monsterUnit.CurrentMonster.CurrentMoves.Count);
            changed = true;
        }
        else if (_battleSystem.BattleInputHandler.MoveScroll > 0 || Input.GetButtonDown("ChangeMoveUp"))
        {
            if (monsterUnit?.CurrentMonster == null) return;
            currentMoveNumber = (currentMoveNumber - 1 + monsterUnit.CurrentMonster.CurrentMoves.Count) % Mathf.Max(1, monsterUnit.CurrentMonster.CurrentMoves.Count);
            changed = true;
        }

        if (_battleSystem.BattleInputHandler.MoveHotkeyPressed != -1)
        {
            currentMoveNumber = _battleSystem.BattleInputHandler.MoveHotkeyPressed; 
            changed = true;
        }

        if (changed) SelectMove(monsterUnit, currentMoveNumber);
    }

    /// <summary>
    /// Get the number of move currently selected
    /// </summary>
    /// <returns></returns>
    public int GetCurrentMoveNumber() => currentMoveNumber;

    // ------------------ PARTY BUTTON ------------------

    /// <summary>
    /// Set whether party button will be visible or not
    /// </summary>
    /// <param name="visible"></param>
    public void SetPartyButtonVisible(bool visible)
    {
        if (_partyButton != null)
            _partyButton.SetActive(visible);
    }

    /// <summary>
    /// Set the timer display indicating the cooldown of the party
    /// </summary>
    /// <param name="remainingTime"></param>
    /// <param name="canSwitch"></param>
    public void UpdatePartyButtonCooldown(float remainingTime, bool canSwitch)
    {
        if (!_partyButton) return;

        var label = _partyButton.GetComponentInChildren<TextMeshProUGUI>();
        if (!label) return;

        if (canSwitch)
            label.text = "";
        else
            label.text = ((int)remainingTime).ToString();
    }
}
