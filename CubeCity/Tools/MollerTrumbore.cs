using Microsoft.Xna.Framework;

namespace CubeCity.Tools
{
    // ReSharper disable IdentifierTypo 
    public class MollerTrumbore
    {
        // https://en.wikipedia.org/wiki/Möller–Trumbore_intersection_algorithm
        public float TriangleIntersection(
            Vector3 orig, Vector3 dir, Vector3 v0, Vector3 v1, Vector3 v2)
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
}
