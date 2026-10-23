using System;
using UnityEngine;

namespace island
{
    [Serializable]
    public readonly struct ChunkCoordinate : IEquatable<ChunkCoordinate>
    {
        public int X { get; }
        public int Y { get; }

        public ChunkCoordinate(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static ChunkCoordinate FromWorldPosition(Vector3 worldPosition, int chunkSize)
        {
            return new ChunkCoordinate(
                Mathf.FloorToInt(worldPosition.x / chunkSize),
                Mathf.FloorToInt(worldPosition.y / chunkSize));
        }

        public Vector3Int CellOrigin(int chunkSize)
        {
            return new Vector3Int(X * chunkSize, Y * chunkSize, 0);
        }

        public bool Equals(ChunkCoordinate other) => X == other.X && Y == other.Y;
        public override bool Equals(object obj) => obj is ChunkCoordinate other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(X, Y);
        public override string ToString() => $"({X}, {Y})";
    }
}
