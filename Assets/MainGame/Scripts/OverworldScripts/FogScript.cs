using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Infinitely offsets the texture of a material in an image in x direction
/// and plays a little "Fog" animation 
/// </summary>
[RequireComponent(typeof(Image))]
public class FogScript : MonoBehaviour
{
    [Tooltip("Speed at which the \"Fog\" animation plays")]
    [SerializeField] private float _scrollSpeed = 0.1f;

    private Image _meshRenderer;  // Image attached to the object

    void Awake()
    {
        _meshRenderer = GetComponent<Image>();
    }

    void FixedUpdate()
    {
        // Get the x speed value with time
        float x = Time.time * _scrollSpeed;

        // Get the x offset to apply as vector
        Vector2 offset = new Vector2(-x, 0);

        // Change x offset of material texture to play little animation
        _meshRenderer.materialForRendering.SetTextureOffset("_MainTex", offset);
    }
}
