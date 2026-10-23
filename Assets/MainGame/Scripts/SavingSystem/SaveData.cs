using System;
using UnityEngine;

namespace island
{
    [Serializable]
    public sealed class SaveData
    {
        public int saveVersion = 1;

        public int worldSeed;

        public PlayerSaveData player =
            new PlayerSaveData();

        public WorldModificationSaveData worldModifications =
            new WorldModificationSaveData();
    }

    [Serializable]
    public sealed class PlayerSaveData
    {
        public float positionX;
        public float positionY;
        public float positionZ;

        public Vector3 GetPosition()
        {
            return new Vector3(
                positionX,
                positionY,
                positionZ);
        }

        public void SetPosition(Vector3 position)
        {
            positionX = position.x;
            positionY = position.y;
            positionZ = position.z;
        }
    }
}