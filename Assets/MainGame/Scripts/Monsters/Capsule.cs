using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Creates a new catching object item
/// </summary>
[CreateAssetMenu(menuName = "New Catching Object", fileName = "catchingObject")]
public class Capsule : ItemBase
{
    [Header("Properties for catching object")]

    [Tooltip("Overworld sprite used for this catching object")]
    public Sprite Capsule_Sprite;

    [Tooltip("Overworld effect used for this catching object when summoning monster")]
    public GameObject Capsule_Particle;
}
