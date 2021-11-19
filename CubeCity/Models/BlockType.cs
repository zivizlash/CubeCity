using System;
using System.Runtime.CompilerServices;

namespace CubeCity.Models
{
    public class BlockType
    {
        public string Name;
        public bool IsSolid;
        public bool IsTransparent;

        public int BackTexture;
        public int FrontTexture;
        public int TopTexture;
        public int BottomTexture;
        public int LeftTexture;
        public int RightTexture;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetTextureId(int faceIndex)
        {
            return faceIndex switch
            {
                0 => BackTexture,
                1 => FrontTexture,
                2 => TopTexture,
                3 => BottomTexture,
                4 => LeftTexture,
                5 => RightTexture,
                _ => throw new ArgumentOutOfRangeException(nameof(faceIndex))
            };
        }
    }
}
