using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MoveInitialiser : MonoBehaviour
{
    [Tooltip("Add the UI text that will hold the name of the move")]
    [SerializeField] private TextMeshProUGUI _moveName;

    [Tooltip("Add the UI text that will hold the stamina cost of the move")]
    [SerializeField] private TextMeshProUGUI _staminaCost;

    private BattleMove _move;

    /// <summary>
    /// Assignes the move from monster to the move displayed in UI
    /// </summary>
    public BattleMove Move {  get => _move; set { _move = value; }  }

    private void Start() => InitialiseMove();

    /// <summary>
    /// Sets the name and stamina cost UI texts to the name and stamina cost of the move
    /// </summary>
    public void InitialiseMove()
    {
        if (_move == null || _move.Base == null)
            return;

        _moveName.text = _move.Base.name; //  Set the name of move UI
        _staminaCost.text = _move.Base.StaminaCost.ToString();    //  Set the stamina cost of move UI
    }
}
