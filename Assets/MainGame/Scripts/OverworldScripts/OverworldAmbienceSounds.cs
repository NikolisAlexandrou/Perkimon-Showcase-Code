using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Place this to an object and it will play the sound to the player, the closer the player is
/// to the object the louder the sound will get, the further the player is the quieter the sound will get
/// until maxDistance is reached where sound will be mute
/// </summary>
public class OverworldAmbienceSounds : MonoBehaviour
{
    [Header("Ambience Settings")]

    [Tooltip("Ambience sound to play")]
    [SerializeField] private AudioSource _ambienceSound;

    [Tooltip("Maximum distance the ambience sound will be heared")]
    [SerializeField] private float _maxDistance;

    [Header("Editor Settings")]

    [Tooltip("Set the color of the max distance range to be displayed")]
    [SerializeField] private Color _maxDistanceColor = Color.red;

    private void Start()
    {
        //  Start playing the ambience sound on start of game
        AudioSystem.i.PlayAmbienceSound(_ambienceSound);
    }
    
    private void FixedUpdate()
    {
        ChangeSoundWithDistance();
    }

    // Visualize the max distance in editor
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, _maxDistance);
        Gizmos.color = _maxDistanceColor;
    }

    /// <summary>
    /// Changes sound volume depending on distance of player from object, the closer the player
    /// the louder the sound
    /// </summary>
    private void ChangeSoundWithDistance()
    {
        // Check if distance between player and ambience sound object is less than max distance
        if (Vector2.Distance(GameUtils.i.player.transform.position, transform.position) <= _maxDistance)
        {
            // get the volume depending on distance and set the sounds volume
            float volume = 1 - (Vector2.Distance(GameUtils.i.player.transform.position, transform.position) / _maxDistance);
            _ambienceSound.volume = volume;
        }
        else
        {
            // player is out of range, set volume to 0
            _ambienceSound.volume = 0;
        }
    }
}
