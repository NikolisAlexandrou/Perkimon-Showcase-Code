using UnityEngine;

public class ChoosePerkimonScript : MonoBehaviour
{
    [SerializeField] BattleTrainer rival;
    [SerializeField] CutsceneTrigger cutsceneGrass;
    [SerializeField] CutsceneTrigger cutsceneFire;
    [SerializeField] CutsceneTrigger cutsceneWater;

    [Header("Perkimon Overworld")]
    [SerializeField] GameObject perkimon1Overworld;
    [SerializeField] GameObject perkimon2Overworld;
    [SerializeField] GameObject perkimon3Overworld;


    [Header("General Settings")]
    public string promptMessage = "Press E to interact";

    private bool playerInRange = false;
    bool interacted = false;

    private void Start()
    {
        //when logging back in the game disable story object
        if (PlayerStats.i.StoryNumber > 0)
        {
            gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (playerInRange && Input.GetButtonDown("Interact") && interacted == false)
        {
            ShowPerkimonUI();
        }
    }

    void ShowPerkimonUI()
    {
        interacted = true;
        UIManager.i.EnableChoosePerkimonUI(true);
        UIManager.i.ChoosePerkimonUI.GetComponent<ChoosePerkimonUIScript>().choseGrasscutscene = cutsceneGrass;
        UIManager.i.ChoosePerkimonUI.GetComponent<ChoosePerkimonUIScript>().choseFirecutscene = cutsceneFire;
        UIManager.i.ChoosePerkimonUI.GetComponent<ChoosePerkimonUIScript>().choseWatercutscene = cutsceneWater;
        UIManager.i.ChoosePerkimonUI.GetComponent<ChoosePerkimonUIScript>().perkimon1Overworld = perkimon1Overworld;
        UIManager.i.ChoosePerkimonUI.GetComponent<ChoosePerkimonUIScript>().perkimon2Overworld = perkimon2Overworld;
        UIManager.i.ChoosePerkimonUI.GetComponent<ChoosePerkimonUIScript>().perkimon3Overworld = perkimon3Overworld;
        //Destroy(gameObject); // Example: remove item after interaction
    }

    //not working
    public void AddRivalGrassMember()
    {
        
        UIManager.i.ChoosePerkimonUI.GetComponent<ChoosePerkimonUIScript>().perkimon1.Initialise();
        rival.TrainerParty[0] = UIManager.i.ChoosePerkimonUI.GetComponent<ChoosePerkimonUIScript>().perkimon1;
    }
    //not working
    public void AddRivalFireMember()
    {
        UIManager.i.ChoosePerkimonUI.GetComponent<ChoosePerkimonUIScript>().perkimon2.Initialise();
        rival.TrainerParty[0] = UIManager.i.ChoosePerkimonUI.GetComponent<ChoosePerkimonUIScript>().perkimon2;
    }
    //not working
    public void AddRivalWaterMember()
    {
        UIManager.i.ChoosePerkimonUI.GetComponent<ChoosePerkimonUIScript>().perkimon3.Initialise();
        rival.TrainerParty[0] = UIManager.i.ChoosePerkimonUI.GetComponent<ChoosePerkimonUIScript>().perkimon3;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            ShowPrompt(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            ShowPrompt(false);
        }
    }

    void ShowPrompt(bool show)
    {
        if (show == true)
            GameUtils.i.DisplayInteractOb();
        else
            GameUtils.i.InteractDisplayOb.SetActive(false);
    }
}
