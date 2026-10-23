using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using DG.Tweening;
public class MonsterDiaryHolder : MonoBehaviour,IPointerClickHandler,ISelectHandler,IDeselectHandler, IPointerEnterHandler,IPointerExitHandler
{
    [Header("Sprites")]
    [SerializeField] Sprite BGSprite;
    [SerializeField] Sprite SelectedBGSprite;
    [Header("General Settings")]
    [SerializeField] TextMeshProUGUI MonsterNumber;
    [SerializeField] TextMeshProUGUI MonsterName;
    [SerializeField] Image BGImage;
    [SerializeField] Image MonsterSprite;
    [SerializeField] Image SeenStatus;
    int number;
    Vector2 initialPosition;

    private void Start()
    {
        initialPosition = transform.position;
    }
    public TextMeshProUGUI GetMonsterNumber()
    {
        return MonsterNumber;
    }
    public TextMeshProUGUI GetMonsterName()
    {
        return MonsterName;
    }
    public Image GetMonsterSprite()
    {
        return MonsterSprite;
    }
    public Image GetSeenStatus()
    {
        return SeenStatus;
    }

    public void SetMonsterNumber(int newNum)
    {
        MonsterNumber.text = newNum.ToString();
    }
    public void SetMonsterName(string newName)
    {
        MonsterName.text = newName;
    }
    public void SetMonsterSprite(Sprite newSprite)
    {
        MonsterSprite.sprite = newSprite;
    }
    public void SetSeenStatus(Sprite newSeen)
    {
        SeenStatus.sprite = newSeen;
    }

    public int GetNumber()
    {
        return number;
    }

    public void SetNumber(int newNum)
    {
        number = newNum;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        MonsterDiaryManager.i.SelectMonster(number);
    }


    public void Select()
    {
        //MonsterDiaryManager.i.GetMonsterListView().GetComponent<VerticalLayoutGroup>().enabled = false;
        //transform.DOMove(new Vector3(initialPosition.x + 100, initialPosition.y, 0), .1f);
        
        MonsterDiaryManager.i.SelectArrow.transform.parent = transform;
        MonsterDiaryManager.i.SelectArrow.GetComponent<RectTransform>().localPosition = new Vector2(MonsterDiaryManager.i.SelectedArrowPositionX, 0);
        MonsterDiaryManager.i.SelectArrow.SetActive(true);
        BGImage.sprite = SelectedBGSprite;
    }

    public void Deselect()
    {
        BGImage.sprite = BGSprite;
        
        //transform.DOMove(new Vector3(initialPosition.x, initialPosition.y, 0), .1f);
        MonsterDiaryManager.i.SelectArrow.SetActive(false);
        //MonsterDiaryManager.i.GetMonsterListView().GetComponent<VerticalLayoutGroup>().enabled = true;
    }

    public void OnSelect(BaseEventData eventData)
    {
        Select();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        Deselect();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Select();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Deselect();
    }
}
