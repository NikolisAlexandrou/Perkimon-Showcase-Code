using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// Handles EXP gain, level-up UI and "all awards done" tracking.
/// BattleSystem only calls high-level methods on this.
/// </summary>
public class BattleRewardService : MonoBehaviour
{
    [Header("Owner")]
    [SerializeField] private BattleSystem _battleSystem;

    [Header("Level Up Exp rewards")]

    [Tooltip("Multiplier for how much exp other party members get (Default 0.2)")]
    [SerializeField] private float _otherMembersExpMultiplier = 0.2f;

    [Header("Level Up UI Root")]

    [Tooltip("Holds the levelUp UI object")]
    [SerializeField] private GameObject _levelUpUI;

    [Tooltip("Holds the 'Press (button) to continue' UI text in level up UI")]
    [SerializeField] private TextMeshProUGUI _levelUpContinueText;

    [Header("Per Monster UI (1-5)")]

    [Tooltip("Holds all monster names UI texts in levelUp UI in ascending order")]
    [SerializeField] private TextMeshProUGUI[] _monsterNames = new TextMeshProUGUI[5];

    [Tooltip("Holds all monster levels UI texts in levelUp UI in ascending order")]
    [SerializeField] private TextMeshProUGUI[] _monsterLevels = new TextMeshProUGUI[5];

    [Tooltip("Holds all monster sprites UI images in levelUp UI in ascending order")]
    [SerializeField] private Image[] _monsterSprites = new Image[5];

    [Tooltip("Holds all monster Exp bars UI Sliders in levelUp UI in ascending order")]
    [SerializeField] private Slider[] _monsterExpBars = new Slider[5];

    [Tooltip("Holds all monster levelUp indicators UI texts in levelUp UI in ascending order")]
    [SerializeField] private GameObject[] _levelUpIndicators = new GameObject[5];

    // internal state
    private readonly bool[] _awarded = new bool[5];  //  each element corresponds a party member is ascending order
    private bool _canPressContinue;  //  Enable interaction with levelUp UI when finished to close it

    /// <summary>
    /// Checks if level Up UI is currently opened
    /// </summary>
    public bool IsLevelUpUIOpen => _levelUpUI && _levelUpUI.activeSelf;

    /// <summary>
    /// Check if all exp was given to all monsters
    /// </summary>
    public bool AllAwardsComplete => CheckAllAwardsComplete();

    /// <summary>
    /// Get if interaction has been enabled to close Level Up UI
    /// </summary>
    public bool CanPressContinue => _canPressContinue;

    void Awake()
    {
        // If not assigned in inspector, try to find automatically
        if (!_battleSystem)
            _battleSystem = GetComponent<BattleSystem>();
    }

    //-----------------------------------------------------------
    // PUBLIC API
    //-----------------------------------------------------------

    /// <summary>
    /// Reset internal flags and hide UI. Call when battle resets
    /// </summary>
    public void ResetRewards()
    {
        //  Reset all rewards flags
        for (int i = 0; i < _awarded.Length; i++)
            _awarded[i] = false;

        _canPressContinue = false;   //  Disable interaction for closing levelUp UI

        //  Hide Continue UI text
        if (_levelUpContinueText)
            _levelUpContinueText.gameObject.SetActive(false);

        //  Hide level Up UI
        if (_levelUpUI)
            _levelUpUI.SetActive(false);

        // hide level up indicators
        for (int i = 0; i < _levelUpIndicators.Length; i++)
            if (_levelUpIndicators[i])
                _levelUpIndicators[i].SetActive(false);
    }

    /// <summary>
    /// Entry point from BattleSystem when EXP should be awarded.
    /// </summary>
    public void StartAwardExp(BattleMonster enemyMonster)
    {
        if (!_levelUpUI || enemyMonster == null) return;

        //  Enable level Up UI
        _levelUpUI.SetActive(true);
        var party = PlayerStats.i.PlayerParty;
        if (party.Count == 0) return;   //  If there are no members dont do anything (may never happen just added for safe purposes)

        //  main (first) member (index 0) gets full EXP
        AwardToMember(0, party[0], enemyMonster, 1f);

        //  others (1-4) get reduced EXP
        for (int i = 1; i < 5; i++)
        {
            BattleMonster mon = (i < party.Count) ? party[i] : null;
            AwardToMember(i, mon, enemyMonster, _otherMembersExpMultiplier); //  1/5 = 0.2
        }
    }

    /// <summary>
    /// Called by BattleSystem when we have checked that
    /// there are no moves to learn etc. and we want to let
    /// the player close the LevelUp UI.
    /// </summary>
    public void EnableContinue()
    {
        _canPressContinue = true;
        if (_levelUpContinueText)
            _levelUpContinueText.gameObject.SetActive(true);
    }

    /// <summary>
    /// Closes the LevelUp UI with animation and resets flags.
    /// The callback is invoked after everything is closed.
    /// </summary>
    public void CloseLevelUpUI(System.Action onClosed)
    {
        _battleSystem.StartCoroutine(CloseLevelUpUIRoutine(onClosed));
    }

    //---------------------------------------------------------
    // INTERNAL
    //---------------------------------------------------------

    //  Give exp to a party member scaling with enemy monster killed
    private void AwardToMember(int uiIndex, BattleMonster mon, BattleMonster enemyMonster, float share)
    {
        //  No monster in this slot -> hide UI cell and mark awarded
        if (mon == null)
        {
            if (_monsterNames[uiIndex] && _monsterNames[uiIndex].transform.parent)
                _monsterNames[uiIndex].transform.parent.gameObject.SetActive(false);

            _awarded[uiIndex] = true;
            return;
        }

        if (_monsterNames[uiIndex] && _monsterNames[uiIndex].transform.parent)
            _monsterNames[uiIndex].transform.parent.gameObject.SetActive(true);

        //  basic stats
        _monsterNames[uiIndex].text = mon.Base.Name;
        if (_monsterSprites[uiIndex])
            _monsterSprites[uiIndex].sprite = mon.Base.frontSprite;

        //  gain EXP
        float expAmount = enemyMonster.MaxHP * enemyMonster.Level * share;
        mon.GainEXP(expAmount);

        //  start level-up coroutine
        _battleSystem.StartCoroutine(LevelUpRoutine(mon, uiIndex));
    }

    private IEnumerator LevelUpRoutine(BattleMonster mon, int index)
    {
        if (mon == null) yield break;

        var levelLabel = _monsterLevels[index];
        var expBar = _monsterExpBars[index];
        var lvlUpIndicator = _levelUpIndicators[index];

        levelLabel.text = "Level: " + mon.Level;

        float need = (100f * mon.Level * (1f + (mon.Level / 100f))) * mon.getExpGroupMultiplier();
        expBar.maxValue = need;

        expBar.DOValue(Mathf.Min(mon.Exp, need), 1f);

        if (mon.Exp >= need)
        {
            // Level up
            yield return new WaitForSeconds(1f);

            mon.Exp -= need;
            mon.Level += 1;

            if (lvlUpIndicator)
                lvlUpIndicator.SetActive(true);

            //  let BattleSystem handle evolution queueing
            _battleSystem.EnqueueEvolutionCandidate(mon);

            expBar.value = 0f;
            //  recurse until EXP is under next threshold
            _battleSystem.StartCoroutine(LevelUpRoutine(mon, index));
        }
        else
        {
            //  finished awarding this slot
            yield return new WaitForSeconds(1f);
            _awarded[index] = true;

            //  when all awards are done, allow Interact to close UI
            if (CheckAllAwardsComplete())
                EnableContinue();
        }
    }

    private bool CheckAllAwardsComplete()
    {
        for (int i = 0; i < _awarded.Length; i++)
            if (!_awarded[i]) return false;
        return true;
    }

    private IEnumerator CloseLevelUpUIRoutine(System.Action onClosed)
    {
        _canPressContinue = false;

        if (_levelUpContinueText)
            _levelUpContinueText.gameObject.SetActive(false);

        if (_levelUpUI)
        {
            var anim = _levelUpUI.GetComponent<Animator>();
            if (anim)
                anim.SetTrigger("CloseMenu");

            yield return new WaitForSeconds(0.2f);

            // hide indicators
            for (int i = 0; i < _levelUpIndicators.Length; i++)
                if (_levelUpIndicators[i])
                    _levelUpIndicators[i].SetActive(false);

            _levelUpUI.SetActive(false);
        }

        onClosed?.Invoke();
    }

}
