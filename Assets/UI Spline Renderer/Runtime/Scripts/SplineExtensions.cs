using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace UI_Spline_Renderer
{
    public static class SplineExtensions
    {
        /// <summary>
        /// Reorient a single knot to screen direction.
        /// </summary>
        public static void ReorientKnot(this Spline spline, int index, bool withoutNotify = false)
        {
            var knot = spline[index];
            var rot = (Quaternion)knot.Rotation;
            
            var forward = rot * Vector3.forward;
            var projected = Vector3.ProjectOnPlane(forward, Vector3.back);
            var targetRotation = Quaternion.LookRotation(projected, Vector3.back);
            knot.Rotation = targetRotation;
            
            if(withoutNotify)spline.SetKnotNoNotify(index, knot);
            else spline.SetKnot(index, knot);
        }

        /// <summary>
        /// Reorient all knots to screen direction.
        /// </summary>
        public static void ReorientKnots(this SplineContainer container, bool withoutNotify = false)
        {
            foreach (var spline in container.Splines)
            {
                for (int i = 0; i < spline.Count; i++)
                {
                    ReorientKnot(spline, i, withoutNotify);
                }
            }
        }
        

        
        /// <summary>
        /// Reorient all knots to screen direction and make all knots AutoSmooth.
        /// </summary>
        /// <param name="container"></param>
        public static void ReorientKnotsAndSmooth(this SplineContainer container)
        {
            foreach (var spline in container.Splines)
            {
                for (int i = 0; i < spline.Count; i++)
                {
                    var knot = spline[i];
                    var prev = i == 0 ? knot.Position : spline[i - 1].Position;
                    var next = i == spline.Count - 1 ? spline[i].Position : spline[i + 1].Position;

                    knot = SplineUtility.GetAutoSmoothKnot(knot.Position, prev, next, new float3(0, 0, -1));
                    spline.SetKnot(i, knot);
                }
            }
        }

    }
}