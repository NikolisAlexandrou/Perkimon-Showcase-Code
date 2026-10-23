using System;

namespace island
{
    [Serializable]
    public sealed class PlayerPositionSaveData
    {
        public float x;
        public float y;
        public float z;
    }

    [Serializable]
    public sealed class GameSaveData
    {
        // Useful later if we ever change the save format.
        public int saveVersion = 1;

        public int worldSeed;

        public PlayerPositionSaveData playerPosition =
            new PlayerPositionSaveData();

        public WorldModificationSaveData worldModifications =
            new WorldModificationSaveData();
    }
}