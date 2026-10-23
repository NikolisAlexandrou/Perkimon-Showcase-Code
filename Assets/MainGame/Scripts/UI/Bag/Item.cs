using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public enum ITEM_CATEGORY
{
    CAPSULES,
    GENERAL,
    SCROLLS,
    IMPORTANT
}



[System.Serializable]
public class Item
{
    [SerializeField] ItemBase _ItemBase;
    [SerializeField] int _amount;
    public ItemBase ItemBase
    {
        get
        {
            return _ItemBase;
        }
    }
    public void SetItem(ItemBase newItem)
    {
        _ItemBase = newItem;
    }
    public string _name { get; set; }
    public Sprite _sprite { get; set; }
    public string _description { get; set; }
    public ITEM_CATEGORY _Category { get; set; }
    public void Initialise()
    {
        _name = _ItemBase.name;
        _sprite = _ItemBase.Image;
        _description = _ItemBase.Description;
        _Category = _ItemBase.Category;
    }
   
    public int getAmount
    {
        get { return _amount; }
    }
    public Sprite getSprite
    {
        get { return _sprite; }
    }
    public void setAmount(int newAmount)
    {
        _amount = newAmount;
    }
    public void addAmount(int newAmount)
    {
        _amount += newAmount;
    }
}