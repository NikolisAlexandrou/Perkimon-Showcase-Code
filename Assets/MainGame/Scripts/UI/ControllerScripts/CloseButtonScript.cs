using UnityEngine;
using UnityEngine.UI;
public class CloseButtonScript : MonoBehaviour
{
    private void Update()
    {
        CloseMenu();
    }

    void CloseMenu()
    {
        if (UIManager.i.IsTopUI(transform.parent.gameObject))
        {
            if (Input.GetButtonDown("CloseMenu"))
            {

                GetComponent<Button>().onClick?.Invoke();
            }
        }
    }
}
