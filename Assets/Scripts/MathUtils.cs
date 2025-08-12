using System.Collections.Generic;
using UnityEngine;

public static class MathUtils
{
    public static Plane FitPlane(Vector3[] points, out float discardedPointCount)
    {
        /* https://www.jpe-innovations.com/precision-point/fit-plane-through-points/
        
        z = Ax + By + C
        find A, B, and C, then take any 3 points and plug in x & z (since we expect those to have more variance than y)
        y = (z - Ax - C)/B

        where sum(f(x,y)) = summation of f(x,y) over all N points...
        [A]   [sum(x^2)  sum(xy)   sum(x)]^-1    [sum(xz)]
        [B] = [sum(xy)   sum(y^2)  sum(y)]    *  [sum(yz)]
        [C]   [sum(x)    sum(y)    sum(1)]       [sum(z) ]

        need to change to 4x4 matrix to use Unity functions

        [A]   [a b c 0]^1   [j]
        [B] = [d e f 0]   * [k]
        [C]   [g h i 0]     [l]
        [1]   [0 0 0 1]     [1]

        */

        var matrix = new Matrix4x4()
        {
            m00 = 0, m01 = 0, m02 = 0, m03 = 0,
            m10 = 0, m11 = 0, m12 = 0, m13 = 0,
            m20 = 0, m21 = 0, m22 = 0, m23 = 0,
            m30 = 0, m31 = 0, m32 = 0, m33 = 1
        };

        var vector = new Vector4(0, 0, 0, 1);

        // calculate standard deviation of y values and use this to find z-score for each point
        // z-score represents how many standard deviations away a data point is
        // discard points with high or low z-score (2)

        var yAvg = 0f;
        foreach (var point in points)
        {
            yAvg += point.y;
        }

        yAvg /= points.Length;
        var sqYDistSum = 0f;
        foreach (var point in points)
        {
            var dist = Mathf.Abs(point.y - yAvg);
            sqYDistSum += dist * dist;
        }

        var ySD = Mathf.Sqrt(sqYDistSum / points.Length);
        var ySDInv = 1f / ySD;
        var usedPoints = new List<Vector3>();
        foreach (var point  in points)
        {
            if (Mathf.Abs((point.y - yAvg) * ySDInv) >= 2f)
                continue;

            usedPoints.Add(point);
            matrix.m00 += point.x * point.x;
            matrix.m01 += point.x * point.y;
            matrix.m02 += point.x;
            matrix.m10 += point.x * point.y;
            matrix.m11 += point.y * point.y;
            matrix.m12 += point.y;
            matrix.m20 += point.x;
            matrix.m21 += point.y;
            vector.x += point.x * point.z;
            vector.y += point.y * point.z;
            vector.z += point.z;
        }

        matrix.m22 = usedPoints.Count;

        var coeff = matrix.inverse * vector;
        var point1 = usedPoints[0];
        point1.y = (point1.z - coeff.x * point1.x - coeff.z) / coeff.y;
        var point2 = usedPoints[usedPoints.Count / 3];
        point2.y = (point2.z - coeff.x * point2.x - coeff.z) / coeff.y;
        var point3 = usedPoints[usedPoints.Count * 2 / 3];
        point3.y = (point3.z - coeff.x * point3.x - coeff.z) / coeff.y;

        discardedPointCount = points.Length - usedPoints.Count;
        return new Plane(point1, point2, point3);
    }
}
