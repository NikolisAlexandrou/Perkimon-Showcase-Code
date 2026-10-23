using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Create a new seasonal rule tile
/// </summary>
[CreateAssetMenu(fileName = "Season Rule Tile", menuName = "Tiles/Season Rule Tile")]
public class SeasonRuleTile : RuleTile
{
    public RuleTile autumnTile;
    public RuleTile winterTile;
    public RuleTile springTile;
    public RuleTile summerTile;

    /// <summary>
    /// Get the current tile depending on what season it is
    /// </summary>
    private RuleTile CurrentTile
    {
        get
        {
            switch (DaynightCycleScript.i.Season)
            {
                case SEASON.SPRING: return springTile;
                case SEASON.SUMMER: return summerTile;
                case SEASON.AUTUMN: return autumnTile;
                case SEASON.WINTER: return winterTile;
                default:
                    DebugLogs.Warning("SeasonRuleTile: No matching season tile found.");
                    return null;
            }
        }
    }

    /// <summary>
    /// Set current tile data to seasonal tile chosen data
    /// </summary>
    /// <param name="position"></param>
    /// <param name="tilemap"></param>
    /// <param name="tileData"></param>
    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        var current = CurrentTile;
        if (current != null)
        {
            current.GetTileData(position, tilemap, ref tileData);
        }
    }

    /// <summary>
    /// Return true if neighbor tile matches the same rule tiles as this tile
    /// </summary>
    /// <param name="neighbor"></param>
    /// <param name="other"></param>
    /// <returns></returns>
    public override bool RuleMatch(int neighbor, TileBase other)
    {
        var current = CurrentTile;
        if (current == null) return false;

        if (other is SeasonRuleTile otherSeasonTile)
        {
            var otherTile = otherSeasonTile.CurrentTile;
            return current.RuleMatch(neighbor, otherTile);
        }

        return current.RuleMatch(neighbor, other);
    }
}