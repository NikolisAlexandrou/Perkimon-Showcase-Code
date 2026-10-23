using UnityEngine;

namespace island
{
    public sealed class PlayerToolController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private Transform _pointer;

        [SerializeField]
        private Animator _animator;

        [SerializeField]
        private LayerMask _resourceLayer;
        
        [SerializeField]
        private PlayerController _playerController;

        [Header("Tool")]
        [SerializeField]
        private ToolDefinition _equippedTool;

        [Header("Hit")]
        [SerializeField, Min(0.1f)]
        private float _hitRadius = 0.35f;

        private float _nextUseTime;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                TryUseTool();
            }
        }

        private void TryUseTool()
        {
            if (_equippedTool == null || _pointer == null)
            {
                return;
            }

            if (Time.time < _nextUseTime)
                return;

            _nextUseTime = Time.time + _equippedTool.UseCooldown;
            _playerController.CanMove = false;
            PlayToolAnimation();
        }

        // Call this using an Animation Event.
        public void PerformToolHit()
        {
            Vector2 direction = _pointer.right.normalized;

            Vector2 hitPosition = (Vector2)transform.position + direction * _equippedTool.Range;

            Collider2D[] hits = Physics2D.OverlapCircleAll( hitPosition, _hitRadius, _resourceLayer);

            foreach (Collider2D hit in hits)
            {
                if (!hit.TryGetComponent( out ResourceNode resource))
                {
                    continue;
                }

                resource.TakeToolHit( _equippedTool, hitPosition);

                break;
            }
        }

        private void PlayToolAnimation()
        {
            if (_animator == null)
                return;

            Vector2 direction =
                _pointer.right.normalized;

            if (Mathf.Abs(direction.x) >
                Mathf.Abs(direction.y))
            {
                _animator.SetTrigger(
                    direction.x > 0f
                        ? "ToolRight"
                        : "ToolLeft");
            }
            else
            {
                _animator.SetTrigger(
                    direction.y > 0f
                        ? "ToolUp"
                        : "ToolDown");
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_pointer == null ||
                _equippedTool == null)
            {
                return;
            }

            Vector2 direction =
                _pointer.right.normalized;

            Vector2 hitPosition =
                (Vector2)transform.position +
                direction *
                _equippedTool.Range;

            Gizmos.DrawWireSphere(
                hitPosition,
                _hitRadius);
        }
#endif
    }
}