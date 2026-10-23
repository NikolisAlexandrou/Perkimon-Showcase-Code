using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LearnMoveService : MonoBehaviour
{
    [Header("Owner")]
    [Tooltip("Battle system subscribed")]
    [SerializeField] private BattleSystem _battleSystem;

    [Header("Learning monster details")]

    [Tooltip("UI that contains all objects for learning move")]
    [SerializeField] private GameObject _learnMoveUI;

    [Tooltip("Learning move monster image object")]
    [SerializeField] private Image _learningMonsterSprite;

    [Tooltip("Name text object of current monster that is learning new move")]
    [SerializeField] private TextMeshProUGUI _learnMoveMonsterName;

    [Tooltip("Level text object of current monster that is learning new move")]
    [SerializeField] private TextMeshProUGUI _learnMoveMonsterLvl;

    [Tooltip("First element image object of current monster that is learning new move")]
    [SerializeField] private Image _learnMoveMonsterElement1;

    [Tooltip("Second element image object of current monster that is learning new move")]
    [SerializeField] private Image _learnMoveMonsterElement2;

    [Header("Changable moves details")]

    [Tooltip("First changable move name text object")]
    [SerializeField] private TextMeshProUGUI _learnMove1Name;

    [Tooltip("Second changable move name text object")]
    [SerializeField] private TextMeshProUGUI _learnMove2Name;

    [Tooltip("Third changable move name text object")]
    [SerializeField] private TextMeshProUGUI _learnMove3Name;

    [Tooltip("Forth changable move name text object")]
    [SerializeField] private TextMeshProUGUI _learnMove4Name;

    [Tooltip("First changable move stamina cost text object")]
    [SerializeField] private TextMeshProUGUI _learnMove1cost;

    [Tooltip("Second changable move stamina cost text object")]
    [SerializeField] private TextMeshProUGUI _learnMove2cost;

    [Tooltip("Third changable move stamina cost text object")]
    [SerializeField] private TextMeshProUGUI _learnMove3cost;

    [Tooltip("Forth changable move stamina cost text object")]
    [SerializeField] private TextMeshProUGUI _learnMove4cost;

    [Tooltip("First changable move element image object")]
    [SerializeField] private Image _learnMove1element;

    [Tooltip("Second changable move element image object")]
    [SerializeField] private Image _learnMove2element;

    [Tooltip("Third changable move element image object")]
    [SerializeField] private Image _learnMove3element;

    [Tooltip("Forth changable move element image object")]
    [SerializeField] private Image _learnMove4element;

    [Header("Learning move details")]

    [Tooltip("Learning move name text object")]
    [SerializeField] private TextMeshProUGUI _learningMoveName;

    [Tooltip("Learning move stamina cost text object")]
    [SerializeField] private TextMeshProUGUI _learningMovecost;

    [Tooltip("Learning move element image object")]
    [SerializeField] private Image _learningMoveElement;


    // Current learning context
    private BattleMonster _currentLearningMonster;   //  Used to hold current monster tht is learning a move
    private BattleMove _learningMove;    //  Used to hold current move the monster is pending to learn

    // For battle-end flow
    private bool _changedMove;  //  Flag to check if move has been changed
    private bool _checkedThisBattle; //  Flag to check if there was a check in this battle that this monster has any pending moves to learn


    void Awake()
    {
        if (!_battleSystem)
            _battleSystem = GetComponent<BattleSystem>();
    }

    #region BATTLE-END FLOW API

    // ---------------------------------------------------------
    // BATTLE-END FLOW API
    // ---------------------------------------------------------

    /// <summary>
    /// Expose the current learning move in case other systems need it.
    /// </summary>
    public BattleMove GetCurrentLearningMove() => _learningMove;

    /// <summary>
    /// True when we have fully processed learn-move for this battle:
    /// - we've already done the initial CheckForLearningMoves()
    /// - AND there are no pending learning moves in any party member.
    /// </summary>
    public bool IsDone => _checkedThisBattle && NoPendingLearningAcrossParty();

    /// <summary>
    /// Called from BattleSystem's post-battle flow.
    /// Ensures CheckForLearningMoves() runs only once per battle.
    /// </summary>
    public void Process()
    {
        if (!_checkedThisBattle)
        {
            CheckForLearningMoves();
            _checkedThisBattle = true;
        }
    }

    /// <summary>
    /// Reset for a new battle.
    /// </summary>
    public void ResetState()
    {
        _changedMove = false;
        _checkedThisBattle = false;
        _currentLearningMonster = null;
        _learningMove = null;
        DisableLearnMoveUI();
    }



    /// <summary>
    /// Scans party for monsters that should learn new moves.
    /// If any are found, shows the learn-move UI for the first one.
    /// Sets ChangedMove = true only if no UI was opened.
    /// </summary>
    public void CheckForLearningMoves()
    {
        _changedMove = false;

        var party = PlayerStats.i.PlayerParty;
        for (int i = 0; i < Mathf.Min(5, party.Count); i++) //  Scan all members of the party
        {
            var mon = party[i];
            if (mon == null) continue;  //  Check if monster exists

            // Let the BattleMonster queue any new moves to learn
            bool queued = mon.LearnNewMove();
            if (queued || mon.CurrentLearningMoves.Count > 0)   //  Check if there are any moves that monster can learn
            {
                var move = mon.CurrentLearningMoves.Peek(); //  Get the first learning move in queue
                LearnNewMove(mon, move);
                return; // UI opened for first found
            }
        }

        // No learn-move UI opened
        _changedMove = true;
    }

    /// <summary>
    /// Returns true if NO party member (0..4) has pending learning moves in their queue.
    /// </summary>
    public bool NoPendingLearningAcrossParty()
    {
        var party = PlayerStats.i.PlayerParty;
        for (int i = 0; i < 5; i++) //  Scan all members in the party
        {
            if (i < party.Count)
            {
                if (party[i].CurrentLearningMoves.Count > 0)    //  There are pending moves to learn so return false
                    return false;
            }
        }
        return true;
    }

    #endregion

    // ---------------------------------------------------------
    // CORE LEARN-MOVE LOGIC
    // ---------------------------------------------------------

    /// <summary>
    /// Main entry to start learning a move for a specific monster.
    /// Handles "has empty slot" vs "must replace an existing move".
    /// </summary>
    public void LearnNewMove(BattleMonster mon, BattleMove move)
    {
        if (mon == null || move == null) return;

        // If monster already knows this move, do nothing
        if (mon.SearchCurrentMove(move))
            return;

        // If it has 4 moves already, open the replace-move UI
        if (mon.CurrentMoves.Count >= 4)
        {
            _currentLearningMonster = mon;
            _learningMove = move;
            EnableLearningMoveUI();
        }
        else
        {
            // Otherwise, just add the move and show "learned" dialogue
            //  DIALOGUE NOT YET ADDED
            mon.AddCurrentMoves(move);
            GameUtils.i.dialogueSystem.InitialiseNewLearnMove(
                move.Base.Name,
                mon.Base.Name,
                OnUseScrollToMonster
            );
        }
    }

    /// <summary>
    /// Called when this monster finished with one move and we might have more in its queue.
    /// </summary>
    public void CheckInQueueForMoves()
    {
        if (_currentLearningMonster == null) return;

        if (_currentLearningMonster.CurrentLearningMoves.Count > 0)  //  Check if there are any pending moves to learn
        {
            _learningMove = _currentLearningMonster.CurrentLearningMoves.Peek();
            EnableLearningMoveUI(); //  Open learn move UI to teach move
        }
    }

    /// <summary>
    /// Sets up the images and texts of details in UI of current index in current moves of the
    /// current monster that is going to learn a move.
    /// </summary>
    void EnableLearnMoveCell(int movepos, TextMeshProUGUI name, TextMeshProUGUI cost, Image element)
    {
        if (_currentLearningMonster == null || name == null || cost == null || element == null) return;

        if (movepos <= _currentLearningMonster.CurrentMoves.Count - 1)   //  Add move details if there is actually a move in that index
        {
            var mv = _currentLearningMonster.CurrentMoves[movepos];
            name.text = mv.Base.Name;
            cost.text = mv.StaminaCost.ToString();
            element.sprite = mv.Base.ElementType.icon;
        }
        else
        {
            //  There is no move in that index so add empty details
            name.text = " ";
            cost.text = " ";
            element.sprite = null;
        }
    }
    /// <summary>
    /// Enable the learning move UI and initialise its texts,images contents with
    /// detail of current monster that is going to learn a move
    /// </summary>
    void EnableLearningMoveUI()
    {
        if (_currentLearningMonster == null || _learningMove == null || _learnMoveUI == null) return;

        //  Setup the monster UI details (sprite, name, level, elements)
        _learningMonsterSprite.sprite = _currentLearningMonster.Base.frontSprite;
        _learnMoveMonsterName.text = _currentLearningMonster.Base.Name;
        _learnMoveMonsterLvl.text = _currentLearningMonster.Level.ToString();
        _learnMoveMonsterElement1.sprite = _currentLearningMonster.Base.element.icon;
        _learnMoveMonsterElement2.sprite = _currentLearningMonster.Base.secondaryElement.icon;

        _learnMoveUI.SetActive(true);

        //  Initialise all the current moves details
        EnableLearnMoveCell(0, _learnMove1Name, _learnMove1cost, _learnMove1element);
        EnableLearnMoveCell(1, _learnMove2Name, _learnMove2cost, _learnMove2element);
        EnableLearnMoveCell(2, _learnMove3Name, _learnMove3cost, _learnMove3element);
        EnableLearnMoveCell(3, _learnMove4Name, _learnMove4cost, _learnMove4element);

        //  Initialise the move pending to be learnt details
        _learningMoveName.text = _learningMove.Base.Name;
        _learningMoveElement.sprite = _learningMove.Base.ElementType.icon;
        _learningMovecost.text = _learningMove.StaminaCost.ToString();
    }

    // ---------------------------------------------------------
    // UI BUTTON HANDLERS
    // (BattleSystem can forward to these)
    // ---------------------------------------------------------

    /// <summary>
    /// Called when the player chooses which move slot (0..3) to replace.
    /// </summary>
    public void OnClickChangeMoveButton(int movenum, GameState currentGameState)
    {
        //  Check if chosen move to change is within current moves range
        if (_currentLearningMonster == null || _learningMove == null) return;
        if (movenum < 0 || movenum >= _currentLearningMonster.CurrentMoves.Count) return;

        //  Change the move here and close UI
        _currentLearningMonster.CurrentMoves[movenum] = _learningMove;
        _learnMoveUI.SetActive(false);

        //  If game state is FREEZE, that means player is NOT in battle so only way
        //  of changing a move NOT in battle is with using move scroll so OnUseScrollToMonster() is called

        if (currentGameState != GameState.FREEZE)   //  Check if in battle
        {
            if (_currentLearningMonster.CurrentLearningMoves.Count > 0)
                _currentLearningMonster.CurrentLearningMoves.Dequeue();

            CheckInQueueForMoves();
        }
        else
        {
            OnUseScrollToMonster();
        }
    }

    //  Mostly used in UI button
    /// <summary>
    /// Do not learn current learning move to monster so just remove it from queue without changing any moves
    /// </summary>
    public void OnClickNotLearnMove()
    {
        //  Remove the move from the queue
        if (_currentLearningMonster != null && _currentLearningMonster.CurrentLearningMoves.Count >= 1)
            _currentLearningMonster.CurrentLearningMoves.Dequeue();

        //  Close LearnMoveUI and check for other moves pending to be learnt
        _learnMoveUI.SetActive(false);
        CheckInQueueForMoves();

        // If party UI is in LEARNMOVE mode (scroll item), close it when done
        var ps = UIManager.i.PartyScene.GetComponent<PartyScript>();
        if (ps.partyState == PartyState.LEARNMOVE)
            _battleSystem.StartCoroutine(ps.closeUI());
    }

    /// <summary>
    /// Disables the learn move UI
    /// </summary>
    public void DisableLearnMoveUI()
    {
        if (_learnMoveUI != null)
            _learnMoveUI.SetActive(false);
    }

    /// <summary>
    /// Used as callback when learning a move from a move scroll item.
    /// </summary>
    public void OnUseScrollToMonster()
    {
        var party = UIManager.i.PartyScene.GetComponent<PartyScript>();
        BagManager.i.RemoveItemFromBag(party.currentUsingItem, 1);  //  Remove the scroll from the bag
        _battleSystem.StartCoroutine(BagManager.i.ClosePartyWhenEmpty(party.currentUsingItem));
    }

    // ---------------------------------------------------------
    // OVERWORLD / ITEM ENTRYPOINT
    // ---------------------------------------------------------

    /// <summary>
    /// Called when the player opens the party in LEARNMOVE mode
    /// (from an item like a move scroll).
    /// </summary>
    public void OnClickPartyButtonLearnMove(Item item)
    {
        UIManager.i.EnablePartyMenu(true);  //  Open the party UI
        var party = UIManager.i.PartyScene.GetComponent<PartyScript>();
        party.partyState = PartyState.LEARNMOVE;   //  Set the state to LEARNMOVE
        party.currentUsingItem = item;

        if (item?.ItemBase?.TeachableMove?.Base == null)
            return;

        _learningMove = new BattleMove(item.ItemBase.TeachableMove.Base);  //  Use a fresh runtime move instance for this monster
        _learningMove.Initialise();
    }
}
