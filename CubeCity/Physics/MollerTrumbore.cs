using CubeCity.Tools;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace CubeCity.Physics;

public class QuadTree
{
    public Vector2Int Pos;
}

public struct IntersectionResult
{
    public ChunkTriangle Triangle;
    public Vector2Int ChunkPos;
    public float Distance;
}

public struct IntersectionInfo
{
    public IntersectionResult? Result;
    public int TotalCount;
    public int MatchedCount;
}

public class PhysicsEngine
{
    private readonly Dictionary<Vector2Int, ChunkTriangle[]> _triangles;

    public PhysicsEngine()
    {
        _triangles = new(64);
    }

    public IntersectionInfo Raycast(Vector3 orig, Vector3 dir)
    {
        var intersections = new List<IntersectionResult>();

        int totalTriangles = 0;
        int matchedTriangles = 0;

        foreach (var (chunkPos, triangles) in _triangles)
        {
            foreach (var triangle in triangles)
            {
                var intersection = MollerTrumbore.TriangleIntersection(
                    orig, dir, triangle.P0, triangle.P1, triangle.P2);
                totalTriangles++;

                if (intersection > 0.0f)
                {
                    matchedTriangles++;

                    var intersectionInfo = new IntersectionResult
                    {
                        ChunkPos = chunkPos,
                        Triangle = triangle,
                        Distance = intersection
                    };

                    intersections.Add(intersectionInfo);
                }
            }
        }

        var result = new IntersectionInfo
        {
            TotalCount = totalTriangles,
            MatchedCount = matchedTriangles,    
        };

        if (intersections.Count > 0)
        {
            result.Result = intersections.MinBy(i => i.Distance);
        }

        return result;
    }

    public void AddOrUpdateChunkBlocks(Vector2Int chunkPos, Vector3 inWorldPosition, Pooled<ushort[,,]> blocks)
    {
        // 0:0 -> по чанк по иксу от -8 до 8, по зету от -8 до 8
        var factory = new ChunkMetadataFactory(blocks.Resource, inWorldPosition);
        _triangles[chunkPos] = factory.Create().Triangles;
    }

    public void RemoveChunkBlocks(Vector2Int chunkPos)
    {
        _triangles.Remove(chunkPos);
    }
}

// ReSharper disable IdentifierTypo 
public static class MollerTrumbore
{
    // https://en.wikipedia.org/wiki/Möller–Trumbore_intersection_algorithm
    public static float TriangleIntersection(Vector3 orig, Vector3 dir, Vector3 v0, Vector3 v1, Vector3 v2)
    {
        var e1 = v1 - v0;
        var e2 = v2 - v0;

        var pVector = Vector3.Cross(dir, e2);
        var det = Vector3.Dot(e1, pVector);

        if (det < float.Epsilon && det > -float.Epsilon)
            return 0;

        var invDet = 1 / det;
        var tVector = orig - v0;

        var u = Vector3.Dot(tVector, pVector) * invDet;

        if (u < 0 || u > 1)
            return 0;

        var qVector = Vector3.Cross(tVector, e1) * invDet;
        var v = Vector3.Dot(dir, qVector) * invDet;

        if (v < 0 || u + v > 1)
            return 0;

        return Vector3.Dot(e2, qVector) * invDet;
    }
}
