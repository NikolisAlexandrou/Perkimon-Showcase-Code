using System;
using System.Collections.Generic;

namespace island
{
    [Serializable]
    public sealed class ChunkModificationData
    {
        public int chunkX;
        public int chunkY;

        public List<string> destroyedEntityIds =
            new List<string>();
    }

    [Serializable]
    public sealed class WorldModificationSaveData
    {
        public int worldSeed;

        public List<ChunkModificationData> chunks =
            new List<ChunkModificationData>();
    }
}