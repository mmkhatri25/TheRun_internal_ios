using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace UI_Spline_Renderer
{
    [BurstCompile]
    internal static class InternalUtility
    {
        struct FrenetFrame
        {
            public float3 origin;
            public float3 tangent;
            public float3 normal;
            public float3 binormal;
        }
        const int k_NormalsPerCurve = 16;
        const float k_Epsilon = 0.0001f;
        
        // source of this methods
        // https://forum.unity.com/threads/moving-just-recttransform-pivot-in-world-space.1380249/
        // by halley
        internal static Vector3 GetPivotInWorldSpace(this RectTransform source)
        {
            var pivot = new Vector2(
                source.rect.xMin + source.pivot.x * source.rect.width,
                source.rect.yMin + source.pivot.y * source.rect.height);
            
            return source.TransformPoint(new Vector3(pivot.x, pivot.y, 0f));
        }
        
        // source of this methods
        // https://forum.unity.com/threads/moving-just-recttransform-pivot-in-world-space.1380249/
        // by halley
        internal static void SetPivotWithoutRect(this RectTransform source, Vector3 pivot)
        {
            var rect = source.rect;
            if(float.IsNaN(rect.x) || float.IsNaN(rect.y) || float.IsNaN(rect.size.x) || float.IsNaN(rect.size.y)) return;
            pivot = source.InverseTransformPoint(pivot);
            var pivot2 = new Vector2(
                (pivot.x - rect.xMin) / rect.width,
                (pivot.y - rect.yMin) / rect.height);
 
            var offset = pivot2 - source.pivot;
            offset.Scale(source.rect.size);
            var worldPos = source.position + source.TransformVector(offset);
            
            source.pivot = pivot2;
            source.position = worldPos;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static float Remap(this float value, float beforeRangeMin, float beforeRangeMax, float targetRangeMin, float targetRangeMax)
        {
            var denominator = beforeRangeMax - beforeRangeMin;
            if (denominator == 0) return targetRangeMin;
        
            var ratio = (value - beforeRangeMin) / denominator;
            var result = (targetRangeMax - targetRangeMin) * ratio + targetRangeMin;
            return result;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static float Remap(this int value, float beforeRangeMin, float beforeRangeMax, float targetRangeMin, float targetRangeMax)
        {
            return Remap((float)value, beforeRangeMin, beforeRangeMax, targetRangeMin, targetRangeMax);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static float sq(this float value) => value * value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static float LerpPoint(float value, float min, float max)
        {
            return (value - min) / (max - min);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Angle(this float3 a, float3 b)
        {
            float dot = math.dot(math.normalizesafe(a), math.normalizesafe(b));
            dot = math.clamp(dot, -1f, 1f); // 클램핑을 하지 않고 acos를 실행하면 NaN을 반환함.
            return math.degrees(math.acos(dot));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SignedAngle(this float3 a, float3 b)
        {
            var angle = math.acos(math.dot(math.normalizesafe(a), math.normalizesafe(b)));
            var cross = math.cross(a, b);
            angle *= math.sign(math.dot(math.up(), cross));
            return math.degrees(angle);
        }

        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float RadAngle(this float3 a, float3 b)
        {
            var angle = math.acos(math.dot(math.normalizesafe(a), math.normalizesafe(b)));
            var cross = math.cross(a, b);
            angle *= math.sign(math.dot(math.up(), cross));
            return angle;
        }
        
        internal static StartEndImagePreset GetCurrentStartImagePreset(Sprite target)
        {
            if (target == null)              return StartEndImagePreset.None;
            if (target == UISplineRendererSettings.Instance.triangleHead)     return StartEndImagePreset.Triangle;
            if (target == UISplineRendererSettings.Instance.arrowHead)        return StartEndImagePreset.Arrow;
            if (target == UISplineRendererSettings.Instance.emptyCircleHead)  return StartEndImagePreset.EmptyCircle;
            if (target == UISplineRendererSettings.Instance.filledCircleHead) return StartEndImagePreset.FilledCircle;

            return StartEndImagePreset.Custom;
        }

        public static float Repeat(float t, float length)
        {
            return math.clamp(t - math.floor(t / length) * length, 0.0f, length);
        }
        public static void CalculateCurveLengths(BezierCurve curve, NativeArray<DistanceToInterpolation> lookupTable)
        {
            var resolution = lookupTable.Length;

            float magnitude = 0f;
            float3 prev = CurveUtility.EvaluatePosition(curve, 0f);
            lookupTable[0] = new DistanceToInterpolation() { Distance = 0f , T = 0f };

            for (int i = 1; i < resolution; i++)
            {
                var t = i / ( resolution - 1f );
                var point = CurveUtility.EvaluatePosition(curve, t);
                var dir = point - prev;
                magnitude += math.length(dir);
                lookupTable[i] = new DistanceToInterpolation() { Distance = magnitude , T = t};
                prev = point;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int CalcVertexCount(this Spline spline, float segmentLength, float2 renderRange)
        {
            var length = spline.GetLength() * (renderRange.y - renderRange.x);
            return math.max((int)math.ceil(length * segmentLength), 1) * 2 + 4;
        }

        
        internal static int GetNearestKnotIndex(this CopiedNativeSpline spline, float t, out float distanceSq)
        {
            var p = spline.EvaluatePosition(t);
            distanceSq = float.MaxValue;
            var minIdx = -1;
            for (int i = 0; i < spline.Knots.Length; i++)
            {
                var knot = spline.Knots[i];
                var distSq = math.distancesq(knot.Position, p);
                if (distSq < distanceSq)
                {
                    minIdx = i;
                    distanceSq = distSq;
                }
            }

            return minIdx;
        }

        internal static float CircularDistance(this float a, float b, float max = 1)
        {
            var half = max * 0.5f;
            var aIsSmallSide = a < half;
            var bIsSmallSide = b < half;
            if (aIsSmallSide == bIsSmallSide)
            {
                return a > b ? a - b : b - a;
            }
            else
            {
                if (a > b)
                {
                    b += max;
                    return b - a;
                }
                else
                {
                    a += max;
                    return a - b;
                }
            }
        }

        internal static NativeColorGradient ToNative (this Gradient gradient, Allocator allocator = Allocator.TempJob)
        {
            var aKeys = new NativeArray<float2>(gradient.alphaKeys.Length, allocator);
            for (int i = 0; i < gradient.alphaKeys.Length; i++)
            {
                var key = gradient.alphaKeys[i];
                aKeys[i] = new float2(key.alpha, key.time);
            }
            
            var cKeys = new NativeArray<float4>(gradient.colorKeys.Length, allocator);
            for (int i = 0; i < gradient.colorKeys.Length; i++)
            {
                var key = gradient.colorKeys[i];
                cKeys[i] = new float4(key.color.r, key.color.g, key.color.b, key.time);
            }


            var native = new NativeColorGradient()
            {
                alphaKeyFrames = aKeys,
                colorKeyFrames = cKeys
            };

            return native;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [BurstCompile]
        internal static void ExtrudeEdge(
            float w, float V, in Color clr, ref float3 pos, in float3 tan, in float3 up, 
            bool keepBillboard, bool keepZeroZ, in float2 uvMultiplier, in float2 uvOffset,
            out UIVertex v0, out UIVertex v1)
        {
            var perpendicular =
                keepBillboard ? math.normalizesafe(math.cross(tan, new float3(0, 0, -1))) : math.normalizesafe(math.cross(tan, up));

            if (keepZeroZ)
            {
                pos.z = 0;
            }

            var uv = new float2(0, V) * uvMultiplier - uvOffset;
            var vert = new UIVertex
            {
                position = pos + perpendicular * w * 0.5f,
                uv0 = new Vector4(uv.x, uv.y),
                color = clr
            };

            v0 = vert;

            uv = new float2(1, V) * uvMultiplier - uvOffset;
            vert = new UIVertex
            {
                position = pos - perpendicular * w * 0.5f,
                uv0 = new Vector4(uv.x, uv.y),
                color = clr
            };

            v1 = vert;
        }
        internal static float3 GetExplicitLinearTangent(float3 point, float3 to)
        {
            return (to - point) / 3.0f;
        }
        internal static float3 CalculateUpVector<T>(this T spline, int curveIndex, float curveT) where T : ISpline
        {
            if (spline.Count < 1)
                return float3.zero;

            var curve = spline.GetCurve(curveIndex);

            var curveStartRotation = spline[curveIndex].Rotation;
            var curveStartUp = math.rotate(curveStartRotation, math.up());
            if (curveT == 0f)
                return curveStartUp;

            var endKnotIndex = spline.NextIndex(curveIndex);
            var curveEndRotation = spline[endKnotIndex].Rotation;
            var curveEndUp = math.rotate(curveEndRotation, math.up());
            if (curveT == 1f)
                return curveEndUp;

            var up = EvaluateUpVector(curve, curveT, curveStartUp, curveEndUp);
                
            return up;
        }

        internal static void EvaluateUpVectors(BezierCurve curve, float3 startUp, float3 endUp, Vector3[] upVectors)
        {
            upVectors[0] = startUp;
            upVectors[upVectors.Length - 1] = endUp;

            for(int i = 1; i < upVectors.Length - 1; i++)
            {
                var curveT = i / (float)(upVectors.Length - 1);
                upVectors[i] = EvaluateUpVector(curve, curveT, upVectors[0], endUp);
            }
        }
        
        internal static float3 EvaluateUpVector(BezierCurve curve, float t, float3 startUp, float3 endUp)
        {
            // Ensure we have workable tangents by linearizing ones that are of zero length
            var linearTangentLen = math.length(GetExplicitLinearTangent(curve.P0, curve.P3));
            var linearTangentOut = math.normalize(curve.P3 - curve.P0) * linearTangentLen;
            if (Approximately(math.length(curve.P1 - curve.P0), 0f))
                curve.P1 = curve.P0 + linearTangentOut;
            if (Approximately(math.length(curve.P2 - curve.P3), 0f))
                curve.P2 = curve.P3 - linearTangentOut;

            var normalBuffer = new NativeArray<float3>(k_NormalsPerCurve, Allocator.Temp);
            
            // Construct initial frenet frame
            FrenetFrame frame;
            frame.origin = curve.P0;
            frame.tangent = curve.P1 - curve.P0;
            frame.normal = startUp;
            frame.binormal = math.normalize(math.cross(frame.tangent, frame.normal));
            // SPLB-185 : If the tangent and normal are parallel, we can't construct a valid frame
            // rather than returning a value based on startUp and endUp, we return a zero vector
            // to indicate that this is not a valid up vector.
            if(float.IsNaN(frame.binormal.x))
                return float3.zero;
            
            normalBuffer[0] = frame.normal;
            
            // Continue building remaining rotation minimizing frames
            var stepSize = 1f / (k_NormalsPerCurve - 1);
            var currentT = stepSize;
            var prevT = 0f;
            var upVector = float3.zero;
            FrenetFrame prevFrame;
            for (int i = 1; i < k_NormalsPerCurve; ++i)
            {
                prevFrame = frame;
                frame = GetNextRotationMinimizingFrame(curve, prevFrame, currentT);                
                
                normalBuffer[i] = frame.normal;

                if (prevT <= t && currentT >= t)
                {
                    var lerpT = (t - prevT) / stepSize;
                    upVector = Vector3.Slerp(prevFrame.normal, frame.normal, lerpT);
                }

                prevT = currentT;
                currentT += stepSize;
            }

            if (prevT <= t && currentT >= t)
                upVector = endUp;

            var lastFrameNormal = normalBuffer[k_NormalsPerCurve - 1];

            var angleBetweenNormals = math.acos(math.clamp(math.dot(lastFrameNormal, endUp), -1f, 1f));
            if (angleBetweenNormals == 0f)
                return upVector;

            // Since there's an angle difference between the end knot's normal and the last evaluated frenet frame's normal,
            // the remaining code gradually applies the angle delta across the evaluated frames' normals.
            var lastNormalTangent = math.normalize(frame.tangent);
            var positiveRotation = quaternion.AxisAngle(lastNormalTangent, angleBetweenNormals);
            var negativeRotation = quaternion.AxisAngle(lastNormalTangent, -angleBetweenNormals);
            var positiveRotationResult = math.acos(math.clamp(math.dot(math.rotate(positiveRotation, endUp), lastFrameNormal), -1f, 1f));
            var negativeRotationResult = math.acos(math.clamp(math.dot(math.rotate(negativeRotation, endUp), lastFrameNormal), -1f, 1f));

            if (positiveRotationResult > negativeRotationResult)
                angleBetweenNormals *= -1f;

            currentT = stepSize;
            prevT = 0f;
            
            for (int i = 1; i < normalBuffer.Length; i++)
            {
                var normal = normalBuffer[i];
                var adjustmentAngle = math.lerp(0f, angleBetweenNormals, currentT);
                var tangent = math.normalize(CurveUtility.EvaluateTangent(curve, currentT));
                var adjustedNormal = math.rotate(quaternion.AxisAngle(tangent, -adjustmentAngle), normal);

                normalBuffer[i] = adjustedNormal;

                // Early exit if we've already adjusted the normals at offsets that curveT is in between
                if (prevT <= t && currentT >= t)
                {
                    var lerpT = (t - prevT) / stepSize;
                    upVector = Vector3.Slerp(normalBuffer[i - 1], normalBuffer[i], lerpT);

                    return upVector;
                }

                prevT = currentT;
                currentT += stepSize;
            }

            return endUp;
        }
        static bool Approximately(float a, float b)
        {
            // Reusing Mathf.Approximately code
            return math.abs(b - a) < math.max(0.000001f * math.max(math.abs(a), math.abs(b)), k_Epsilon * 8);
        }
        
        static FrenetFrame GetNextRotationMinimizingFrame(BezierCurve curve, FrenetFrame previousRMFrame, float nextRMFrameT)
        {
            FrenetFrame nextRMFrame;
            // Evaluate position and tangent for next RM frame
            nextRMFrame.origin = CurveUtility.EvaluatePosition(curve, nextRMFrameT);
            nextRMFrame.tangent = CurveUtility.EvaluateTangent(curve, nextRMFrameT);

            // Mirror the rotational axis and tangent
            float3 toCurrentFrame = nextRMFrame.origin - previousRMFrame.origin;
            float c1 = math.dot(toCurrentFrame, toCurrentFrame);
            float3 riL = previousRMFrame.binormal - toCurrentFrame * 2f / c1 * math.dot(toCurrentFrame, previousRMFrame.binormal);
            float3 tiL = previousRMFrame.tangent - toCurrentFrame * 2f / c1 * math.dot(toCurrentFrame, previousRMFrame.tangent);

            // Compute a more stable binormal
            float3 v2 = nextRMFrame.tangent - tiL;
            float c2 = math.dot(v2, v2);

            // Fix binormal's axis
            nextRMFrame.binormal = math.normalize(riL - v2 * 2f / c2 * math.dot(v2, riL));
            nextRMFrame.normal = math.normalize(math.cross(nextRMFrame.binormal, nextRMFrame.tangent));

            return nextRMFrame;
        }
    }
}