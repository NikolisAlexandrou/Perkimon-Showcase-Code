using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


/// <summary>
/// --------------------------------------------------------------------------------------
/// (!)              NOT USED, USE InteractableScript FOR GIVING ITEMS                 (!)
/// --------------------------------------------------------------------------------------
/// </summary>  
public class OverworldItem : MonoBehaviour
{
    [SerializeField] private Item item;
    [SerializeField] private GameObject _outlineObject;
    
    [Header(("Moving Settings"))]
    [SerializeField] private float floatHeight = 0.15f;
    [SerializeField] private float floatDuration = 0.7f;
    
    private bool playerInRange = false;
    private bool interacted = false;

    public void SetItem(Item item)
    {
        this.item = item;
        GetComponent<SpriteRenderer>().sprite = item._sprite;
        _outlineObject.GetComponent<SpriteRenderer>().sprite = item._sprite;
    }
    
    private void Start()
    {
        item.Initialise();
        GetComponent<SpriteRenderer>().sprite = item._sprite;
        _outlineObject.GetComponent<SpriteRenderer>().sprite = item._sprite;

        transform
            .DOLocalMoveY(transform.localPosition.y + floatHeight, floatDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }
    
    private void PickUpItem()
    {
        item.Initialise();
        DebugLogs.Log("INTERACTABLE > Player interacted with item: " + item._name);
        interacted = true;
        
        AudioSystem.i.PlaySoundClip(AudioSystem.i.PlayRandomSound(AudioSystem.i.PickUpItemSounds), AudioMaster.SFX);
        addToBag();
    }

    private void addToBag()
    {
        BagManager.i.AddItemToBag(item);
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (interacted == false)
            {
                PickUpItem();
            }
        }
    }
}