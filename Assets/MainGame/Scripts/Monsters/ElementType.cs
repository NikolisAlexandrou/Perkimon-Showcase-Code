using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Create a new element type and add stengths and weaknesses
/// </summary>
[CreateAssetMenu(fileName = "element type" , menuName = "New Element Type")]
public class ElementType : ScriptableObject
{
    [Tooltip("The display name used for the type e.g. fire, water, grass etc.")]
    public string displayName; // e.g. "Fire" (for UI)

    [Tooltip("Color of the type for UI e.g. Red for fire etc.")]
    public Color color = Color.white; 

    [Tooltip("Image of element mostly displayed in UI")]
    public Sprite icon;

    [Header("Type Interactions")]
    [Tooltip("Elements that this element deals DOUBLE damage to.")]
    public List<ElementType> strongAgainst = new List<ElementType>();

    [Tooltip("Elements that this element deals HALF damage to.")]
    public List<ElementType> weakAgainst = new List<ElementType>();

    [Tooltip("Elements that this element deals ZERO damage to.")]
    public List<ElementType> noEffectAgainst = new List<ElementType>();
}