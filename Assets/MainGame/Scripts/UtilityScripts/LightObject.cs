using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Enables light to an object if it is night and disables light
/// to that object if it is morning
/// </summary>
[RequireComponent(typeof(Light2D))]
public class LightObject : MonoBehaviour
{
     private Light2D _lightScript;

    private void Start()
    {
        _lightScript = GetComponent<Light2D>();
    }

    private void FixedUpdate()
    {
        // Check if it is night and turn on lights
        if (DaynightCycleScript.i.IsNight == true)
            _lightScript.enabled = true;
        else
            _lightScript.enabled = false;
    }
}
