using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Surface Audio Set")]
public class SurfaceAudioSet : ScriptableObject
{
    public AudioClip[] clips;
    public AudioClip GetRandom() => (clips != null && clips.Length > 0) ? clips[Random.Range(0, clips.Length)] : null;
}
