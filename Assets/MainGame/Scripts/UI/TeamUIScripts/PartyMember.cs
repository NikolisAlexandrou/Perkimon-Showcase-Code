using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
public class PartyMember : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler,ISelectHandler,IDeselectHandler
{   
    [SerializeField] TextMeshProUGUI Name;
    [SerializeField] GameObject arrowObject;
    [SerializeField] Image image;
    [SerializeField] Image capsuleImage;
    [SerializeField] TextMeshProUGUI Lvl;
    [SerializeField] int memberPosition;
    [SerializeField] Slider healthDisplay;
    [SerializeField] Slider expDisplay;
    [SerializeField] Image BackgroundImage1;
    [SerializeField] Image NameBackgroundImage1;


    Vector2 initialPosition;
    bool draggingAround;
    bool isSelected;
    PartyScript partyScript;
    Button button;
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

    public int GetMemberPos()
    {
        return memberPosition;
    }
    private void Start()
    {
        initialPosition = transform.position;
        partyScript = GetComponentInParent<PartyScript>();
        button = GetComponent<Button>();
        backgroundImage = GetComponent<Image>();
        InitialiseMoves();
    }
    private void Update()
    {
        refreshTimer -= Time.deltaTime;
        if (refreshTimer <= 0f)
        {
            InitialiseMoves();
            refreshTimer = RefreshInterval;
        }

        DragMonster();
        if (GameUtils.i.ControllerConnected() == true && UIManager.i.IsTopUI(partyScript.gameObject))
        {
            if (IsSelected() == true)
            {
                ControllerBehaviour();
                //SelectMonster();
            }
            else
            {
                //DeSelectMonster();
            }
        }
    }

    void DragMonster()
    {
        if (GameUtils.i.battleSystem.CurrentState is InBattleState || partyScript.partyState == PartyState.HEAL)
            return;

        if (draggingAround == true)
        {
            if (GameUtils.i.ControllerConnected() == true)
            {
                if (UIManager.i.IsTopUI(partyScript.gameObject))
                {
                    MovePartyMember();
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
        if (Input.GetButtonDown("Interact"))
            draggingAround = true;
        if (Input.GetButtonUp("Interact"))
            StopDragging();
    }


    void MovePartyMember()
    {
        Navigation navigation = button.navigation;
        navigation.mode = Navigation.Mode.None;
        button.navigation = navigation;
        transform.position += new Vector3(0, Input.GetAxis("Vertical"), 0) * 600 * Time.deltaTime;
    }
   
    bool isInArea(Vector2 currentOB, Vector2 Area, float radius)
    {
        bool inArea = false;
        if (currentOB.x >= Area.x - radius*2.5f && currentOB.x <= Area.x + radius * 2.5f)
        {
            if (currentOB.y >= Area.y - radius && currentOB.y <= Area.y + radius)
            {
                //Debug.Log("X: " + Area.x + " Y: " + Area.y);
                inArea = true;
            }
        }
        return inArea;
    }
    public void InitialiseMoves()
    {
        if (partyScript == null)
            partyScript = GetComponentInParent<PartyScript>();

        bool t = partyScript.initialiseIndividual(image, Name, Lvl, memberPosition, healthDisplay, expDisplay, capsuleImage, BackgroundImage1, NameBackgroundImage1);
        if (t == false)
        {
            gameObject.SetActive(false);
        }
    }
    public void OnSelect(BaseEventData eventData)
    {
        if (GameUtils.i.ControllerConnected() == false)
            return;

        isSelected = true;
        SelectMonster();
    }
    public void OnDeselect(BaseEventData eventData)
    {
        if (GameUtils.i.ControllerConnected() == false)
            return;

        isSelected = false;
        DeSelectMonster();
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
        //transform.DOScale(new Vector3(1f, 1f, 1f), 0.1f);
        transform.DOMove(new Vector3(initialPosition.x, initialPosition.y, 0), .1f);
        arrowObject.SetActive(false);
    }
    void SelectMonster()
    {
        partyScript.SelectPartyMember(memberPosition);
        //GetComponentInParent<PartySelectionScript>().SelectOB(memberPosition);
        backgroundImage.DOFade(0.8f, 0.1f);
        //transform.DOScale(new Vector3(0.8f, 0.8f, 0.8f), 0.1f);
        transform.DOMove(new Vector3(initialPosition.x + 100, initialPosition.y, 0), .1f);
        arrowObject.transform.position = new Vector2(arrowObject.transform.position.x, initialPosition.y);
        arrowObject.SetActive(true);
        partyScript.deleteLearnableMoveContent();
    }
    void StopDragging()
    {
        draggingAround = false;
        if (GameUtils.i.battleSystem.CurrentState is InBattleState)
            return;

        GameObject ob = GameUtils.i.IsUIElementOnTop(gameObject.GetComponent<RectTransform>());
        transform.position = initialPosition;
        if (ob != gameObject)
        {
            partyScript.ChangeMonsterPositions(memberPosition, ob.GetComponent<PartyMember>().memberPosition);
        }
        if (GameUtils.i.ControllerConnected() == true)
        {
            Navigation navigation = button.navigation;
            navigation.mode = Navigation.Mode.Explicit;
            button.navigation = navigation;
            SelectMonster();
        }
    }

   
}
