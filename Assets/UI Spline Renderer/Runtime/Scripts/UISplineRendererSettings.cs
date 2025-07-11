using UnityEngine;

namespace UI_Spline_Renderer
{
    public class UISplineRendererSettings : ScriptableObject
    {
        public static UISplineRendererSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<UISplineRendererSettings>("UISplineRenderer Settings");
                }

                return _instance;
            }
        }
        static UISplineRendererSettings _instance;
        public Texture defaultLineTexture;
        public Texture uvTestLineTexture;

        public Sprite triangleHead;
        public Sprite arrowHead;
        public Sprite emptyCircleHead;
        public Sprite filledCircleHead;
    }
}