using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains door opening and closing animations and sounds when the player enters or exits the triggers
/// of the object this script is attached to
/// </summary>
public class DoorScript : TeleportScript
{
    private bool _openedDoor;  // Flag to show if door is opened or not

    //  When player enters the trigger of the door, Play animation of door opening and sound of door opening
    public override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && _openedDoor == false)
        {
            _openedDoor = true;
            GetComponent<Animator>().SetBool("isOpened", true); //  Play animation of door opening
            AudioSystem.i.PlaySoundClip(AudioSystem.i.PlayRandomSound(AudioSystem.i.DoorOpenSounds), AudioMaster.SFX);   //  Play sound of door opening
            Teleport();
        }
    }

    //  When player exits the trigger of the door, Play animation of door closing and sound of door closing
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && _openedDoor == true)
        {
            _openedDoor = false;
            GetComponent<Animator>().SetBool("isOpened", false); //  Play animation of door closing
            AudioSystem.i.PlaySoundClip(AudioSystem.i.PlayRandomSound(AudioSystem.i.DoorCloseSounds), AudioMaster.SFX);   //  Play sound of door closing
        }
    }
}
