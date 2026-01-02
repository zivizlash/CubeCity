using OpenTK.Mathematics;

namespace LearnOpenTK.Mesh;

public static class VerticesPlaneData
{
    private static readonly float[] _vertices =
    [
         0.5f,  0.5f, -0.5f,
         0.5f, -0.5f, -0.5f,
        -0.5f, -0.5f, -0.5f,
        -0.5f, -0.5f, -0.5f,
        -0.5f,  0.5f, -0.5f,
         0.5f,  0.5f, -0.5f,
    ];

    private static readonly float[] _uvs =
    [
        0.0f, 0.0f, 1.0f, 0.0f,
        1.0f, 1.0f, 1.0f, 1.0f,
        0.0f, 1.0f, 0.0f, 0.0f
    ];
}

public static class VerticesCubeData
{
    private static readonly float[] _vertices =
    [
         0.5f,  0.5f, -0.5f, 
         0.5f, -0.5f, -0.5f, 
        -0.5f, -0.5f, -0.5f,
        -0.5f, -0.5f, -0.5f,
        -0.5f,  0.5f, -0.5f,
         0.5f,  0.5f, -0.5f,

        -0.5f, -0.5f,  0.5f, 
         0.5f, -0.5f,  0.5f, 
         0.5f,  0.5f,  0.5f, 
         0.5f,  0.5f,  0.5f, 
        -0.5f,  0.5f,  0.5f, 
        -0.5f, -0.5f,  0.5f, 

        -0.5f,  0.5f,  0.5f, 
        -0.5f,  0.5f, -0.5f, 
        -0.5f, -0.5f, -0.5f, 
        -0.5f, -0.5f, -0.5f, 
        -0.5f, -0.5f,  0.5f, 
        -0.5f,  0.5f,  0.5f, 

         0.5f, -0.5f, -0.5f, 
         0.5f,  0.5f, -0.5f, 
         0.5f,  0.5f,  0.5f, 
         0.5f,  0.5f,  0.5f,
         0.5f, -0.5f,  0.5f, 
         0.5f, -0.5f, -0.5f,

        -0.5f, -0.5f, -0.5f, 
         0.5f, -0.5f, -0.5f, 
         0.5f, -0.5f,  0.5f, 
         0.5f, -0.5f,  0.5f, 
        -0.5f, -0.5f,  0.5f, 
        -0.5f, -0.5f, -0.5f, 

         0.5f,  0.5f,  0.5f, 
         0.5f,  0.5f, -0.5f, 
        -0.5f,  0.5f, -0.5f,
        -0.5f,  0.5f, -0.5f,
        -0.5f,  0.5f,  0.5f, 
         0.5f,  0.5f,  0.5f, 
    ];

    private static readonly float[] _uvs =
    [
         0.0f, 0.0f, 1.0f, 0.0f,
         1.0f, 1.0f, 1.0f, 1.0f,
         0.0f, 1.0f, 0.0f, 0.0f,

         0.0f, 0.0f, 1.0f, 0.0f,
         1.0f, 1.0f, 1.0f, 1.0f,
         0.0f, 1.0f, 0.0f, 0.0f,

         1.0f, 0.0f, 1.0f, 1.0f,
         0.0f, 1.0f, 0.0f, 1.0f,
         0.0f, 0.0f, 1.0f, 0.0f,

         1.0f, 0.0f, 1.0f, 1.0f,
         0.0f, 1.0f, 0.0f, 1.0f,
         0.0f, 0.0f, 1.0f, 0.0f,

         0.0f, 1.0f, 1.0f, 1.0f,
         1.0f, 0.0f, 1.0f, 0.0f,
         0.0f, 0.0f, 0.0f, 1.0f,

         0.0f, 1.0f, 1.0f, 1.0f,
         1.0f, 0.0f, 1.0f, 0.0f,
         0.0f, 0.0f, 0.0f, 1.0f
    ];

    public static float[] GetRawVertices()
    {
        return [.. _vertices];
    }

    public static float[] GetTexturedVertices()
    {
        var result = new List<float>();

        for (int verticeIndex = 0, uvIndex = 0; verticeIndex < _vertices.Length; verticeIndex += 3, uvIndex += 2)
        {
            result.Add(_vertices[verticeIndex + 0]);
            result.Add(_vertices[verticeIndex + 1]);
            result.Add(_vertices[verticeIndex + 2]);
            result.Add(_uvs[uvIndex + 0]);
            result.Add(_uvs[uvIndex + 1]);
        }

        return [.. result];
    }

    private static void CalculateNormals(Vertex[] vertices)
    {
        for (int i = 0; i < vertices.Length; i += 3)
        {
            ref var v1 = ref vertices[i];
            ref var v2 = ref vertices[i + 1];
            ref var v3 = ref vertices[i + 2];

            var normal = MeshNormalHelper.CalculateFaceNormal(v1.Position, v2.Position, v3.Position);
            v3.Normal = v2.Normal = v1.Normal = normal;
        }
    }

    private static Vector3[] CalculateNormals(float[] vertices)
    {
        Vector3 GetTriangle(int start) => new(vertices[start], vertices[start + 1], vertices[start + 2]);

        var result = new List<Vector3>();

        const int pointsInFaces = 3 * 3; // 9

        for (int verticeIndex = 0; verticeIndex < vertices.Length; verticeIndex += pointsInFaces)
        {
            var v1 = GetTriangle(verticeIndex);
            var v2 = GetTriangle(verticeIndex + 3);
            var v3 = GetTriangle(verticeIndex + 6);

            result.Add(MeshNormalHelper.CalculateFaceNormal(v1, v2, v3));
        }

        return [.. result];
    }

    public static float[] GetTextureNormalsVertices()
    {
        var normals = CalculateNormals(_vertices);
        var result = new List<float>();

        for (int verticeIndex = 0, uvIndex = 0; verticeIndex < _vertices.Length; verticeIndex += 3, uvIndex += 2)
        {
            result.Add(_vertices[verticeIndex + 0]);
            result.Add(_vertices[verticeIndex + 1]);
            result.Add(_vertices[verticeIndex + 2]);

            result.Add(_uvs[uvIndex + 0]);
            result.Add(_uvs[uvIndex + 1]);

            var normal = normals[verticeIndex / 9];
            result.Add(normal.X);
            result.Add(normal.Y);
            result.Add(normal.Z);
        }

        return [.. result];
    }

    public static Vertex[] GetTextureNormalsVertices2()
    {
        var normals = CalculateNormals(_vertices);
        var result = new List<Vertex>();

        for (int verticeIndex = 0, uvIndex = 0; verticeIndex < _vertices.Length; verticeIndex += 3, uvIndex += 2)
        {
            var vertex = new Vertex
            {
                TexCoords = new Vector2(_uvs[uvIndex + 0], _uvs[uvIndex + 1]),
                Normal = normals[verticeIndex / 9],
                Position = new Vector3(
                    _vertices[verticeIndex + 0],
                    _vertices[verticeIndex + 1],
                    _vertices[verticeIndex + 2])
            };

            result.Add(vertex);
        }

        return [.. result];
    }
    
    public static float[] GetColoredVertices(Vector3 color)
    {
        var result = new List<float>();

        for (int verticeIndex = 0; verticeIndex < _vertices.Length; verticeIndex += 3)
        {
            result.Add(_vertices[verticeIndex + 0]);
            result.Add(_vertices[verticeIndex + 1]);
            result.Add(_vertices[verticeIndex + 2]);
            result.Add(color.X);
            result.Add(color.Y);
            result.Add(color.Z);
        }

        return [.. result];
    }

}

public class MeshNormalHelper
{
    public static Vector3 CalculateFaceNormal(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        // Calculate two edge vectors of the triangle
        Vector3 edge1 = v2 - v1;
        Vector3 edge2 = v3 - v1;

        // Calculate the cross product to get the normal
        Vector3 normal = Vector3.Cross(edge1, edge2);

        // Normalize the normal vector to ensure it has a length of 1
        return Vector3.Normalize(normal);
    }
}
