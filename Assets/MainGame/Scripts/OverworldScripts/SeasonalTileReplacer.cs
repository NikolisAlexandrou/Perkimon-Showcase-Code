using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Gets all tilemaps and change their tile data and sprites if SeasonTiles were found in to current season data
/// When creating a tile replacer you need to subscribe to the OnSeasonChange event from DaynightCycleScript
/// </summary>
public class SeasonalTileReplacer : MonoBehaviour
{
    
    [Header("Used for any individual tiles, place component to tilemaps")]

    [Tooltip("Current tilemap to search tiles to replace")]
    [SerializeField] Tilemap tilemap;

    [Tooltip("Speed of changing tiles in the tilemap (the slower the better performance)")]
    [SerializeField] int changeTileSpeed = 50;

    [Header("Individual Tiles to replace")]

    [Tooltip("Contains a list of all tiles to search in tilemap and change sprites")]
    [SerializeField] List<TileBase> replacableTiles = new List<TileBase>();

    [Header("Season Tiles")]

    [Tooltip("Autumn tile to replace all replacableTiles")]
    [SerializeField] TileBase autumnTiles;

    [Tooltip("Winter tile to replace all replacableTiles")]
    [SerializeField] TileBase winterTiles;

    [Tooltip("Spring tile to replace all replacableTiles")]
    [SerializeField] TileBase springTiles;

    [Tooltip("Summer tile to replace all replacableTiles")]
    [SerializeField] TileBase summerTiles;

    [Header("LIST INDEXES: 0:AUTUMN / 1:WINTER / 2:SPRING / 3:SUMMER")]

    [Header("t = top sprite")]
    [Header("b = bottom sprite")]
    [Header("r = right sprite")]
    [Header("l = left sprite")]
    //t = top sprite
    //b = bottom sprite
    //r = right sprite
    //l = left sprite

    //  Mainly used for mountain edges tiles

    [Tooltip("top tiles per season to replace")]
    [SerializeField] List<TileBase> tTile = new List<TileBase>();

    [Tooltip("bottom tiles per season to replace")]
    [SerializeField] List<TileBase> bTile = new List<TileBase>();

    [Tooltip("left tiles per season to replace")]
    [SerializeField] List<TileBase> lTile = new List<TileBase>();

    [Tooltip("right tiles per season to replace")]
    [SerializeField] List<TileBase> rTile = new List<TileBase>();

    [Tooltip("top left tiles per season to replace")]
    [SerializeField] List<TileBase> tlTile = new List<TileBase>();

    [Tooltip("top right tiles per season to replace")]
    [SerializeField] List<TileBase> trTile = new List<TileBase>();

    [Tooltip("bottom left tiles per season to replace")]
    [SerializeField] List<TileBase> blTile = new List<TileBase>();

    [Tooltip("bottom right tiles per season to replace")]
    [SerializeField] List<TileBase> brTile = new List<TileBase>();

    [Tooltip("Inner top left tiles per season to replace")]
    [SerializeField] List<TileBase> innerTLTile = new List<TileBase>();

    [Tooltip("Inner top right tiles per season to replace")]
    [SerializeField] List<TileBase> innerTRTile = new List<TileBase>();

    [Tooltip("Inner bottom left tiles per season to replace")]
    [SerializeField] List<TileBase> innerBLTile = new List<TileBase>();

    [Tooltip("Inner bottom right tiles per season to replace")]
    [SerializeField] List<TileBase> innerBRTile = new List<TileBase>();

    private void OnEnable()
    {
        StartCoroutine(loadEvent());
    }

    private void OnDisable()
    {
        if(DaynightCycleScript.i != null)
            DaynightCycleScript.i.StartCoroutine(unloadEvent());
    }

    /// <summary>
    /// Loads refresh tilemap event
    /// </summary>
    /// <returns></returns>
    IEnumerator loadEvent()
    {
        yield return new WaitForSeconds(1);
        DaynightCycleScript.i.OnSeasonChanged.AddListener(RefreshTilemap);
    }

    /// <summary>
    /// Unloads refresh tilemap event
    /// </summary>
    /// <returns></returns>
    IEnumerator unloadEvent()
    {
        yield return new WaitForSeconds(1);
        DaynightCycleScript.i.OnSeasonChanged?.RemoveListener(RefreshTilemap);
    }

    /// <summary>
    /// Refreshes the entire tilemap to display correct season tiles
    /// </summary>
    public void RefreshTilemap() => StartCoroutine(RefreshTiles(tilemap));

    /// <summary>
    /// Get the tiles in a season to replace all current tiles that have to be changed in tilemap
    /// </summary>
    /// <param name="season"></param>
    /// <returns></returns>
    public TileBase GetSeasonTile(SEASON season)
    {
        switch (season)
        {
            case SEASON.AUTUMN: return autumnTiles;
            case SEASON.WINTER: return winterTiles;
            case SEASON.SPRING: return springTiles;
            case SEASON.SUMMER: return summerTiles;
            default: return autumnTiles;
        }
    }

    /// <summary>
    /// Get the number of the season
    /// </summary>
    /// <param name="season"></param>
    /// <returns></returns>
    public int GetSeasonByNumber(SEASON season)
    {
        switch (season)
        {
            case SEASON.AUTUMN: return 0;
            case SEASON.WINTER: return 1;
            case SEASON.SPRING: return 2;
            case SEASON.SUMMER: return 3;
            default: return 0;
        }
    }

    /// <summary>
    /// Return true if tile is in given tile list
    /// </summary>
    /// <param name="tile"></param>
    /// <param name="inList"></param>
    /// <returns></returns>
    public bool CheckForTile(TileBase tile, List<TileBase> inList)
    {
        //  Checks if tile actually exists
        if (tile == null)
            return false;

        //  Checks if list contains tiles in it
        if (inList.Count <= 0)
            return false;

        //  Get all tiles in the list
        foreach (TileBase item in inList)
        {
            //  Check if current searching tile has same name as tile
            if (tile.name == item.name)
                return true;
        }
        return false;

    }

    /// <summary>
    /// Check if given tile is in tilemap, if it is in, replace that tile with correspinding seasonal tile
    /// </summary>
    /// <param name="tilemap"></param>
    /// <param name="tile"></param>
    /// <param name="pos"></param>
    /// <param name="inList"></param>
    /// <param name="count"></param>
    public void ChangeIndividualTile(Tilemap tilemap, TileBase tile,Vector3Int pos, List<TileBase> inList, ref int count)
    {
        //  Check if tile is in tiles list
        if (CheckForTile(tile, inList) == true)
        {
            //  Change that tile to corresponding season tile
            tilemap.SetTile(pos, inList[GetSeasonByNumber(DaynightCycleScript.i.Season)]);
            count++;
        }
    }

    /// <summary>
    /// Searches all tiles in the tilemap for seasonal tiles and replaces them with corresponding season tile
    /// </summary>
    /// <param name="tilemap"></param>
    /// <returns></returns>
    IEnumerator RefreshTiles(Tilemap tilemap)
    {
        BoundsInt bounds = tilemap.cellBounds;
        int count = 0;

        //  Get all individual tile position in tilemap
        foreach (var pos in bounds.allPositionsWithin)
        {

            //  Get current tile
            TileBase tile = tilemap.GetTile(pos);
            //  check if tiles to replace are the same type of tiles as the season change tiles
            if (tTile.Count > 0)
            {
                ChangeIndividualTile(tilemap, tile, pos, tTile, ref count);
                ChangeIndividualTile(tilemap, tile, pos, bTile, ref count);
                ChangeIndividualTile(tilemap, tile, pos, lTile, ref count);
                ChangeIndividualTile(tilemap, tile, pos, rTile, ref count);

                ChangeIndividualTile(tilemap, tile, pos, tlTile, ref count);
                ChangeIndividualTile(tilemap, tile, pos, trTile, ref count);
                ChangeIndividualTile(tilemap, tile, pos, blTile, ref count);
                ChangeIndividualTile(tilemap, tile, pos, brTile, ref count);

                ChangeIndividualTile(tilemap, tile, pos, innerTLTile, ref count);
                ChangeIndividualTile(tilemap, tile, pos, innerTRTile, ref count);
                ChangeIndividualTile(tilemap, tile, pos, innerBLTile, ref count);
                ChangeIndividualTile(tilemap, tile, pos, innerBRTile, ref count);
            }

            //  Check if tile is a seasonal tile and is in searching list
            if (autumnTiles != null)
            {
                if (tile == autumnTiles
               || tile == winterTiles
               || tile == springTiles
               || tile == summerTiles
               || CheckForTile(tile, replacableTiles) == true)
                {
                    /* // Refresh this tile and its 8 neighbors
                     for (int y = -1; y <= 1; y++)
                     {
                         for (int x = -1; x <= 1; x++)
                         {
                             Vector3Int neighborPos = new Vector3Int(pos.x + x, pos.y + y, pos.z);
                             tilemap.SetTile(neighborPos, GetSeasonTile(DaynightCycleScript.i.GetSeason()));
                             tilemap.RefreshTile(neighborPos);
                         }
                     }*/

                    //  Replace the tile with current season tile
                    tilemap.SetTile(pos, GetSeasonTile(DaynightCycleScript.i.Season));
                    count++;
                }
            }

            if (count >= changeTileSpeed)
            {
                count = 0;
                yield return null;
            }
        }
    }

}