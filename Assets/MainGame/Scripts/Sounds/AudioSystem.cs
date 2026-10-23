using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Type of sound that is played
/// </summary>
public enum AudioMaster 
{
    SFX,
    BATTLE,
    UI
}

/// <summary>
/// Holds information about sound effects, audio masters and methods that play certain audios
/// </summary>
public class AudioSystem : MonoBehaviour
{
    public static AudioSystem i;

    [Header("Audio Masters")]

    [Tooltip("Audio source that all SFX sounds will be played")]
    [SerializeField] AudioSource _SFXAudioMaster;

    [Tooltip("Audio source that all in battle sounds will be played")]
    [SerializeField] AudioSource _battleAudioMaster;

    [Tooltip("Audio source that all UI sounds will be played")]
    [SerializeField] AudioSource _UIAudioMaster;

    [Header("SFX Sounds")]

    [Tooltip("List of sound of water splashing")]
    public List<AudioClip> WaterSplashSounds = new List<AudioClip>();

    [Tooltip("List of sounds of moving in water")]
    public List<AudioClip> WaterMoveSounds = new List<AudioClip>();

    [Tooltip("List of sounds of moving on grass")]
    public List<AudioClip> GrassFootstepsSounds = new List<AudioClip>();

    [Tooltip("List of sounds of boulder breaking")]
    public List<AudioClip> BoulderBreakSounds = new List<AudioClip>();

    [Tooltip("List of sounds of tree breaking")]
    public List<AudioClip> TreeBreakSounds = new List<AudioClip>();

    [Tooltip("List of sounds of picking up item from ground")]
    public List<AudioClip> PickUpItemSounds = new List<AudioClip>();

    [Tooltip("List of sounds of door opening")]
    public List<AudioClip> DoorOpenSounds = new List<AudioClip>();

    [Tooltip("List of sounds of door closing")]
    public List<AudioClip> DoorCloseSounds = new List<AudioClip>();

    [Tooltip("List of sounds of trainer alert")]
    public List<AudioClip> TrainerAlertSounds = new List<AudioClip>();

    [Tooltip("Sound of throwing catching object")]
    public AudioClip ThrowCatchingObjectSound;

    [Header("Battle Sounds")]

    [Tooltip("List of sounds of monster taking damage in battle")]
    public List<AudioClip> TakeDamageSound = new List<AudioClip>();

    [Header("UI Sounds")]

    [Tooltip("List of sounds of backpack zip opening/closing")]
    public List<AudioClip> BackpackZipSound = new List<AudioClip>();

    [Tooltip("Sound of clicking a UI button")]
    public AudioClip UIClickSound;

    [Tooltip("Sound of buying an item from shop UI")]
    public AudioClip BuyItemSound;

    [Header("Themes")]

    [Tooltip("Theme playing when battling a trainer")]
    public AudioSource TrainerBattleTheme;

    [Header("Ambience")]

    [Tooltip("Ambience sound when in a forest")]
    public AudioSource ForestAmbience;

    private readonly List<AudioSource> currentOverworldAmbience = new List<AudioSource>();  //  Contains all ambience audio sources in the scene
    
    private void Awake()
    {
        if (i != null)
            Destroy(i);
        i = this;
    }

    /// <summary>
    /// Pauses all ambience sounds currently playing in the scene
    /// </summary>
    public void PauseAllAmbienceSounds()
    {
        for (int i = 0; i < currentOverworldAmbience.Count - 1; i++)
            PauseAmbienceSound(currentOverworldAmbience[i]);
    }

    /// <summary>
    /// Resume playing all ambience sounds currently playing in the scene
    /// </summary>
    public void ResumeAllAmbienceSounds()
    {
        for (int i = 0; i < currentOverworldAmbience.Count - 1; i++)
            PlayAmbienceSound(currentOverworldAmbience[i]);
    }

    /// <summary>
    /// Play an ambience sound, if it is not already playing add it to current ambience sounds
    /// and start playing else, unpause it
    /// </summary>
    /// <param name="sound"></param>
    public void PlayAmbienceSound(AudioSource sound)
    {
        if (sound.isPlaying == true)
        {
            sound.UnPause();
        }
        else
        {
            sound.Play();
            currentOverworldAmbience.Add(sound);
        }
    }

    /// <summary>
    /// Pause an ambience sound
    /// </summary>
    /// <param name="sound"></param>
    public void PauseAmbienceSound(AudioSource sound) => sound.Pause();

    /// <summary>
    /// Play a sound clip using the corresponding audio master
    /// </summary>
    /// <param name="sound"></param>
    /// <param name="audioMaster"></param>
    public void PlaySoundClip(AudioClip sound, AudioMaster audioMaster = AudioMaster.SFX)
    {
        switch (audioMaster)
        {
            case AudioMaster.SFX:
                _SFXAudioMaster.PlayOneShot(sound);
                break;
            case AudioMaster.BATTLE:
                _battleAudioMaster.PlayOneShot(sound);
                break;
            case AudioMaster.UI:
                _UIAudioMaster.PlayOneShot(sound);
                break;
            default:
                _SFXAudioMaster.PlayOneShot(sound);
                break;
        }
    }

    /// <summary>
    /// Play a random sound clip from sound list given
    /// </summary>
    /// <param name="soundClips"></param>
    /// <returns></returns>
    public AudioClip PlayRandomSound(List<AudioClip> soundClips) { return soundClips[Random.Range(0, soundClips.Count)]; }
}
