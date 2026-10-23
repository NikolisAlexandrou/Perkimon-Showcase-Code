using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace island
{
    [Serializable]
    public sealed class MountainAutotileRule
    {
        [Tooltip("Exact 8-neighbour mask. N=1, NE=2, E=4, SE=8, S=16, SW=32, W=64, NW=128.")]
        [Range(0, 255)] public int mask;

        public TileBase tile;
    }

    /// <summary>
    /// Custom 8-neighbour mountain autotile lookup.
    ///
    /// Bit layout:
    /// NW(128) N(1) NE(2)
    /// W (64)  X    E (4)
    /// SW(32)  S(16) SE(8)
    ///
    /// Exact 8-neighbour rules are checked first. When no exact rule exists,
    /// the system falls back to the older cardinal lookup so the tileset can
    /// be configured incrementally rather than requiring all 256 masks.
    /// </summary>
    [CreateAssetMenu(
        fileName = "MountainAutotileSet8",
        menuName = "Project Atlas/World/Mountain Autotile Set 8-Neighbour")]
    public sealed class MountainAutotileSet : ScriptableObject
    {
        [Header("Exact 8-Neighbour Rules")]
        [Tooltip("Add only the diagonal/inner-corner patterns that need unique artwork. Exact masks override the cardinal fallback below.")]
        [SerializeField] private List<MountainAutotileRule> _exactRules = new();

        [Header("Fallback")]
        [Tooltip("Used when neither an exact rule nor a cardinal fallback tile is assigned.")]
        public TileBase fallback;

        [Header("Cardinal Fallback - Ends")]
        public TileBase isolated;
        public TileBase north;
        public TileBase east;
        public TileBase south;
        public TileBase west;

        [Header("Cardinal Fallback - Corners / Straight")]
        public TileBase northEast;
        public TileBase eastSouth;
        public TileBase southWest;
        public TileBase westNorth;
        public TileBase vertical;
        public TileBase horizontal;

        [Header("Cardinal Fallback - T Junctions")]
        public TileBase teeOpenWest;
        public TileBase teeOpenSouth;
        public TileBase teeOpenEast;
        public TileBase teeOpenNorth;

        [Header("Cardinal Fallback - Surrounded")]
        public TileBase surrounded;

        private Dictionary<int, TileBase> _ruleLookup;

        public TileBase GetTile(int eightNeighbourMask)
        {
            EnsureLookup();

            if (_ruleLookup.TryGetValue(eightNeighbourMask & 255, out TileBase exact) && exact != null)
                return exact;

            int cardinalMask = ConvertEightMaskToCardinalMask(eightNeighbourMask);
            TileBase cardinalTile = GetCardinalTile(cardinalMask);
            return cardinalTile != null ? cardinalTile : fallback;
        }

        public static int ConvertEightMaskToCardinalMask(int mask)
        {
            int cardinal = 0;

            // 4-bit format retained from v5:
            // North=1, East=2, South=4, West=8.
            if ((mask & 1) != 0) cardinal |= 1;
            if ((mask & 4) != 0) cardinal |= 2;
            if ((mask & 16) != 0) cardinal |= 4;
            if ((mask & 64) != 0) cardinal |= 8;

            return cardinal;
        }

        private TileBase GetCardinalTile(int cardinalMask)
        {
            return cardinalMask switch
            {
                0 => isolated,
                1 => north,
                2 => east,
                3 => northEast,
                4 => south,
                5 => vertical,
                6 => eastSouth,
                7 => teeOpenWest,
                8 => west,
                9 => westNorth,
                10 => horizontal,
                11 => teeOpenSouth,
                12 => southWest,
                13 => teeOpenEast,
                14 => teeOpenNorth,
                15 => surrounded,
                _ => null
            };
        }

        private void EnsureLookup()
        {
            if (_ruleLookup != null)
                return;

            _ruleLookup = new Dictionary<int, TileBase>();

            foreach (MountainAutotileRule rule in _exactRules)
            {
                if (rule == null || rule.tile == null)
                    continue;

                _ruleLookup[rule.mask & 255] = rule.tile;
            }
        }

        private void OnValidate()
        {
            _ruleLookup = null;
        }
    }
}
