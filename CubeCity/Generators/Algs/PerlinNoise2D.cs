using System;

namespace CubeCity.Tools
{
    public class PerlinNoise2D
    {
        private readonly byte[] _permutationTable;

        public PerlinNoise2D(int seed = 0)
        {
            _permutationTable = new byte[8096];
            new Random(seed).NextBytes(_permutationTable);
        }

        private float[] GetPseudoRandomGradientVector(int x, int y)
        {
            int v = (int)(((x * 1836311903) ^ (y * 2971215073) + 4807526976) & 1023);
            v = _permutationTable[v] & 3;

            switch (v)
            {
                case 0: return new float[] { 1, 0 };
                case 1: return new float[] { -1, 0 };
                case 2: return new float[] { 0, 1 };
                default: return new float[] { 0, -1 };
            }
        }

        static float QunticCurve(float t)
        {
            return t * t * t * (t * (t * 6 - 15) + 10);
        }

        static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * t;
        }

        static float Dot(float[] a, float[] b)
        {
            return a[0] * b[0] + a[1] * b[1];
        }

        public float Noise(float fx, float fy)
        {
            int left = (int)Math.Floor(fx);
            int top = (int)Math.Floor(fy);
            float pointInQuadX = fx - left;
            float pointInQuadY = fy - top;

            var topLeftGradient = GetPseudoRandomGradientVector(left, top);
            var topRightGradient = GetPseudoRandomGradientVector(left + 1, top);
            var bottomLeftGradient = GetPseudoRandomGradientVector(left, top + 1);
            var bottomRightGradient = GetPseudoRandomGradientVector(left + 1, top + 1);

            float[] distanceToTopLeft = { pointInQuadX, pointInQuadY };
            float[] distanceToTopRight = { pointInQuadX - 1, pointInQuadY };
            float[] distanceToBottomLeft = { pointInQuadX, pointInQuadY - 1 };
            float[] distanceToBottomRight = { pointInQuadX - 1, pointInQuadY - 1 };

            float tx1 = Dot(distanceToTopLeft, topLeftGradient);
            float tx2 = Dot(distanceToTopRight, topRightGradient);
            float bx1 = Dot(distanceToBottomLeft, bottomLeftGradient);
            float bx2 = Dot(distanceToBottomRight, bottomRightGradient);

            pointInQuadX = QunticCurve(pointInQuadX);
            pointInQuadY = QunticCurve(pointInQuadY);

            float tx = Lerp(tx1, tx2, pointInQuadX);
            float bx = Lerp(bx1, bx2, pointInQuadX);
            float tb = Lerp(tx, bx, pointInQuadY);

            return tb;
        }

        public float Noise(float fx, float fy, int octaves, float persistence = 0.5f)
        {
            float amplitude = 1;
            float max = 0;
            float result = 0;

            while (octaves-- > 0)
            {
                max += amplitude;
                result += Noise(fx, fy) * amplitude;
                amplitude *= persistence;
                fx *= 2;
                fy *= 2;
            }

            return result / max;
        }
    }
}
