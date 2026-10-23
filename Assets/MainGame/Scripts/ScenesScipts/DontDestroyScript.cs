using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script does not destroy the object it is currently attached to when changing scenes
/// </summary>
public class DontDestroyScript : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
}
