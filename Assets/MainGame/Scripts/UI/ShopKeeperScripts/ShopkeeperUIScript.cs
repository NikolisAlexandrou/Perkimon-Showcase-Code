using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class ShopkeeperUIScript : MonoBehaviour
{

    public static ShopkeeperUIScript i;
    [Header("Selected Item Objects")]
    public Image ItemImage;
    public TextMeshProUGUI ItemName;
    public TextMeshProUGUI ItemDescription;
    public TextMeshProUGUI ItemAmount;
    public TextMeshProUGUI moneyDisplay;
    [Header("Arrows Settings")]
    [SerializeField] float arrowJumpCD;
    public GameObject ArrowUpImage;
    public GameObject ArrowDownImage;

    [Header("Items Settings")]
    [SerializeField] GameObject contentUI;
    [SerializeField] GameObject ItemUIHolder;
    [SerializeField] GameObject selectAmountUI;
    [SerializeField] TextMeshProUGUI selectAmountText;
    [SerializeField] int selectAmount;

    [Header("Shopkeeper Lists")]
    public List<ShopkeeperItem> ShopkeeperItemList = new List<ShopkeeperItem>();

    private void Awake()
    {
        if (i != null)
        {
            Destroy(i);
        }
        i = this;
    }

    public void InitialiseShopkeeperList(List<ShopkeeperItem> itemlist)
    {
        ShopkeeperItemList.Clear();
        //GetComponent<SelectionMenuScript>().selectedImage.transform.parent = contentUI.transform;
        for (int i = 0; i < GetComponent<SelectionMenuScript>().menuContent.Count; i++)
        {
            Destroy(GetComponent<SelectionMenuScript>().menuContent[i]);
        }
        GetComponent<SelectionMenuScript>().menuContent.Clear();
        for (int c = 0; c < itemlist.Count; c++)
        {
            AddItemToShop(itemlist[c]);
        }
        SelectItemInShopkeeperUI();
    }


    public void OpenShopkeeperMenu()
    {
        GameUtils.i.FreezeGame();
        gameObject.SetActive(true);
        
        //if (ShopkeeperItemList.Count > 0)
        //{
        //    GetComponent<SelectionMenuScript>().selectedImage.SetActive(true);
        //
        //}
        //else
        //{
        //    GetComponent<SelectionMenuScript>().selectedImage.SetActive(false);
        //}
    }
    public void AddItemToShop(ShopkeeperItem item)
    {
        item.ShopItem.Initialise();

        ShopkeeperItemList.Add(item);
        DisplayItemInMenu(item.ShopItem);
    }

    public void DisplayItemInMenu(Item item)
    {
        GameObject ob = Instantiate(ItemUIHolder, new Vector2(0, 0), Quaternion.identity, contentUI.transform);
        GetComponent<SelectionMenuScript>().menuContent.Add(ob);
        ob.GetComponent<ShopkeeperItemHolderUI>().SetNameText(item._name);
        ob.GetComponent<ShopkeeperItemHolderUI>().SetCostText(ShopkeeperItemList[getItemPosition(item)].ItemCost.ToString());
        ob.GetComponent<ShopkeeperItemHolderUI>().SetitemImage(ShopkeeperItemList[getItemPosition(item)].ShopItem.getSprite);
        ob.GetComponent<ShopkeeperItemHolderUI>().item = item;
        ob.GetComponent<ShopkeeperItemHolderUI>().price = ShopkeeperItemList[getItemPosition(item)].ItemCost;
    }
    public void SelectItemInShopkeeperUI()
    {
        if (selectAmountUI.activeSelf == true)
            return;


        for (int i = 0; i < contentUI.transform.childCount; i++)
        {
            contentUI.transform.GetChild(i).GetComponent<ShopkeeperItemHolderUI>().SetSelected(false);
        }
        contentUI.transform.GetChild(UIManager.i.ShopkeeperUI.GetComponent<SelectionMenuScript>().current).GetComponent<ShopkeeperItemHolderUI>().SetSelected(true);
        contentUI.transform.GetChild(UIManager.i.ShopkeeperUI.GetComponent<SelectionMenuScript>().current).GetComponent<ShopkeeperItemHolderUI>().Select();
    }
    public void CloseShopkeeperMenu()
    {
        UIManager.i.ShopkeeperUI.SetActive(false);
        GameUtils.i.UnFreezeGame();
    }
    public int getItemPosition(Item item)
    {
        bool found = false;
        int pos = 0;
        if (ShopkeeperItemList.Count >= 1)
        {
            int i = 0;
            while (i < ShopkeeperItemList.Count && found == false)
            {
                if (ShopkeeperItemList[i].ShopItem._name == item._name)
                {
                    pos = i;
                    found = true;
                }
                i++;
            }
        }
        return pos;
    }
    [SerializeField] Item buyingItem;

    public void ActivateBuyItem(Item item)
    {
        int cost = ShopkeeperItemList[getItemPosition(item)].ItemCost;
        int playerMoney = PlayerStats.i.Money;
        buyingItem = new Item();
        buyingItem.SetItem(item.ItemBase);
        buyingItem.Initialise();

        if (playerMoney >= cost) //Player has enough money to buy item
            EnableSelectAmountUI();
        else //Player does not have enough money to buy item
            //play unable to buy sound


        moneyDisplay.text = PlayerStats.i.PlayerParty.ToString();
    }

    //TODO adding different item to bag menu overrides current item
    public void PurchaseItem()// Button event
    {
        
        int playerMoney = PlayerStats.i.Money;
    
        int cost = ShopkeeperItemList[getItemPosition(buyingItem)].ItemCost * selectAmount;
        if (playerMoney >= cost) //Player has enough money to buy item
        {
            PlayerStats.i.Money = playerMoney - cost;
            BagManager.i.AddItemToBag(buyingItem, selectAmount);
            AudioSystem.i.PlaySoundClip(AudioSystem.i.BuyItemSound);
        }
        CloseUI();
    }

    public void EnableSelectAmountUI()
    {
        selectAmountUI.SetActive(true);
        selectAmount = 1;
        selectAmountText.text = selectAmount.ToString();
        if (!UIManager.i.IsTopUI(selectAmountUI))
        {
            UIManager.i.currentUI.Push(selectAmountUI);
        }

    }

    public void SelectAmountUIFunctionalities()
    {
        if (!UIManager.i.IsTopUI(selectAmountUI))
            return;
        int cost = ShopkeeperItemList[getItemPosition(buyingItem)].ItemCost;
        int y = (int)(Input.GetAxis("Mouse ScrollWheel") * 10);

        if (GameUtils.i.ControllerConnected() == true)
        {
            y = (int)(Input.GetAxis("Vertical"));
        }
        else
        {
            y = (int)(Input.GetAxis("Mouse ScrollWheel") * 10);
        }
        selectAmount += y;
        SelectAmountArrow(y);
        if (selectAmount <= 0 )
        {
            if (PlayerStats.i.Money <= 0)
            {
                selectAmount = 1;
            }
            else
            {
                selectAmount = (int)(PlayerStats.i.Money / cost);
            }
            
        }else if (selectAmount > (int)(PlayerStats.i.Money / cost))
        {
            selectAmount = 1;
        }
        selectAmountText.text = selectAmount.ToString();

        if (Input.GetButtonDown("Interact"))
        {
            selectAmountUI.GetComponent<Button>().onClick.Invoke();
        }
        CloseUI();
    }
    public void CloseUI()
    {
        if (Input.GetButtonDown("CloseMenu"))
        {
            if (UIManager.i.IsTopUI(selectAmountUI))
                UIManager.i.currentUI.Pop();
            selectAmountUI.SetActive(false);
        }
    }
    public void CloseSelectUIButton()
    {
        if (selectAmountUI.activeSelf == true)
        {
            if (UIManager.i.IsTopUI(selectAmountUI))
                UIManager.i.currentUI.Pop();
            selectAmountUI.SetActive(false);
        }
    }
    //play animation of arrows when player selects amount
    //if amount is positive, play animation of up arrow
    //if amount is negative, play animation of down arrow
    bool cooldown = false;
    public void SelectAmountArrow(int amount)
    {
        if (cooldown == true)
            return;

        cooldown = true;
        if (amount < 0)//positive
        {
            ArrowUpImage.transform.DOPunchPosition(new Vector3(0, -40, 0), arrowJumpCD, 10, 1);
        }
        else if(amount > 0)// negative
        {
            ArrowDownImage.transform.DOPunchPosition(new Vector3(0, 40, 0), arrowJumpCD, 10, 1);
        }
        else//reset animations
        {

        }
        StartCoroutine(resetCD());
    }
    IEnumerator resetCD()
    {
        yield return new WaitForSeconds(arrowJumpCD);
        cooldown = false;
    }

    private void Update()
    {
        moneyDisplay.text = PlayerStats.i.Money.ToString();
        SelectAmountUIFunctionalities();
    }
}
