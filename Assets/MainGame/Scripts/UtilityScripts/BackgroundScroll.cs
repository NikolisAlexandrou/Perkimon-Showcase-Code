using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Moves a UI or a normal object horizontally and moves back to initial position after
/// it has moved its width.
///     - Better create 2 copies of the same object, one next to another for infinite scrolling effect
/// </summary>
public class BackgroundScroll : MonoBehaviour
{
    [Tooltip("Speed of the background moving horizontally")]
    [SerializeField] private float _scrollSpeed = 0.5f; // Speed of the background scrolling

    [Tooltip("Width of the background image that is moving (set to 0 for auto calculating width)")]
    [SerializeField] private float _backgroundWidth = 0; // Width of the background image

    [Tooltip("If the background that is moving is a UI object or a world object")]
    [SerializeField] private bool _isUI = true;

    private Vector3 _startPosition;

    void Start()
    {
        // Save the initial position of the background
        _startPosition = transform.position;

        // Calculate the width of the background automatically if the image has a Renderer
        if (_backgroundWidth == 0)
        {
            if (_isUI == false)
            {
                SpriteRenderer renderer = GetComponent<SpriteRenderer>();
                if (renderer != null)
                    _backgroundWidth = renderer.bounds.size.x;
            }
            else
            {
                Image renderer = GetComponent<Image>();
                if (renderer != null)
                    _backgroundWidth = GameUtils.i.MainCamera.WorldToScreenPoint(new Vector3(GetComponent<RectTransform>().rect.width, GetComponent<RectTransform>().rect.height,0)).x;
            }
           
        }
    }

    void Update()
    {
        // Move the background horizontally
        float newPosition = Mathf.Repeat(Time.time * _scrollSpeed, _backgroundWidth);
        transform.position = _startPosition + Vector3.left * newPosition;
    }
}
