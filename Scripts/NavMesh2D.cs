using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace JRPGNavAgent2D
{

    public class NavMesh2D : MonoBehaviour
    {

        public Vector2Int Size = new Vector2Int(32, 32);
        public float TileSize = 1;
        public LayerMask LayerMask;

        public bool ShowTilesGizmos = false;

        [SerializeField]

        bool[] walkableArea;

        void OnValidate()
        {
            walkableArea = new bool[Size.x * Size.y];     
        }

        public void Bake()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            walkableArea = new bool[Size.x * Size.y];

            for (int x = 0; x < Size.x; x++)
            {
                for (int y = 0; y < Size.y; y++)
                {
                    var hits = Physics2D.BoxCastAll(TileToWorld(x, y), new Vector2(TileSize * 0.9f, TileSize * 0.9f), 0, Vector2.zero, 0, LayerMask);
                    //Debug.Log(hits.Length);
                    walkableArea[x + y * Size.x] = hits.Length == 0;
                }
            }

            stopwatch.Stop();

            UnityEngine.Debug.Log("Baked in: " + stopwatch.ElapsedMilliseconds + "ms");

        }

        public Vector3 TileToWorld(int x, int y)
        {
            float pX = (Size.x / -2.0f * TileSize) + (TileSize * 0.5f) + (x * TileSize);
            float pY = (Size.y / -2.0f * TileSize) + (TileSize * 0.5f) + (y * TileSize);
            return new Vector3(pX, pY, 0);
        }

        public Vector2Int WorldToTile(Vector2 world)
        {

            float halfX = (Size.x / 2.0f * TileSize);
            float halfY = (Size.y / 2.0f * TileSize);

            int x = Mathf.FloorToInt((world.x + halfX) / TileSize);
            int y = Mathf.FloorToInt((world.y + halfY) / TileSize);

            return new Vector2Int(x, y);
        }

        public bool InBounds(Vector2Int position)
        {
            return !(position.x > Size.x || position.x < 0 || position.y > Size.y || position.y < 0);
        }

        public bool InBounds(int x, int y)
        {
            return (x < Size.x && x >= 0 && y < Size.y && y >= 0);
        }

        public bool IsWalkable(int x, int y)
        {
            return InBounds(x, y) && walkableArea[x + (y * Size.x)];
        }

        public bool IsWalkable(Vector2Int position)
        {
            return InBounds(position) && walkableArea[position.x + (position.y * Size.x)];
        }

        struct TileSample
        {
            public int x, y;
            public int xOrigin, yOrigin;
            public float scoreF;
            public float scoreG;
            public bool visited;
        }

        void CheckNeighbor(int xOrigin, int yOrigin, int x, int y, int xEnd, int yEnd, float addedGScore, MinList<Vector2Int> samples, TileSample[,] tiles)
        {
            if (IsWalkable(x, y))
            {
                if (tiles[x, y].visited == false)
                {
                    float scoreG = tiles[xOrigin, yOrigin].scoreG + addedGScore;
                    float scoreF = Mathf.Abs(xEnd - x) + Mathf.Abs(yEnd - y) + scoreG;

                    tiles[x, y] = new TileSample()
                    {
                        xOrigin = xOrigin,
                        yOrigin = yOrigin,
                        scoreF = scoreF,
                        scoreG = scoreG,
                        x = x,
                        y = y,
                        visited = true,
                    };

                    samples.Add(scoreF, new Vector2Int(x, y));
                }
                else
                {
                    TileSample sample = tiles[x, y];

                    float scoreG = tiles[xOrigin, yOrigin].scoreG + addedGScore;
                    float scoreF = Mathf.Abs(xEnd - x) + Mathf.Abs(yEnd - y) + scoreG;

                    if (sample.scoreF > scoreF)
                    {
                        sample.scoreG = scoreG;
                        sample.scoreF = scoreF;
                        sample.xOrigin = xOrigin;
                        sample.yOrigin = yOrigin;
                        tiles[x, y] = sample;
                    }
                }
            }
        }

        void GetNeighbors(int xOrigin, int yOrigin, int xEnd, int yEnd, MinList<Vector2Int> samples, TileSample[,] tiles, bool canMoveDiagonally)
        {
            CheckNeighbor(xOrigin, yOrigin, xOrigin + 1, yOrigin, xEnd, yEnd, 1, samples, tiles);
            CheckNeighbor(xOrigin, yOrigin, xOrigin - 1, yOrigin, xEnd, yEnd, 1, samples, tiles);
            CheckNeighbor(xOrigin, yOrigin, xOrigin, yOrigin + 1, xEnd, yEnd, 1, samples, tiles);
            CheckNeighbor(xOrigin, yOrigin, xOrigin, yOrigin - 1, xEnd, yEnd, 1, samples, tiles);

            if (canMoveDiagonally)
            {
                CheckNeighbor(xOrigin, yOrigin, xOrigin + 1, yOrigin + 1, xEnd, yEnd, 1.414f, samples, tiles);
                CheckNeighbor(xOrigin, yOrigin, xOrigin + 1, yOrigin - 1, xEnd, yEnd, 1.414f, samples, tiles);
                CheckNeighbor(xOrigin, yOrigin, xOrigin - 1, yOrigin + 1, xEnd, yEnd, 1.414f, samples, tiles);
                CheckNeighbor(xOrigin, yOrigin, xOrigin - 1, yOrigin - 1, xEnd, yEnd, 1.414f, samples, tiles);
            }
        }

        public Vector2[]? CreatePath(Vector2 from, Vector2 to, bool canMoveDiagonally)
        {

            var start = WorldToTile(from);
            var end = WorldToTile(to);

            if (start == end) return new Vector2[0];

            if (IsWalkable(start) == false) return null;
            if (IsWalkable(end) == false) return null;

            MinList<Vector2Int> openSet = new();
            openSet.Add(0, start);
            TileSample[,] area = new TileSample[Size.x, Size.y];
            area[start.x, start.y] = new TileSample()
            {
                x = start.x,
                y = start.y,
            };

            while (openSet.Count > 0)
            {
                var sample = openSet.PopFirst();

                if (sample.x == end.x && sample.y == end.y)
                {
                    List<Vector2> path = new();

                    var toAdd = area[sample.x, sample.y];

                    while (toAdd.x != start.x || toAdd.y != start.y)
                    {
                        path.Add(TileToWorld(toAdd.x, toAdd.y));
                        toAdd = area[toAdd.xOrigin, toAdd.yOrigin];
                    }

                    path.Reverse();

                    return path.ToArray();
                }
                else
                {
                    GetNeighbors(sample.x, sample.y, end.x, end.y, openSet, area, canMoveDiagonally);
                }
            }

            return new Vector2[0];

        }

        void OnDrawGizmosSelected()
        {

            if (ShowTilesGizmos)
            {
                for (int y = 0; y < Size.y; y++)
                {
                    for (int x = 0; x < Size.x; x++)
                    {
                        if (walkableArea[x + y * Size.x])
                        {
                            Gizmos.color = new Color(0, 0, 1, 0.1f);
                            Gizmos.DrawCube(TileToWorld(x, y), new Vector3(TileSize, TileSize, TileSize) * 0.9f);
                            Gizmos.color = new Color(0, 0, 1, 0.2f);
                            Gizmos.DrawWireCube(TileToWorld(x, y), new Vector3(TileSize, TileSize, TileSize) * 0.9f);
                        }
                    }
                }
            }

            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(TileSize * Size.x, TileSize * Size.y, TileSize));
        }

    }
}