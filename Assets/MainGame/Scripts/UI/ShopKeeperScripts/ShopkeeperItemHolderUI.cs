using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
public class ShopkeeperItemHolderUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] TextMeshProUGUI NameText;
    [SerializeField] TextMeshProUGUI CostText;
    [SerializeField] Image itemImage;


    public Item item;
    public int price;
    bool isSelected;

    public void SetSelected(bool sel)
    {
        isSelected = sel;
    }
    public bool IsSelected()
    {
        return isSelected;
    }
    public TextMeshProUGUI GetNameText()
    {
        return NameText;
    }

    public TextMeshProUGUI GetCostText()
    {
        return CostText;
    }

    public Image GetitemImage()
    {
        return itemImage;
    }

    public void SetNameText(string text)
    {
        NameText.text = text;
    }

    public void SetCostText(string text)
    {
        CostText.text = text;
    }

    public void SetitemImage(Sprite sprite)
    {
        itemImage.sprite = sprite;
    }

    private void FixedUpdate()
    {
        if (GameUtils.i.ControllerConnected() == true && UIManager.i.IsTopUI(GetComponentInParent<SelectionMenuScript>().gameObject))
        {
            ControllerBehaviour();
        }
    }
    void ControllerBehaviour()
    {


        if (isSelected == true)
        {
            Select();
            if (Input.GetButtonDown("Interact"))
            {
                Interact();
            }
        }
        else
        {
            DeSelect();
        }
    }
    void Interact()
    {
        UIManager.i.ShopkeeperUI.GetComponent<ShopkeeperUIScript>().ActivateBuyItem(item);
    }
    public void Select()
    {
        UIManager.i.ShopkeeperUI.GetComponent<ShopkeeperUIScript>().ItemImage.sprite = item._sprite;
        UIManager.i.ShopkeeperUI.GetComponent<ShopkeeperUIScript>().ItemName.text = item._name;
        UIManager.i.ShopkeeperUI.GetComponent<ShopkeeperUIScript>().ItemDescription.text = item._description;
        UIManager.i.ShopkeeperUI.GetComponent<ShopkeeperUIScript>().ItemAmount.text = "$" + price;
        GetComponent<Image>().DOFade(0.8f, 0.1f);
        transform.DOScale(new Vector3(0.8f, 0.8f, 0.8f), 0.1f);
    }
    void DeSelect()
    {
        GetComponent<Image>().DOFade(1f, 0.1f);
        transform.DOScale(new Vector3(1f, 1f, 1f), 0.1f);
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        Select();
        Interact();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!UIManager.i.IsTopUI(GetComponentInParent<SelectionMenuScript>().gameObject))
            return;

        Select();
    }


    public void OnPointerExit(PointerEventData eventData)
    {
        DeSelect();
    }

}
