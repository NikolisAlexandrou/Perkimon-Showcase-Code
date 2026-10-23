using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using TMPro;
public class CatchingManager : MonoBehaviour
{
    public static CatchingManager i;

    [Header("UI")]
    [SerializeField] GameObject CatchOrLeaveUI;
    [SerializeField] GameObject CatchingObjectsUI;
    [SerializeField] GameObject CatchingPowerMeterUI;
    [SerializeField] GameObject CatchingSystemOb;
    [SerializeField] GameObject TipText;
    [SerializeField] GameObject TrialCounterText;
    public List<Item> catchingObList = new List<Item>();
    [SerializeField] GameObject individualCatchingButtons;

    [Header("Objects")]
    [SerializeField] Item currentDisk;
    [SerializeField] GameObject CatchingObject;
    [SerializeField] Transform CatchingObjectInitialPosition;
    [SerializeField] float ObjectThrowTimer;
    public float EnemyInObjectTimer;
    [SerializeField] bool isPlayingCatchAnimation;

    [Header("Settings")]
    [SerializeField] float cooldown = .2f;
    bool canPress;
    private void Awake()
    {
        if (i != null)
        {
            Destroy(i);
        }
        i = this;
    }

    private void Update()
    {
        //check if back button is pressed in each senario
        if (Input.GetButtonDown("CloseMenu") && canPress == false)
        {
            //if catcher selector UI is enabled
            if (CatchingObjectsUI.activeSelf == true)
            {
                canPress = true;
                InitialiseCatchingSystem();
                StartCoroutine(resetCd());
            }
            else if (CatchingPowerMeterUI.activeSelf == true)
            {
                canPress = true;
                OnPressCatchButtonUI();
                BreakDisk();
                StartCoroutine(resetCd());
            }
        }
    }
    IEnumerator resetCd()
    {
        yield return new WaitForSeconds(cooldown);
        canPress = false;
    }

    public GameObject GetCatchingPowerMeterOb()
    {
        return CatchingPowerMeterUI;
    }
    public GameObject GetTrialCounterOb()
    {
        return TrialCounterText;
    }

    //plays when catching system ui is enabled
    public void InitialiseCatchingSystem()
    {
        CatchingSystemOb.SetActive(true);
        CatchOrLeaveUI.SetActive(true);
        CatchingObjectsUI.SetActive(false);
        CatchingPowerMeterUI.SetActive(false);
        TrialCounterText.SetActive(false);
        TipText.SetActive(false);
        GameUtils.i.battleSystem.InterruptBattle = true;
    }
    //plays when player selects catch button
    public void OnPressCatchButtonUI()
    {
        CatchOrLeaveUI.SetActive(false);
        CatchingObjectsUI.SetActive(true);
        CatchingPowerMeterUI.SetActive(false);
        TrialCounterText.SetActive(false);
        TipText.SetActive(false);
        InitialiseCatchingListUI();
        GameUtils.i.battleSystem.EnemyUnit.playSpawnAnimation();
    }
    //plays when player selects leave button
    public void OnPressLeaveButtonUI()
    {
        DisableCatchingSystem();
        isPlayingCatchAnimation = false;
        GameUtils.i.battleSystem.EnemyUnit.playDeathAnimation();
        GameUtils.i.battleSystem.DisplayEndBattleDialogue(false);
    }

    public void InitialiseCatchingListUI()
    {
        catchingObList.Clear();


       
        for (int i = 0; i < CatchingObjectsUI.transform.childCount; i++)
        {
            Destroy(CatchingObjectsUI.transform.GetChild(i).gameObject);
        }
        //initialises list for catching objects available in bag
        BagManager.i.ApplyCatchingDisksInBag(/*ref catchingObList*/);
        //spawn catching elements depending on list
        for (int i = 0 ; i < catchingObList.Count; i++)
        {
            GameObject ob = Instantiate(individualCatchingButtons, CatchingObjectsUI.transform);
            ob.GetComponent<CatchingElementUI>().SetCapsuleItem(catchingObList[i]);
            ob.GetComponent<CatchingElementUI>().InitialiseCapsule();
        }
        StartCoroutine(CatchingSystemOb.GetComponent<DynamicControllerUISetUp>().initDelay(true));
        
    }
    bool IsMultiple(int number, int divisor)
    {
        return number % divisor == 0;
    }

    //initialises new Power Meter when catching element is picked
    public void InitialisePowerMeterUI(Item NewItem)
    {
        currentDisk = NewItem;
        CatchOrLeaveUI.SetActive(false);
        CatchingObjectsUI.SetActive(false);
        CatchingPowerMeterUI.SetActive(true);
        TrialCounterText.SetActive(true);
        
        TipText.SetActive(true);
        CalculateNewPowerMeter();
    }
    public void CalculateNewPowerMeter()
    {
        float scale = currentDisk.ItemBase.GreenZoneIncreaseScale;
        float timeScale = currentDisk.ItemBase.PointerSpeedIncrease;
        int trials = currentDisk.ItemBase.MaxTrials;
        float monsterStrength = GameUtils.i.battleSystem.EnemyUnit.CurrentMonster.CatchDifficulty;
        int monsterLevel = GameUtils.i.battleSystem.EnemyUnit.CurrentMonster.Level;
        TrialCounterText.GetComponent<TextMeshProUGUI>().text = "0/" + trials;
        CatchingPowerMeterUI.GetComponent<PowerMeter>().InitialisePowerMeter(scale, monsterStrength, timeScale, trials, 1.2f, monsterLevel);
    }

   

    void SetNavigation(Selectable current, Selectable left, Selectable right, Selectable Up, Selectable Down)
    {
        Navigation nav = new Navigation();
        nav.mode = Navigation.Mode.Explicit; // Set manual navigation mode
        nav.selectOnLeft = left;
        nav.selectOnRight = right;
        nav.selectOnUp = Up;
        nav.selectOnDown = Down;

        current.navigation = nav; // Apply navigation to the button
    }

    public void DisableCatchingSystem()
    {
        CatchOrLeaveUI.SetActive(false);
        CatchingObjectsUI.SetActive(false);
        CatchingPowerMeterUI.SetActive(false);
        CatchingSystemOb.SetActive(false);
        TipText.SetActive(false);
    }

    GameObject currentCatch;

    public GameObject GetCurrentCatchOB()
    {
        return currentCatch;
    }
    public IEnumerator PlayCatchAnimationDelay(Item NewItem)
    {
        isPlayingCatchAnimation = true;
        currentDisk = NewItem;
        CatchOrLeaveUI.SetActive(false);
        CatchingObjectsUI.SetActive(false);
        CatchingPowerMeterUI.SetActive(false);
        BattleUnit EnemyUnit = GameUtils.i.battleSystem.EnemyUnit;
        //play throw animation
        GameObject _CatchObject = Instantiate(CatchingObject, new Vector3(CatchingObjectInitialPosition.position.x, CatchingObjectInitialPosition.position.y, -15), Quaternion.identity, GameUtils.i.battleSystem.BattleArena.transform);
        _CatchObject.GetComponent<SpriteRenderer>().sprite = currentDisk.ItemBase.CatchingObject.Capsule_Sprite;
        currentCatch = _CatchObject;
        _CatchObject.transform.DOMove(new Vector3(EnemyUnit.transform.position.x, EnemyUnit.transform.position.y, - 15), ObjectThrowTimer);
        yield return new WaitForSeconds(ObjectThrowTimer + 0.5f);
        //play enemy getting inside object animation
        EnemyUnit.GetComponent<SpriteRenderer>().DOColor(Color.red, EnemyInObjectTimer / 2);
        EnemyUnit.GetComponent<SpriteRenderer>().DOFade(0, EnemyInObjectTimer);
        yield return new WaitForSeconds(EnemyInObjectTimer + 0.2f);
        //isPlayingCatchAnimation = true;

        //Initialise power meter
        InitialisePowerMeterUI(NewItem);
    }
  
    public void FinishCatchingMonsterSuccess()
    {
        //add monster to party
        if (PlayerStats.i.PlayerParty.Count < 5)
        {
            PlayerStats.i.AddUninitialisedMonsterToParty(GameUtils.i.battleSystem.EnemyUnit.CurrentMonster, 10);
            UIManager.i.PartyScene.GetComponent<PartySelectionScript>().CreateNewPartyMember();
        }
        else//add monster to storage
        {
            MonsterStorageManager.i.AddMonsterToStorage(GameUtils.i.battleSystem.EnemyUnit.CurrentMonster, 10);
        }
        //register to diary as caught
        MonsterDiaryManager.i.AddMonsterToCaught(GameUtils.i.battleSystem.EnemyUnit.CurrentMonster.Base);

        BagManager.i.RemoveItemFromBag(currentDisk , 1);
        GameUtils.i.battleSystem.EnemyUnit.GetComponent<SpriteRenderer>().DOColor(Color.white, 1);
        GameUtils.i.battleSystem.EnemyUnit.gameObject.SetActive(false);
        isPlayingCatchAnimation = false;
        DisableCatchingSystem();
        //GameUtils.i.battleSystem.interruptBattle = false;
        //GameUtils.i.battleSystem.OnBattleEndEvent?.Invoke();
        GameUtils.i.battleSystem.DisplayEndBattleDialogue(true);
    }
    public void FinishCatchingMonsterFailure()
    {
        BreakDisk();
        InitialiseCatchingSystem();
    }

    public void BreakDisk()
    {
        BagManager.i.RemoveItemFromBag(currentDisk, 1);
        Destroy(currentCatch.gameObject);
        GameUtils.i.battleSystem.EnemyUnit.GetComponent<SpriteRenderer>().DOColor(Color.white, .5f);
        GameUtils.i.battleSystem.EnemyUnit.GetComponent<SpriteRenderer>().DOFade(1, .5f);
        GameObject ob = Instantiate(currentDisk.ItemBase.CatchingObject.Capsule_Particle.gameObject);
        ob.transform.position = GameUtils.i.battleSystem.EnemyUnit.transform.position;

        Destroy(ob.gameObject, 1f);

    }

}
