#if ENABLE_SPLINES
#if ENABLE_COLLECTIONS
#if ENABLE_MATHEMATICS
#if ENABLE_BURST
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

namespace UI_Spline_Renderer
{
    [BurstCompile]
    internal struct SplineExtrudeJob : IJob
    {
        [ReadOnly] public NativeSpline spline;

        [ReadOnly] public NativeCurve widthCurve;
        [ReadOnly] public float width;
        [ReadOnly] public bool keepZeroZ;
        [ReadOnly] public bool keepBillboard;
        [ReadOnly] public float2 clipRange;

        [ReadOnly] public float2 uvMultiplier;
        [ReadOnly] public float2 uvOffset;
        [ReadOnly] public UVMode uvMode;
        [ReadOnly] public Color color;
        [ReadOnly] internal NativeColorGradient colorGradient;

        [ReadOnly] public int resolution;
        [ReadOnly] public bool smooth;
        [ReadOnly] public bool roundEnds;

        public NativeList<UIVertex> vertices;
        public NativeList<int3> triangles;
        public int addedEdgeCount;

        int sampleCount => (int)(resolution * length * 0.03f);
        float v;
        float length;


        public void Execute()
        {
            if (spline.Count < 2) return;
            
            var edgePoints = new NativeList<EdgePoint>(Allocator.Temp);
            if(smooth)
            {
                SmoothSample(ref edgePoints);
                Extrude(in edgePoints);
            }
            else
            {
                UniformEvaluate(ref edgePoints);
                Extrude(in edgePoints);
            }

            if (roundEnds && ((spline.Closed && (clipRange.x > math.EPSILON || clipRange.y < 1)) || !spline.Closed))
            {
                MakeRoundEdges(in edgePoints);
            }
        }

        void SmoothSample(ref NativeList<EdgePoint> edgePoints)
        {
            var samples = new NativeList<EdgePoint>(Allocator.Temp);

            // 전체 스플라인 샘플링
            length = spline.GetLength();
            for (int i = 0; i < spline.Count; i++)
            {
                var t = spline.CurveToSplineT(i);
                var unitT = (GetWidthAt(t) / length) * 0.5f;
                var knot = spline[i];
                add_smooth_sample_point(in spline, samples, t, unitT, math.length(knot.TangentIn), math.length(knot.TangentOut));
            }


            var sampleUnitT = 1f / sampleCount;
            for (int i = 0; i < spline.Count; i++)
            {
                var t = spline.CurveToSplineT(i);

                var t_1 = spline.CurveToSplineT(i - 1);
                var t1 = spline.CurveToSplineT(i + 1);

                var leftT = math.lerp(t, t_1, 0.5f);
                var rightT = math.lerp(t, t1, 0.5f);

                var leftCount = (int)(math.abs(t - leftT) / sampleUnitT);
                var rightCount = (int)(math.abs(rightT - t) / sampleUnitT);

                if (spline.Closed)
                {
                    if(i == 0)
                    {
                        t_1 = spline.CurveToSplineT(spline.Count - 1);
                        leftT = math.lerp(1, t_1, 0.5f);
                        leftCount = (int)(math.abs(1 - leftT) / sampleUnitT);
                    }
                }
                else
                {
                    if (i == 0) leftCount = 0;
                    if (i == spline.Count - 1) rightCount = 0;
                }
                
                for (int j = 1; j < leftCount+1; j++)
                {
                    var tt = t - sampleUnitT*j;
                    if (i == 0 && spline.Closed) tt += 1;
                    add_smooth_sample_point(in spline, samples, tt, sampleUnitT, 1, 1);
                }
                for (int j = 1; j < rightCount+1; j++)
                {
                    var tt = t + sampleUnitT*j;
                    add_smooth_sample_point(in spline, samples, tt, sampleUnitT, 1, 1);
                }
            }
            
            // SortByT(ref samples);
            samples.Sort();

            // 꼭짓점 스무딩
            for (int i = 0; i < samples.Length; i++)
            {
                var sample = samples[i];
                if(sample.angle < 10) continue;
                
                // 열린 스플라인의 시작점과 끝점에선 스무딩을 안함.
                if(!spline.Closed && (i == 0 || i == samples.Length - 1)) continue;
                
                var preSampleIdx = previous_index(i, samples.Length);
                var preSample = samples[preSampleIdx];
                var nextSampleIdx = next_index(i, samples.Length);
                var nextSample = samples[nextSampleIdx];
                
                var mp0 = sample.pos;
                var mp1 = math.lerp(preSample.pos, nextSample.pos, 0.5f);
                var tanLength = (sample.tanInLength + sample.tanOutLength);
                var curvature = tanLength < math.EPSILON ? 0 : tanLength / GetWidthAt(sample.t);
                curvature = 1 - math.clamp(curvature, 0, 1);
                
                var lerp = InternalUtility.Remap(sample.angle, 0, 180, 0, curvature);
                var middlePos = math.lerp(mp0, mp1, lerp);
                
                
                sample.pos = middlePos;
                samples[i] = sample;
            }

            var knots = new NativeArray<BezierKnot>(samples.Length, Allocator.Temp);
            for (int i = 0; i < samples.Length; i++)
            {
                var sample = samples[i];
                var preSamplePos = samples[previous_index(i, samples.Length)].pos;
                var nextSamplePos = samples[next_index(i, samples.Length)].pos;
                var knot = SplineUtility.GetAutoSmoothKnot(sample.pos, preSamplePos, nextSamplePos, sample.up);
                knots[i] = knot;
            }  

            var nSpline = new CopiedNativeSpline(knots, spline.Closed, float4x4.identity);
            
            for (int i = 0; i < nSpline.Count; i++)
            {
                var knot = nSpline[i];
                var t = nSpline.CurveToSplineT(i);
                // nSpline.Evaluate(t, out var pos, out var tan, out var up);
                var tan = nSpline.EvaluateTangent(t);
                var up = keepBillboard ? new float3(0,0,-1) : nSpline.EvaluateUpVector(t);
                
                var sample = samples[i];
                sample.t = t;
                sample.pos = knot.Position;
                sample.tan = tan;
                sample.up = up;
                samples[i] = sample;
            }

            var tempEdgePoints = new NativeList<EdgePoint>(Allocator.Temp);
            // 각 세그먼트의 EdgePoint 계산
            for (int i = 0; i < samples.Length; i++)
            {
                var isClosingSegment = i == samples.Length - 1 && spline.Closed;
                
                var sample0 = samples[i];
                var sample1 = isClosingSegment ? samples[0] : samples[i + 1];
                
                // 열린 스플라인의 마지막 세그먼트에선 포인트를 추가하기만 함.
                // (length - 1)은 마지막 ep라서 열린 스플라인에서 마지막 세그먼트가 아님
                if (i == samples.Length - 2 && !spline.Closed)
                {
                    tempEdgePoints.Add(sample0);
                    tempEdgePoints.Add(sample1);
                    break;
                }
                
                var inTan = sample0.tan;
                var outTan = sample1.tan;

                var angle = InternalUtility.Angle(inTan, outTan);

                var edgePointCount = (int)math.round((angle / 30f) * 5);

                // 실제 Extrude에 사용할 EdgePoints 추가
                tempEdgePoints.Add(sample0);
                for (int j = 0; j < edgePointCount; j++)
                {
                    var t = (float)(j + 1) / (edgePointCount + 1);
                    t = t.Remap(0, 1, sample0.t, isClosingSegment ? 1 : sample1.t);
                    
                    var ep = GetEdgePoint(nSpline, t);
                    tempEdgePoints.Add(ep);
                }
                if(i == samples.Length - 1 && spline.Closed) tempEdgePoints.Add(sample1);
            }

            var clipStartAdded = false;
            var clipEndAdded = false;
            var shouldClipping = clipRange.x > math.EPSILON || clipRange.y < 1;
            for (int i = 0; i < tempEdgePoints.Length; i++)
            {
                var point = tempEdgePoints[i];
                if (spline.Closed && shouldClipping && point.t == 0 && clipRange.y < 1)
                {
                    continue;
                }
                
                if(shouldClipping && (point.t < clipRange.x || point.t > clipRange.y)) continue;
                if (point.t == clipRange.x)
                {
                    edgePoints.Add(point);
                    clipStartAdded = true;
                    continue;
                }
                if (point.t == clipRange.y)
                {
                    edgePoints.Add(point);
                    clipEndAdded = true;
                    continue;
                }

                if (clipRange.x < point.t && point.t < clipRange.y)
                {
                    if(!clipStartAdded)
                    {
                        var ep = GetEdgePoint(nSpline, clipRange.x);
                        edgePoints.Add(ep);
                        clipStartAdded = true;
                    }
                    else
                    {
                        edgePoints.Add(point);
                    }
                }
            }
            
            if (!clipEndAdded)
            {
                var ep = GetEdgePoint(nSpline, clipRange.y);
                edgePoints.Add(ep);
            }
        }

        static void SortByT(ref NativeList<EdgePoint> samples)
        {
            for (int i = 0; i < samples.Length - 1; i++)
            {
                for (int j = i + 1; j < samples.Length; j++)
                {
                    if (samples[i].t > samples[j].t)
                    {
                        // 두 객체의 위치를 교환
                        (samples[i], samples[j]) = (samples[j], samples[i]);
                    }
                }
            }
        }
        
        void add_smooth_sample_point(in NativeSpline s, NativeList<EdgePoint> samples, float t, float unitT, float tanInLength, float tanOutLength)
        {
            var preT = InternalUtility.Repeat(t - unitT, 1);
            var preTTan = s.EvaluateTangent(preT);

            var nextT = InternalUtility.Repeat(t + unitT, 1);
            var nextTTan = s.EvaluateTangent(nextT);

            var angle = InternalUtility.Angle(preTTan, nextTTan);

            var ep = GetEdgePoint(s, t, angle, tanInLength, tanOutLength);
            samples.Add(ep);
        }
        int previous_index(int i, int maxLength)
        {
            if (i == 0)
            {
                if (spline.Closed) return maxLength - 1;
                return 0;
            }

            return i - 1;
        }
        int next_index(int i, int maxLength)
        {
            if (i == maxLength - 1)
            {
                if (spline.Closed) return 0;
                return i;
            }

            return i + 1;
        }

        bool is_used(float t, float unitT, in NativeList<EdgePoint> list)
        {
            for (int i = 0; i < list.Length; i++)
            {
                var diff = t.CircularDistance(list[i].t, list.Length - 1);
                if (diff < unitT) return true;
            }

            return false;
        }
        void UniformEvaluate(ref NativeList<EdgePoint> edgePoints)
        {
            length = spline.GetLength();
            
            var clippedLength = length * (clipRange.y - clipRange.x);
            var edgeCount = math.max((int)math.ceil(clippedLength * resolution * 0.05f), 1) + 2;

            
            for (int i = 0; i < edgeCount; i++)
            {
                var t = (float)i / (edgeCount - 1);
                t = t.Remap(0, 1, clipRange.x, clipRange.y);
                var ep = GetEdgePoint(spline, t); 
                edgePoints.Add(ep);
            }
        }

        EdgePoint GetEdgePoint(in NativeSpline s, float t, float angle = 0, float tanInLength = 0, float tanOutLength = 0)
        {
            if (keepBillboard)
            {
                var pos = s.EvaluatePosition(t);
                var tan = s.EvaluateTangent(t);
                return new EdgePoint(t, angle, pos, tan, new float3(0,0,-1), tanInLength, tanOutLength);
            }
            else
            {
                s.Evaluate(t, out var pos, out var tan, out var up);
                
                return new EdgePoint(t, angle, pos, tan, up, tanInLength, tanOutLength);
            }
        }
        EdgePoint GetEdgePoint(in CopiedNativeSpline s, float t)
        {
            if (keepBillboard)
            {
                var pos = s.EvaluatePosition(t);
                var tan = s.EvaluateTangent(t);
                return new EdgePoint(t, 0, pos, tan, new float3(0,0,-1));
            }
            else
            {
                s.Evaluate(t, out var pos, out var tan, out var up);
                
                return new EdgePoint(t, 0, pos, tan, up);
            }
        }

        void Extrude(in NativeList<EdgePoint> edgePoints)
        {
            for (int i = 0; i < edgePoints.Length; i++)
            {
                var ep = edgePoints[i];
                var t = ep.t;
                var pos = ep.pos;
                var tan = ep.tan;
                var up = ep.up;
                
                // resolve (0,0,0) tangent
                if (tan is { x: 0, y: 0 })
                {
                    var prev = i == 0 ? pos : edgePoints[^2].pos;
                    var next = i == edgePoints.Length - 1 ? pos : edgePoints[i + 1].pos;
                    tan = next - prev;
                }
                

                InternalUtility.ExtrudeEdge(
                    GetWidthAt(t), GetVAt(t, i), GetColorAt(t), ref pos, tan, up,
                    keepBillboard, keepZeroZ, uvMultiplier, uvOffset, out var v0, out var v1);
                
                AddVert(in v0, in v1);
                
                if (i > 0)
                {
                    AddQuadUsingLastVertices();
                }
            }
        }

        void AddVert(in UIVertex left, in UIVertex right)
        {
            vertices.Add(left);
            vertices.Add(right);
        }

        void AddQuadUsingLastVertices()
        {
            var vi = vertices.Length;
            triangles.Add(new int3
            (
                vi - 2,
                vi - 3,
                vi - 4
            ));
            triangles.Add(new int3
            (
                vi - 2,
                vi - 1,
                vi - 3
            ));
        }
        
        
        float GetWidthAt(float t)
        {
            return width * widthCurve.Evaluate(t);
        }

        Color GetColorAt(float t)
        {
            return color * colorGradient.Evaluate(t);
        }

        float GetVAt(float t, int i)
        {
            switch (uvMode)
            {
                case UVMode.Tile:
                    return length / width * t;
                case UVMode.RepeatPerSegment:
                    return i;
                case UVMode.Stretch:
                    return t;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        void MakeRoundEdges(in NativeList<EdgePoint> edgePoints)
        {
            var sl = vertices[0];
            var sr = vertices[1];
            
            var el = vertices[^2];
            var er = vertices[^1];
            {
                // at start
                var start = edgePoints[0];
                var t = start.t;
                var pos = start.pos;
                var tan = start.tan;
                var up = start.up;
                
                var V = GetVAt(t, 0);
                var uv = new float2(0, V) * uvMultiplier - uvOffset;
                var clr = GetColorAt(t);
                
                MakeRoundEdge(in pos, sl.position, sr.position, false, up, uv, clr);
            }
            
            {
                // at end
                var end = edgePoints[^1];
                var t = end.t;
                var pos = end.pos;
                var tan = end.tan;
                var up = end.up;
                
                var V = GetVAt(t, edgePoints.Length - 1);
                var uv = new float2(0, V) * uvMultiplier - uvOffset;
                var clr = GetColorAt(t);
                
                MakeRoundEdge(in pos, el.position, er.position, true, up, uv, clr);
            }

            
        }
        void MakeRoundEdge(in float3 center, in float3 left, in float3 right, bool invert,
            in float3 up, in float2 uv, in Color clr)
        {
            var vertexCount = resolution + 7;
            var marginAngle = 180f / vertexCount;
            var radius = math.length(center - left);
            var axis = keepBillboard ? math.back() : up;
            var arm = invert ? left - center : right - center;
            
            var centerVertex = new UIVertex();
            centerVertex.position = center;
            centerVertex.uv0 = new Vector4(0.5f, 0);
            centerVertex.color = clr;
            vertices.Add(centerVertex);

            var startIndex = vertices.Length - 1;

            var toRadians =
#if ENABLE_MATHMATICS__1_3_1_OR_NEWER
                math.TORADIANS;
#else
                Mathf.Deg2Rad;
#endif
            for (int i = 0; i <= vertexCount; i++)
            {
                var vector = math.rotate(quaternion.AxisAngle(axis, marginAngle * i * toRadians), arm);
                vector = math.normalizesafe(vector);
                var ratio = (float)i / vertexCount;
                var vert = new UIVertex
                {
                    position = center + vector * radius,
                    uv0 = new Vector4(0, ratio),
                    color = clr
                };
                vertices.Add(vert);
                if(i > 0) triangles.Add(new int3(startIndex, startIndex + i, startIndex + i + 1));
            }
        }
    }

    [BurstCompile]
    public struct EdgePoint : IComparable<EdgePoint>
    {
        public float t;
        public readonly float angle;
        public float3 pos;
        public float3 tan;
        public float3 up;
        public readonly float tanInLength;
        public readonly float tanOutLength;
        public bool smoothed;

        public EdgePoint(float t, float angle, float3 pos, float3 tan, float3 up, float tanInLength = 0, float tanOutLength = 0)
        {
            this.t = t;
            this.pos = pos;
            this.tan = tan;
            this.up = up;
            this.angle = angle;
            this.tanInLength = tanInLength;
            this.tanOutLength = tanOutLength;
            smoothed = false;
        }

        public int CompareTo(EdgePoint other)
        {
            return t.CompareTo(other.t);
        }
    }
}
#endif
#endif
#endif
#endif