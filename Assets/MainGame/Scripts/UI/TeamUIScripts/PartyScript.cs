using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
public enum PartyState
{
    OVERWORLD,
    BATTLE,
    HEAL,
    LEARNMOVE
}

public class PartyScript : MonoBehaviour
{

    public PartyState partyState;
    public List<GameObject> members;
    public float healAmount;
    public Item currentUsingItem { get; set; }
    public GameObject statsHolderMenu;
    public GameObject changeMovesHolderMenu;

    [Header("General Info")]
    [SerializeField] BattleSystem battleScene;
    [SerializeField] GameObject statsHolderObject;
    [SerializeField] GameObject movesHolderObject;

    [SerializeField] Image selectedMonsterStatus;

    [SerializeField] TextMeshProUGUI selectedName;
    
    [SerializeField] TextMeshProUGUI selectedLvl;
    [SerializeField] TextMeshProUGUI selectedHP;
    [SerializeField] TextMeshProUGUI selectedAttack;
    [SerializeField] TextMeshProUGUI selectedMagic;
    [SerializeField] TextMeshProUGUI selectedStamina;
    [SerializeField] TextMeshProUGUI selectedSpeed;
    [SerializeField] TextMeshProUGUI selectedEndurance;

    //[SerializeField] TextMeshProUGUI NPHP;
    [SerializeField] TextMeshProUGUI NPAttack;
    [SerializeField] TextMeshProUGUI NPMagic;
    [SerializeField] TextMeshProUGUI NPDefense;
    [SerializeField] TextMeshProUGUI NPSpeed;
    [SerializeField] TextMeshProUGUI NPEndurance;


    [SerializeField] TextMeshProUGUI selectedMove1Name;
    [SerializeField] TextMeshProUGUI selectedMove2Name;
    [SerializeField] TextMeshProUGUI selectedMove3Name;
    [SerializeField] TextMeshProUGUI selectedMove4Name;

    [SerializeField] TextMeshProUGUI selectedMove1Staminacost;
    [SerializeField] TextMeshProUGUI selectedMove2Staminacost;
    [SerializeField] TextMeshProUGUI selectedMove3Staminacost;
    [SerializeField] TextMeshProUGUI selectedMove4Staminacost;

    [SerializeField] Image selectedMove1Element;
    [SerializeField] Image selectedMove2Element;
    [SerializeField] Image selectedMove3Element;
    [SerializeField] Image selectedMove4Element;



    [SerializeField] Image selectedImage;
    [SerializeField] Image selectedElement;
    [SerializeField] Image selectedElementSecondary;
    [SerializeField] private TextMeshProUGUI _selectedAbility;

    [Header("Current Move Stats")]
    [SerializeField] GameObject MoveStatsOb;
    [SerializeField] Image MoveStatsElementImage;
    [SerializeField] TextMeshProUGUI MoveStatsPhysicalSpecialText;
    [SerializeField] TextMeshProUGUI MoveStatsStaminaCostText;
    [SerializeField] TextMeshProUGUI MoveStatsNameText;
    [SerializeField] TextMeshProUGUI MoveStatsDescriptionText;
    [SerializeField] TextMeshProUGUI MoveStatsPowerText;

    [Header("Change Moves Info")]
    [SerializeField] GameObject ChangeMoveObject;
    [SerializeField] GameObject ChangeMoveContent;
    [SerializeField] GameObject MoveUIPrefab;

    [Header("Dead Info")]

    [SerializeField] Color aliveColor;
    [SerializeField] Color deadColor;

    public BattleMonster selectedMonster;
    public int selectedMonsterNum;
    public int currentMove;
    bool useItemPreCooldown;

    private void OnEnable()
    {

        InitialiseParty();
        useItemPreCooldown = true;
        StartCoroutine(resetPreCooldown());
    }

    public void EnableMoveStatsOB(bool enable)
    {
        MoveStatsOb.SetActive(enable);
    }

    public void SetDeadDisplay(int member, Image backgroundImage, Image nameBackgroundImage)
    {
        if (members.Count > member)
        {
            if (PlayerStats.i.PlayerParty[member].HP > 0)
            {
                backgroundImage.color = aliveColor;
                nameBackgroundImage.color = aliveColor;
            }
            else
            {
                backgroundImage.color = deadColor;
                nameBackgroundImage.color = deadColor;
            }
        }
    }

    public void ChangeCurrentMoveStatsTexts(Sprite element, string physicalspecialText, string stmncost, string name, string desc, string power)
    {
        MoveStatsElementImage.sprite = element;
        MoveStatsPhysicalSpecialText.text = physicalspecialText;
        MoveStatsStaminaCostText.text = "Stmna Cost: " +stmncost;
        MoveStatsNameText.text = name;
        MoveStatsDescriptionText.text = desc;
        MoveStatsPowerText.text = "Power: " + power;
    }

    IEnumerator resetPreCooldown()
    {
        yield return new WaitForSeconds(0.2f);
        useItemPreCooldown = false;
    }

    public void InitialiseParty()
    {
        foreach (GameObject item in members)
        {
            item.SetActive(true);
        }
        SelectPartyMember(0);
    }

    public void SelectPartyMember(int num)
    {
        BattleMonster mon = PlayerStats.i.PlayerParty[num];

        if (mon == null)
            return;

        selectedName.text = mon.Base.Name;
        selectedElement.sprite = mon.Base.element.icon;
        selectedElementSecondary.sprite = mon.Base.secondaryElement.icon;
        _selectedAbility.text = mon.Ability.Name;
        selectedLvl.text = "Lvl: "+mon.Level;
        selectedHP.text = "HP: " + (int)mon.MaxHP;
        selectedAttack.text = "Attack: " + (int)mon.PhysicalAttack;
        selectedMagic.text = "Magic: " + (int)mon.MagicalAttack;
        selectedStamina.text = "Defense: " + (int)mon.Defense;
        selectedSpeed.text = "Speed: " + (int)mon.Speed;
        selectedEndurance.text = "Endurance: " + (int)mon.StaminaRegenSpeed;

        //NPHP.text = (int)mon.np;
        NPAttack = ConvertNPText(NPAttack, mon.NPAttack);
        NPMagic = ConvertNPText(NPMagic, mon.NPMagicAttack);
        NPDefense = ConvertNPText(NPDefense, mon.NPDefense);
        NPSpeed = ConvertNPText(NPSpeed, mon.NPSpeed);
        NPEndurance = ConvertNPText(NPEndurance, mon.NPStaminaRegen);

        selectedImage.sprite = mon.Base.frontSprite;

        SetMoveSlot(selectedMove1Name, selectedMove1Staminacost, selectedMove1Element, mon, 0);
        SetMoveSlot(selectedMove2Name, selectedMove2Staminacost, selectedMove2Element, mon, 1);
        SetMoveSlot(selectedMove3Name, selectedMove3Staminacost, selectedMove3Element, mon, 2);
        SetMoveSlot(selectedMove4Name, selectedMove4Staminacost, selectedMove4Element, mon, 3);

        if (mon.Status != Status.NONE)
        {
            selectedMonsterStatus.gameObject.SetActive(true);
            selectedMonsterStatus.sprite = GameUtils.i.IdentifyStatusConditionSprite(mon.Status);
        }
        else
        {
            selectedMonsterStatus.gameObject.SetActive(false);
        }

      
        selectedMonster = mon;
        selectedMonsterNum = num;
    }

    void SetMoveSlot(TextMeshProUGUI nameText, TextMeshProUGUI staminaText, Image elementImage, BattleMonster mon, int moveIndex)
    {
        GameObject slot = nameText.transform.parent.gameObject;
        MoveHolderUI holder = slot.GetComponent<MoveHolderUI>();
        bool hasMove = mon.CurrentMoves.Count > moveIndex && mon.CurrentMoves[moveIndex] != null && mon.CurrentMoves[moveIndex].Base != null;

        slot.SetActive(hasMove);

        if (!hasMove)
        {
            holder?.SetMove(null);
            return;
        }

        nameText.gameObject.SetActive(true);
        staminaText.gameObject.SetActive(true);
        elementImage.gameObject.SetActive(true);

        BattleMove move = mon.CurrentMoves[moveIndex];
        holder?.SetMove(move);
        nameText.text = move.Base.Name;
        staminaText.text = move.StaminaCost.ToString();
        elementImage.sprite = move.Base.ElementType.icon;
    }

    public TextMeshProUGUI ConvertNPText(TextMeshProUGUI text, float np)
    {
        TextMeshProUGUI t = text;
        t.text = (int)np + "%";
        if (np >= 0)
        {
            t.color = Color.green;
        }
        else
        {
            t.color = Color.red;
        }
        return t;
    }

    public void onClickStatsButton()
    {
        deleteLearnableMoveContent();
        movesHolderObject.SetActive(false);
        statsHolderObject.SetActive(true);
    }

    public void onClickMovesButton()
    {
        statsHolderObject.SetActive(false);
        movesHolderObject.SetActive(true);
    }
    public void OnClickBackButton()
    {
        if (partyState == PartyState.BATTLE)
        {

            if (battleScene.PlayerUnit.IsDead || battleScene.PlayerUnit.CurrentMonster.HP <= 0)
                return;

            if (pressed == false)
            {
                pressed = true;
                StartCoroutine(closeUI());
            }

            battleScene.InitialiseTimer();
        }
        else
        {
            if (partyState == PartyState.HEAL || partyState == PartyState.LEARNMOVE)
            {
                partyState = PartyState.OVERWORLD;
            }
            if (pressed == false)
            {
                pressed = true;
                StartCoroutine(closeUI());
            }
        }
        deleteLearnableMoveContent();
    }
    bool pressed;
    public IEnumerator closeUI()
    {
        yield return new WaitForSeconds(.1F);
        GameUtils.i.battleSystem.DisableLearnMoveUI();
        UIManager.i.PartyScene.SetActive(false);
        pressed = false;
        BagManager.i.GetBagMenu().GetComponent<DynamicControllerUISetUp>().SetSelected(BagManager.i.GetBagMenu().GetComponent<DynamicControllerUISetUp>().GetElementContainer().transform.GetChild(0).gameObject);
    }
    public void EnableStatsHolderMenu()
    {
        statsHolderMenu.SetActive(true);
    }

    public bool StatsHolderMenuEnabled()
    {

        return statsHolderMenu.activeSelf;
    }

    public void DisableStatsHolderMenu()
    {
        
        statsHolderMenu.SetActive(false);
    }

    public void EnableMovesHolderMenu()
    {
        changeMovesHolderMenu.SetActive(true);
    }

    public void EnableStatsObject()
    {
        if (movesHolderObject.activeSelf == true)
            return;
        statsHolderObject.SetActive(true);
    }
    public void EnableMovesObject()
    {
        movesHolderObject.SetActive(true);
    }    
    public void DisableStatsObject()
    {
        if (movesHolderObject.activeSelf == false)
            return;
        statsHolderObject.SetActive(false);

    }
    public void DisableMovesHolderMenu()
    {

        changeMovesHolderMenu.SetActive(false);
    }
    public void OnClickChangeMove(int currentMoveNum)
    {
        if (partyState != PartyState.OVERWORLD)
            return;
        if (selectedMonsterNum < 0 || selectedMonsterNum >= PlayerStats.i.PlayerParty.Count)
            return;
        if (currentMoveNum < 0 || currentMoveNum >= PlayerStats.i.PlayerParty[selectedMonsterNum].CurrentMoves.Count)
            return;
        if (PlayerStats.i.PlayerParty[selectedMonsterNum].CurrentMoves[currentMoveNum] == null)
            return;


        //check if player is trying to view monster moves
        if (ChangeMoveObject.activeSelf == false || currentMove != currentMoveNum)
        {
            ChangeMoveObject.GetComponent<SelectionMenuScript>().menuContent.Clear();
            //ChangeMoveObject.GetComponent<SelectionMenuScript>().selectedImage.transform.SetParent(ChangeMoveContent.transform);
            for (int i = 0; i < ChangeMoveContent.transform.childCount; i++)
            {
                if (ChangeMoveContent.transform.GetChild(i).GetComponent<MoveChangeScript>() != null)
                {
                    Destroy(ChangeMoveContent.transform.GetChild(i).gameObject);
                }
            }
            currentMove = currentMoveNum;
            //enable monster moves UI
            ChangeMoveObject.SetActive(true);

            //initialise moves in scrollable UI
            for (int i = 0; i < selectedMonster.Moves.Count; i++)
            {
                GameObject ob = Instantiate(MoveUIPrefab, ChangeMoveContent.transform);
                ob.name = i.ToString();
                ob.GetComponent<MoveChangeScript>().InitialiseMove(selectedMonster.Moves[i].Base.Name, selectedMonster.Moves[i].Base.StaminaCost, selectedMonster.Moves[i].Base.ElementType.icon);
                ob.GetComponent<MoveChangeScript>().partyScript = this;
                ob.GetComponent<MoveChangeScript>().moveNumber = i;
                ChangeMoveObject.GetComponent<SelectionMenuScript>().menuContent.Add(ob);
            }
        }
        else
        {
            deleteLearnableMoveContent();
        }
    }
    public void deleteLearnableMoveContent()
    {
        if (ChangeMoveObject.activeSelf == false)
            return;


        //check if player presses on move to close UI

        //delete all move Objects;
       
        ChangeMoveObject.GetComponent<SelectionMenuScript>().menuContent.Clear();
        //ChangeMoveObject.GetComponent<SelectionMenuScript>().selectedImage.transform.SetParent(ChangeMoveContent.transform);
        for (int i = 0; i < ChangeMoveContent.transform.childCount; i++)
        {
            if (ChangeMoveContent.transform.GetChild(i).GetComponent<MoveChangeScript>() != null)
            {
                Destroy(ChangeMoveContent.transform.GetChild(i).gameObject);
            }
        }
        ChangeMoveObject.SetActive(false);
        EventSystem.current.SetSelectedGameObject(selectedMove1Name.transform.parent.gameObject);
    }
    public bool initialiseIndividual(Image sprite, TextMeshProUGUI name, TextMeshProUGUI level, int member, Slider health, Slider exp, Image capsuleSprite, Image BackG, Image BackGName)
    {
       
        List<BattleMonster> party = PlayerStats.i.PlayerParty;
        if (party.Count - 1 >= member)
        {
            
            name.text = party[member].Base.Name;
            level.text = "Lvl: " + party[member].Level;
            sprite.sprite = party[member].Base.frontSprite;
            capsuleSprite.sprite = party[member].CapsuleCaught.Image;
            party[member].updateHealth(health);
            party[member].updateExpBar(exp);
            SetDeadDisplay(member, BackG, BackGName);
            return true;
        }
        else
        {
            return false;
        }
    }
    UnityAction<int> useItemEvent;
    public void onUseItemToMonster(Item item, int member)
    {
        
        if (BagManager.i.SearchForItemInBag(item) == true)
        {
            useItemEvent?.Invoke(member);
           
        }
        else
        {
            OnClickBackButton();
        }
    }
    public void UseHealingItem(int member)
    {
        List<BattleMonster> party = PlayerStats.i.PlayerParty;
        if (party[member].HP >= party[member].MaxHP || party[member].HP <= 0)
            return;

        GameUtils.i.HealMonster(party[member], healAmount);
        BagManager.i.RemoveItemFromBag(currentUsingItem, 1);
       
    }


    public void ChangeActiveMonster(int member)
    {
        List<BattleMonster> party = PlayerStats.i.PlayerParty;
        if (useItemPreCooldown)
            return;

        // --- ITEM USE CASES FIRST (unchanged) ---

        if (partyState == PartyState.HEAL)
        {
            useItemEvent += UseHealingItem;

            onUseItemToMonster(currentUsingItem, member);

            useItemEvent -= UseHealingItem;
        }

        if (partyState == PartyState.LEARNMOVE)
        {
            if (!party[member].SearchCurrentMove(currentUsingItem.ItemBase.TeachableMove))
            {
                onUseItemToMonster(currentUsingItem, member);
                LearnMoveService srv = GameUtils.i.battleSystem.LearnMoveService;
                srv.LearnNewMove(party[member], srv.GetCurrentLearningMove());
            }
        }

        // If we're not in BATTLE state, don't try to switch the active monster.
        var battle = GameUtils.i.battleSystem;

        // If you use the enum:
        if (battle.CurrentState is not InBattleState)
            return;

        // (If you prefer the state-machine type check, you can instead do:)
        // if (battle.GetCurrentState() is not InBattleState)
        //     return;

        var partyService = battle.BattlePartyService;

        // If switching is on cooldown, do nothing
        if (!partyService.CanSwitch)
            return;

        // Already active? do nothing
        if (member == 0)
            return;

        // Safety: index & HP checks
        if (member < 0 || member >= party.Count)
            return;

        if (party[member].HP <= 0)
            return;

        // --- DO THE ACTUAL PARTY SWAP (same as before) ---

        BattleMonster temp = party[0];
        party[0] = party[member];
        party[member] = temp;

        // Tell the party service that a switch has been requested.
        // It will handle capsule, disabling old unit, cooldown, events, etc.
        partyService.NotifyPlayerChoseSwitch();
    }

    public void ChangeMonsterPositions(int currentMember, int toMember)
    {
        List<BattleMonster> party = PlayerStats.i.PlayerParty;
        //check if selected monster is not already active
        if (currentMember != toMember)
        {
            //switch them positions
            BattleMonster temp = party[currentMember];
            party[currentMember] = party[toMember];
            party[toMember] = temp;
        }
    }
}
