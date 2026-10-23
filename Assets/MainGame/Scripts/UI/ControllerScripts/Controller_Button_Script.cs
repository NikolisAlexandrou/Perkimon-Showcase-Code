using UnityEngine;
using UnityEngine.UI;
public enum ControllerButtonType
{
    SQUARE,
    TRIANGLE,
    CIRCLE,
    CROSS
}
public class Controller_Button_Script : MonoBehaviour
{
    [SerializeField] ControllerButtonType ButtonType;

    private void OnEnable()
    {
        GetComponent<Image>().sprite = GameUtils.i.IdentifyButton(ButtonType);
        GameUtils.i.GetControllerButtons().Add(this.gameObject);
    }

    private void OnDisable()
    {
        GameUtils.i.GetControllerButtons().Remove(this.gameObject);
    }
}
