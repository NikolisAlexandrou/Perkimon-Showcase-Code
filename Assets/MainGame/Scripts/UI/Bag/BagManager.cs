using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BagManager : MonoBehaviour
{
    public static BagManager i;
    public List<Item> bagList = new List<Item>();
    public List<Item> currentDisplayList = new List<Item>();
    public Image ItemImage;
    public TextMeshProUGUI ItemName;
    public TextMeshProUGUI ItemDescription;
    public TextMeshProUGUI ItemAmount;
    public TextMeshProUGUI CategoryName;
    [SerializeField] GameObject contentUI;
    [SerializeField] GameObject ItemUIHolder;
    [SerializeField] GameObject BagMenu;

    int currentSelected; //used for dynamic controller UI
                         //BagList stores all the items currently in bag menu (EVERY category, unsorted)
                         //
                         //currentDisplayList stores the items in Currently selected category, eg. Stores ONLY capsules when
                         //capsule category is selected
                         //
                         //currentDisplayList is used for VISUAL display of bags and needs to be changed every time
                         //a category is selected or when an item is removed from bag

    public void InitialiseCurrentDisplayList(ITEM_CATEGORY category)
    {
        //clear current Display list
        currentDisplayList.Clear();
        //loop through ALL the items in bag
        for (int i = 0; i < bagList.Count; i++)
        {
            //find and add ONLY items from a specific category to current display list
            if (bagList[i]._Category == category)
            {
                currentDisplayList.Add(bagList[i]);
            }
        }
    }
    public void InitialiseCurrentDisplayListObjects()
    {
        //check if category is already selected
        //if (contentUI.transform.childCount > 1 &&contentUI.transform.GetChild(getItemPositionCategory(currentDisplayList[0])).GetComponent<ItemHolderUI>().item._Category == bagList[getItemPositionCategory(currentDisplayList[0])]._Category)
         //   return;

        for (int i = 0; i < contentUI.transform.childCount; i++)
        {
            Destroy(contentUI.transform.GetChild(i).gameObject);
        }
        for (int i = 0; i < currentDisplayList.Count; i++)
        {
            GameObject ob = Instantiate(ItemUIHolder, new Vector2(0, 0), Quaternion.identity, contentUI.transform);
            ob.GetComponentInChildren<TextMeshProUGUI>().text = currentDisplayList[i]._name + " x" + currentDisplayList[i].getAmount;
            ob.GetComponent<ItemHolderUI>().item = currentDisplayList[i];
        }
        StartCoroutine(BagMenu.GetComponent<DynamicControllerUISetUp>().initDelayVertical(false));
    }
    //used in category buttons All 4
    public void ClickCapsulesCategoryButton()
    {
        ClickCategoryButton(ITEM_CATEGORY.CAPSULES);
    }
    public void ClickGeneralCategoryButton()
    {
        ClickCategoryButton(ITEM_CATEGORY.GENERAL);
    }
    public void ClickScrollsCategoryButton()
    {
        ClickCategoryButton(ITEM_CATEGORY.SCROLLS);
    }
    public void ClickImportantCategoryButton()
    {
        ClickCategoryButton(ITEM_CATEGORY.IMPORTANT);
    }
    public void ClickCategoryButton(ITEM_CATEGORY category)
    {
        switch (category)
        {
            case ITEM_CATEGORY.CAPSULES:
                CategoryName.text = "CAPSULES";
                break; 
            case ITEM_CATEGORY.GENERAL:
                CategoryName.text = "GENERAL ITEMS";
                break;
            case ITEM_CATEGORY.SCROLLS:
                CategoryName.text = "SCROLLS";
                break;
            case ITEM_CATEGORY.IMPORTANT:
                CategoryName.text = "IMPORTANT ITEMS";
                break;
            default:
                break;
        }
        InitialiseCurrentDisplayList(category);
        InitialiseCurrentDisplayListObjects();
       
    }
    IEnumerator setSelectedDelay()
    {
        yield return new WaitForSeconds(0.15f);
        GameUtils.i.SetNewSelectedUIObject(BagManager.i.GetBagMenu().GetComponent<DynamicControllerUISetUp>().GetStandardLeftButton()[0].gameObject);
    }

    public void SetCurrentSelected(int now)
    {
        currentSelected = now;
    }
    //plays after removing the item
    public int GetCurrentSelected()
    {
        int newSelect = currentSelected;
        if (currentSelected >= currentDisplayList.Count)
        {
            newSelect = currentDisplayList.Count - 1;
        }else if (currentDisplayList.Count == 0)
        {
            newSelect = 0;
        }else if (currentSelected == 0 && currentDisplayList.Count > 1)
        {
            newSelect = 1;
        }
        else
        {
            newSelect = currentSelected - 1;
        }
        return newSelect;
    }
    public GameObject GetBagMenu()
    {
        return BagMenu;
    }
    private void Awake()
    {
       
        if (i != null)
        {
            Destroy(i);
        }
        i = this;
    }
    public bool SearchForItemInBag(Item item)
    {
        bool found = false;
        if (bagList.Count >= 1)//check if bag is not empty
        {
            int i = 0;
            while (i < bagList.Count && found == false)
            {
                if (bagList[i]._name == item._name && bagList[i].getAmount >= 1)
                {
                    
                    found = true;
                }
                i++;
            }
        }
        return found;
    }
    public bool SearchForItemAndAmountInBag(Item item)
    {
        bool found = false;
        if (bagList.Count >= 1)
        {
            int i = 0;
            while (i < bagList.Count && found == false)
            {
                if (bagList[i]._name == item._name && bagList[i].getAmount >= item.getAmount)
                {
                    found = true;
                }
                i++;
            }
        }
        return found;
    }
    public IEnumerator ClosePartyWhenEmpty(Item item)
    {
        yield return new WaitForSeconds(.05f);
        if (SearchForItemInBag(item) == false)
        {
            
            UIManager.i.EnablePartyMenu(false);
            if (currentDisplayList.Count >= 1)
            {
                yield return new WaitForSeconds(.05f);
                BagManager.i.GetBagMenu().GetComponent<DynamicControllerUISetUp>().SetSelected(BagManager.i.GetCurrentSelected());
                StartCoroutine(resetBag());
            }
            else
            {
                GameUtils.i.SetNewSelectedUIObject(BagManager.i.GetBagMenu().GetComponent<DynamicControllerUISetUp>().GetStandardLeftButton()[0].gameObject);
            }
            
           
        }
    }
    IEnumerator resetBag()
    {
        yield return new WaitForSeconds(0.2f);
        GetBagMenu().GetComponent<DynamicControllerUISetUp>().InitialiseCatchingListVerticalUI(false);
    }
    public int getItemPosition(Item item)
    {
        bool found = false;
        int pos = 0;
        if (bagList.Count >= 1)
        {
            int i = 0;
            while (i < bagList.Count && found == false)
            {
                if (bagList[i]._name == item._name)
                {
                    pos = i;
                    found = true;
                }
                i++;
            }
        }
        return pos;
    }
    public int getItemPositionCategory(Item item)
    {
        bool found = false;
        int pos = -1;
        if (currentDisplayList.Count >= 1)
        {
            int i = 0;
            while (i < currentDisplayList.Count && found == false)
            {
                if (currentDisplayList[i]._name == item._name)
                {
                    pos = i;
                    found = true;
                }
                i++;
            }
        }
        return pos;
    }
    //TODO correct amount not adding on top of bag
    public void AddItemToBag(Item item)
    {
        if (SearchForItemInBag(item) == true)// && bagList[getItemPosition(item)].getAmount < 999)
        {
            int position = getItemPosition(item);
            bagList[position].addAmount(item.getAmount);
            
        }
        else
        {
            bagList.Add(item);
            //GameObject ob = Instantiate(ItemUIHolder, new Vector2(0, 0), Quaternion.identity, contentUI.transform);
            //BagMenu.GetComponent<SelectionMenuScript>().menuContent.Add(ob);
        }
       //change image and text of item added in specific category
        //contentUI.transform.GetChild(getItemPositionCategory(item)).GetComponentInChildren<TextMeshProUGUI>().text = bagList[getItemPosition(item)]._name + " x" + bagList[getItemPosition(item)].getAmount;
        //contentUI.transform.GetChild(getItemPositionCategory(item)).GetComponent<ItemHolderUI>().item = bagList[getItemPosition(item)];
    }
    public void AddItemToBag(Item item, int amount)
    {

        if (SearchForItemInBag(item) == true) // && bagList[getItemPosition(item)].getAmount < 999 && bagList[getItemPosition(item)].getAmount + amount <= 999)
        {
            int position = getItemPosition(item);
            bagList[position].addAmount(amount);
        }
        else
        {
            item.setAmount(amount);
            bagList.Add(item);
            //GameObject ob = Instantiate(ItemUIHolder, new Vector2(0, 0), Quaternion.identity, contentUI.transform);
            //BagMenu.GetComponent<SelectionMenuScript>().menuContent.Add(ob);
        }

        //contentUI.transform.GetChild(getItemPositionCategory(item)).GetComponentInChildren<TextMeshProUGUI>().text = bagList[getItemPosition(item)]._name + " x" + bagList[getItemPosition(item)].getAmount;
        //contentUI.transform.GetChild(getItemPositionCategory(item)).GetComponent<ItemHolderUI>().item = bagList[getItemPosition(item)];
    }
    public void RemoveItemFromBag(Item item, int amount)
    {
        if (SearchForItemInBag(item) == false)
            return;
        int position = getItemPosition(item);
        if (bagList[position].getAmount > amount)
        {
            bagList[position].addAmount(-amount);
            if (getItemPositionCategory(item) != -1 && contentUI.transform.childCount > getItemPositionCategory(item))
            {
                contentUI.transform.GetChild(getItemPositionCategory(item)).GetComponentInChildren<TextMeshProUGUI>().text = bagList[position]._name + " x" + bagList[position].getAmount;
            }
            
        }
        else
        {
            //BagMenu.GetComponent<SelectionMenuScript>().selectedImage.transform.SetParent(contentUI.transform.parent);
            //BagMenu.GetComponent<SelectionMenuScript>().selectedImage.SetActive(false);
            if (position >= bagList.Count -1 && bagList.Count > 1)
            {
                //BagMenu.GetComponent<SelectionMenuScript>().selectedImage.transform.SetParent(BagMenu.GetComponent<SelectionMenuScript>().menuContent[position - 1].transform);
                BagMenu.GetComponent<SelectionMenuScript>().current -= 1;
            }

            //BagMenu.GetComponent<SelectionMenuScript>().menuContent.RemoveAt(position);
            //TODO Change position of selection item
            //GameUtils.i.SetNewSelectedUIObject(null);
            if (getItemPositionCategory(item) != -1)
            {
                //Destroy(contentUI.transform.GetChild(getItemPositionCategory(item)).gameObject);
            }
            bagList.RemoveAt(position);
            //currentDisplayList.RemoveAt(getItemPositionCategory(item));
            
        }
       
    }
    public void SelectItemInBag()
    {
        for (int i = 0; i < contentUI.transform.childCount; i++)
        {
            contentUI.transform.GetChild(i).GetComponent<ItemHolderUI>().SetSelected(false);
        }
        contentUI.transform.GetChild(BagMenu.GetComponent<SelectionMenuScript>().current).GetComponent<ItemHolderUI>().SetSelected(true);
    }
    public void OpenBagMenu()
    {
        ClickCapsulesCategoryButton();
        BagMenu.SetActive(true);
        //if (bagList.Count > 0)
        //{
        //    BagMenu.GetComponent<SelectionMenuScript>().selectedImage.SetActive(true);
        // 
        //}
        //else
        //{
        //    BagMenu.GetComponent<SelectionMenuScript>().selectedImage.SetActive(false);
        //}
        AudioSystem.i.PlaySoundClip(AudioSystem.i.PlayRandomSound(AudioSystem.i.BackpackZipSound), AudioMaster.UI);
        if (currentDisplayList.Count < 1)
        {
            StartCoroutine(setSelectedDelay());
        }

    }
    public bool ApplyCatchingDisksInBag(/*ref List<Item> disksList*/)
    {
        bool found = false;
        if (bagList.Count >= 1)//check if bag is not empty
        {
            int i = 0;
            while (i < bagList.Count)
            {
                if (bagList[i].ItemBase.IsCatchingObject == true && bagList[i].getAmount >= 1)
                {
                    found = true;
                    //disksList.Add(bagList[i]);
                    CatchingManager.i.catchingObList.Add(bagList[i]);
                }
                i++;
            }
        }
        return found;
    }
    public bool SearchForCatchingDisksInBag()
    {
        bool found = false;
        if (bagList.Count >= 1)//check if bag is not empty
        {
            int i = 0;
            while (i < bagList.Count && found == false)
            {
                if (bagList[i].ItemBase.IsCatchingObject == true && bagList[i].getAmount >= 1)
                {

                    found = true;
                }
                i++;
            }
        }
        return found;
    }
    public void CloseBagMenu()
    {
        BagMenu.SetActive(false);
    }
}
