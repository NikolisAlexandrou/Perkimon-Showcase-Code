using UnityEngine;
using TMPro;

public class TimeDisplayScript : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI timeText;


    public void ChangeTimeText(string text)
    {
        timeText.text = text;
    }
}
