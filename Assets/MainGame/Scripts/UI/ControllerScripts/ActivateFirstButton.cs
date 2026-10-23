using UnityEngine;
using UnityEngine.EventSystems;

public class ActivateFirstButton : MonoBehaviour
{
    void OnEnable()
    {
        GameUtils.i.SetNewSelectedUIObject(gameObject);
    }

}