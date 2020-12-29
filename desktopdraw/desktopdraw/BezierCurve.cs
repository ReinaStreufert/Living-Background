using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace desktopdraw
{
    class BezierCurve
    {
        private List<PointF> curvePts = new List<PointF>();

        public BezierCurve(IEnumerable<PointF> CurveNodes)
        {
            foreach (PointF pt in CurveNodes)
            {
                curvePts.Add(pt);
            }
        }
        public PointF GetPointAlongCurve(float Percentage)
        {
            return bezier(curvePts, Percentage);
        }
        public IEnumerable<PointF> GetPoints(float Resolution)
        {
            for (float i = 0; i <= 1; i += Resolution)
            {
                yield return GetPointAlongCurve(i);
            }
        }

        private static float distance(PointF a, PointF b)
        {
            return (float)Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }
        private static PointF pixelsToward(PointF a, PointF b, float pixels)
        {
            float dist = distance(a, b);
            float rise = (b.Y - a.Y) / dist;
            float run = (b.X - a.X) / dist;
            return new PointF(a.X + (run * pixels), a.Y + (rise * pixels));
        }
        private static PointF bezierCubic(List<PointF> curve, float t)
        {
            if (curve.Count != 3)
            {
                throw new ArgumentException("Curve must have exactly 3 nodes");
            }
            PointF a = pixelsToward(curve[0], curve[1], distance(curve[0], curve[1]) * t);
            PointF b = pixelsToward(curve[1], curve[2], distance(curve[1], curve[2]) * t);
            return pixelsToward(a, b, distance(a, b) * t);
        }
        private static PointF bezier(List<PointF> curve, float t)
        {
            if (curve.Count == 3)
            {
                return bezierCubic(curve, t);
            }
            else if (curve.Count > 3)
            {
                List<PointF> refinedCurve = new List<PointF>();
                for (int i = 0; i < curve.Count - 1; i++)
                {
                    PointF thisPoint = curve[i];
                    PointF nextPoint = curve[i + 1];
                    refinedCurve.Add(pixelsToward(thisPoint, nextPoint, distance(thisPoint, nextPoint) * t));
                }
                return bezier(refinedCurve, t);
            }
            else
            {
                throw new ArgumentException("Curve must have at least 3 nodes");
            }
        }
    }
}
