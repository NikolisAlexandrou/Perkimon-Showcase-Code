using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

/// <summary>
/// Checks wether to load or not AlwaysActive scene as additive to current scene to load
/// objects and scripts once between multiple scenes
/// </summary>
public class ZoneManager : MonoBehaviour
{
    public static ZoneManager i;

    bool _alreadyActivated;

    /// <summary>
    /// Get if AwlaysActive scene has already been activated and loaded as additive scene or not
    /// </summary>
    public bool AlreadyActivated => _alreadyActivated;

    private void Awake()
    {
        if (i != null)
            Destroy(i.gameObject);
        else
            ActivateScenes();
        i = this;
    }

    /// <summary>
    /// Loads AlwaysActive scene as additive to current scene
    /// </summary>
    public void ActivateScenes()
    {
        //  Get if AwlaysActive scene has not already been loaded as additive to this scene
        if (_alreadyActivated == false)
        {
            //  Check if current scene is not AlwaysActive scene and if not load AlwaysActive scene as additive
            if (SceneManager.GetActiveScene().name != "AlwaysActive")
                SceneManager.LoadScene("AlwaysActive", LoadSceneMode.Additive);

            _alreadyActivated = true;
        }
    }


    /*
    IEnumerator RefreshTilesSmooth(Tilemap tilemap)
    {
        BoundsInt bounds = tilemap.cellBounds;
        int count = 0;

        foreach (var pos in bounds.allPositionsWithin)
        {
            TileBase tile = tilemap.GetTile(pos);
            if (tile is SeasonRuleTile)
            {
                // Refresh this tile and its 8 neighbors
                for (int y = -1; y <= 1; y++)
                {
                    for (int x = -1; x <= 1; x++)
                    {
                        Vector3Int neighborPos = new Vector3Int(pos.x + x, pos.y + y, pos.z);
                        tilemap.RefreshTile(neighborPos);
                    }
                }

                count++;
            }

            if (count >= 50)
            {
                count = 0;
                yield return null;
            }
        }
    }*/
}
