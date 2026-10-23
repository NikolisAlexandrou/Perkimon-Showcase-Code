using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MonsterFollowScript : MonoBehaviour
{
    [Header("Monster Sprites")]

    [Tooltip("Main monster sprite renderer.")]
    [SerializeField] private SpriteRenderer _monsterSprite;

    [Tooltip("Shadow sprite renderer under the monster.")]
    [SerializeField] private SpriteRenderer _shadowSprite;

    [Header("Follow Settings")]

    [Tooltip("If true, monster will follow the player.")]
    [SerializeField] private bool _isFollowing;

    [Tooltip("Distance at which the monster stops moving toward the player. (Default: 1.3)")]
    [SerializeField] private float _stopDistance = 1.3f;

    [Tooltip("Time in seconds between walking animation frames. (Default: 0.2)")]
    [SerializeField] private float moveAnimationSpeed = 0.2f;

    [Tooltip("Maximum distance at which following is allowed. (Default: 15)")]
    [SerializeField] private float farAwayValue = 15;

    [Tooltip("Distance at which monster swaps positions with the player. (Default: 0.7)")]
    [SerializeField] private float SwapPositionsDistance = 0.7f;

    [Tooltip("Offset applied during swap positions. (Default: 0.3)")]
    [SerializeField] private float SwapPositionDirectionValue = 0.3f;

    /// <summary>Current walking animation frame index.</summary>
    private int _currentAnimationFrame;

    /// <summary>True while walking animation should play.</summary>
    private bool _movingAnimation;

    /// <summary>Timer controlling walk animation frame switching.</summary>
    private float _animationTimer;

    /// <summary>Initial sprite sorting layer.</summary>
    private string _initialSortingLayer;

    /// <summary>Initial sprite sorting order.</summary>
    private int _initialSortNumber;

    /// <summary>Original shadow sprite.</summary>
    private Sprite _originalShadowSprite;

    [Header("Get Back In Settings")]

    [Tooltip("Duration of the recall animation. (Default: 0.5)")]
    [SerializeField] private float _inDuration = 0.5f;

    [Tooltip("Color the monster fades to when recalling.")]
    [SerializeField] private Color _inColor;

    [Tooltip("Scale monster shrinks to when recalling.")]
    [SerializeField] private Vector2 _inEndScale;

    /// <summary>Original color of the monster sprite.</summary>
    private Color _initialColor;

    /// <summary>Original scale of the monster transform.</summary>
    private Vector2 _initialScale;

    /// <summary>True while monster is recalling or returning from water.</summary>
    private bool _goingIn;

    /// <summary>Cached Rigidbody2D reference.</summary>
    Rigidbody2D _rb;

    /// <summary>Cached player Transform reference.</summary>
    Transform _playerTransform;

    /// <summary>Cached PlayerController reference.</summary>
    PlayerController _playerController;

    /// <summary>Cached sprite renderer of the player.</summary>
    SpriteRenderer _playerSprite;

    /// <summary>
    /// Duration of recall animation
    /// </summary>
    public float InDuration => _inDuration;

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _playerTransform = GameUtils.i.player.transform;
        _playerController = GameUtils.i.player.GetComponent<PlayerController>();
        _playerSprite = GameUtils.i.player.GetComponent<SpriteRenderer>();

        _initialSortingLayer = _monsterSprite.sortingLayerName;
        _initialSortNumber = _monsterSprite.sortingOrder;
        _initialColor = _monsterSprite.color;
        _initialScale = transform.localScale;
        _originalShadowSprite = _shadowSprite.sprite;

        if (PlayerStats.i.PlayerParty.Count > 0) //  Sets the sprite of the monster to first party monster
            _monsterSprite.sprite = PlayerStats.i.PlayerParty[0].Base.overworldSprites[0];
    }

    /// <summary>
    /// Swaps this monster’s position with another GameObject (player),
    /// applying a small offset so they don't overlap.
    /// </summary>
    private void SwapPositions(GameObject newOb)
    {
        if (PlayerStats.i.PlayerParty.Count == 0) return;    //  Check if party is not empty
        if (newOb == null) return;

        Vector2 oldPos = newOb.transform.position;

        Vector2 dir = (transform.position - newOb.transform.position).normalized;   //  Get direction of this object and new object

        newOb.transform.position = (Vector2)transform.position + (dir * SwapPositionDirectionValue);    //  Set position new object to this object's original position
        transform.position = oldPos - (dir * SwapPositionDirectionValue);   //  Set position of this object to new object's original position
    }

    /// <summary>
    /// Moves the monster toward the player's position and plays walking animation.
    /// </summary>
    void FollowPlayer()
    {
        _movingAnimation = true;

        if (_rb == null || GameUtils.i.player.transform == null || _playerController == null)
            return;

        //  Move the monster towards player's position
        Vector3 target = Vector3.MoveTowards(
            transform.position,
            GameUtils.i.player.transform.position,
            _playerController.WalkSpeed * Time.deltaTime
        );

        PlayWalkAnimation();    //  Get walking animation of monster

        _rb.MovePosition(target);
    }

    /// <summary>
    /// Plays the recall (destroy) animation and re-enables following afterwards.
    /// </summary>
    public IEnumerator PlayDestroyAnimation(GameObject ob)
    {
        _monsterSprite.DOColor(_inColor, InDuration); //  Changes color over time
        _monsterSprite.transform.DOScale(_inEndScale, InDuration);    //  Changes scale over time

        yield return new WaitForSeconds(InDuration);

        _isFollowing = true;  //  re enable following after InDuration
    }

    /// <summary>
    /// Restores monster appearance and spawns the capsule particle upon exiting water.
    /// </summary>
    public void PlayThrowAnimation(GameObject ob)
    {
        _monsterSprite.DOColor(_initialColor, InDuration);    //  Changes color over time
        _monsterSprite.transform.DOScale(_initialScale, InDuration);  //  Changes scale over time

        //  Spawn catching object animation when it spawns a monster
        Instantiate(
            PlayerStats.i.PlayerParty[0].CapsuleCaught.Capsule_Particle,
            ob.transform.position,
            Quaternion.identity
        );

    }

    /// <summary>
    /// Enables or disables monster/shadow sprites based on party presence.
    /// </summary>
    void ChangeSpritesWhenNoParty()
    {
        if (PlayerStats.i.PlayerParty.Count > 0) //  If party is not empty, set shadow sprite to object
        {
            _shadowSprite.sprite = _originalShadowSprite;
        }
        else
        {
            _monsterSprite.sprite = null;    //  Hide sprite of monster
            _shadowSprite.sprite = null;     //  Hide sprite of shadow
        }
    }

    private void FixedUpdate()
    {
        ChangeSpritesWhenNoParty();
        if (_playerTransform == null) return;

        float distance = Vector2.Distance(transform.position, _playerTransform.position);

        if (distance >= farAwayValue)   //  When distance between player and monster is greater than farAwayValue stop following
            _isFollowing = false;
        else
            _isFollowing = true;

        if (!_isFollowing)
            return;

        if (distance > _stopDistance && distance < farAwayValue) //  Follows player when distance between them is greater than stopDistance and less than farAwayValue
            FollowPlayer();
        else
            _movingAnimation = false;

        if (distance <= SwapPositionsDistance && !_goingIn)  //  When distance between monster and player is less than swap positions distance, swap their positions
            SwapPositions(GameUtils.i.player);
    }

    //  When monster enters water trigger, Play animation of going inside catching object
    //  and hide its sprites
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Water")) return;

        if (!_goingIn)
        {
            _goingIn = true;
            StartCoroutine(PlayDestroyAnimation(gameObject));
        }
    }

    //  When monster exits water trigger, play animation of coming outside from catching object
    //  and re-show monster sprite
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Water")) return;

        if (_goingIn)
        {
            PlayThrowAnimation(gameObject);
            _goingIn = false;
        }
    }

    /// <summary>
    /// Plays 4-direction walk animation based on input axis while moving.
    /// </summary>
    public void PlayWalkAnimation()
    {
        if (PlayerStats.i.PlayerParty.Count == 0)
            return;

        if (!_movingAnimation)
            return;

        if (_animationTimer > 0)
        {
            float x = Input.GetAxis("Horizontal");
            float y = Input.GetAxis("Vertical");

            var party = PlayerStats.i.PlayerParty[0];
            var baseData = party.Base;

            if (y > 0)
            {
                _monsterSprite.sprite = baseData.overworldWalkSpritesBack[_currentAnimationFrame];
                _monsterSprite.sortingLayerName = _playerSprite.sortingLayerName;
                _monsterSprite.sortingOrder = _playerSprite.sortingOrder + 1;
            }
            else if (y < 0)
            {
                _monsterSprite.sprite = baseData.overworldWalkSpritesFront[_currentAnimationFrame];
                _monsterSprite.sortingLayerName = _initialSortingLayer;
                _monsterSprite.sortingOrder = _initialSortNumber;
            }

            if (x > 0)
            {
                _monsterSprite.sprite = baseData.overworldWalkSpritesRight[_currentAnimationFrame];
                _monsterSprite.sortingLayerName = _playerSprite.sortingLayerName;
                _monsterSprite.sortingOrder = _playerSprite.sortingOrder + 1;
            }
            else if (x < 0)
            {
                _monsterSprite.sprite = baseData.overworldWalkSpritesLeft[_currentAnimationFrame];
                _monsterSprite.sortingLayerName = _playerSprite.sortingLayerName;
                _monsterSprite.sortingOrder = _playerSprite.sortingOrder + 1;
            }

            _animationTimer -= Time.deltaTime;
        }
        else
        {
            _animationTimer = moveAnimationSpeed;

            if (_currentAnimationFrame + 1 < PlayerStats.i.PlayerParty[0].Base.overworldWalkSpritesFront.Count)
                _currentAnimationFrame++;
            else
                _currentAnimationFrame = 0;
        }
    }
}
