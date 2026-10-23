using UnityEngine;
using UnityEngine.Playables;

public class CutsceneTrigger : MonoBehaviour
{
    [SerializeField] PlayableDirector cutsceneToPlay;
    [SerializeField] string triggerTag;
    [SerializeField] bool pausePlayer;
    [SerializeField] int StoryNumber;
    bool triggered;


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered == true)
            return;
        if (StoryNumber != PlayerStats.i.StoryNumber)
            return;


        if (other.CompareTag(triggerTag))
        {
            PlayCutscene();
            triggered = true;
        }
    }
    public void EndCutscene()
    {
        GameUtils.i.SetInCutscene(false);
    }

    public void UnpausePlayer()
    {
        //Unpause player Controllers
        GameUtils.i.SetGameState(GameState.OVERWORLD);
    }
    public void PausePlayer()
    {
        GameUtils.i.SetGameState(GameState.FREEZE);
    }

    public void PlayCutscene()
    {
        //Pause player controllers
        if (pausePlayer == true)
        {
            GameUtils.i.SetGameState(GameState.FREEZE);
        }
        //Play cutscene
        GetComponent<TimelineBinder>().Bind();
        cutsceneToPlay.Play();
        GameUtils.i.SetInCutscene(true);
    }

    public void ProgressStory()
    {
        PlayerStats.i.StoryNumber += 1;
    }

    public void DisableObject(GameObject ob)
    {
        ob.SetActive(false);
    }

    public void EnableObject(GameObject ob)
    {
        ob.SetActive(true);
    }

    public void DestroyObject(GameObject ob)
    {
        Destroy(ob);
    }

    public void HealTeam()
    {
        GameUtils.i.HealPlayerTeam();
    }
}
