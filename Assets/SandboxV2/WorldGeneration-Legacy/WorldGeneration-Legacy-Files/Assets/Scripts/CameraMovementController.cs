using UnityEngine;

namespace island
{
    public class CameraMovementController : MonoBehaviour
    {
        [Header("Camera Movement Settings")] [SerializeField]
        private float moveSpeed = 5f;

        [SerializeField] private float zoomSpeed = 5f;

        private void Update()
        {
            MoveCamera();
            CameraZoom();
        }

        private void MoveCamera()
        {
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");
            Vector3 movement = new Vector2(horizontalInput, verticalInput) * moveSpeed * Time.deltaTime;
            transform.Translate(movement, Space.World);
        }

        private void CameraZoom()
        {
            float scrollInput = Input.GetAxis("Mouse ScrollWheel");
            if (scrollInput != 0f)
            {
                if (GetComponent<Camera>().orthographicSize >= 10)
                {
                    GetComponent<Camera>().orthographicSize -= scrollInput * zoomSpeed;
                }
                else
                {
                    GetComponent<Camera>().orthographicSize += scrollInput * zoomSpeed;
                }

            }
        }
    }
}
