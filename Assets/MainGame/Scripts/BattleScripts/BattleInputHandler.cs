using UnityEngine;

/// <summary>
/// Centralised input reader for battle. 
/// Keeps everything real-time (AttackHeld etc.).
/// </summary>
public class BattleInputHandler : MonoBehaviour
{
    public bool AttackHeld { get; private set; }
    public bool PartyPressed { get; private set; }
    public bool InteractPressed { get; private set; }

    /// <summary>
    /// Mouse scroll up/down each frame (positive/negative).
    /// </summary>
    public float MoveScroll { get; private set; }

    /// <summary>
    /// 0 to 3 when a number key (1 to 4) was pressed this frame, -1 otherwise.
    /// </summary>
    public int MoveHotkeyPressed { get; private set; } = -1;

    void Update()
    {
        //  Set values for each input
        AttackHeld = Input.GetButton("Attack");
        PartyPressed = Input.GetButtonDown("PartyButton");
        InteractPressed = Input.GetButtonDown("Interact");
        MoveScroll = Input.mouseScrollDelta.y;

        //  Set the move key value when pressed
        MoveHotkeyPressed = -1;
        if (Input.GetKeyDown(KeyCode.Alpha1)) MoveHotkeyPressed = 0;
        else if (Input.GetKeyDown(KeyCode.Alpha2)) MoveHotkeyPressed = 1;
        else if (Input.GetKeyDown(KeyCode.Alpha3)) MoveHotkeyPressed = 2;
        else if (Input.GetKeyDown(KeyCode.Alpha4)) MoveHotkeyPressed = 3;
    }

    /// <summary>
    /// Clear one shot inputs after BattleSystem has consumed them.
    /// Held inputs (AttackHeld) should not be cleared.
    /// </summary>
    public void ClearFrameInputs()
    {
        PartyPressed = false;
        InteractPressed = false;
        MoveScroll = 0f;
        MoveHotkeyPressed = -1;
    }

}
