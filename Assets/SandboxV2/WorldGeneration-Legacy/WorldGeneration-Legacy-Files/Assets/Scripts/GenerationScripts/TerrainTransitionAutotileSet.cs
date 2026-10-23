using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace island
{
    [Serializable]
    public sealed class TerrainTransitionRule
    {
        [Tooltip("Exact 8-neighbour mask. N=1, NE=2, E=4, SE=8, S=16, SW=32, W=64, NW=128.")]
        [Range(0, 255)] public int mask;

        public TileBase tile;
    }

    /// <summary>
    /// Reusable 8-neighbour autotile lookup for terrain transitions.
    ///
    /// A bit is set when the neighbouring cell belongs to the same visual
    /// terrain group as the current cell.
    ///
    /// Exact diagonal rules are checked first, then a cardinal fallback.
    /// This lets you start with 16 cardinal tiles and add only the diagonal
    /// cases that need unique artwork.
    /// </summary>
    [CreateAssetMenu(
        fileName = "TerrainTransitionAutotileSet",
        menuName = "Project Atlas/World/Terrain Transition Autotile Set")]
    public sealed class TerrainTransitionAutotileSet : ScriptableObject
    {
        [Header("Exact 8-Neighbour Rules")]
        [SerializeField] private List<TerrainTransitionRule> _exactRules = new();

        [Header("Fallback")]
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

            int normalizedMask =
                eightNeighbourMask & 255;

            // First check all exact 8-neighbour rules.
            // This includes inner corners and diagonal-only outer corners.
            if (_ruleLookup.TryGetValue(
                    normalizedMask,
                    out TileBase exact) &&
                exact != null)
            {
                return exact;
            }

            int cardinalMask =
                ConvertEightMaskToCardinalMask(
                    normalizedMask);

            // --------------------------------------------------
            // IMPORTANT FOR OVERLAY AUTOTILES
            // --------------------------------------------------

            // No neighbours at all.
            // There is no transition to draw.
            if (normalizedMask == 0)
                return null;

            // We have ONLY diagonal neighbours.
            //
            // Example:
            // mask = 8 (SE)
            //
            // The old system converted this to cardinal mask 0,
            // which returned "isolated" and placed a full terrain
            // tile in the ocean.
            //
            // If you want diagonal outer-corner artwork,
            // configure mask 2 / 8 / 32 / 128 as Exact Rules.
            if (cardinalMask == 0)
                return null;

            // Fully surrounded by this terrain.
            // No overlay is necessary.
            if (normalizedMask == 255)
                return null;

            TileBase tile =
                GetCardinalTile(
                    cardinalMask);

            if (tile != null)
                return tile;

            return fallback;
        }

        public static int ConvertEightMaskToCardinalMask(int mask)
        {
            int cardinal = 0;

            if ((mask & 1) != 0) cardinal |= 1;   // N
            if ((mask & 4) != 0) cardinal |= 2;   // E
            if ((mask & 16) != 0) cardinal |= 4;  // S
            if ((mask & 64) != 0) cardinal |= 8;  // W

            return cardinal;
        }

        private TileBase GetCardinalTile(int mask)
        {
            return mask switch
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

            foreach (TerrainTransitionRule rule in _exactRules)
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
