using UnityEngine;

/// <summary>
/// When player enters trigger of object this script is attached to, plays transition of entering
/// a new scene and sets player position to teleportLocation in the new Scene, sceneToTeleport
/// </summary>
public class EnterNextZone : MonoBehaviour
{
    [SerializeField] private string _sceneToTeleport;
    [SerializeField] private Vector2 _teleportLocation;

    /// <summary>
    /// Get the scene name that player is going to teleport to
    /// </summary>
    public string SceneToTeleport => _sceneToTeleport;

    /// <summary>
    /// Get the position that the player is going to teleport to in the "to teleport" scene
    /// </summary>
    public Vector2 TeleportLocation => _teleportLocation;

    public void OnTriggerEnter2D(Collider2D collision)
    {
        //  Check if player enters trigger of this object
        if (collision.CompareTag("Player"))
        {
            //  Sets the temporary next zone object to this to retreive its data on other scene
            GameUtils.i.currentNextZone = this;

            //  Sets the game to freeze state
            GameUtils.i.FreezeGame();

            //  Play transition animation, scene changing and position changing are handled
            //  in the animation event of sceneTransition
            UIManager.i.sceneTransition.SetActive(true);

        }
    }
}
