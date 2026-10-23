using UnityEngine;

namespace island
{
    public static class WorldEntityId
    {
        public static string Create(
            int worldSeed,
            ChunkCoordinate chunk,
            Vector2Int worldCell,
            WorldEntityCategory category,
            int deterministicSalt = 0)
        {
            return
                $"{worldSeed}:" +
                $"{chunk.X}:{chunk.Y}:" +
                $"{worldCell.x}:{worldCell.y}:" +
                $"{category}:" +
                $"{deterministicSalt}";
        }
    }
}