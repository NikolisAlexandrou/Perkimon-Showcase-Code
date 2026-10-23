using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class UIManager : MonoBehaviour
{
    public static UIManager i;
    public GraphicRaycaster raycaster;
    public Stack<GameObject> currentUI { get; set; } = new Stack<GameObject>();
    public GameObject SelectionMenusOB;
    public GameObject MonsterStorage;
    public GameObject PartyScene;

    public GameObject ChoosePerkimonUI;

    public GameObject caveTransition;
    public GameObject sceneTransition;
    public GameObject VSTitleTransition;
    public GameObject VSTitlePlayerName;
    public GameObject VSTitleTrainerName;
    public GameObject VSTitleTrainerImage;

    public GameObject DashOB;
    public GameObject DashFillOB;

    public GameObject ProfileMenu;

    public GameObject LoadingScreen;

    [SerializeField] GameObject miscObjectsParent;

    [Header("Selection Menus")]
    [SerializeField] TextMeshProUGUI showRouteName;

    [Header("Shopkeeper")]
    public GameObject ShopkeeperUI;

    private void Awake()
    {
        if (i != null)
        {
            Destroy(i);
        }
        i = this;
    }

    public bool IsTopUI(GameObject uiObject)
    {
        return uiObject != null && currentUI.Count > 0 && currentUI.Peek() == uiObject;
    }

    public void ClickSound()
    {
        AudioSystem.i.PlaySoundClip(AudioSystem.i.UIClickSound, AudioMaster.UI);
    }

    public void EnableProfileMenu()
    {
        ProfileMenu.GetComponent<ProfileMenuScript>().InitialiseProfile1();
        ProfileMenu.GetComponent<ProfileMenuScript>().InitialiseProfile2();
        ProfileMenu.GetComponent<ProfileMenuScript>().InitialiseProfile3();
        ProfileMenu.SetActive(true);
    }
    public void DisableProfileMenu()
    {
        ProfileMenu.SetActive(false);
    }
    public void EnablePartyMenu(bool enable)
    {
        GameUtils.i.SetNewSelectedUIObject(null);
        PartyScene.SetActive(enable);
    }
    public void EnableMonsterStorage()
    {

        MonsterStorage.SetActive(true);
        MonsterStorageManager.i.InitialiseMonstersToStorage();
        GameUtils.i.battleSystem.SetState(GameState.FREEZE);
    }
    public void DisableMonsterStorage()
    {
        MonsterStorage.SetActive(false);
        GameUtils.i.battleSystem.SetState(GameState.OVERWORLD);
    }

    public void EnableLoadingScreen(bool state)
    {
        LoadingScreen.SetActive(state);
    }

    public void EnableChoosePerkimonUI(bool state)
    {
        ChoosePerkimonUI.SetActive(state);
        if (state == true)
        {
            GameUtils.i.SetGameState(GameState.FREEZE);
        }
        else
        {
            GameUtils.i.SetGameState(GameState.OVERWORLD);
        }
    }

    /// <summary>
    /// TODO CHANGE EVERYTHING REDO THE LOGIC FOR OPENING THE MENU
    /// </summary>
    /// <param name="init"></param>
    /// <returns></returns>
    public IEnumerator InitialiseMainMenu(bool init)
    {
       
        if (init == false)
        {
            if (SelectionMenusOB.activeSelf)
            {
                SelectionMenusOB.GetComponent<Animator>()?.SetTrigger("CloseMenu");

                Animator animator = SelectionMenusOB.GetComponent<Animator>();
                float closeDelay = animator != null ? animator.GetCurrentAnimatorStateInfo(0).length : 0f;
                yield return new WaitForSeconds(closeDelay);

                currentUI.Clear();
                GameUtils.i.SetGameState(GameState.OVERWORLD);
                SelectionMenusOB.SetActive(false);
                DebugLogs.Log("Force Closed Main Menu");
            }
        }
        else
        {
            if (!currentUI.Contains(SelectionMenusOB) && currentUI.Count == 0)
            {
                showRouteName.text = GameUtils.i.player.GetComponent<PlayerController>().CurrentArea.AreaName;
                SelectionMenusOB.SetActive(init);
            }
           
        }
        
    }
    public GameObject GetSelectionMenusOB()
    {
        return SelectionMenusOB;
    }
    public void CloseSelectionMenu()
    {
        StartCoroutine(InitialiseMainMenu(false));
       
    }

    private void Update()
    {
    }

    public GameObject GetMiscObjectsParent() => miscObjectsParent;
}
