using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MoveChangeScript : MonoBehaviour
{
    public PartyScript partyScript;
    public int moveNumber;
    
    [Header("Move Properties")]
    [SerializeField] private TextMeshProUGUI _moveNameText;
    [SerializeField] private TextMeshProUGUI _moveStaminaText;
    [SerializeField] private Image _moveElementImage;

    public void InitialiseMove(string name, float stamina, Sprite element)
    {
        _moveNameText.text = name;
        _moveStaminaText.text = stamina.ToString();
        _moveElementImage.sprite = element;
    }

    public void OnChangeCurrentMove()
    {
        if (partyScript.partyState != PartyState.OVERWORLD)
            return;

        if (partyScript.selectedMonster == null || moveNumber < 0 || moveNumber >= partyScript.selectedMonster.Moves.Count)
            return;

        BattleMove move1 = new BattleMove(partyScript.selectedMonster.Moves[moveNumber].Base);//Takedown eg
        move1.Initialise();
        if (partyScript.currentMove < 0 || partyScript.currentMove >= partyScript.selectedMonster.CurrentMoves.Count)
            return;

        BattleMove currentMove = new BattleMove(partyScript.selectedMonster.CurrentMoves[partyScript.currentMove].Base);//Meditate eg
        currentMove.Initialise();
        if (SearchMoveExists(move1) != -1)
        {
            //Swap positions
            
            partyScript.selectedMonster.CurrentMoves[SearchMoveExists(move1)] = currentMove; //Takedown <= Meditate eg
            partyScript.selectedMonster.CurrentMoves[partyScript.currentMove] = move1; // Meditate <= Takedown eg
            partyScript.SelectPartyMember(partyScript.selectedMonsterNum);
        }
        else
        {
            partyScript.selectedMonster.CurrentMoves[partyScript.currentMove] = move1;
            partyScript.SelectPartyMember(partyScript.selectedMonsterNum);
        }
    }

    //returns position of move in current moves if it exists, if not display -1
    public int SearchMoveExists(BattleMove move)
    {
        bool exists = false;
        int num = 0;
        int pos = -1;

        while (exists == false && num < partyScript.selectedMonster.CurrentMoves.Count)
        {
            if (partyScript.selectedMonster.CurrentMoves[num] == null || partyScript.selectedMonster.CurrentMoves[num].Base == null)
            {
                num++;
                continue;
            }

            if (partyScript.selectedMonster.CurrentMoves[num].Base.Name == move.Base.Name)
            {
                pos = num;
                exists = true;
            }
            num++;
        }

        return pos;
    }

}
