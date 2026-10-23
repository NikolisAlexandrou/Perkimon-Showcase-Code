using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


/// <summary>
/// Creates fake shadows to 2D objects
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class ShadowsScript : MonoBehaviour
{
    [Tooltip("Shadow rotation from original object in degrees")]
    [SerializeField] private float _shadowRotation = 22;

    [Tooltip("Offset position of shadow from original object")]
    [SerializeField] private Vector2 _shadowOffset = new Vector2(-0.3f, 0.3f);

    [Tooltip("Scale of the shadow")]
    [SerializeField] private Vector2 _shadowScale = new Vector2(1, 0.8f);

    private SpriteRenderer rb;
    private GameObject _currentShadow;

    private void Awake()
    {
        rb = GetComponent<SpriteRenderer>();
    }
    private void Start()
    {
        createShadow();
        updateShadowTransform();
    }

    /// <summary>
    /// Creates the shadow and assignes it to the original object
    /// </summary>
    private void createShadow()
    {
        // Get sprite of original object
        Sprite sprite = rb.sprite;

        // Spawn the shadow
        _currentShadow = new GameObject($"{ gameObject.name } Shadow");
        _currentShadow.transform.parent = gameObject.transform;

        // Setup SpriteRenderer for shadow
        _currentShadow.AddComponent<SpriteRenderer>();
        _currentShadow.GetComponent<SpriteRenderer>().sprite = sprite;

        // Set color to black
        _currentShadow.GetComponent<SpriteRenderer>().color = new Color(0,0,0,0.4f);

        // Set sorting layer of shadow to -1 of that of original object to appear "down"
        _currentShadow.GetComponent<SpriteRenderer>().sortingLayerName = rb.sortingLayerName;
        _currentShadow.GetComponent<SpriteRenderer>().sortingOrder = rb.sortingOrder - 1;

    }

    /// <summary>
    /// Updates the transform details for the shadow
    /// </summary>
    private void updateShadowTransform()
    {
        _currentShadow.transform.position = (Vector2)gameObject.transform.position + _shadowOffset;
        _currentShadow.transform.rotation = Quaternion.Euler(0, 0, _shadowRotation);
        _currentShadow.transform.localScale = _shadowScale;
    }
}
