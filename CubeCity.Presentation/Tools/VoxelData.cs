using Microsoft.Xna.Framework;

namespace CubeCity.Tools;

public static class VoxelData
{
    public static readonly Vector3Int[] Faces =
    {
        new( 0,  0, -1),
        new( 0,  0,  1),
        new( 0,  1,  0),
        new( 0, -1,  0),
        new(-1,  0,  0),
        new( 1,  0,  0)
    };

    public static readonly Vector3[] Verts =
    {
        new(0.0f, 0.0f, 0.0f),
        new(1.0f, 0.0f, 0.0f),
        new(1.0f, 1.0f, 0.0f),
        new(0.0f, 1.0f, 0.0f),
        new(0.0f, 0.0f, 1.0f),
        new(1.0f, 0.0f, 1.0f),
        new(1.0f, 1.0f, 1.0f),
        new(0.0f, 1.0f, 1.0f)
    };

    public static readonly int[,] Tris =
    {
        // Зад, Перед, Верх, Низ, Лево, Право.
        // 0 1 2 2 1 3
        { 0, 3, 1, 2 }, // Заднее лицо.
        { 5, 6, 4, 7 }, // Переднее лицо.
        { 3, 7, 2, 6 }, // Верхнее лицо.
        { 1, 5, 0, 4 }, // Нижнее лицо.
        { 4, 7, 0, 3 }, // Левое лицо.
        { 1, 2, 5, 6 }  // Правое лицо.
    };

    public static readonly Vector2[] VoxelUvs =
    {
        new(0.0f, 0.0f),
        new(0.0f, 1.0f),
        new(1.0f, 0.0f),
        new(1.0f, 1.0f)
    };
}
