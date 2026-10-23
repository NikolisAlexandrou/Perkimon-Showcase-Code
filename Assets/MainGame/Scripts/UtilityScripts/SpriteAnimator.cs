using System.Threading;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// This component is used to play sprite animations like walk animation from lists provided
/// and play sprite animations based on direction given
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteAnimator : MonoBehaviour
{
    [Tooltip("How fast the frames change per second of the walking animation of sprite")]
    [SerializeField] float moveAnimationSpeed = 0.25f;

    // Indexes of stationarySprites list
    // 0 for front sprite
    // 1 for left sprite
    // 2 for right sprite
    // 3 for back sprite
    List<Sprite> stationarySprites = new List<Sprite>();    //  Contains the stationary direction sprites in given indexes

    List<Sprite> walkFrontSprites = new List<Sprite>(); //  Contains the front walk sprites frames
    List<Sprite> walkBackSprites = new List<Sprite>();  //  Contains the back walk sprites frames
    List<Sprite> walkLeftSprites = new List<Sprite>();  //  Contains the left walk sprites frames
    List<Sprite> walkRightSprites = new List<Sprite>(); //  Contains the right walk sprites frames

    private GameObject FOVObject;
    private SpriteRenderer spriteRenderer;

    private float animTimer;    //  current timer for frame to change
    private int currentAnimationFrame;  //  current frame of the walk animation

    private void Awake()
    {
        if(spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// Takes the stationary and all direction walk sprites list of an object and
    /// initialises all corresponding sprite lists in the SpriteAnimator component to be used
    /// in its methods
    /// </summary>
    /// <param name="newStationarySprites"></param>
    /// <param name="newWalkFrontSprites"></param>
    /// <param name="newWalkBackSprites"></param>
    /// <param name="newWalkLeftSprites"></param>
    /// <param name="newWalkRightSprites"></param>
    public void InitialiseSprites(List<Sprite> newStationarySprites, List<Sprite> newWalkFrontSprites, List<Sprite> newWalkBackSprites, List<Sprite> newWalkLeftSprites, List<Sprite> newWalkRightSprites)
    {
        stationarySprites = newStationarySprites;
        walkFrontSprites = newWalkFrontSprites;
        walkBackSprites = newWalkBackSprites;
        walkLeftSprites = newWalkLeftSprites;
        walkRightSprites = newWalkRightSprites;
    }

    public void InitialiseFOVObject(GameObject newFOVObject) => FOVObject = newFOVObject;


    /// <summary>
    /// "Rotates" sprite of monster to look at x,y direction while stationary
    /// </summary>
    public void getStationarySpriteFromDirection(Vector2 direction)
    {
        float angle = (Mathf.Atan2(direction.y, direction.x) * 180) / Mathf.PI;
        if (angle <= 45 && angle >= -45)
            GetComponent<SpriteRenderer>().sprite = stationarySprites[2];
        else if (angle > 45 && angle < 135)
            GetComponent<SpriteRenderer>().sprite = stationarySprites[3];
        else if (angle < -45 && angle > -135)
            GetComponent<SpriteRenderer>().sprite = stationarySprites[0];
        else
            GetComponent<SpriteRenderer>().sprite = stationarySprites[1];
    }


    /// <summary>
    /// Play Idle animation of monster front walk sprites
    /// Usually used for playing animations of flying monsters
    /// while not moving
    /// </summary>
    public void PlayIdleAnimation(List<Sprite> walkSpritesList)
    {
        if (animTimer > 0)
        {
            spriteRenderer.sprite = walkSpritesList[currentAnimationFrame];
            animTimer -= Time.deltaTime;
        }
        else
        {
            animTimer = moveAnimationSpeed;
            if (currentAnimationFrame + 1 < walkSpritesList.Count)
                currentAnimationFrame++;
            else
                currentAnimationFrame = 0;
        }
    }

    /// <summary>
    /// Plays walk animation of sprite and "rotates" sprite to look at 
    /// a direction given (x,y) also rotates FOV object if any to look at direction
    /// </summary>
    public void PlayWalkAnimation(Vector2 direction)
    {
        if (animTimer > 0)
        {
            float angle = (Mathf.Atan2(direction.y, direction.x) * 180) / Mathf.PI;
            if (angle <= 45 && angle >= -45)
                spriteRenderer.sprite = walkRightSprites[currentAnimationFrame];
            else if (angle > 45 && angle < 135)
                spriteRenderer.sprite = walkBackSprites[currentAnimationFrame];
            else if (angle < -45 && angle > -135)
                spriteRenderer.sprite = walkFrontSprites[currentAnimationFrame];
            else
                spriteRenderer.sprite = walkLeftSprites[currentAnimationFrame];

            animTimer -= Time.deltaTime;
            if (FOVObject != null)
                FOVObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        }
        else
        {
            animTimer = moveAnimationSpeed;
            if (currentAnimationFrame + 1 < walkFrontSprites.Count)
                currentAnimationFrame++;
            else
                currentAnimationFrame = 0;
        }
    }
}
