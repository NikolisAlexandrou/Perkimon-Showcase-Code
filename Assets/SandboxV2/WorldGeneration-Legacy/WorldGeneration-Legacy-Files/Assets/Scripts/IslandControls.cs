using UnityEngine;

namespace island
{
    public class IslandControls : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TerrainGenerator terrainGenerator;

        [Header("Controls")]
        [SerializeField] private KeyCode generateKey = KeyCode.R;

        private void Awake()
        {
            if (terrainGenerator == null)
                terrainGenerator = FindFirstObjectByType<TerrainGenerator>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(generateKey))
            {
                GenerateWorld();
            }
        }

        private void GenerateWorld()
        {
            if (terrainGenerator == null)
            {
                Debug.LogError(
                    "IslandControls could not find a TerrainGenerator.",
                    this
                );

                return;
            }

            terrainGenerator.GenerateIsland();
        }
    }
}