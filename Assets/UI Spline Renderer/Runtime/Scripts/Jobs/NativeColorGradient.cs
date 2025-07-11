using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace UI_Spline_Renderer
{
    internal struct NativeColorGradient : IDisposable
    {
        // y is time.
        [ReadOnly]
        public NativeArray<float2> alphaKeyFrames;
        // w is time.
        [ReadOnly]
        public NativeArray<float4> colorKeyFrames;

        public Color Evaluate(float t)
        {
            var nextAlphaIdx = -1;
            for (int i = 0; i < alphaKeyFrames.Length; i++)
            {
                if(t > alphaKeyFrames[i].y) continue;
                nextAlphaIdx = i;
                break;
            }
            
            var nextColorKeyIdx = -1;
            for (int i = 0; i < colorKeyFrames.Length; i++)
            {
                if (t > colorKeyFrames[i].w) continue;
                nextColorKeyIdx = i;
                break;
            }


            
            float alpha;
            if (nextAlphaIdx == -1)
            {
                alpha = alphaKeyFrames[^1].x;
            }
            else if (nextAlphaIdx == 0)
            {
                alpha = alphaKeyFrames[0].x;
            }
            else
            {
                var preAlpha = alphaKeyFrames[nextAlphaIdx - 1];
                var nextAlpha = alphaKeyFrames[nextAlphaIdx];
                var remappedT = t.Remap(0, 1, preAlpha.y, nextAlpha.y);
                alpha = math.lerp(preAlpha, nextAlpha, remappedT).x;
            }

            
            Color color;
            if(nextColorKeyIdx == -1)
            {
                color = toColor(colorKeyFrames[^1]);
            }
            else if(nextColorKeyIdx == 0)
            {
                color = toColor(colorKeyFrames[0]);
            }
            else
            {
                var preColor = toColor(colorKeyFrames[nextColorKeyIdx - 1]);
                var nextKey = toColor(colorKeyFrames[nextColorKeyIdx]);
                var remappedT = (t - preColor.a) / (nextKey.a - preColor.a);
                color = Color.Lerp(preColor, nextKey, remappedT);
            }


            
            
            color.a = alpha;

            return color;
        }

        Color toColor(float4 f)
        {
            return new Color(f.x, f.y, f.z, f.w);
        }
        
        public void Dispose()
        {
            alphaKeyFrames.Dispose();
            colorKeyFrames.Dispose();
        }
    }
}