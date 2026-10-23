using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MonsterFollowScriptTest : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private SpriteRenderer _monsterSprite;
    [SerializeField] private SpriteRenderer _shadowSprite;
    [SerializeField] private SpriteRenderer _capsuleSprite;

    [Header("Following")]
    [SerializeField] private bool _isFollowing = true;

    [Tooltip("Distance behind the player, measured along the trail.")]
    [SerializeField, Min(0.1f)]
    private float _followDistance = 1.3f;

    [SerializeField, Min(1f)]
    private float _followSpeedMultiplier = 1f;

    [SerializeField, Min(0.01f)]
    private float _animationFrameDuration = 0.2f;

    [Header("Follow Smoothing")]
    [SerializeField, Min(0.01f)]
    private float _followDistanceBuffer = 0.1f;

    [Header("Personal Space")]
    [Tooltip("Reposition when the moving player gets closer than this.")]
    [SerializeField, Min(0.1f)]
    private float _minimumPlayerDistance = 0.7f;

    [SerializeField, Min(0.1f)]
    private float _repositionCooldown = 0.3f;

    [Header("Capsule")]
    [SerializeField, Min(0.1f)]
    private float _releaseDistance = 1.3f;

    [SerializeField, Min(0.01f)]
    private float _movementBeforeRelease = 0.3f;

    [Tooltip("Player movement above this distance in one physics step is treated as a teleport.")]
    [SerializeField, Min(0.1f)]
    private float _teleportThreshold = 3f;

    [Header("Recall / Release Animation")]
    [SerializeField, Min(0.01f)]
    private float _inDuration = 0.2f;

    [SerializeField, Min(0.01f)]
    private float _outDuration = 0.2f;

    [SerializeField, Range(0f, 1f)]
    private float _recalledScaleMultiplier = 0f;

    [Header("Collision Checks")]
    [SerializeField] private LayerMask _obstacleLayers;
    [SerializeField] private LayerMask _waterLayers;

    [Tooltip("World-space dimensions of the monster's footprint.")]
    [SerializeField] private Vector2 _footprintSize =
        new Vector2(0.6f, 0.5f);

    [SerializeField] private Vector2 _footprintOffset;

    private Rigidbody2D _rb;
    private Transform _playerTransform;
    private PlayerController _playerController;
    private SpriteRenderer _playerSprite;

    private readonly Queue<Vector2> _trail = new Queue<Vector2>();

    private readonly Collider2D[] _overlaps = new Collider2D[1];
    private readonly RaycastHit2D[] _hits = new RaycastHit2D[1];

    private float _trailLength;
    private Vector2 _trailEnd;

    private Vector2 _lastPlayerPosition;
    private Vector2 _playerDirection = Vector2.down;
    private Vector2 _monsterDirection = Vector2.down;
    private Vector2 _recallPosition;

    private bool _insideCapsule;
    private bool _recalling;
    private bool _moving;
    private bool _followingTrail;
    private bool _initialised;
    private bool _hadParty;

    private float _nextRepositionTime;
    private float _animationTimer;
    private int _animationFrame;
    private Sprite _lastIdleSprite;

    private Vector3 _normalMonsterScale;
    private Color _normalMonsterColor;
    private Sequence _visualSequence;

    public float InDuration => _inDuration;

    private bool HasMonster =>
        PlayerStats.i.PlayerParty.Count > 0;

    private int BlockedLayers =>
        _obstacleLayers.value | _waterLayers.value;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();

        _normalMonsterScale = _monsterSprite.transform.localScale;
        _normalMonsterColor = _monsterSprite.color;

        if (_normalMonsterScale.x == 0f ||
            _normalMonsterScale.y == 0f)
        {
            _normalMonsterScale = Vector3.one;
        }

        if (_normalMonsterColor.a <= 0f)
        {
            _normalMonsterColor.a = 1f;
        }

        RestoreMonsterAppearance();
    }

    private void Start()
    {
        Initialise();
    }

    private void Initialise()
    {
        if (_initialised)
            return;

        _playerTransform = GameUtils.i.player.transform;

        _playerController =
            GameUtils.i.player.GetComponent<PlayerController>();

        _playerSprite =
            GameUtils.i.player.GetComponent<SpriteRenderer>();

        _lastPlayerPosition = _playerTransform.position;
        _initialised = true;
        _hadParty = HasMonster;

        ResetTrail();

        if (HasMonster)
        {
            bool unsafeStart =
                !IsSafe(_rb.position) ||
                !IsPathClear(_rb.position, _playerTransform.position);

            if (unsafeStart)
            {
                RecallIntoCapsule();
                _rb.position = _playerTransform.position;
            }
        }

        RefreshSprites();
    }

    private void FixedUpdate()
    {
        if (!_initialised)
            return;

        _moving = false;

        Vector2 playerPosition = _playerTransform.position;
        Vector2 playerMovement = playerPosition - _lastPlayerPosition;

        _lastPlayerPosition = playerPosition;

        bool teleported =
            playerMovement.magnitude > _teleportThreshold;

        if (!teleported && playerMovement.sqrMagnitude > 0.000001f)
        {
            _playerDirection = playerMovement.normalized;
        }

        if (!HasMonster)
        {
            if (_hadParty)
            {
                StopVisualAnimation();

                _insideCapsule = false;
                _recalling = false;

                RestoreMonsterAppearance();
            }

            _hadParty = false;

            ClearTrail();
            RefreshSprites();
            return;
        }

        if (!_hadParty)
        {
            _hadParty = true;
            RecallIntoCapsule();
            _rb.position = playerPosition;
        }

        RefreshSprites();

        if (!_isFollowing)
        {
            ResetTrail();
            return;
        }

        if (teleported)
        {
            RecallIntoCapsule();

            _rb.position = playerPosition;
            _recallPosition = playerPosition;
        }

        if (!_insideCapsule &&
            (Overlaps(playerPosition, _waterLayers.value) ||
             !IsSafe(_rb.position)))
        {
            RecallIntoCapsule();
        }

        if (_insideCapsule)
        {
            FollowAsCapsule();

            bool playerMoved =
                !teleported &&
                playerMovement.sqrMagnitude > 0.000001f;

            if (!_recalling && playerMoved && CanRelease())
            {
                ReleaseMonster();
            }

            return;
        }

        // Move the monster out of the player's way when necessary.
        if (HandlePlayerOverlap(playerMovement))
            return;

        RecordPlayerPosition(playerPosition);
        FollowTrail();
    }

    private void Update()
    {
        if (!_initialised || !HasMonster)
            return;

        if (!_insideCapsule)
        {
            UpdateWalkAnimation();
        }

        UpdateSorting();
    }

    private bool HandlePlayerOverlap(Vector2 playerMovement)
    {
        if (_insideCapsule)
            return false;

        if (playerMovement.sqrMagnitude <= 0.000001f)
            return false;

        if (Time.time < _nextRepositionTime)
            return false;

        Vector2 playerPosition = _playerTransform.position;

        float distance = Vector2.Distance(
            _rb.position,
            playerPosition
        );

        if (distance >= _minimumPlayerDistance)
            return false;

        _nextRepositionTime = Time.time + _repositionCooldown;

        Vector2 targetPosition =
            playerPosition - _playerDirection * _releaseDistance;

        if (!IsSafe(targetPosition) ||
            !IsPathClear(_rb.position, targetPosition))
        {
            RecallIntoCapsule();
            return true;
        }

        // Only reposition the monster, never the player.
        _rb.position = targetPosition;

        _monsterDirection = _playerDirection;
        _moving = false;

        ResetTrail();

        return true;
    }

    private void RecordPlayerPosition(Vector2 position)
    {
        float distance = Vector2.Distance(_trailEnd, position);

        if (distance < 0.0001f)
            return;

        _trail.Enqueue(position);
        _trailLength += distance;
        _trailEnd = position;

        if (_trail.Count > 2048)
        {
            RecallIntoCapsule();
        }
    }

    private void ClearTrail()
    {
        _trail.Clear();
        _trailLength = 0f;
        _followingTrail = false;
    }

    private void ResetTrail()
    {
        ClearTrail();

        _trailEnd = _rb.position;

        RecordPlayerPosition(_playerTransform.position);
    }

    private void FollowTrail()
    {
        _moving = false;

        if (_insideCapsule)
        {
            _followingTrail = false;
            return;
        }

        float startDistance =
            _followDistance + _followDistanceBuffer;

        float stopDistance = Mathf.Max(
            0.05f,
            _followDistance - _followDistanceBuffer
        );

        if (!_followingTrail)
        {
            if (_trailLength <= startDistance)
                return;

            _followingTrail = true;
        }

        if (_trailLength <= stopDistance || _trail.Count == 0)
        {
            _followingTrail = false;
            return;
        }

        Vector2 startPosition = _rb.position;
        Vector2 position = startPosition;

        float movementBudget = Mathf.Min(
            _playerController.WalkSpeed *
            _followSpeedMultiplier *
            Time.fixedDeltaTime,
            _trailLength - stopDistance
        );

        while (movementBudget > 0.00001f && _trail.Count > 0)
        {
            Vector2 waypoint = _trail.Peek();

            float distance = Vector2.Distance(
                position,
                waypoint
            );

            if (distance <= 0.00001f)
            {
                _trailLength = Mathf.Max(
                    0f,
                    _trailLength - distance
                );

                _trail.Dequeue();
                continue;
            }

            float step = Mathf.Min(movementBudget, distance);

            Vector2 nextPosition = Vector2.MoveTowards(
                position,
                waypoint,
                step
            );

            // Check the next section of the trail.
            if (!IsSafe(nextPosition) ||
                !IsPathClear(position, nextPosition))
            {
                RecallIntoCapsule();
                return;
            }

            // Avoid cutting through corners when combining waypoints.
            if (!IsPathClear(startPosition, nextPosition))
            {
                break;
            }

            _monsterDirection =
                (nextPosition - position).normalized;

            position = nextPosition;
            movementBudget -= step;
            _trailLength = Mathf.Max(0f, _trailLength - step);

            if (step >= distance)
            {
                _trail.Dequeue();
            }
        }

        if ((position - startPosition).sqrMagnitude > 0.00000001f)
        {
            _moving = true;
            _rb.MovePosition(position);
        }

        if (_trailLength <= stopDistance + 0.001f)
        {
            _followingTrail = false;
        }
    }

    private void FollowAsCapsule()
    {
        Vector2 current = _rb.position;
        Vector2 target = _playerTransform.position;

        float distance = Vector2.Distance(current, target);

        if (distance <= _followDistance)
            return;

        float step = Mathf.Min(
            _playerController.WalkSpeed *
            _followSpeedMultiplier *
            Time.fixedDeltaTime,
            distance - _followDistance
        );

        _rb.MovePosition(
            Vector2.MoveTowards(current, target, step)
        );
    }

    public void RecallIntoCapsule()
    {
        if (!_initialised)
            Initialise();

        if (!HasMonster || _insideCapsule)
            return;

        StopVisualAnimation();

        _insideCapsule = true;
        _recalling = true;
        _moving = false;
        _recallPosition = _playerTransform.position;

        ClearTrail();
        RefreshSprites();

        Vector3 endScale =
            _normalMonsterScale * _recalledScaleMultiplier;

        _visualSequence = DOTween.Sequence();

        _visualSequence.Join(
            _monsterSprite.transform
                .DOScale(endScale, _inDuration)
                .SetEase(Ease.InQuad)
        );

        _visualSequence.Join(
            _monsterSprite.DOFade(0f, _inDuration)
        );

        _visualSequence.OnComplete(() =>
        {
            _visualSequence = null;
            _recalling = false;

            RefreshSprites();
            RestoreMonsterAppearance();
        });
    }

    private Vector2 GetReleasePosition()
    {
        return (Vector2)_playerTransform.position
               - _playerDirection * _releaseDistance;
    }

    private bool CanRelease()
    {
        if (_recalling)
            return false;

        Vector2 playerPosition = _playerTransform.position;

        if (Vector2.Distance(playerPosition, _recallPosition) <
            _movementBeforeRelease)
        {
            return false;
        }

        if (Overlaps(playerPosition, _waterLayers.value))
            return false;

        Vector2 releasePosition = GetReleasePosition();

        return IsSafe(releasePosition) &&
               IsPathClear(playerPosition, releasePosition);
    }

    private void ReleaseMonster()
    {
        StopVisualAnimation();

        _rb.position = GetReleasePosition();

        _insideCapsule = false;
        _recalling = false;
        _monsterDirection = _playerDirection;
        _animationFrame = 0;
        _animationTimer = 0f;

        _nextRepositionTime = Time.time + _repositionCooldown;

        ResetTrail();

        _monsterSprite.transform.localScale =
            _normalMonsterScale * _recalledScaleMultiplier;

        Color transparentColor = _normalMonsterColor;
        transparentColor.a = 0f;

        _monsterSprite.color = transparentColor;

        RefreshSprites();

        _visualSequence = DOTween.Sequence();

        _visualSequence.Join(
            _monsterSprite.transform
                .DOScale(_normalMonsterScale, _outDuration)
                .SetEase(Ease.OutQuad)
        );

        _visualSequence.Join(
            _monsterSprite.DOColor(
                _normalMonsterColor,
                _outDuration
            )
        );

        _visualSequence.OnComplete(() =>
        {
            _visualSequence = null;
            RestoreMonsterAppearance();
        });

        var particle =
            PlayerStats.i.PlayerParty[0].CapsuleCaught.Capsule_Particle;

        if (particle != null)
        {
            Instantiate(
                particle,
                transform.position,
                Quaternion.identity
            );
        }
    }

    private void StopVisualAnimation()
    {
        if (_visualSequence == null)
            return;

        _visualSequence.Kill(false);
        _visualSequence = null;
    }

    private void RestoreMonsterAppearance()
    {
        _monsterSprite.transform.localScale =
            _normalMonsterScale;

        _monsterSprite.color = _normalMonsterColor;
    }

    /// <summary>
    /// Call after placing the player inside or outside a building.
    /// </summary>
    public void OnEnteredBuilding()
    {
        if (!_initialised)
            Initialise();

        RecallIntoCapsule();

        _rb.position = _playerTransform.position;
        _lastPlayerPosition = _playerTransform.position;
        _recallPosition = _playerTransform.position;

        ClearTrail();
    }

    private ContactFilter2D MakeFilter(int layers)
    {
        ContactFilter2D filter = new ContactFilter2D();

        filter.SetLayerMask(layers);
        filter.useTriggers = true;

        return filter;
    }

    private bool Overlaps(Vector2 position, int layers)
    {
        return Physics2D.OverlapBox(
            position + _footprintOffset,
            _footprintSize,
            0f,
            MakeFilter(layers),
            _overlaps
        ) > 0;
    }

    private bool IsSafe(Vector2 position)
    {
        return !Overlaps(position, BlockedLayers);
    }

    private bool IsPathClear(Vector2 from, Vector2 to)
    {
        if (!IsSafe(from) || !IsSafe(to))
            return false;

        Vector2 movement = to - from;

        if (movement.sqrMagnitude < 0.000001f)
            return true;

        return Physics2D.BoxCast(
            from + _footprintOffset,
            _footprintSize,
            0f,
            movement.normalized,
            MakeFilter(BlockedLayers),
            _hits,
            movement.magnitude
        ) == 0;
    }

    private void RefreshSprites()
    {
        bool hasMonster = HasMonster;

        _monsterSprite.enabled =
            hasMonster && (!_insideCapsule || _recalling);

        _shadowSprite.enabled =
            hasMonster && !_insideCapsule;

        _capsuleSprite.enabled =
            hasMonster && _insideCapsule;

        if (!hasMonster)
        {
            _lastIdleSprite = null;
            return;
        }

        var monster = PlayerStats.i.PlayerParty[0];

        Sprite idleSprite =
            monster.Base.overworldSprites[0];

        if (_lastIdleSprite != idleSprite)
        {
            _lastIdleSprite = idleSprite;
            _monsterSprite.sprite = idleSprite;
            _animationFrame = 0;
        }

        _capsuleSprite.sprite =
            monster.CapsuleCaught.Capsule_Sprite;
    }

    public void PlayWalkAnimation()
    {
        if (!_initialised || !HasMonster || _insideCapsule)
            return;

        UpdateWalkAnimation();
    }

    private void UpdateWalkAnimation()
    {
        if (_moving)
        {
            _animationTimer -= Time.deltaTime;

            if (_animationTimer <= 0f)
            {
                _animationTimer = _animationFrameDuration;
                _animationFrame++;
            }
        }
        else
        {
            _animationFrame = 0;
            _animationTimer = 0f;
        }

        var data = PlayerStats.i.PlayerParty[0].Base;

        var frames =
            Mathf.Abs(_monsterDirection.x) >
            Mathf.Abs(_monsterDirection.y)
                ? (_monsterDirection.x > 0f
                    ? data.overworldWalkSpritesRight
                    : data.overworldWalkSpritesLeft)
                : (_monsterDirection.y > 0f
                    ? data.overworldWalkSpritesBack
                    : data.overworldWalkSpritesFront);

        if (frames.Count > 0)
        {
            _animationFrame %= frames.Count;
            _monsterSprite.sprite = frames[_animationFrame];
        }
    }

    private void UpdateSorting()
    {
        if (_playerSprite == null)
            return;

        // Equal sorting order allows your custom Y axis to handle depth.
        _monsterSprite.sortingLayerID =
            _playerSprite.sortingLayerID;

        _monsterSprite.sortingOrder =
            _playerSprite.sortingOrder;

        _capsuleSprite.sortingLayerID =
            _playerSprite.sortingLayerID;

        _capsuleSprite.sortingOrder =
            _playerSprite.sortingOrder;
    }

    public IEnumerator PlayDestroyAnimation(GameObject ob)
    {
        RecallIntoCapsule();

        while (_recalling)
        {
            yield return null;
        }
    }

    public void PlayThrowAnimation(GameObject ob)
    {
        if (_initialised &&
            HasMonster &&
            _insideCapsule &&
            CanRelease())
        {
            ReleaseMonster();
        }
    }

    private void OnDisable()
    {
        StopVisualAnimation();

        _recalling = false;
        _moving = false;
        _followingTrail = false;

        if (_monsterSprite != null)
        {
            RestoreMonsterAppearance();

            _monsterSprite.enabled =
                _hadParty && !_insideCapsule;
        }

        if (_shadowSprite != null)
        {
            _shadowSprite.enabled =
                _hadParty && !_insideCapsule;
        }

        if (_capsuleSprite != null)
        {
            _capsuleSprite.enabled =
                _hadParty && _insideCapsule;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Vector3 centre =
            transform.position + (Vector3)_footprintOffset;

        Gizmos.DrawWireCube(
            centre,
            new Vector3(
                _footprintSize.x,
                _footprintSize.y,
                0f
            )
        );
    }
}