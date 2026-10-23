using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyCatchingObjectAnimationScript : MonoBehaviour
{
    [Tooltip("The animation clip of the catching object when it throws out a monster")]
    [SerializeField] private AnimationClip _catchingObjectDestroyAnimation;

    private float timer; // Current timer used

    private void Update()
    {
        // Destroy object after animation ends
        if (timer >= _catchingObjectDestroyAnimation.averageDuration)
            Destroy(gameObject);
        else
            timer += Time.deltaTime;
    }
}
