using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class ChoosePerkimonUIScript : MonoBehaviour
{
    public BattleMonster perkimon1;
    public BattleMonster perkimon2;
    public BattleMonster perkimon3;
    public CutsceneTrigger choseGrasscutscene;
    public CutsceneTrigger choseFirecutscene;
    public CutsceneTrigger choseWatercutscene;

    [Header("Perkimon Overworld")]
    public GameObject perkimon1Overworld;//assigned automatically
    public GameObject perkimon2Overworld;//assigned automatically
    public GameObject perkimon3Overworld;//assigned automatically

    [Header("UI Settings")]
    [SerializeField] TextMeshProUGUI perkimonDescription;
    [SerializeField] Image perkimon1Image;
    [SerializeField] Image perkimon2Image;
    [SerializeField] Image perkimon3Image;

    [SerializeField] TextMeshProUGUI perkimon1Name;
    [SerializeField] TextMeshProUGUI perkimon2Name;
    [SerializeField] TextMeshProUGUI perkimon3Name;

    private void OnEnable()
    {
        InitialiseUI();
    }
    public void InitialiseUI()
    {
        perkimon1Image.sprite = perkimon1.Base.frontSprite;
        perkimon2Image.sprite = perkimon2.Base.frontSprite;
        perkimon3Image.sprite = perkimon3.Base.frontSprite;

        perkimon1Name.text = perkimon1.Base.Name;
        perkimon2Name.text = perkimon2.Base.Name;
        perkimon3Name.text = perkimon3.Base.Name;
    }

    //used in UI button when pressing perkimon to add to party
    public void ChoosePerkimon(int value)
    {
        if (value == 1)
        {
            PlayerStats.i.AddMonsterToParty(perkimon1);
            choseGrasscutscene.PlayCutscene();
            perkimon1Overworld.SetActive(false);
        }
        else if (value == 2)
        {
            PlayerStats.i.AddMonsterToParty(perkimon2);
            choseFirecutscene.PlayCutscene();
            perkimon2Overworld.SetActive(false);
        }
        else if (value == 3)
        {
            PlayerStats.i.AddMonsterToParty(perkimon3);
            choseWatercutscene.PlayCutscene();
            perkimon3Overworld.SetActive(false);
        }
        UIManager.i.EnableChoosePerkimonUI(false);
       
    }
}
