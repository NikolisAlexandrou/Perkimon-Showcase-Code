using UnityEngine;

namespace island
{
    [CreateAssetMenu(
        fileName = "Tool",
        menuName = "Items/Tool")]
    public class ToolDefinition : ItemBase
    {
        [Header("Tool")]
        [SerializeField, Min(1)]
        private int _damage = 1;

        [SerializeField, Min(0.1f)]
        private float _useCooldown = 0.45f;

        [SerializeField, Min(0.1f)]
        private float _range = 1f;

        public int Damage => _damage;
        public float UseCooldown => _useCooldown;
        public float Range => _range;
    }
}