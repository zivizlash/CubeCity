using OpenTK.Mathematics;

namespace LearnOpenTK;

public static class VerticesData
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

    private static Vector3[] CalculateNormals()
    {
        static Vector3 GetTriangle(int start) => 
            new(_vertices[start], _vertices[start + 1], _vertices[start + 2]);

        var result = new List<Vector3>();

        const int pointsInFaces = 3 * 3; // 9

        for (int verticeIndex = 0; verticeIndex < _vertices.Length; verticeIndex += pointsInFaces)
        {
            var v1 = GetTriangle(verticeIndex);
            var v2 = GetTriangle(verticeIndex + 3);
            var v3 = GetTriangle(verticeIndex + 6);

            var normal = CalculateFaceNormal(v1, v2, v3);
            result.Add(normal);
        }

        return [.. result];
    }

    public static float[] GetTextureNormalsVertices()
    {
        var normals = CalculateNormals();
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
