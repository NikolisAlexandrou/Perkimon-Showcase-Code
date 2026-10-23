using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


public class CatchingObjectScript : MonoBehaviour
{
    [SerializeField] BattleMonster currentCatchingMonster;
    [SerializeField] GameObject currentCatchingMonsterOB;
    [SerializeField] bool catchMon;
    [SerializeField] int turns;
    bool alreadyOccupied;

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Monster") && alreadyOccupied == false)
        {
            if (other.GetComponent<Overworld_MonsterBehaviour>() != null && other.GetComponent<Overworld_MonsterBehaviour>().Interrupt == false)
            {
                alreadyOccupied = true;
                currentCatchingMonsterOB = other.gameObject;
                currentCatchingMonster = other.GetComponent<Overworld_MonsterBehaviour>().GetMonster;
                other.GetComponent<Overworld_MonsterBehaviour>().Interrupt = true;
                other.GetComponent<Rigidbody2D>().linearVelocity = new Vector3(0, 0, 0);
                other.GetComponent<CapsuleCollider2D>().enabled = false;
                GetComponent<Rigidbody2D>().linearVelocity = new Vector3(0,0,0);
                StartCoroutine(PlayCatchAnimationDelay(other.gameObject));
            }
        }
    }

    public IEnumerator PlayCatchAnimationDelay(GameObject monster)
    {
        monster.GetComponent<SpriteRenderer>().DOColor(Color.red, CatchingManager.i.EnemyInObjectTimer / 2);
        monster.GetComponent<SpriteRenderer>().DOFade(0, CatchingManager.i.EnemyInObjectTimer);
        yield return new WaitForSeconds(CatchingManager.i.EnemyInObjectTimer + 0.2f);
        //isPlayingCatchAnimation = true;
        StartCoroutine(PlayCatchAnimation(gameObject));
    }

    public bool CalculateChanceToCatch()
    {
        float ran = Random.Range(0, 100 - 1);
        float percentageOFF = ((ran - currentCatchingMonster.CatchDifficulty) / 100) * 100;
        catchMon = false;
        if (percentageOFF >= 70) // no turns
        {
            turns = 0;
        }
        else if (percentageOFF >= 30 && percentageOFF > 60) // 1 turn no catch
        {
            turns = 1;
        }
        else if (percentageOFF >= 10 && percentageOFF > 30) // 2 turn no catch
        {
            turns = 2;
        }
        else if (percentageOFF >= 1 && percentageOFF > 10) // 3 turn no catch
        {
            turns = 3;
        }
        else if (percentageOFF <= 0) // 3 turn catch
        {
            turns = 3;
            catchMon = true;
        }
        return catchMon;
    }


    public IEnumerator PlayCatchAnimation(GameObject ob)
    {
        GameObject rotateOb = ob.transform.GetChild(0).gameObject;
        bool ChanceToCatch = CalculateChanceToCatch();
        for (int i = 0; i < turns; i++)
        {
            yield return new WaitForSeconds(1f);
            rotateOb.transform.Rotate(new Vector3(0, 0, -90));
        }
        yield return new WaitForSeconds(0.5f);
        if (ChanceToCatch == true)
        {
            //end Battle


            yield return new WaitForSeconds(1f);
            Destroy(ob);
            currentCatchingMonster.Initialise();
            PlayerStats.i.AddMonsterToParty(currentCatchingMonster);
            Destroy(currentCatchingMonsterOB);
        }
        else
        {
            Destroy(ob);
            currentCatchingMonsterOB.GetComponent<SpriteRenderer>().DOColor(Color.white, .5f);
            currentCatchingMonsterOB.GetComponent<SpriteRenderer>().DOFade(1, .5f);
            currentCatchingMonsterOB.GetComponent<Overworld_MonsterBehaviour>().Interrupt = false;
            currentCatchingMonsterOB.GetComponent<CapsuleCollider2D>().enabled = true;
        }

    }
}
