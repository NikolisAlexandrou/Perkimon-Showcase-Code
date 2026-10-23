using UnityEngine;
using System.Collections.Generic;

public class StoryObjectScript : MonoBehaviour
{
    //--------------------------------------------------------------------------------------------------
    //This script will detect if player is near a story object and enable it if the story number matches
    //HOW TO CHECK: there will be a box trigger that if player enters, it will enable the story objects
    //--------------------------------------------------------------------------------------------------

    [SerializeField] int storyNumber;
    [SerializeField] List<GameObject> storyObjectsToEnable = new List<GameObject>();


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (storyNumber == PlayerStats.i.StoryNumber)//check if current story number matches the story number of these objects
            {
                //enable objects
                EnableObjects(true);
            }
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (storyNumber == PlayerStats.i.StoryNumber)//check if current story number matches the story number of these objects
            {
                //enable objects
                EnableObjects(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            //Disable objects
            EnableObjects(false);
        }
    }

    void EnableObjects(bool state)
    {
        foreach (GameObject item in storyObjectsToEnable)
        {
            if (GameUtils.i.IsInCutscene() == false)
            {
                item.SetActive(state);
            }
        }
    }

}
