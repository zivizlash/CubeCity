using CubeCity.Models;
using Newtonsoft.Json;

namespace CubeCity
{
    public class GameSettings
    {
        public int ChunksViewDistance { get; set; } = 12;

        public float MouseSensitivity { get; set; } = 0.1f;
        public float GamepadSensitivity { get; set; } = 0.2f;
        public int GeneratingChunkThreads { get; set; } = 2;

        [JsonIgnore]
        public BlockType[] Blocks => new[]
        {
            new BlockType
            {
                Name = "Air",
                IsSolid = false,
                IsTransparent = true
            },
            new BlockType
            { // 1
                Name = "Stone",
                IsSolid = true,
                IsTransparent = false,
                TopTexture = 12,
                FrontTexture = 12,
                LeftTexture = 12,
                BottomTexture = 12,
                RightTexture = 12,
                BackTexture = 12
            }, // 2
            new BlockType
            {
                Name = "Dirt",
                IsSolid = true,
                IsTransparent = false,
                TopTexture = 11,
                FrontTexture = 14,
                LeftTexture = 14,
                BottomTexture = 13,
                RightTexture = 14,
                BackTexture = 14
            },
            new BlockType
            {
                Name = "WoodPlanks",
                IsSolid = true,
                IsTransparent = false,
                TopTexture = 8,
                FrontTexture = 8,
                LeftTexture = 8,
                BottomTexture = 8,
                RightTexture = 8,
                BackTexture = 8
            }, // 4
            new BlockType
            {
                Name = "Wood",
                IsSolid = true,
                IsTransparent = false,
                TopTexture = 10,
                FrontTexture = 11,
                LeftTexture = 11,
                BottomTexture = 10,
                RightTexture = 11,
                BackTexture = 11
            },
            new BlockType
            {
                Name = "Sand",
                IsSolid = true,
                IsTransparent = false,
                TopTexture = 6,
                FrontTexture = 6,
                LeftTexture = 6,
                BottomTexture = 6,
                RightTexture = 6,
                BackTexture = 6
            }, 
            new BlockType
            { // 6
                Name = "HardRock",
                IsSolid = true,
                IsTransparent = false,
                TopTexture = 5,
                FrontTexture = 5,
                LeftTexture = 5,
                BottomTexture = 5,
                RightTexture = 5,
                BackTexture = 5
            }
        };
    }
}
