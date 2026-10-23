using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;
public class StorageUnit : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Image CapsuleImage;//capsule caught sprite;
    [SerializeField] private Image MonsterImage;//monster sprite
    public BattleMonster monster;
    public int MonsterPosition;//get position in all 400 monsters
    public bool isPartyMember;
    Vector2 initialPosition;
    bool draggingAround;
    bool isSelected;
    LayoutElement layoutElement;
    Image backgroundImage;
    float refreshTimer;
    const float RefreshInterval = 0.25f;

    public bool IsDragging()
    {
        return draggingAround;
    }
    public bool IsSelected()
    {
        return isSelected;
    }
    public void SetSelected(bool se)
    {
        isSelected = se;
    }
    public void SetDragging(bool se)
    {
        draggingAround = se;
    }
    private void Start()
    {
        //CapsuleImage = GetComponent<Image>();
        //MonsterImage = GetComponentInChildren<Image>();
        initialPosition = transform.position;
        layoutElement = GetComponent<LayoutElement>();
        backgroundImage = GetComponent<Image>();
        InitialiseMonster();
    }
    
    private void Update()
    {
        refreshTimer -= Time.deltaTime;
        if (refreshTimer <= 0f)
        {
            InitialiseMonster();
            refreshTimer = RefreshInterval;
        }

        DragMonster();
        if (GameUtils.i.ControllerConnected() == true && UIManager.i.IsTopUI(UIManager.i.MonsterStorage.gameObject))
        {
            
            if (IsSelected() == true && MonsterStorageManager.i.currentUnit == this)
            {
                ControllerBehaviour();
                SelectMonster();
            }
            else
            {
                DeSelectMonster();
            }
        }
    }

    void DragMonster()
    {
        if (monster == null || monster.Level == 0)
            return;

        if (draggingAround == true)
        {
            //transform.parent = MonsterStorageManager.i.getDragOb().transform;
            //MonsterStorageManager.i.getStorageScrollContentOb().GetComponent<ScrollRect>().enabled = false;
            layoutElement.ignoreLayout = true;
            MonsterStorageManager.i.ContainerObject.GetComponent<GridLayoutGroup>().enabled = false;
            MonsterStorageManager.i.PartyContainerObject.GetComponent<VerticalLayoutGroup>().enabled = false;
            if (GameUtils.i.ControllerConnected() == true)
            {
                if (isSelected == false)
                    return;
                if (UIManager.i.IsTopUI(UIManager.i.MonsterStorage.gameObject))
                {
                    MoveUnit();
                }

            }
            else
            {
                transform.position = Input.mousePosition;
            }
        }
    }
    
    void ControllerBehaviour()
    {
        if (Input.GetButton("Interact"))
        {
            draggingAround = true;
        }
        else
        {
            ResetLayout();
            if (draggingAround == false)
                return;
            StopDragging();
        }
    }


    //Move Unit around
    void MoveUnit()
    {
        transform.position += new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0) * 1000 * Time.deltaTime;
    }

    public void InitialiseMonster()
    {
        if (isPartyMember == true)
        {
            MonsterStorageManager.i.initialiseIndividualPartyMember(MonsterPosition, MonsterImage, CapsuleImage);
        }
        else
        {
            MonsterStorageManager.i.initialiseIndividual(MonsterPosition, MonsterImage, CapsuleImage);
        }
      
        //MonsterStorageManager.i.InitialiseStoragePermUnitsIndividual(UnitPosition);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SelectMonster();
    }


    public void OnPointerExit(PointerEventData eventData)
    {
        DeSelectMonster();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        draggingAround = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        StopDragging();
    }
    void DeSelectMonster()
    {
        backgroundImage.DOFade(1f, 0.1f);
        transform.DOScale(new Vector3(1f, 1f, 1f), 0.1f);
        isSelected = false;
      
    }
    public void SelectMonster()
    {
        if (monster == null || monster.Level == 0)
            return;

        MonsterStorageManager.i.ShowMonsterDetails(monster);
        //GetComponentInParent<SelectionMenuScript>().selec(UnitPosition);
        backgroundImage.DOFade(0.8f, 0.1f);
        transform.DOScale(new Vector3(0.8f, 0.8f, 0.8f), 0.1f);
        //GetComponentInParent<PartyScript>().deleteLearnableMoveContent();
    }
    public void StopDragging()//not working
    {
       
        draggingAround = false;
        //transform.parent = MonsterStorageManager.i.getContainerOb().transform;
        layoutElement.ignoreLayout = false;
        
        GameObject ob = GameUtils.i.IsUIElementOnTop(gameObject.GetComponent<RectTransform>());
        transform.position = initialPosition;
        if (ob != gameObject)
        {
            //NOT WORKING
            StorageUnit targetUnit = ob.GetComponent<StorageUnit>();
            bool droppedOnStorageUnit = targetUnit != null;
            bool droppedOnStorageMask = ob.GetComponent<Mask>() != null;

            if (isPartyMember == true || (droppedOnStorageUnit && targetUnit.isPartyMember == true) || (droppedOnStorageMask && isPartyMember == true))
            {
                MonsterStorageManager.i.ChangeMonsterPositionsWithPartyMember(this, ob);
            }
            else if (droppedOnStorageUnit)
            {
                MonsterStorageManager.i.ChangeMonsterPositions(this, targetUnit);
            }
        }
        MonsterStorageManager.i.ContainerObject.GetComponent<GridLayoutGroup>().enabled = true;
        MonsterStorageManager.i.PartyContainerObject.GetComponent<VerticalLayoutGroup>().enabled = true;
        //MonsterStorageManager.i.getStorageScrollContentOb().GetComponent<ScrollRect>().enabled = true;
    }
    public void ResetLayout()
    {
        layoutElement.ignoreLayout = false;
    }
}
