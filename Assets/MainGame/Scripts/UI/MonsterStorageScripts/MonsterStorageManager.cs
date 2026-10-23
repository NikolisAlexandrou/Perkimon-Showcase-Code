using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class MonsterStorageManager : MonoBehaviour
{
    public EventSystem eventSystem;
    [SerializeField] List<BattleMonster> MonsterStorageList = new List<BattleMonster>();// holds monsters, 400 max first 5 elements are party members
    public static MonsterStorageManager i;
   
    [Header("Storage Units")]
    [SerializeField] Sprite missingCapsuleSprite;
    [SerializeField] Sprite missingMonSprite;
    [SerializeField] GameObject storageContainer;
    [SerializeField] GameObject partyContainer;
    [SerializeField] GameObject storageUnitOb;
    [SerializeField] GameObject storageScrollContent;
    public int currentUnitNum;
    public StorageUnit currentUnit;
    private SelectionMenuScript storageSelectionMenu;
    private GridLayoutGroup storageGridLayout;
    private VerticalLayoutGroup partyVerticalLayout;
    private int lastCurrentUnitNum = -1;

    [Header("Selected Monster Details")]
    [SerializeField] GameObject StatsOb;
    [SerializeField] GameObject MovesOb;
    [SerializeField]Image MonsterImage;
    [SerializeField] Image MonsterCapsuleImage;
    [SerializeField] Image MonsterElementImage;
    [SerializeField] Image MonsterSecondElementImage;
    [SerializeField] private TextMeshProUGUI _monsterAbility;
    [SerializeField] TextMeshProUGUI MonsterNameText;
    [SerializeField] TextMeshProUGUI MonsterLvlText;
    [SerializeField] TextMeshProUGUI MonsterHPText;
    [SerializeField] TextMeshProUGUI MonsterAttackText;
    [SerializeField] TextMeshProUGUI MonsterMagicText;
    [SerializeField] TextMeshProUGUI MonsterDefenseText;
    [SerializeField] TextMeshProUGUI MonsterSpeedText;
    [SerializeField] TextMeshProUGUI MonsterEnduranceText;

    [SerializeField] TextMeshProUGUI MonsterNPHPText;
    [SerializeField] TextMeshProUGUI MonsterNPAttackText;
    [SerializeField] TextMeshProUGUI MonsterNPMagicText;
    [SerializeField] TextMeshProUGUI MonsterNPDefenseText;
    [SerializeField] TextMeshProUGUI MonsterNPSpeedText;
    [SerializeField] TextMeshProUGUI MonsterNPEnduranceText;

    [Header("Selected Monster Moves")]
    [SerializeField] TextMeshProUGUI selectedMove1Name;
    [SerializeField] TextMeshProUGUI selectedMove1Staminacost;
    [SerializeField] Image selectedMove1Element;

    [SerializeField] TextMeshProUGUI selectedMove2Name;
    [SerializeField] TextMeshProUGUI selectedMove2Staminacost;
    [SerializeField] Image selectedMove2Element;

    [SerializeField] TextMeshProUGUI selectedMove3Name;
    [SerializeField] TextMeshProUGUI selectedMove3Staminacost;
    [SerializeField] Image selectedMove3Element;

    [SerializeField] TextMeshProUGUI selectedMove4Name;
    [SerializeField] TextMeshProUGUI selectedMove4Staminacost;
    [SerializeField] Image selectedMove4Element;

    private void Awake()
    {
        if (i != null)
        {
            Destroy(i);
        }
        i = this;
    }
    private void Start()
    {
        //InitialiseMonstersInStorage();
        //InitialiseStoragePermUnits();
        //InitialiseMonstersToStorage();
        EnsureCachedReferences();
    }

    private void EnsureCachedReferences()
    {
        if (storageSelectionMenu == null)
            storageSelectionMenu = UIManager.i.MonsterStorage.GetComponent<SelectionMenuScript>();
        if (storageGridLayout == null)
            storageGridLayout = storageContainer.GetComponent<GridLayoutGroup>();
        if (partyVerticalLayout == null)
            partyVerticalLayout = partyContainer.GetComponent<VerticalLayoutGroup>();
    }

    private void FixedUpdate()
    {
        if (UIManager.i.MonsterStorage.activeSelf == true)
        {
            EnsureCachedReferences();
            currentUnitNum = storageSelectionMenu.current;
            convertPositionToUnit();
            if (currentUnitNum != lastCurrentUnitNum)
            {
                SelectStorageUnit();
                lastCurrentUnitNum = currentUnitNum;
            }

            if (currentUnit.IsDragging() == false && (!storageGridLayout.enabled || !partyVerticalLayout.enabled))
            {
                ResetLayoutStorage();
            }
        }
        
    }

    private void convertPositionToUnit()
    {
        if (currentUnitNum <= 4)
        {
            currentUnit = partyContainer.transform.GetChild(currentUnitNum).GetComponent<StorageUnit>();
        }
        else
        {
            currentUnit = storageContainer.transform.GetChild(currentUnitNum - 5).GetComponent<StorageUnit>();
        }
    }


    public void InitialiseMonstersToStorage()//initialised in UIManager script
    {
        EnsureCachedReferences();
        InitialiseFirstPartyMembers();
        bool initialised = false;
        for (int i = (0 + storageContainer.transform.childCount); i < MonsterStorageList.Count; i++)
        {
            GameObject ob = Instantiate(storageUnitOb, storageContainer.transform);
            MonsterStorageList[i].Initialise();
            ob.GetComponent<StorageUnit>().monster = MonsterStorageList[i];
            ob.GetComponent<StorageUnit>().MonsterPosition = i;
            storageSelectionMenu.menuContent.Add(ob);
            if (initialised == false)
            {
                initialised = true;
                ob.GetComponent<StorageUnit>().SelectMonster();
            }
        }
    }
    public void ReInitialiseMonsterStorage()
    {
        for (int i = 0; i < MonsterStorageList.Count; i++)
        {
            storageContainer.transform.GetChild(i).GetComponent<StorageUnit>().monster = MonsterStorageList[i];
            storageContainer.transform.GetChild(i).GetComponent<StorageUnit>().MonsterPosition = i;
            //Debug.Log("  Initialised: " + storageContainer.transform.GetChild(i).GetComponent<StorageUnit>().MonsterPosition +" "+ storageContainer.transform.GetChild(i).GetComponent<StorageUnit>().monster.Base.name);
        }
    }
    public void ReInitialisePartyStorage()
    {
        for (int i = 0; i < PlayerStats.i.PlayerParty.Count; i++)
        {
            partyContainer.transform.GetChild(i).GetComponent<StorageUnit>().monster = PlayerStats.i.PlayerParty[i];
            partyContainer.transform.GetChild(i).GetComponent<StorageUnit>().MonsterPosition = i;
            //Debug.Log("  Initialised: " + storageContainer.transform.GetChild(i).GetComponent<StorageUnit>().MonsterPosition +" "+ storageContainer.transform.GetChild(i).GetComponent<StorageUnit>().monster.Base.name);
        }
        for (int i = 4; i >= PlayerStats.i.PlayerParty.Count; i--)
        {
          
            partyContainer.transform.GetChild(i).GetComponent<StorageUnit>().monster = GameUtils.i.missingMon;
            partyContainer.transform.GetChild(i).GetComponent<StorageUnit>().MonsterPosition = i;

            //Debug.Log("  Initialised: " + storageContainer.transform.GetChild(i).GetComponent<StorageUnit>().MonsterPosition +" "+ storageContainer.transform.GetChild(i).GetComponent<StorageUnit>().monster.Base.name);
        }
    }
    public void InitialiseFirstPartyMembers()
    {
        EnsureCachedReferences();
        for (int i = (0 + partyContainer.transform.childCount); i < 5; i++)
        {
            GameObject ob = Instantiate(storageUnitOb, partyContainer.transform);
            ob.GetComponent<StorageUnit>().monster = GameUtils.i.missingMon;
            ob.GetComponent<StorageUnit>().MonsterPosition = i;
            ob.GetComponent<StorageUnit>().isPartyMember = true;
            storageSelectionMenu.menuContent.Add(ob);
        }
        for (int i = 0; i < PlayerStats.i.PlayerParty.Count; i++)
        {
            partyContainer.transform.GetChild(i).GetComponent<StorageUnit>().monster = PlayerStats.i.PlayerParty[i];
            
        }
    }

    public void DeleteMonstersInStorage()
    {
        MonsterStorageList.Clear();
        for (int i = 0; i < storageContainer.transform.childCount; i++)
        {
            Destroy(storageContainer.transform.GetChild(i).gameObject);
        }
    }

   /* public void InitialiseMonstersInStorage()
    {
        //create an empty list of 400 non existing monsters
        for (int i = 0; i < 400; i++)
        {
            MonsterStorageList.Add(GameUtils.i.missingMon);
        }
    }

    //box takes values from 1 to 10
    /*public void InitialiseStorageUnits(int box = 1)
    {
        for (int i = (((box-1) * 40) + 0); i < (box * 40); i++)
        {
            
            //loop through the list
            //add any monsters found in current position of monsterStorageList
            //to the current unit and display it
            //change monster sprite and capsule caught
        }
    }

    //box takes values from 1 to 10
    public void InitialiseStoragePermUnits()
    {
        for (int i = 0; i < 40; i++)
        {
            StorageObjectList[i].GetComponent<StorageUnit>().monster = MonsterStorageList[i];
            StorageObjectList[i].GetComponent<StorageUnit>().UnitPosition = i;
            StorageObjectList[i].GetComponent<StorageUnit>().MonsterPosition = i;

            //loop through the list
            //add any monsters found in current position of monsterStorageList
            //to the current unit and display it
            //change monster sprite and capsule caught
        }
    }
    */
    public List<BattleMonster> GetMonsterStorageList()
    {
        return MonsterStorageList;
    }
    public GameObject StorageScrollContentObject => storageScrollContent;
    public GameObject ContainerObject => storageContainer;
    public GameObject PartyContainerObject => partyContainer;
    public void initialiseIndividual(int UnitPos, Image MonsterSprite, Image CapsuleSprite)//NOT WORKING
    {
        if (MonsterStorageList[UnitPos].Level > 0)
        {
            MonsterSprite.sprite = MonsterStorageList[UnitPos].Base.frontSprite;
            CapsuleSprite.sprite = MonsterStorageList[UnitPos].CapsuleCaught.Image;
            //StorageObjectList[UnitPos].GetComponent<StorageUnit>().monster = MonsterStorageList[UnitPos];

        }
    }
    public void initialiseIndividualPartyMember(int UnitPos, Image MonsterSprite, Image CapsuleSprite)//NOT WORKING
    {
        if (UnitPos < PlayerStats.i.PlayerParty.Count)
        {
            MonsterSprite.sprite = PlayerStats.i.PlayerParty[UnitPos].Base.frontSprite;
            CapsuleSprite.sprite = PlayerStats.i.PlayerParty[UnitPos].CapsuleCaught.Image;
        }
        else
        {
            MonsterSprite.sprite = missingMonSprite;
            CapsuleSprite.sprite = missingCapsuleSprite;
        }
       
        //StorageObjectList[UnitPos].GetComponent<StorageUnit>().monster = MonsterStorageList[UnitPos];
    }
    public void ChangeMonsterPositions(StorageUnit currentMember, StorageUnit toMember)//TODO NOT WORKING
    {

        //check if selected monster is not already active
        if (currentMember.MonsterPosition != toMember.MonsterPosition)
        {
            //switch them positions
            BattleMonster temp = MonsterStorageList[currentMember.MonsterPosition];
            MonsterStorageList[currentMember.MonsterPosition] = MonsterStorageList[toMember.MonsterPosition];
            MonsterStorageList[toMember.MonsterPosition] = temp;
            currentMember.monster = MonsterStorageList[currentMember.MonsterPosition];
            toMember.monster = MonsterStorageList[toMember.MonsterPosition];

        }
    }
    public void AddMonsterToStorage(BattleMonster monster , float hp)
    {
        EnsureCachedReferences();
        monster.HP = hp;
        MonsterStorageList.Add(monster);
        GameObject ob = Instantiate(storageUnitOb, storageContainer.transform);
        ob.GetComponent<StorageUnit>().monster = monster;
        ob.GetComponent<StorageUnit>().MonsterPosition = MonsterStorageList.Count - 1;
        storageSelectionMenu.menuContent.Add(ob);
    }

    public void ChangeMonsterPositionsWithPartyMember(StorageUnit currentMember, GameObject toMember)
    {
        EnsureCachedReferences();
       
        //check if party member is thrown in storage
        //if yes remove member from party and add it to storage
        if (toMember.GetComponent<Mask>() != null)
        {
            //check if there is more than one monster in party
            if (PlayerStats.i.PlayerParty.Count == 1)
                return;


            AddMonsterToStorage(currentMember.monster, currentMember.monster.HP);
            //UIManager.i.PartyScene.GetComponent<PartySelectionScript>().RemoveMonsterFromSelection(currentMember.monster);
            //UIManager.i.PartyScene.GetComponent<PartySelectionScript>().RemoveMonsterFromSelection2();
            PlayerStats.i.RemoveMonsterFromParty(currentMember.monster);
            ReInitialisePartyStorage();
            return;
        }
        if (toMember.GetComponent<StorageUnit>() == null)
            return;

        //check if current member is party member and to member is not
        if (currentMember.isPartyMember == true && toMember.GetComponent<StorageUnit>().isPartyMember == false)
        {
            BattleMonster storageMember = toMember.GetComponent<StorageUnit>().monster;
            BattleMonster partyMember = currentMember.monster;

            PlayerStats.i.PlayerParty[currentMember.MonsterPosition] = storageMember;
            MonsterStorageList[toMember.GetComponent<StorageUnit>().MonsterPosition] = partyMember;
            currentMember.monster = PlayerStats.i.PlayerParty[currentMember.MonsterPosition];
            toMember.GetComponent<StorageUnit>().monster = MonsterStorageList[toMember.GetComponent<StorageUnit>().MonsterPosition];
        }
        //check if current member not party member and to member is a party member
        else if (currentMember.isPartyMember == false && toMember.GetComponent<StorageUnit>().isPartyMember == true)
        {
            
            BattleMonster storageMember = currentMember.monster;
            BattleMonster partyMember = toMember.GetComponent<StorageUnit>().monster;
            //check if party monster is not a missing monster
            if (partyMember.Level > 0)
            {
                //check if there is only one party member and if monster storage unit does not have HP
                if (PartyRules.CountDead(PlayerStats.i.PlayerParty) == 1 && partyMember.HP > 0 && storageMember.HP <= 0)
                {

                }
                else
                {
                    PlayerStats.i.PlayerParty[toMember.GetComponent<StorageUnit>().MonsterPosition] = storageMember;
                    MonsterStorageList[currentMember.MonsterPosition] = partyMember;
                    currentMember.monster = MonsterStorageList[currentMember.MonsterPosition];
                    toMember.GetComponent<StorageUnit>().monster = PlayerStats.i.PlayerParty[toMember.GetComponent<StorageUnit>().MonsterPosition];
                }

                
            }
            //check if party monster is missing
            //if yes add it to party
            else
            {

                PlayerStats.i.AddUninitialisedMonsterToParty(storageMember, storageMember.HP);
                //toMember.GetComponent<StorageUnit>().monster = PlayerStats.i.GetParty()[PlayerStats.i.GetParty().Count - 1];
                partyContainer.transform.GetChild(FindFirstEmptyPartyStorgeUnit()).GetComponent<StorageUnit>().monster = PlayerStats.i.PlayerParty[PlayerStats.i.PlayerParty.Count - 1];
                MonsterStorageList.Remove(storageMember);
                
                storageContainer.transform.GetChild(storageContainer.transform.childCount - 1).gameObject.SetActive(false);
                storageSelectionMenu.tempMenuContent.Remove(storageContainer.transform.GetChild(storageContainer.transform.childCount - 1).gameObject);
                Destroy(storageContainer.transform.GetChild(storageContainer.transform.childCount - 1).gameObject);
                UIManager.i.PartyScene.GetComponent<PartySelectionScript>().CreateNewPartyMember();
                ReInitialiseMonsterStorage();
            }
        }
        //if both members are party members
        else if (currentMember.isPartyMember == true && toMember.GetComponent<StorageUnit>().isPartyMember == true)
        {
            BattleMonster temp = PlayerStats.i.PlayerParty[currentMember.MonsterPosition];
            PlayerStats.i.PlayerParty[currentMember.MonsterPosition] = PlayerStats.i.PlayerParty[toMember.GetComponent<StorageUnit>().MonsterPosition];
            PlayerStats.i.PlayerParty[toMember.GetComponent<StorageUnit>().MonsterPosition] = temp;
            currentMember.monster = PlayerStats.i.PlayerParty[currentMember.MonsterPosition];
            toMember.GetComponent<StorageUnit>().monster = PlayerStats.i.PlayerParty[toMember.GetComponent<StorageUnit>().MonsterPosition];
        }
        storageSelectionMenu.current = toMember.GetComponent<StorageUnit>().MonsterPosition;
    }
    public int FindFirstEmptyPartyStorgeUnit()
    {
        bool found = false;
        int c = 0;
        int position = -1;
        while (found == false && c < PlayerStats.i.PlayerParty.Count && position < 0)
        {
            if (partyContainer.transform.GetChild(c).GetComponent<StorageUnit>().monster.Level <= 0)
            {
                position = c;
                found = true;
            }
            c++;
        }
        return position;
    }


    public void SelectStorageUnit()
    {
        EnsureCachedReferences();
        ForceStopDragStorageUnit();
        for (int i = 0; i < storageContainer.transform.childCount; i++)
        {
            storageContainer.transform.GetChild(i).GetComponent<StorageUnit>().SetSelected(false);
        }
        for (int i = 0; i < partyContainer.transform.childCount; i++)
        {
            partyContainer.transform.GetChild(i).GetComponent<StorageUnit>().SetSelected(false);
        }
        currentUnit.SetSelected(true);
       
    }
    public void ResetLayoutStorage()
    {
        EnsureCachedReferences();
        for (int i = 0; i < storageContainer.transform.childCount; i++)
        {
            storageContainer.transform.GetChild(i).GetComponent<StorageUnit>().ResetLayout();
        }
        for (int i = 0; i < partyContainer.transform.childCount; i++)
        {
            partyContainer.transform.GetChild(i).GetComponent<StorageUnit>().ResetLayout();
        }
        storageGridLayout.enabled = true;
        partyVerticalLayout.enabled = true;
    }
    public void ForceStopDragStorageUnit()
    {
        for (int i = 0; i < storageContainer.transform.childCount; i++)
        {
            storageContainer.transform.GetChild(i).GetComponent<StorageUnit>().SetDragging(false);
        }
        for (int i = 0; i < partyContainer.transform.childCount; i++)
        {
            partyContainer.transform.GetChild(i).GetComponent<StorageUnit>().SetDragging(false);
        }
    }
    public void EnableMonsterMoves(bool enable)
    {
        MovesOb.SetActive(enable);
    }
    public void EnableMonsterStats(bool enable)
    {
        StatsOb.SetActive(enable);
    }

    public void ShowMonsterDetails(BattleMonster currentMonster)
    {
        if (currentMonster.Level == 0)
            return;

        MonsterNameText.text = currentMonster.Base.name;
        MonsterElementImage.sprite = currentMonster.Base.element.icon;
        MonsterSecondElementImage.sprite = currentMonster.Base.secondaryElement.icon;
        _monsterAbility.text = currentMonster.Ability.Name;
        
        MonsterLvlText.text = "Lvl: " + currentMonster.Level;
        MonsterHPText.text = "HP: " + (int)currentMonster.MaxHP;
        MonsterAttackText.text = "Attack: " + (int)currentMonster.PhysicalAttack;
        MonsterMagicText.text = "Magic: " + (int)currentMonster.MagicalAttack;
        MonsterDefenseText.text = "Defense: " + (int)currentMonster.Defense;
        MonsterSpeedText.text = "Speed: " + (int)currentMonster.Speed;
        MonsterEnduranceText.text = "Endurance: " + (int)currentMonster.StaminaRegenSpeed;

        MonsterNPAttackText = UIManager.i.PartyScene.GetComponent<PartyScript>().ConvertNPText(MonsterAttackText, currentMonster.NPAttack);
        MonsterNPMagicText = UIManager.i.PartyScene.GetComponent<PartyScript>().ConvertNPText(MonsterMagicText, currentMonster.NPMagicAttack);
        MonsterNPDefenseText = UIManager.i.PartyScene.GetComponent<PartyScript>().ConvertNPText(MonsterDefenseText, currentMonster.NPDefense);
        MonsterNPSpeedText = UIManager.i.PartyScene.GetComponent<PartyScript>().ConvertNPText(MonsterSpeedText, currentMonster.NPSpeed);
        MonsterNPEnduranceText = UIManager.i.PartyScene.GetComponent<PartyScript>().ConvertNPText(MonsterEnduranceText, currentMonster.NPStaminaRegen);
        MonsterImage.sprite = currentMonster.Base.frontSprite;

        selectedMove1Name.text = currentMonster.CurrentMoves[0].Base.name;
        selectedMove1Staminacost.text = currentMonster.CurrentMoves[0].StaminaCost.ToString();
        selectedMove1Element.sprite = currentMonster.CurrentMoves[0].Base.ElementType.icon;


        if (currentMonster.CurrentMoves.Count >= 2)
        {
            selectedMove2Name.gameObject.SetActive(true);
            selectedMove2Staminacost.gameObject.SetActive(true);
            selectedMove2Element.gameObject.SetActive(true);
            selectedMove2Name.text = currentMonster.CurrentMoves[1].Base.name;
            selectedMove2Staminacost.text = currentMonster.CurrentMoves[1].StaminaCost.ToString();
            selectedMove2Element.sprite = currentMonster.CurrentMoves[1].Base.ElementType.icon;
        }
        else
        {
            selectedMove2Name.gameObject.SetActive(false);
            selectedMove2Staminacost.gameObject.SetActive(false);
            selectedMove2Element.gameObject.SetActive(false);

        }

        if (currentMonster.CurrentMoves.Count >= 3)
        {
            selectedMove3Name.gameObject.SetActive(true);
            selectedMove3Staminacost.gameObject.SetActive(true);
            selectedMove3Element.gameObject.SetActive(true);
            selectedMove3Name.text = currentMonster.CurrentMoves[2].Base.name;
            selectedMove3Staminacost.text = currentMonster.CurrentMoves[2].StaminaCost.ToString();
            selectedMove3Element.sprite = currentMonster.CurrentMoves[2].Base.ElementType.icon;
        }
        else
        {
            selectedMove3Name.gameObject.SetActive(false);
            selectedMove3Staminacost.gameObject.SetActive(false);
            selectedMove3Element.gameObject.SetActive(false);
        }

        if (currentMonster.CurrentMoves.Count >= 4)
        {
            selectedMove4Name.gameObject.SetActive(true);
            selectedMove4Staminacost.gameObject.SetActive(true);
            selectedMove4Element.gameObject.SetActive(true);
            selectedMove4Name.text = currentMonster.CurrentMoves[3].Base.name;
            selectedMove4Staminacost.text = currentMonster.CurrentMoves[3].StaminaCost.ToString();
            selectedMove4Element.sprite = currentMonster.CurrentMoves[3].Base.ElementType.icon;
        }
        else
        {
            selectedMove4Name.gameObject.SetActive(false);
            selectedMove4Staminacost.gameObject.SetActive(false);
            selectedMove4Element.gameObject.SetActive(false);
        }


        //selectedMonster = mon;
        //selectedMonsterNum = num;
    }
}
