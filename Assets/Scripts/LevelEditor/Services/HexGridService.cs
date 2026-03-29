using UnityEngine;

namespace SteelSurge.LevelEditor.Services
{
    public class HexGridService
    {
        private readonly float _hexSize;
        public HexGridService(float hexSize)
        {
            _hexSize = hexSize;
        }

        public Vector3 GetWorldPosition(int q, int r)
        {
            // Pointy-topped hex grid math based on ArenaForest analysis
            // Delta X between adjacent hexes in a row is exactly 2.0
            // Delta Z between rows is exactly 1.732 (sqrt(3))
            // Offset for odd rows is 1.0 on X
            
            float width = 2f * _hexSize;
            float height = Mathf.Sqrt(3f) * _hexSize;
            
            float posX = width * q;
            if (r % 2 != 0)
            {
                posX += width / 2f;
            }
            
            // In ArenaForest, Z goes from -55.25 to -53.52, which is a difference of 1.732
            // So Z increases by height * 0.5 * sqrt(3) ? No, 1.732 IS sqrt(3).
            // So Z increases by height * r.
            // Wait, if height is sqrt(3) * hexSize, and hexSize is 1, then height is 1.732.
            // But in pointy-topped, vertical spacing is 1.5 * size.
            // Let's check the math:
            // Hex 1 (row 0): Z = -55.25
            // Hex 40 (row 1): Z = -53.52
            // Delta Z = 1.732.
            // This means vertical spacing is exactly sqrt(3).
            // This is a Flat-Topped hex grid rotated 90 degrees, or a Pointy-Topped grid where the distance between rows is sqrt(3).
            // Actually, if Delta X = 2.0 and Delta Z = 1.732, this is a Pointy-Topped grid where size = 1.1547 (2/sqrt(3))
            // OR it's just a grid where X spacing is 2.0 and Z spacing is 1.732.
            
            float posZ = 1.732051f * _hexSize * r;

            return new Vector3(posX, 0, posZ);
        }

        public Vector2Int GetSymmetricCoordinate(int q, int r, int mapWidth, int mapHeight, Configs.SymmetryType symmetryType)
        {
            switch (symmetryType)
            {
                case Configs.SymmetryType.Point:
                    return new Vector2Int(mapWidth - 1 - q, mapHeight - 1 - r);
                case Configs.SymmetryType.Horizontal:
                    return new Vector2Int(q, mapHeight - 1 - r);
                case Configs.SymmetryType.Vertical:
                    return new Vector2Int(mapWidth - 1 - q, r);
                default:
                    return new Vector2Int(q, r);
            }
        }

        private Vector3Int OffsetToCube(int col, int row)
        {
            // Convert odd-r offset to cube coordinates
            int q = col - (row - (row & 1)) / 2;
            int r = row;
            int s = -q - r;
            return new Vector3Int(q, r, s);
        }

        public int GetDistance(int q1, int r1, int q2, int r2)
        {
            Vector3Int a = OffsetToCube(q1, r1);
            Vector3Int b = OffsetToCube(q2, r2);
            return Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y), Mathf.Abs(a.z - b.z));
        }
    }
}
