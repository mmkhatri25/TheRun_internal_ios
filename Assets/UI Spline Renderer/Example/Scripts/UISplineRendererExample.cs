using System;
using System.Collections;
using System.Collections.Generic;
using UI_Spline_Renderer;
using UnityEngine;
using UnityEngine.UI;

namespace UI_Spline_Renderer.Example
{
    public class UISplineRendererExample : MonoBehaviour
    {
        public UISplineRenderer target_uvAnimation;
        public UISplineRenderer target_interaction;

        void Start()
        {
            target_interaction.GetComponent<Button>().onClick.AddListener(() => Debug.Log($"Spline Clicked !"));
        }

        void Update()
        {
            UpdateUV();
        }

        void UpdateUV()
        {
            target_uvAnimation.uvOffset += new Vector2(0, Time.deltaTime * 2);
            target_uvAnimation.clipRange = new Vector2(0, (Mathf.Sin(Time.time) + 1) * 0.5f);
        }
    }

}