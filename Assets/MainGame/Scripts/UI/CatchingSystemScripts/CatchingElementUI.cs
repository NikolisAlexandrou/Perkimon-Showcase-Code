using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
public class CatchingElementUI : MonoBehaviour
{
    [SerializeField] Item currentCapsule;
    [SerializeField] Image CatchSprite;
    [SerializeField] TextMeshProUGUI AmountText;

    public void InitialiseCapsule()
    {
        CatchSprite.sprite = currentCapsule.getSprite;
        AmountText.text = "x" + currentCapsule.getAmount;
    }
    public Item GetCapsuleItem()
    {
        return currentCapsule;
    }

    public void SetCapsuleItem(Item newItem)
    {
        currentCapsule = newItem;
    }


    public void OnButtonPressed()
    {
        CatchingManager.i.StartCoroutine(CatchingManager.i.PlayCatchAnimationDelay(currentCapsule));
        
    }
}
