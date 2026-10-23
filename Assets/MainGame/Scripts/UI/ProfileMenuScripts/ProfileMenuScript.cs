using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ProfileMenuScript : MonoBehaviour
{
    [SerializeField] GameObject profile1;
    [SerializeField] GameObject profile2;
    [SerializeField] GameObject profile3;

    [Header("Profile Page 1")]
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI moneyText;
    [SerializeField] TextMeshProUGUI perkimonOwnedText;

    [Header("Profile Page 2")]
    [SerializeField] TextMeshProUGUI mainquestsCompletedText;
    [SerializeField] TextMeshProUGUI otherquestsCompletedText;

    [Header("Profile Page 3")]
    [SerializeField] TextMeshProUGUI TournamentsWonText;
    [SerializeField] TextMeshProUGUI TournamentsLostText;
    [SerializeField] TextMeshProUGUI TournamentRankText;
    [SerializeField] TextMeshProUGUI BattleTowerTrainersDefeatedText;
    [SerializeField] TextMeshProUGUI BattleTowerBossesDefeatedText;
    [SerializeField] TextMeshProUGUI BattleTowerHighestFloorText;

    [Header("Explorer Rank")]
    [SerializeField] TextMeshProUGUI explorerRankText;
    [SerializeField] GameObject starHolderOB;
    [SerializeField] GameObject starPrefab;

    [Header("Trail Masters")]
    [SerializeField] Image mouxtarisProfile;
    [SerializeField] Image gerosProfile;
    [SerializeField] Image nikosProfile;

    [SerializeField] Sprite mouxtarisProfileUndefeated;
    [SerializeField] Sprite gerosProfileUndefeated;
    [SerializeField] Sprite nikosProfileUndefeated;

    [SerializeField] Sprite mouxtarisProfileDefeated;
    [SerializeField] Sprite gerosProfileDefeated;
    [SerializeField] Sprite nikosProfileDefeated;
    [SerializeField] TextMeshProUGUI TrialMasterText;

    private void Start()
    {
        EnableProfile1();
        InitialiseProfile1();
        InitialiseProfile2();
        InitialiseProfile3();
    }

    public void InitialiseProfile1()
    {
        nameText.text = "Name: " + PlayerStats.i.PlayerParty;
        moneyText.text = "Money: " + PlayerStats.i.Money+"$";
        perkimonOwnedText.text = "Perkimon Owned: " + PlayerStats.i.MonstersOwned;
        explorerRankText.text = "Explorer Rank: " + PlayerStats.i.PlayerLevel;
        for (int i = 0; i < PlayerStats.i.PlayerLevel; i++)
        {
            Instantiate(starPrefab, starHolderOB.transform);
        }
    }


    public void InitialiseProfile2()
    {
        mainquestsCompletedText.text = "Main Quests Completed: " + PlayerStats.i.StoryNumber;
        otherquestsCompletedText.text = "Other Quests Completed: " + PlayerStats.i.OtherQuestsCompleted;
        TrialMasterText.text = "Trial Masters Defeated: " + PlayerStats.i.BossTrainersDefeated;
        InitialiseTrialMasters();
    }

    public void InitialiseProfile3()
    {
        TournamentsWonText.text = "Tournaments Won: " + PlayerStats.i.TournamentsWon;
        TournamentsLostText.text = "Tournaments Lost: " + PlayerStats.i.TournamentsLost;
        TournamentRankText.text = "Tournament Rank: " + PlayerStats.i.TournamentRank;

        BattleTowerTrainersDefeatedText.text = "Trainers Defeated: " + PlayerStats.i.BattletowerTrainersDefeated;
        BattleTowerBossesDefeatedText.text = "Bosses Defeated: " + PlayerStats.i.BattletowerBossesDefeated;
        BattleTowerHighestFloorText.text = "Highest Floor: " + PlayerStats.i.BattletowerHighestFloorReached;
    }

    public void EnableProfile1()
    {
        profile1.SetActive(true);
        profile2.SetActive(false);
        profile3.SetActive(false);
    }
    public void EnableProfile2()
    {
        profile2.SetActive(true);
        profile1.SetActive(false);
        profile3.SetActive(false);
    }
    public void EnableProfile3()
    {
        profile1.SetActive(false);
        profile2.SetActive(false);
        profile3.SetActive(true);
    }

    public void InitialiseTrialMasters()
    {
        if (PlayerStats.i.IsMouxtarisDefeated == true)
            mouxtarisProfile.sprite = mouxtarisProfileUndefeated;
        else
            mouxtarisProfile.sprite = mouxtarisProfileDefeated;

        if (PlayerStats.i.IsGerosDefeated == true)
            gerosProfile.sprite = gerosProfileUndefeated;
        else
            gerosProfile.sprite = gerosProfileDefeated;

        if (PlayerStats.i.IsNikosDefeated == true)
            nikosProfile.sprite = nikosProfileUndefeated;
        else
            nikosProfile.sprite = nikosProfileDefeated;
    }


    bool pressed;
    IEnumerator closeUI()
    {
        yield return new WaitForSeconds(.1F);
        gameObject.SetActive(false);
        pressed = false;
    }
    public void CloseProfileMenu()
    {
        if (pressed == false)
        {
            pressed = true;
            StartCoroutine(closeUI());
        }
    }
}
