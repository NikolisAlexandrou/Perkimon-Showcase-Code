using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
/// <summary>
/// Script is added to Cinemachine camera and automatically finds the object where the
/// map boundaries are set as polygon collider
/// </summary>
public class BoundsScript : MonoBehaviour
{
    
    void Start()
    {
        ApplyBounds();
    }

    /// <summary>
    /// Searches for object with tag "Bounds" and gets its PolygonCollider2D and assignes it to
    /// The CinemachineConfiner script that is attached to this object
    /// </summary>
    public void ApplyBounds()
    {
        //  Check if CinemachineConfiner is not added to this object and return warning message
        //  - You can change this later to add the CinemachineConfiner script if not found
        if (!TryGetComponent(out CinemachineConfiner confiner))
        {
            DebugLogs.Warning($"CinemachineConfiner not added to {name} and may cause errors!");
            return;
        }

        //  Searches for first object with "Bounds" tag and if no objects are found return warning message
        var boundsObject = GameObject.FindGameObjectWithTag("Bounds");
        if (boundsObject == null)
        {
            DebugLogs.Warning("No object with tag \"Bounds\" was found. Create one and add PolygonCollider2D to it.");
            return;
        }

        //  Check if the Bounds object has polygon collider and if not return warning message
        if (!boundsObject.TryGetComponent(out PolygonCollider2D polygon))
        {
            DebugLogs.Warning($"No PolygonCollider2D found on {boundsObject.name}. Consider adding it.");
            return;
        }

        //  Assign the polygon collider to the CinemachineConfiner script
        confiner.m_BoundingShape2D = polygon;
    }

}
