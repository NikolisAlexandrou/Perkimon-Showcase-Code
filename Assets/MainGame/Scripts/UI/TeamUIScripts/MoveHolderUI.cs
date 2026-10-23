using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
public class MoveHolderUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, ISelectHandler, IDeselectHandler
{
    public BattleMove move;
    bool isSelected;


    public void SetMove(BattleMove newMove)
    {
        move = newMove;
    }
    public void SetSelected(bool sel)
    {
        isSelected = sel;
    }
    public bool IsSelected()
    {
        return isSelected;
    }
    /*
    private void Update()
    {
        if (GameUtils.i.ControllerConnected() == true && UIManager.i.currentUI.Peek() == GetComponentInParent<DynamicControllerUISetUp>().gameObject)
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
    }*/
    void Interact()//not currently available
    {
       //when interacting move
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
        if (move == null || move.Base == null)
            return;

        UIManager.i.PartyScene.GetComponent<PartyScript>().EnableMoveStatsOB(true);
        UIManager.i.PartyScene.GetComponent<PartyScript>().ChangeCurrentMoveStatsTexts(
        move.Base.ElementType.icon, move.Base.DamageType.ToString().ToUpper(), move.StaminaCost.ToString(), move.Base.Name,move.Base.Description, move.Base.Power.ToString());

        GetComponent<Image>().DOFade(0.8f, 0.1f);
        //transform.DOScale(new Vector3(0.8f, 0.8f, 0.8f), 0.1f);
    }
    void DeSelect()
    {
        UIManager.i.PartyScene.GetComponent<PartyScript>().EnableMoveStatsOB(false);
        GetComponent<Image>().DOFade(1f, 0.1f);
        //transform.DOScale(new Vector3(1f, 1f, 1f), 0.1f);
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
