using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionFunctions : MonoBehaviour
{
    public void StartBattleTransitions()
    {
        gameObject.SetActive(false);
        GameUtils.i.battleSystem.TriggerTrainerBattle();
    }
    public void StartBattleParent()
    {
        gameObject.transform.parent.gameObject.SetActive(false);
        GameUtils.i.battleSystem.TriggerTrainerBattle();
    }
    public void EndBattle()
    {
        GameUtils.i.battleSystem.OnBattleEndEvent?.Invoke();
        GameUtils.i.battleSystem.EndBattle();
    }

    public void EnterCave()
    {
        StartCoroutine(GameUtils.i.currentCaveEntrance.TeleportObject(GameUtils.i.player));
    }


    public void FreezePlayer()
    {
        GameUtils.i.SetGameState(GameState.FREEZE);
    }
    public void UnFreezePlayer()
    {
        GameUtils.i.SetGameState(GameState.OVERWORLD);
    }
    public void SetInCutscene()
    {
        GameUtils.i.SetInCutscene(true);
    }
    public void SetNotInCutscene()
    {
        GameUtils.i.SetInCutscene(false);
    }
    public void DisablePlayerActions()
    {
        GameUtils.i.player.GetComponent<PlayerController>().IsActionsDisabled = true;
    }
    public void EnablePlayerActions()
    {
        GameUtils.i.player.GetComponent<PlayerController>().IsActionsDisabled = false;
    }
    public void DisableSelf()
    {
        gameObject.SetActive(false);
        if (GameUtils.i.IsInCutscene() == false)
        {
            GameUtils.i.UnFreezeGame();
        }
       
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }
    public void DisableParent()
    {
        gameObject.transform.parent.gameObject.SetActive(false);
        if (GameUtils.i.IsInCutscene() == false)
        {
            GameUtils.i.UnFreezeGame();
        }
    }
    public void LoadNextZone()
    {
        string scene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(GameUtils.i.currentNextZone.SceneToTeleport);
        SceneManager.UnloadSceneAsync(scene);
    }
    public void TeleportToNextZone()
    {
        GameUtils.i.virtualCamera.GetComponent<BoundsScript>().ApplyBounds();
        GameUtils.i.player.transform.position = GameUtils.i.currentNextZone.TeleportLocation;
    }

    public void ChangeSeason()
    {
        DaynightCycleScript.i.CheckForSeasonChange();
    }

}
