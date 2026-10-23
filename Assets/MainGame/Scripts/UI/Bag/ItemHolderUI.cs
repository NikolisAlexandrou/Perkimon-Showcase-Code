using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
public class ItemHolderUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, ISelectHandler,IDeselectHandler
{
    public Item item;
    bool isSelected;

    public void SetSelected(bool sel)
    {
        isSelected = sel;
    }
    public bool IsSelected()
    {
        return isSelected;
    }
    private void Update()
    {
        if (GameUtils.i.ControllerConnected() == true && UIManager.i.IsTopUI(GetComponentInParent<DynamicControllerUISetUp>().gameObject))
        {
           ControllerBehaviour();
        }
    }
    void ControllerBehaviour()
    {
        if (isSelected == true)
        {
            if (Input.GetButtonDown("Interact"))
            {
                Interact();
            }
        }
    }
    void Interact()
    {
        if (item.ItemBase.IsInteractable == false)
            return;

        if (item.ItemBase.IsHealingMonster == true)
        {
            GameUtils.i.SelectMonToHeal(item);
        }
        if (item.ItemBase.IsMoveScroll == true)
        {
            GameUtils.i.SelectMonToLearnMove(item);
        }

    }
    public void OnSelect(BaseEventData eventData)
    {
        if (GameUtils.i.ControllerConnected() == false)
            return;

        isSelected = true;
        Select();
    }
    public void OnDeselect(BaseEventData eventData)
    {
        if (GameUtils.i.ControllerConnected() == false)
            return;

        isSelected = false;
        DeSelect();
    }
    void Select()
    {
        BagManager.i.ItemImage.sprite = item._sprite;
        BagManager.i.ItemName.text = item._name;
        BagManager.i.ItemDescription.text = item._description;
        BagManager.i.ItemAmount.text = "x" + item.getAmount;
        GetComponent<Image>().DOFade(0.8f, 0.1f);
        transform.DOScale(new Vector3(0.8f, 0.8f, 0.8f), 0.1f);
        BagManager.i.SetCurrentSelected(BagManager.i.getItemPositionCategory(item));

    }
    void DeSelect()
    {
        GetComponent<Image>().DOFade(1f, 0.1f);
        transform.DOScale(new Vector3(1f, 1f, 1f), 0.1f);
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        Interact();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Select();
    }


    public void OnPointerExit(PointerEventData eventData)
    {
        DeSelect();
    }

}
