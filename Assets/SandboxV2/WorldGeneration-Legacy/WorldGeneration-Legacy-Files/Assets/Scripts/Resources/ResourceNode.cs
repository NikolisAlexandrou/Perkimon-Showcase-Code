using System;
using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using Random = UnityEngine.Random;

namespace island
{
    [RequireComponent(typeof(WorldEntity))]
    public sealed class ResourceNode : MonoBehaviour
    {
        [Header("Resource")]
        [SerializeField] private ResourceType _resourceType;
        [SerializeField] private GameObject _itemDropPrefab;
        [SerializeField] private List<Item> _lootTable;
        
        [Header("VFX")]
        [SerializeField] private List<GameObject> _resourceHitVFX;
        [SerializeField] private GameObject _resourceBreakParticles;
        [SerializeField] private AudioClip _hitSound;
        [SerializeField] private AudioClip _breakSound;
        
        [Header("Shake Settings")]
        [SerializeField] private float _shakeDuration = 0.12f;
        [SerializeField] private float _shakeStrength = 0.08f;
        [SerializeField] private int _shakeVibrato = 12;
        [SerializeField] private float _shakeRandomness = 90f;
        

        [SerializeField, Min(1)] private int _maxHealth = 5;

        [Header("Debug")] [SerializeField] private int _currentHealth;
        
        private WorldEntity _worldEntity;

        public ResourceType ResourceType => _resourceType;


        private void Awake()
        {
            _worldEntity = GetComponent<WorldEntity>();
        }

        private void OnEnable()
        {
            _currentHealth = _maxHealth;
        }

        /// <summary>
        /// When the resource takes damage from player tool
        /// </summary>
        /// <param name="tool"></param>
        public void TakeToolHit( ToolDefinition tool, Vector2 hitPosition) {
            if (tool == null)
                return;

            _currentHealth -= tool.Damage;
            
            // Display VFX of hit
            ShakeObject();
            
            if (_hitSound != null)
                AudioSystem.i.PlaySoundClip(_hitSound);
            
            foreach(var resourceHit in _resourceHitVFX)
                Instantiate(resourceHit, hitPosition, Quaternion.identity);

            if (_currentHealth <= 0)
                BreakResource();
        }

        /// <summary>
        /// When the resource has 0 health remaining and break
        /// </summary>
        private void BreakResource()
        {
            Debug.Log($"Broke resource: {_resourceType}");
            
            // Display VFX of break
            if (_breakSound != null)
                AudioSystem.i.PlaySoundClip(_breakSound);
            
            if(_resourceBreakParticles != null)
                Instantiate(_resourceBreakParticles, transform.position, Quaternion.identity);
            
            DropItems();

            if (_worldEntity == null)
            {
                _worldEntity =
                    GetComponent<WorldEntity>();
            }

            if (_worldEntity != null)
            {
                _worldEntity
                    .DestroyPermanently();

                return;
            }

            Debug.LogWarning(
                $"{name} was destroyed but has no WorldEntity. " +
                "Its destruction will NOT persist.",
                this);

            gameObject.SetActive(false);
        }

        private void ShakeObject()
        {
            transform.DOShakePosition(_shakeDuration, _shakeStrength, _shakeVibrato, _shakeRandomness);
        }
        
        private void DropItems()
        {
            Debug.Log("Item dropped");
            foreach (Item item in _lootTable)
            {
                GameObject itemOb = Instantiate(_itemDropPrefab, new Vector2(Random.Range(transform.position.x - 1.2f, transform.position.x + 1.2f), Random.Range(transform.position.y - 1.2f, transform.position.y + 1.2f)), Quaternion.identity);
                itemOb?.GetComponent<OverworldItem>().SetItem(item);
            }
        }
    }
}