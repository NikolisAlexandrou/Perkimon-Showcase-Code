using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace island
{

    public class UIManager : MonoBehaviour
    {
        [Header("Island Generation")] [SerializeField]
        private TerrainGenerator _terrainGeneratorScript;

        [SerializeField] private TreeGenerator _treeGeneratorScript;
        [SerializeField] private DecorationGenerator _decorationGeneratorScript;

        [Header("UI Sliders")] [SerializeField]
        private Slider _widthSlider;

        [SerializeField] private Slider _heightSlider;
        [SerializeField] private Slider _noiseScaleSlider;
        [SerializeField] private Slider _landThresholdSlider;
        [SerializeField] private Slider _tallGrassChanceSlider;
        [SerializeField] private Slider _treeChanceSlider;
        [SerializeField] private Slider _treeDensitySlider;

        [Header("UI Buttons")] [SerializeField]
        private GameObject _settingsHolder;

        [SerializeField] private Button _showButton;
        [SerializeField] private GameObject _showButtonStartOffset;
        [SerializeField] private GameObject _showButtonEndOffset;
        [SerializeField] private float _animationDuration = 0.5f;
        private bool _isShowingBox = true;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _widthSlider.value = _terrainGeneratorScript.Width;
            _heightSlider.value = _terrainGeneratorScript.Height;
            _noiseScaleSlider.value = _terrainGeneratorScript.NoiseScale;
            _landThresholdSlider.value = _terrainGeneratorScript.LandThreshold;
            _tallGrassChanceSlider.value = _decorationGeneratorScript.TallGrassChance;
            _treeChanceSlider.value = _treeGeneratorScript.TreeChance;
            _treeDensitySlider.value = _treeGeneratorScript.TreeDensity;
        }

        // Update is called once per frame
        void Update()
        {
            _terrainGeneratorScript.Width = (int)_widthSlider.value;
            _terrainGeneratorScript.Height = (int)_heightSlider.value;
            _terrainGeneratorScript.NoiseScale = _noiseScaleSlider.value;
            _terrainGeneratorScript.LandThreshold = _landThresholdSlider.value;
            _decorationGeneratorScript.TallGrassChance = _tallGrassChanceSlider.value;
            _treeGeneratorScript.TreeChance = _treeChanceSlider.value;
            _treeGeneratorScript.TreeDensity = (int)_treeDensitySlider.value;
        }

        /**
         * Calls the GenerateIsland method of the ProceduralIslandScript to regenerate the island with the current settings.
         */
        public void OnRegenerateButtonPress()
        {
            _terrainGeneratorScript.GenerateIsland();
        }

        /**
         * Toggles the visibility of the settings box by moving it between two positions using DOTween for smooth animation.
        */
        public void OnClickShowBox()
        {
            if (_isShowingBox == true)
            {
                _settingsHolder.transform.DOMove(_showButtonStartOffset.transform.position, _animationDuration);
            }
            else
            {
                _settingsHolder.transform.DOMove(_showButtonEndOffset.transform.position, _animationDuration);
            }

            _isShowingBox = !_isShowingBox;
            _showButton.transform.DORotate(new Vector3(0, 0, !_isShowingBox ? 180 : 0), _animationDuration);
        }
    }
}
