using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerWaterController : MonoBehaviour
{
    public enum WaterSurface
    {
        Land,
        ShallowWater,
        DeepWater
    }

    [Header("Water Layers")]
    [SerializeField] private LayerMask _shallowWaterLayers;
    [SerializeField] private LayerMask _deepWaterLayers;

    [Tooltip("Solid obstacles. Exclude player, follower, and water layers.")]
    [SerializeField] private LayerMask _solidObstacleLayers;

    [Header("Swim Permission")]
    [Tooltip("Temporary setting until your skill system is implemented.")]
    [SerializeField] private bool _hasSwimSkill;

    [Tooltip("Assign your Water ElementType asset.")]
    [SerializeField] private ElementType _waterElement;

    [Header("Movement")]
    [Tooltip("Added to the player's movement multiplier in shallow water.")]
    [SerializeField] private float _shallowWaterSpeedBoost = -0.5f;

    [Tooltip("Added to the player's movement multiplier in deep water.")]
    [SerializeField] private float _deepWaterSpeedBoost = -0.25f;

    [Header("Player Footprint")]
    [Tooltip("World-space size used to test water entry. Match the player's feet.")]
    [SerializeField] private Vector2 _footprintSize =
        new Vector2(0.4f, 0.25f);

    [Tooltip("World-space offset from the player root to the feet.")]
    [SerializeField] private Vector2 _footprintOffset;

    [Header("Normal Visuals")]
    [Tooltip("Normal body renderers hidden while swimming. Do not include the swimming renderer.")]
    [SerializeField] private SpriteRenderer[] _normalBodyRenderers;

    [SerializeField] private GameObject _legsObject;
    [SerializeField] private GameObject _shadowObject;

    [Header("Swimming Visual")]
    [Tooltip("Separate child renderer, not controlled by the normal player Animator.")]
    [SerializeField] private SpriteRenderer _swimmingRenderer;

    [SerializeField] private Sprite _swimmingSprite;

    [Tooltip("Usually the player's main SpriteRenderer.")]
    [SerializeField] private SpriteRenderer _sortingReference;

    private Rigidbody2D _rb;

    private WaterSurface _surface;
    private bool _visualsInitialised;
    private WaterSurface _lastVisualSurface;

    private bool[] _savedBodyEnabled;
    private bool _savedLegsActive;
    private bool _savedShadowActive;

    private Vector2 _lastSafeShorePosition;
    private bool _hasSafeShorePosition;
    private bool _reportedMissingShore;

    private readonly Collider2D[] _overlapResults =
        new Collider2D[1];

    private readonly RaycastHit2D[] _castResults =
        new RaycastHit2D[1];

    public WaterSurface Surface => _surface;

    public bool InWater => _surface != WaterSurface.Land;

    public bool InDeepWater => _surface == WaterSurface.DeepWater;

    public bool HasSwimSkill
    {
        get => _hasSwimSkill;
        set => _hasSwimSkill = value;
    }

    public bool CanSwim =>
        _hasSwimSkill && FindSwimmingMonster() != null;

    public float MovementSpeedBoost
    {
        get
        {
            switch (_surface)
            {
                case WaterSurface.ShallowWater:
                    return _shallowWaterSpeedBoost;

                case WaterSurface.DeepWater:
                    return _deepWaterSpeedBoost;

                default:
                    return 0f;
            }
        }
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();

        if (_normalBodyRenderers == null)
        {
            _normalBodyRenderers = new SpriteRenderer[0];
        }

        _savedBodyEnabled =
            new bool[_normalBodyRenderers.Length];
    }

    private void Start()
    {
        RefreshSurface();

        if (IsSafeShore(_rb.position))
        {
            RememberShore(_rb.position);
        }

        ApplyVisuals();
    }

    private void LateUpdate()
    {
        // Refresh after physics and apply visuals after normal animation updates.
        RefreshSurface();
        ApplyVisuals();

        if (_swimmingRenderer != null &&
            _sortingReference != null &&
            _swimmingRenderer != _sortingReference)
        {
            _swimmingRenderer.sortingLayerID =
                _sortingReference.sortingLayerID;

            _swimmingRenderer.sortingOrder =
                _sortingReference.sortingOrder;
        }
    }

    /// <summary>
    /// Called by PlayerController before physics movement.
    /// Returns false when recovering from an invalid deep-water state.
    /// </summary>
    public bool PreparePhysicsStep()
    {
        RefreshSurface();

        if (IsSafeShore(_rb.position))
        {
            RememberShore(_rb.position);
        }

        if (!InDeepWater || CanSwim)
            return true;

        // Swim permission was lost, or the player was teleported
        // into deep water without permission.
        _rb.linearVelocity = Vector2.zero;

        if (_hasSafeShorePosition &&
            IsSafeShore(_lastSafeShorePosition))
        {
            _rb.position = _lastSafeShorePosition;

            RefreshSurface();
            ApplyVisuals();

            _reportedMissingShore = false;
        }
        else if (!_reportedMissingShore)
        {
            _reportedMissingShore = true;

            Debug.LogError(
                "Player is in deep water without swimming permission, " +
                "but no valid shore position has been recorded. " +
                "Place the player on land or shallow water.",
                this
            );
        }

        return false;
    }

    /// <summary>
    /// Checks the full movement segment, preventing entry into deep
    /// water without permission, including diagonal movement.
    /// </summary>
    public bool CanMoveTo(Vector2 targetPosition)
    {
        if (CanSwim)
            return true;

        if (OverlapsLayer(targetPosition, _deepWaterLayers.value))
            return false;

        Vector2 movement = targetPosition - _rb.position;

        if (movement.sqrMagnitude < 0.000001f)
            return true;

        ContactFilter2D filter =
            CreateFilter(_deepWaterLayers.value);

        int hits = Physics2D.BoxCast(
            _rb.position + _footprintOffset,
            _footprintSize,
            0f,
            movement.normalized,
            filter,
            _castResults,
            movement.magnitude
        );

        return hits == 0;
    }

    /// <summary>
    /// Returns the first alive water-type monster in the party.
    /// Primary and secondary elements are both supported.
    /// </summary>
    public BattleMonster FindSwimmingMonster()
    {
        if (_waterElement == null || PlayerStats.i == null)
            return null;

        var party = PlayerStats.i.PlayerParty;

        if (party == null)
            return null;

        foreach (BattleMonster monster in party)
        {
            if (monster == null ||
                monster.Base == null ||
                monster.HP <= 0f)
            {
                continue;
            }

            if (MatchesWater(monster.Base.element) ||
                MatchesWater(monster.Base.secondaryElement))
            {
                return monster;
            }
        }

        return null;
    }

    private bool MatchesWater(ElementType element)
    {
        if (element == null || _waterElement == null)
            return false;

        // Compare the actual assigned element assets.
        return element == _waterElement;
    }

    private void RefreshSurface()
    {
        Vector2 position = _rb.position;

        if (OverlapsLayer(position, _deepWaterLayers.value))
        {
            _surface = WaterSurface.DeepWater;
        }
        else if (OverlapsLayer(position, _shallowWaterLayers.value))
        {
            _surface = WaterSurface.ShallowWater;
        }
        else
        {
            _surface = WaterSurface.Land;
        }
    }

    private bool IsSafeShore(Vector2 position)
    {
        return
            !OverlapsLayer(position, _deepWaterLayers.value) &&
            !OverlapsLayer(position, _solidObstacleLayers.value);
    }

    private void RememberShore(Vector2 position)
    {
        _lastSafeShorePosition = position;
        _hasSafeShorePosition = true;
        _reportedMissingShore = false;
    }

    private ContactFilter2D CreateFilter(int layers)
    {
        ContactFilter2D filter = new ContactFilter2D();

        filter.SetLayerMask(layers);
        filter.useTriggers = true;

        return filter;
    }

    private bool OverlapsLayer(Vector2 position, int layers)
    {
        return Physics2D.OverlapBox(
            position + _footprintOffset,
            _footprintSize,
            0f,
            CreateFilter(layers),
            _overlapResults
        ) > 0;
    }

    private void ApplyVisuals()
    {
        bool enteringDeep =
            InDeepWater &&
            (!_visualsInitialised ||
             _lastVisualSurface != WaterSurface.DeepWater);

        bool leavingDeep =
            _visualsInitialised &&
            _lastVisualSurface == WaterSurface.DeepWater &&
            !InDeepWater;

        if (enteringDeep)
        {
            for (int i = 0; i < _normalBodyRenderers.Length; i++)
            {
                SpriteRenderer renderer = _normalBodyRenderers[i];

                if (renderer == null || renderer == _swimmingRenderer)
                    continue;

                _savedBodyEnabled[i] = renderer.enabled;
            }
        }

        if (leavingDeep)
        {
            for (int i = 0; i < _normalBodyRenderers.Length; i++)
            {
                SpriteRenderer renderer = _normalBodyRenderers[i];

                if (renderer == null || renderer == _swimmingRenderer)
                    continue;

                renderer.enabled = _savedBodyEnabled[i];
            }
        }

        if (InDeepWater)
        {
            // Reapply after the Animator has updated.
            foreach (SpriteRenderer renderer in _normalBodyRenderers)
            {
                if (renderer != null && renderer != _swimmingRenderer)
                {
                    renderer.enabled = false;
                }
            }
        }

        bool enteringWater =
            InWater &&
            (!_visualsInitialised ||
             _lastVisualSurface == WaterSurface.Land);

        bool leavingWater =
            _visualsInitialised &&
            _lastVisualSurface != WaterSurface.Land &&
            !InWater;

        if (enteringWater)
        {
            if (_legsObject != null)
                _savedLegsActive = _legsObject.activeSelf;

            if (_shadowObject != null)
                _savedShadowActive = _shadowObject.activeSelf;
        }

        if (InWater)
        {
            if (_legsObject != null)
                _legsObject.SetActive(false);

            if (_shadowObject != null)
                _shadowObject.SetActive(false);
        }
        else if (leavingWater)
        {
            if (_legsObject != null)
                _legsObject.SetActive(_savedLegsActive);

            if (_shadowObject != null)
                _shadowObject.SetActive(_savedShadowActive);
        }

        if (_swimmingRenderer != null)
        {
            _swimmingRenderer.sprite = _swimmingSprite;
            _swimmingRenderer.enabled = InDeepWater;
        }

        _lastVisualSurface = _surface;
        _visualsInitialised = true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        Gizmos.DrawWireCube(
            transform.position + (Vector3)_footprintOffset,
            new Vector3(_footprintSize.x, _footprintSize.y, 0f)
        );
    }
}