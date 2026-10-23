using UnityEngine;
using UnityEngine.Tilemaps;
/// <summary>
/// Creates a custom tile that changes sprites when the season changes (compatible with Animate tiles)
/// </summary>
[CreateAssetMenu(fileName = "Animated Season Tile", menuName = "Tiles/Animated Season Tile")]
public class AnimatedSeasonTiles : Tile
{
    [Tooltip("Sprite of tile during autumn season")]
    public AnimatedTile autumnTile;

    [Tooltip("Sprite of tile during winter season")]
    public AnimatedTile winterTile;

    [Tooltip("Sprite of tile during spring season")]
    public AnimatedTile springTile;

    [Tooltip("Sprite of tile during summer season")]
    public AnimatedTile summerTile;

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        TileBase currentTile = autumnTile;
        switch (DaynightCycleScript.i.Season)
        {
            case SEASON.AUTUMN:
                currentTile = autumnTile;
                break;
            case SEASON.WINTER:
                currentTile = winterTile;
                break;
            case SEASON.SPRING:
                currentTile = springTile;
                break;
            case SEASON.SUMMER:
                currentTile = summerTile;
                break;
            default:
                break;
        }
        currentTile.GetTileData(position, tilemap, ref tileData);
    }

    public override bool GetTileAnimationData(Vector3Int position, ITilemap tilemap, ref TileAnimationData tileAnimationData)
    {
        TileBase currentTile = autumnTile;
        switch (DaynightCycleScript.i.Season)
        {
            case SEASON.AUTUMN:
                currentTile = autumnTile;
                break;
            case SEASON.WINTER:
                currentTile = winterTile;
                break;
            case SEASON.SPRING:
                currentTile = springTile;
                break;
            case SEASON.SUMMER:
                currentTile = summerTile;
                break;
            default:
                break;
        }
        return currentTile.GetTileAnimationData(position, tilemap, ref tileAnimationData);
    }

    public override void RefreshTile(Vector3Int position, ITilemap tilemap)
    {
        tilemap.RefreshTile(position);
    }
}