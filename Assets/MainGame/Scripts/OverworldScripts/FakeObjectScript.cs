using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// When player or an overworld monster enters the trigger for the object this script is attached to, the object image
/// becomes transparent while the player or overworld monster is in it and goes back to solid after player or overworld monster leaves
/// </summary>
[RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
public class FakeObjectScript : MonoBehaviour
{

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // When player or overworld monster goes in object, fade the object image
        if (collision.CompareTag("Player") || collision.CompareTag("OverworldMonster"))
            GetComponent<SpriteRenderer>().DOFade(0.4f, 0.2f);
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        // When player or overworld monster goes out of the object, fade out the object image
        if (collision.CompareTag("Player") || collision.CompareTag("OverworldMonster"))
            GetComponent<SpriteRenderer>().DOFade(1, 0.2f);
    }
}
