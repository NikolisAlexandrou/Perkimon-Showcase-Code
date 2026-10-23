using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/// <summary>
/// Used like a build system, the player gives some items required and if player has enough,
/// show a game object that was hidden, like rebuilding building a house for example
/// </summary>
public class RebuildSystem : MonoBehaviour
{
    [Tooltip("Object to rebuild, initialy set it as invisible")]
    [SerializeField] GameObject RebuildObject;

    [Tooltip("")]
    [SerializeField] GameObject DisabledObject;

    [Tooltip("Items needed for player to give to rebuild the object")]
    [SerializeField] List<Item> ItemsNeeded;

    [Tooltip("Cooldown for re-enabling interacting")]
    [SerializeField] float cooldownTimer;

    [Header("Overworld UI Settings")]

    [Tooltip("Game object that will contain the UI sprites of required items")]
    [SerializeField] GameObject UIItemContainer;

    [Tooltip("")]
    [SerializeField] GameObject ItemsUI;

    [Tooltip("Prefab of the item with required amount that will be displayed")]
    [SerializeField] GameObject ItemPrefab;
    List<GameObject> ItemsNeededOverworld = new List<GameObject>();

    bool cooldown;
    bool hasRebuilt;
    void InitialiseItems()
    {
        for (int i = 0; i < ItemsNeeded.Count; i++)
        {
            ItemsNeeded[i].Initialise();
        }
    }


    void InstantiateRequredItems()
    {
        foreach (Item item in ItemsNeeded)
        {
            GameObject ob = Instantiate(ItemPrefab, UIItemContainer.transform);
            ob.GetComponent<Image>().sprite = item.getSprite;
            ob.GetComponentInChildren<TextMeshProUGUI>().text = item._name + " x" + item.getAmount;
            ItemsNeededOverworld.Add(ob);
        }
    }

    void ChangeAmount()
    {
       

        for (int i = 0; i < ItemsNeeded.Count; i++)
        {
            if (BagManager.i.SearchForItemInBag(ItemsNeeded[i]) == false)
                return;

            ItemsNeededOverworld[i].GetComponentInChildren<TextMeshProUGUI>().text = ItemsNeeded[i]._name + " " + BagManager.i.bagList[BagManager.i.getItemPosition(ItemsNeeded[i])].getAmount + "/" + ItemsNeeded[i].getAmount;
        } 
    }
    private void FixedUpdate()
    {
        ChangeAmount();
    }
    bool HasItems()
    {
        bool hasItems = true;
        foreach (Item item in ItemsNeeded)
        {
            if (BagManager.i.SearchForItemAndAmountInBag(item) == false)
            {
                hasItems = false;
            }
        }

        return hasItems;
    }

    void RemoveItemsFromBag()
    {
        foreach (Item item in ItemsNeeded)
        {
            BagManager.i.RemoveItemFromBag(item, item.getAmount);
        }
    }


    void Rebuild()
    {
        if (HasItems() == true)
        {
            hasRebuilt = true;
            RebuildObject.SetActive(true);
            DisabledObject.SetActive(false);
            ItemsUI.SetActive(false);
            RemoveItemsFromBag();
        }
        else
        {
            ItemsUI.transform.DOShakePosition(0.05f,1,5);
        }
    }
    private void Start()
    {
        InitialiseItems();
        InstantiateRequredItems();
    }


    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (Input.GetButtonDown("Interact") && hasRebuilt == false && cooldown == false)
            {
                cooldown = true;
                Rebuild();
                StartCoroutine(resetCD());
            }
        }
      
    }

    IEnumerator resetCD()
    {
        yield return new WaitForSeconds(cooldownTimer);
        cooldown = false;
    }
}
