using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace UI_Spline_Renderer
{
    public struct CopiedNativeSpline : ISpline, IDisposable
    {
        [ReadOnly]
        NativeArray<BezierKnot> m_Knots;

        [ReadOnly]
        NativeArray<BezierCurve> m_Curves;
        
        [ReadOnly]
        NativeArray<DistanceToInterpolation> m_SegmentLengthsLookupTable;
        
        bool m_Closed;
        float m_Length;
        const int k_SegmentResolution = 30;

        public NativeArray<BezierKnot> Knots => m_Knots;

        public NativeArray<BezierCurve> Curves => m_Curves;

        public bool Closed => m_Closed;
        public int Count => m_Knots.Length;

        public readonly float GetLength() => m_Length;
        public BezierKnot this[int index] => m_Knots[index];
        public IEnumerator<BezierKnot> GetEnumerator() => m_Knots.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        
        public CopiedNativeSpline(NativeArray<BezierKnot> knots, bool closed, float4x4 transform, Allocator allocator = Allocator.Temp)
        {
            int kc = knots.Length;
            m_Knots = knots;
            m_Curves = new NativeArray<BezierCurve>(kc, allocator);
            m_SegmentLengthsLookupTable = new NativeArray<DistanceToInterpolation>(kc * k_SegmentResolution, allocator);
            m_Closed = closed;
            m_Length = 0f;
            
            NativeArray<DistanceToInterpolation> distanceToTimes = new NativeArray<DistanceToInterpolation>(k_SegmentResolution, Allocator.Temp);

            if (knots.Length > 0)
            {
                BezierKnot cur = knots[0].Transform(transform);
                for (int i = 0; i < kc; ++i)
                {
                    BezierKnot next = knots[(i + 1) % kc].Transform(transform);
                    m_Knots[i] = cur;

                    m_Curves[i] = new BezierCurve(cur, next);
                    InternalUtility.CalculateCurveLengths(m_Curves[i], distanceToTimes);

                    if (m_Closed || i < kc - 1)
                        m_Length += distanceToTimes[k_SegmentResolution - 1].Distance;

                    for (int index = 0; index < k_SegmentResolution; index++)
                    {
                        m_SegmentLengthsLookupTable[i * k_SegmentResolution + index] = distanceToTimes[index];
                    }

                    cur = next;
                }
            }
        }

        /// <summary>
        /// Get a <see cref="BezierCurve"/> from a knot index.
        /// </summary>
        /// <param name="index">The knot index that serves as the first control point for this curve.</param>
        /// <returns>
        /// A <see cref="BezierCurve"/> formed by the knot at index and the next knot.
        /// </returns>
        public BezierCurve GetCurve(int index) => m_Curves[index];


        /// <summary>
        /// Get the length of a <see cref="BezierCurve"/>.
        /// </summary>
        /// <param name="curveIndex">The 0 based index of the curve to find length for.</param>
        /// <returns>The length of the bezier curve at index.</returns>
        public float GetCurveLength(int curveIndex)
        {
            return m_SegmentLengthsLookupTable[curveIndex * k_SegmentResolution + k_SegmentResolution - 1].Distance;    
        }
        
        public float3 GetCurveUpVector(int index, float t)
        {
            return this.CalculateUpVector(index, t);
        }

        /// <summary>
        /// Release allocated resources.
        /// </summary>
        public void Dispose()
        {
            m_Knots.Dispose();
            m_Curves.Dispose();
            m_SegmentLengthsLookupTable.Dispose();
        }

        // Wrapper around NativeSlice<T> because the native type does not implement IReadOnlyList<T>.
        struct Slice<T> : IReadOnlyList<T> where T : struct
        {
            NativeSlice<T> m_Slice;
            public Slice(NativeArray<T> array, int start, int count) { m_Slice = new NativeSlice<T>(array, start, count); }
            public IEnumerator<T> GetEnumerator() => m_Slice.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            public int Count => m_Slice.Length;
            public T this[int index] => m_Slice[index];
        }

        /// <summary>
        /// Return the normalized interpolation (t) corresponding to a distance on a <see cref="BezierCurve"/>.
        /// </summary>
        /// <param name="curveIndex"> The zero-based index of the curve.</param>
        /// <param name="curveDistance">The curve-relative distance to convert to an interpolation ratio (also referred to as 't').</param>
        /// <returns>  The normalized interpolation ratio associated to distance on the designated curve.</returns>
        public float GetCurveInterpolation(int curveIndex, float curveDistance)
        {
            if(curveIndex <0 || curveIndex >= m_SegmentLengthsLookupTable.Length || curveDistance <= 0)
                return 0f;
            var curveLength = GetCurveLength(curveIndex);
            if(curveDistance >= curveLength)
                return 1f;
            var startIndex = curveIndex * k_SegmentResolution;
            var slice = new Slice<DistanceToInterpolation>(m_SegmentLengthsLookupTable, startIndex, k_SegmentResolution);
            return CurveUtility.GetDistanceToInterpolation(slice, curveDistance);
        }
    }
}
