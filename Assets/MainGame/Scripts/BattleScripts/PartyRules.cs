using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains methods that search for values in a party list
/// </summary>
public static class PartyRules
{
    /// <summary>
    /// Returns true if there is at least one alive monster in the
    /// give party
    /// </summary>
    public static bool HasAliveMonster(List<BattleMonster> party)
    {
        for (int i = 0; i < party.Count; i++)   //  Search through all members in the party
            if (party[i].HP > 0) return true;   //  return true if an alive monster was found  
        return false;
    }

    /// <summary>
    /// Returns number of dead monsters in the given party
    /// </summary>
    public static int CountDead(List<BattleMonster> party)
    {
        int count = 0;
        for (int i = 0; i < party.Count; i++)
            if (party[i].HP <= 0) count++;
        return count;
    }

    /// <summary>
    /// Return the index of party where the next alive monster was found
    /// </summary>
    public static int GetNextAliveSlot(List<BattleMonster> party, int startPos)
    {
        for (int i = Mathf.Max(0, startPos); i < party.Count; i++)
            if (party[i].HP > 0) return i;
        return 0;
    }
}
