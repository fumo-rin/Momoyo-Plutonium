using rinCore;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Momoyo
{
    #region Extension
    internal static class MineChunkerExtension
    {
        internal static bool TryRemove(this Tilemap t, Vector2 position)
        {
            Vector3Int v = position.Int3();
            if (t.GetTile(v) != null)
            {
                t.SetTile(v, null);
                return true;
            }
            return false;
        }
    }
    #endregion
    public class MineChunker : MonoBehaviour
    {
        static MineChunker instance;
        [SerializeField] TileBase rockTile;
        [SerializeField] Tilemap rockTilemap;
        private void Awake()
        {
            instance = this;
        }
        #region Player Actions
        public static void GenPlayerGap(Vector2 position)
        {
            if (instance is not MineChunker c)
                return;
            for (int i = -2; i <= 2; i++)
            {
                for (int j = -2; j <= 3; j++)
                {
                    c.rockTilemap.SetTile((position + new Vector2(i, j)).Int3(), null);
                }
            }
        }
        public static bool TryPlayerCarve(Vector2 target)
        {
            if (instance is not MineChunker c)
                return false;

            bool success = false;
            c.rockTilemap.TryRemove(target);
            return success;
        }
        #endregion
        #region Mapping
        public static void Vec2Convert(Vector2 v, out (int, int) xy)
        {
            xy = (v.x.ToInt(), v.y.ToInt());
        }
        public static void XYConvert(int chunkX, int chunkY, (int, int) xy, out Vector2 v)
        {
            v.x = chunkX * 16 + xy.Item1;
            v.y = chunkY * 16 + xy.Item2;
        }
        public static (int, int) GetChunk(Vector2 position)
        {
            (int, int) result = (position.x.ReverseQuantize(16f).ToInt() / 16, position.y.ReverseQuantize(16f).ToInt() / 16);
            return result;
        }
        #endregion
        #region Gen
        public static void _GEN(int chunkX, int chunkY)
        {
            if (instance == null)
                return;

            instance.ClearAll();
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    instance.Gen(chunkX + i, chunkY + j);
                }
            }
        }
        private void ClearAll()
        {
            rockTilemap.ClearAllTiles();
        }
        private void Gen(int chunkX, int chunkY)
        {
            Vector2 v;
            for (int i = 0; i < 16; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    XYConvert(chunkX, chunkY, (i, j), out v);
                    rockTilemap.SetTile(v.Int3(), rockTile);
                }
            }
            Tunnel(chunkX, chunkY);
        }
        private void Tunnel(int chunkX, int chunkY)
        {
            Vector2 v; Vector3Int vecint;
            for (int i = 0; i < 16; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    if (RNG.SeededRandomFloat01 < 0.98f)
                        continue;

                    int tunnelFactor = RNG.Int255 < 40 ? 22 : 8;
                    int tunnelingLeft = RNG.Int255 % tunnelFactor + 3;
                    int attempts = 100;

                    TileBase checkTile;
                    XYConvert(chunkX, chunkY, (i, j), out v);

                    while (tunnelingLeft > 0 && attempts > 0)
                    {
                        vecint = v.Int3();
                        attempts--;
                        int direction = RNG.Int255 % 4;
                        v += Vector2.right.Rotate2D(direction.AsFloat(90f));
                        checkTile = rockTilemap.GetTile(vecint);
                        if (checkTile == null)
                        {
                            tunnelingLeft--;
                            continue;
                        }
                        if (checkTile == rockTile)
                        {
                            Debug.Log($"{i} {j} : {v.ToString()}");
                            rockTilemap.SetTile(vecint, null);
                            rockTilemap.SetTile(vecint + new Vector3Int(0, -1), null);
                            tunnelingLeft--;
                        }
                    }
                }
            }
        }
        #endregion
    }
}
