using UnityEngine;
using UnityEngine.EventSystems;

public class UIControllerTest : MonoBehaviour
{
    public GameObject firstButton;

    void OnEnable()
    {
        EventSystem.current.SetSelectedGameObject(firstButton);
    }
}