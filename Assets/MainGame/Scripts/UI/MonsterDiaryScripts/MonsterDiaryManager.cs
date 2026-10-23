using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.Events;
public class MonsterDiaryManager : MonoBehaviour
{
    public static MonsterDiaryManager i;

    [Header("Events")]
    public UnityEvent OnMonsterRegisterSeen;
    public UnityEvent OnMonsterRegisterCaught;

    [Header("Monster Lists")]
    //used to store ALL monsters in a list  and display them when diary is initialised
    public List<Monster> permanentMonsterList = new List<Monster>();
    //used to store monsters seen by the player to compare with permanent monster list and display name/sprite of monster seen
    public List<Monster> seenMonstersList = new List<Monster>();
    //used to store monsters caught by the player to compare with permanent monster list and display the appropriate caught sprite;
    public List<Monster> caughtMonstersList = new List<Monster>();

    [Header("Constants")]
    [SerializeField] string NotSeenName;
    [SerializeField] Sprite CaughtSprite;
    [SerializeField] Sprite NotCaughtSprite;
    [SerializeField] Sprite QuestionmarkSprite;

    [Header("Book UI")]
    [SerializeField] GameObject MonsterDiaryOb;
    [SerializeField] Image monsterSprite;

    [SerializeField] Image monsterSpriteFront;
    [SerializeField] Image monsterSpriteLeft;
    [SerializeField] Image monsterSpriteRight;
    [SerializeField] Image monsterSpriteBack;

    [SerializeField] TextMeshProUGUI monsterNumber;
    [SerializeField] TextMeshProUGUI monsterName;
    [SerializeField] TextMeshProUGUI monsterDescription;
    [SerializeField] TextMeshProUGUI monsterMass;

    [Header("Monster List UI")]
    [SerializeField] GameObject MonsterHolderPrefab;
    [SerializeField] GameObject MonsterListFadeOB;
    [SerializeField] GameObject MonsterListOB;
    [SerializeField] GameObject MonsterListView;
    
    [Header("Selected Arrow")]
    public GameObject SelectArrow;

    [SerializeField] private float _selectedArrowPositionX;

    [Header("Diary Map objects")]
    [SerializeField] GameObject MapOB;
    [SerializeField] TextMeshProUGUI RoutesFoundText;
    [SerializeField] TextMeshProUGUI AreaName;
    [SerializeField] TextMeshProUGUI EnvironmentName;
    [SerializeField] Image MapMonsterImage;
    [SerializeField] TextMeshProUGUI MapMonsterName;
    //used to search gameobjects that have matching enums names
    [Header("Diary Map")]
    [SerializeField] List<AreaMapGameObject> Map = new List<AreaMapGameObject>();


    [Header("Base Stats")]
    [SerializeField] GameObject baseStatsOb;
    [SerializeField] TextMeshProUGUI healthText;
    [SerializeField] TextMeshProUGUI attackText;
    [SerializeField] TextMeshProUGUI magicText;
    [SerializeField] TextMeshProUGUI defenseText;
    [SerializeField] TextMeshProUGUI speedText;
    [SerializeField] TextMeshProUGUI enduranceText;

    [Header("Moves Learned")]
    [SerializeField] GameObject movesLearnedOb;
    [SerializeField] GameObject movePrefab;
    [SerializeField] GameObject movesContainer;

    int currentSelected;
    bool alreadyInitialisedMonsterList;
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
        InitialiseMonstersList();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            if (MonsterDiaryOb.activeSelf == false)
            {
                OpenMonsterDiaryMenu(true);
                ReCheckMonstersList();
            }
            else
            {
                OpenMonsterDiaryMenu(false);
            }
            
        }
    }
    public GameObject GetMonsterListView()
    {
        return MonsterListView;
    }
    
    public float SelectedArrowPositionX => _selectedArrowPositionX;

    public void OpenMonsterDiaryMenu(bool active)
    {
        MonsterDiaryOb.SetActive(active);
    }
    public void OpenMonsterListMenu(bool active)
    {
        MonsterListFadeOB.SetActive(active);
       
        if (active == true)
        {
            MonsterListOB.transform.DOLocalMoveY(7, 0.5f);
            MonsterListOB.SetActive(active);

            SelectArrow.transform.parent = MonsterListView.transform.GetChild(currentSelected);
            SelectArrow.GetComponent<RectTransform>().localPosition = new Vector2(-170, 0);
            SelectArrow.SetActive(true);
            ReCheckMonstersList();
        }
        else
        {
            MonsterListOB.transform.DOLocalMoveY(560, 0.5f);
            StartCoroutine(delayOpenMenu(.5f, false, MonsterListOB));
        }
    }
    public void OpenMapMenu(bool active)
    {
        //MonsterListFadeOB.SetActive(active);

        if (active == true)
        {
            MapOB.transform.DOLocalMoveY(40, 0.5f);
            MapOB.SetActive(active);
            
        }
        else
        {
            
            MapOB.transform.DOLocalMoveY(560, 0.5f);
            StartCoroutine(delayOpenMenu(.5f, false, MapOB));
        }
    }

    //used in stats and moves button, in moves active is set to true, in stats active is set to false
    public void OpenMovesLearnedMenu(bool active)
    {
        baseStatsOb.SetActive(!active);
        movesLearnedOb.SetActive(active);
    }
    IEnumerator delayOpenMenu(float seconds, bool active, GameObject ob)
    {
        yield return new WaitForSeconds(seconds);
        ob.SetActive(active);
    }


    //THIS EXECUTES ONCE PER GAME START
    public void InitialiseMonstersList()
    {
        if (alreadyInitialisedMonsterList == true)
            return;

        for (int i = 0; i < permanentMonsterList.Count; i++)
        {
            GameObject newHolder = Instantiate(MonsterHolderPrefab, MonsterListView.transform);
            newHolder.GetComponent<MonsterDiaryHolder>().SetSeenStatus(NotCaughtSprite);
            newHolder.GetComponent<MonsterDiaryHolder>().SetMonsterNumber(i + 1);
            newHolder.GetComponent<MonsterDiaryHolder>().SetNumber(i);
            CompareSeen(i, ref newHolder);
        }
        SelectMonster(0);
        alreadyInitialisedMonsterList = true;
    }
    //this plays only when list is already initialised
    public void ReCheckMonstersList()
    {
        if (alreadyInitialisedMonsterList == false)
            return;


        for (int i = 0; i < permanentMonsterList.Count; i++)
        {
            GameObject newHolder = MonsterListView.transform.GetChild(i).gameObject;
            CompareSeen(i, ref newHolder);
            CompareCaught(i, ref newHolder);
        }
    }

    public void CompareSeen(int number, ref GameObject newHolder)
    {
        for (int i = 0; i < seenMonstersList.Count; i++)
        {
            //if player has seen current monster
            if (seenMonstersList[i].Name == permanentMonsterList[number].Name)
            {
                //change sprite
                newHolder.GetComponent<MonsterDiaryHolder>().SetMonsterName(seenMonstersList[i].Name);
                newHolder.GetComponent<MonsterDiaryHolder>().SetMonsterSprite(seenMonstersList[i].frontSprite);
                //newHolder.GetComponent<MonsterDiaryHolder>().SetSeenStatus(CaughtSprite);
            }
        }
    }

    public bool IsMonsterSeen(int number)
    {
        bool seen = false;
        for (int i = 0; i < seenMonstersList.Count; i++)
        {
            if (seenMonstersList[i].Name == permanentMonsterList[number].Name)
            {
                
                seen = true;
            }
        }
        return seen;
    }
    public void CompareCaught(int number, ref GameObject newHolder)
    {
        for (int i = 0; i < caughtMonstersList.Count; i++)
        {
            //if player has seen current monster
            if (caughtMonstersList[i].Name == permanentMonsterList[number].Name)
            {
                newHolder.GetComponent<MonsterDiaryHolder>().SetSeenStatus(CaughtSprite);
            }
        }
    }

    public void SelectMonster(int number)
    {
        monsterNumber.text = number + 1 + "";
        InitialiseMap(number);
        DestoyMoves();
        if (IsMonsterSeen(number) == true)
        {
            //for map
            MapMonsterImage.sprite = permanentMonsterList[number].frontSprite;
            MapMonsterName.text = permanentMonsterList[number].Name;
            //for others
            monsterSprite.sprite = permanentMonsterList[number].frontSprite;
            monsterName.text = permanentMonsterList[number].Name;
            monsterDescription.text = permanentMonsterList[number].description;
            monsterMass.text = permanentMonsterList[number].mass.ToString();
            monsterSpriteFront.sprite = permanentMonsterList[number].overworldSprites[0];
            monsterSpriteLeft.sprite = permanentMonsterList[number].overworldSprites[1];
            monsterSpriteRight.sprite = permanentMonsterList[number].overworldSprites[2];
            monsterSpriteBack.sprite = permanentMonsterList[number].overworldSprites[3];
            //for stats
            InitialiseBaseStats(number, true);
            InitialiseMovesLearned(number,true);
        }
        else
        {
            //for map
            MapMonsterImage.sprite = QuestionmarkSprite;
            MapMonsterName.text = NotSeenName;
            //for others
            monsterSprite.sprite = QuestionmarkSprite;
            monsterName.text = NotSeenName;
            monsterDescription.text = NotSeenName;
            monsterMass.text = "-----";
            monsterSpriteFront.sprite = QuestionmarkSprite;
            monsterSpriteLeft.sprite = QuestionmarkSprite;
            monsterSpriteRight.sprite = QuestionmarkSprite;
            monsterSpriteBack.sprite = QuestionmarkSprite;
            //for stats
            InitialiseBaseStats(number, false);
        }
        currentSelected = number;
    }

    public void MoveToNextPage()
    {
        if (currentSelected >= permanentMonsterList.Count - 1)
            return;

        SelectMonster(currentSelected + 1);
    }
    public void MoveToPreviousPage()
    {
        if (currentSelected <= 0)
            return;

        SelectMonster(currentSelected - 1);
    }
    public void AddMonsterToSeen(Monster mon)
    {
        if (seenMonstersList.Contains(mon) == false)
        {
            seenMonstersList.Add(mon);
            OnMonsterRegisterSeen?.Invoke();
        }
    }
    public void AddMonsterToCaught(Monster mon)
    {
        if (caughtMonstersList.Contains(mon) == false)
        {
            caughtMonstersList.Add(mon);
            OnMonsterRegisterCaught?.Invoke();
        }
    }

    public GameObject GetMapGameObject(Area area)
    {
        GameObject ob = Map[0].AreaInMapGameObject;

        for (int i = 0; i < Map.Count; i++)
        {
            if (Map[i].AreaFound == area)
            {
                ob = Map[i].AreaInMapGameObject;
            }
        }
        return ob;
    }

    public void ResetMapColour()
    {
        for (int i = 0; i < Map.Count; i++)
        {
            Map[i].AreaInMapGameObject.GetComponent<Image>().color = Color.white;
        }
    }

    List<Sequence> sq = new List<Sequence>();
    public void MapChangeColorAnimation(Image ob)
    {
        var sequence = DOTween.Sequence();
        sequence.Append(ob.DOColor(Color.red, 0.5f));
        sequence.Append(ob.DOColor(Color.white, 0.5f));
        sequence.SetLoops(-1);
        sq.Add(sequence);
    }
    
    public void ClearMapAnimations()
    {
        for (int i = 0; i < sq.Count; i++)
        {
            sq[i].Kill();
        }
        sq.Clear();
    }


    
    public void InitialiseMap(int number)
    {
        
        int num = 0;
        RoutesFoundText.text = "";
        EnvironmentName.text = "";
        ClearMapAnimations();
        ResetMapColour();
        if (IsMonsterSeen(number) == true)
        {
            if (permanentMonsterList[number].environment_found.Count <= 0)
            {
                EnvironmentName.text = " - Unknown";
            }
            for (int i = 0; i < permanentMonsterList[number].environment_found.Count; i++)
            {
                EnvironmentName.text += "- " + permanentMonsterList[number].environment_found[i].ToString() + System.Environment.NewLine;
            }

            foreach (Area item in permanentMonsterList[number].areas_found)
            {
                MapChangeColorAnimation(GetMapGameObject(item).GetComponent<Image>());

                //GetMapGameObject(item).GetComponent<Image>().DOColor(Color.red, 0.5f);
                RoutesFoundText.text += "- " + item.ToString() + System.Environment.NewLine;
                num++;
            }
        }
        //checks if there was no location found
        if (num == 0)
        {
            RoutesFoundText.text = " - Unknown";
        }
    }


    public void InitialiseBaseStats(int number, bool found)
    {
        if (found == true)
        {
            healthText.text = permanentMonsterList[number].health.ToString();
            attackText.text = permanentMonsterList[number].attack.ToString();
            magicText.text = permanentMonsterList[number].magic_attack.ToString();
            defenseText.text = permanentMonsterList[number].defense.ToString();
            speedText.text = permanentMonsterList[number].speed.ToString();
            enduranceText.text = permanentMonsterList[number].staminaRegen.ToString();
        }
        else
        {
            healthText.text = "---";
            attackText.text = "---";
            magicText.text = "---";
            defenseText.text = "---";
            speedText.text = "---";
            enduranceText.text = "---";
        }
    }
    public void DestoyMoves()
    {
        for (int i = 0; i < movesContainer.transform.childCount; i++)
        {
            Destroy(movesContainer.transform.GetChild(i).gameObject);
        }
    }
    public void InitialiseMovesLearned(int number, bool found)
    {
        


        if (found == false)
            return;


        for (int i = 0; i < permanentMonsterList[number].learnableMoves.Count; i++)
        {
            BattleMove move = new BattleMove(permanentMonsterList[number].learnableMoves[i].Base);
            move.Initialise();
            GameObject moveOb = Instantiate(movePrefab, movesContainer.transform);
            moveOb.GetComponent<MoveInitialiser>().Move = move;
            moveOb.GetComponent<MoveInitialiser>().InitialiseMove();

        }
    }

}
