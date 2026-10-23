using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    private bool isControllerConnected = false;
    private string[] controllers;

    void Update()
    {
        DetectInputType();
    }

    void DetectInputType()
    {
        // Detect if a controller is connected
        controllers = Input.GetJoystickNames();
        isControllerConnected = controllers.Length > 0 && !string.IsNullOrEmpty(controllers[0]);

        // Detect last used input
        if (isControllerConnected == true)
        {
            SetNavigationMode(true);
        }
        else
        {
            SetNavigationMode(false);
        }
        /*
        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
        {
            SetNavigationMode(true);
        }
        else if (Input.GetMouseButtonDown(0) || Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0)
        {
            SetNavigationMode(false);
        }*/
    }

    void SetNavigationMode(bool useController)
    {
        EventSystem.current.sendNavigationEvents = useController;

        if (useController)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }
}