using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
#if !UNITY_EDITOR && !UNITY_WEBGL
using UnityEngine.Scripting;
#endif
using UnityEngine.Profiling;
using UnityEngine.Serialization; // 이 라인을 지우지 말 것.
using UnityEngine.Splines;
using UnityEngine.UI;
using Math = System.Math;
#if LETAI_TRUESHADOW
using LeTai.TrueShadow;
using LeTai.TrueShadow.PluginInterfaces;
#endif

[assembly: InternalsVisibleTo("UISplineRenderer.Editor", AllInternalsVisible = true)]

namespace UI_Spline_Renderer
{
    public enum UVMode
    {
        Tile,
        RepeatPerSegment,
        Stretch
    }

    public enum OffsetMode
    {
        Distance,
        Normalized
    }

    [RequireComponent(typeof(CanvasRenderer))]
    [ExecuteInEditMode]
    public class UISplineRenderer : MaskableGraphic
#if LETAI_TRUESHADOW
        , ITrueShadowCustomHashProvider
#endif
    {

        public LineTexturePreset lineTexturePreset
        {
            get
            {
                if (m_Texture == UISplineRendererSettings.Instance.defaultLineTexture) return LineTexturePreset.Default;
                if (m_Texture == UISplineRendererSettings.Instance.uvTestLineTexture) return LineTexturePreset.UVTest;
                return LineTexturePreset.Custom;
            }
            set
            {
                switch (value)
                {
                    case LineTexturePreset.Default:
                        texture = UISplineRendererSettings.Instance.defaultLineTexture;
                        break;
                    case LineTexturePreset.UVTest:
                        texture = UISplineRendererSettings.Instance.uvTestLineTexture;
                        break;
                    case LineTexturePreset.Custom:
                        Debug.LogWarning("[UI Spline Renderer] If you want to change the line texture, " +
                                         "just set value to the \"texture\" property. " +
                                         "Then It will be automatically changed to LineTexturePreset.Custom");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(value), value, null);
                }
            }
        }

        public StartEndImagePreset startImagePreset
        {
            get => InternalUtility.GetCurrentStartImagePreset(startImageSprite);

            set
            {
                switch (value)
                {
                    case StartEndImagePreset.None:
                        startImageSprite = null;
                        break;
                    case StartEndImagePreset.Triangle:
                        startImageSprite = UISplineRendererSettings.Instance.triangleHead;
                        break;
                    case StartEndImagePreset.Arrow:
                        startImageSprite = UISplineRendererSettings.Instance.arrowHead;
                        break;
                    case StartEndImagePreset.EmptyCircle:
                        startImageSprite = UISplineRendererSettings.Instance.emptyCircleHead;
                        break;
                    case StartEndImagePreset.FilledCircle:
                        startImageSprite = UISplineRendererSettings.Instance.filledCircleHead;
                        break;
                    case StartEndImagePreset.Custom:
                        Debug.LogWarning("[UI Spline Renderer] If you want to change the start image, " +
                                         "just set value to the \"startImageSprite\" property. " +
                                         "Then It will be automatically changed to StartEndImagePreset.Custom");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(value), value, null);
                }
            }
        }

        public StartEndImagePreset endImagePreset
        {
            get => InternalUtility.GetCurrentStartImagePreset(endImageSprite);

            set
            {
                switch (value)
                {
                    case StartEndImagePreset.None:
                        endImageSprite = null;
                        break;
                    case StartEndImagePreset.Triangle:
                        endImageSprite = UISplineRendererSettings.Instance.triangleHead;
                        break;
                    case StartEndImagePreset.Arrow:
                        endImageSprite = UISplineRendererSettings.Instance.arrowHead;
                        break;
                    case StartEndImagePreset.EmptyCircle:
                        endImageSprite = UISplineRendererSettings.Instance.emptyCircleHead;
                        break;
                    case StartEndImagePreset.FilledCircle:
                        endImageSprite = UISplineRendererSettings.Instance.filledCircleHead;
                        break;
                    case StartEndImagePreset.Custom:
                        Debug.LogWarning("[UI Spline Renderer] If you want to change the end image, " +
                                         "just set value to the \"endImageSprite\" property. " +
                                         "Then It will be automatically changed to StartEndImagePreset.Custom");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(value), value, null);
                }
            }
        }

        public override Color color
        {
            get => base.color;
            set
            {
                base.color = value;
                if (recursiveColor)
                {
                    UpdateGraphicColors();
                    UpdateTrueShadowCustomHash();
                }
            }
        }

        public bool recursiveColor
        {
            get => _recursiveColor;
            set
            {
                _recursiveColor = value;
                if (true)
                {
                    UpdateGraphicColors();
                    UpdateTrueShadowCustomHash();
                }
            }
        }

        public int resolution
        {
            get => _resolution;
            set
            {
                value = Mathf.Clamp(value, 1, 20);

                _resolution = value;
                _needToResample = true;
                SetVerticesDirty();
                UpdateTrueShadowCustomHash();
            }
        }

        public override Texture mainTexture => m_Texture == null ? s_WhiteTexture : m_Texture;

        public Texture texture
        {
            get => m_Texture;
            set
            {
                if (value != null && m_Texture == value) return;
                m_Texture = value;
                SetMaterialDirty();
                UpdateTrueShadowCustomHash();
            }
        }

        public Sprite startImageSprite
        {
            get => _startImageSprite;
            set
            {
                _startImageSprite = value;
                UpdateStartEndImages(true);
            }
        }

        public Sprite endImageSprite
        {
            get => _endImageSprite;
            set
            {
                _endImageSprite = value;
                UpdateStartEndImages(false);
            }
        }

        public int vertexCount => _vh?.currentVertCount ?? 0;


        public float startImageSize
        {
            get => _startImageSize;
            set
            {
                var isNew = Math.Abs(_startImageSize - value) > 0.0001f;
                if (isNew)
                {
                    _startImageSize = value;
                    UpdateStartEndImages(true);
                }
            }
        }

        public OffsetMode startImageOffsetMode
        {
            get => _startImageOffsetMode;
            set
            {
                var isNew = _startImageOffsetMode != value;
                if (isNew)
                {
                    _startImageOffsetMode = value;
                    UpdateStartEndImages(true);
                }
            }
        }

        public float startImageOffset
        {
            get => _startImageOffset;
            set
            {
                var isNew = Math.Abs(_startImageOffset - value) > 0.0001f;
                if (isNew)
                {
                    _startImageOffset = value;
                    UpdateStartEndImages(true);
                }
            }
        }
        public float normalizedStartImageOffset
        {
            get => _normalizedStartImageOffset;
            set
            {
                var isNew = Math.Abs(_normalizedStartImageOffset - value) > 0.0001f;
                if (isNew)
                {
                    _normalizedStartImageOffset = value;
                    UpdateStartEndImages(true);
                }
            }
        }

        public float endImageSize
        {
            get => _endImageSize;
            set
            {
                var isNew = Math.Abs(_endImageSize - value) > 0.0001f;
                if (isNew)
                {
                    _endImageSize = value;
                    UpdateStartEndImages(false);
                }
            }
        }
        public OffsetMode endImageOffsetMode
        {
            get => _endImageOffsetMode;
            set
            {
                var isNew = _endImageOffsetMode != value;
                if (isNew)
                {
                    _endImageOffsetMode = value;
                    UpdateStartEndImages(false);
                }
            }
        }

        public float endImageOffset
        {
            get => _endImageOffset;
            set
            {
                var isNew = Math.Abs(_endImageOffset - value) > 0.0001f;
                if (isNew)
                {
                    _endImageOffset = value;
                    UpdateStartEndImages(false);
                }
            }
        }
        public float normalizedEndImageOffset
        {
            get => _normalizedEndImageOffset;
            set
            {
                var isNew = Math.Abs(_normalizedEndImageOffset - value) > 0.0001f;
                if (isNew)
                {
                    _normalizedEndImageOffset = value;
                    UpdateStartEndImages(false);
                }
            }
        }

        public bool recursiveMaterial
        {
            get => _recursiveMaterial;
            set
            {
                _recursiveMaterial = value;
                if (value)
                {
                    ManipulateOtherGraphics(x => x.material = material);
                    UpdateTrueShadowCustomHash();
                }
            }
        }

        public override Material material
        {
            get => base.material;
            set
            {
                base.material = value;
                if (recursiveMaterial)
                {
                    ManipulateOtherGraphics(x => x.material = value);
                    UpdateTrueShadowCustomHash();
                }
            }
        }
        public bool smooth
        {
            get => _smooth;
            set
            {
                _smooth = value;
                _needToResample = true;
                SetVerticesDirty();
                UpdateTrueShadowCustomHash();
            }
        }
        public bool roundEnds
        {
            get => _roundEnds;
            set
            {
                _roundEnds = value;
                _needToResample = true;
                SetVerticesDirty();
                UpdateTrueShadowCustomHash();
            }
        }


        public bool keepZeroZ
        {
            get => _keepZeroZ;
            set
            {
                _keepZeroZ = value;
                _needToResample = true;
                SetVerticesDirty();
                UpdateTrueShadowCustomHash();
            }
        }

        public bool keepBillboard
        {
            get => _keepBillboard;
            set
            {
                _keepBillboard = value;
                _needToResample = true;
                SetVerticesDirty();
                UpdateTrueShadowCustomHash();
            }
        }

        public float width
        {
            get => _width;
            set
            {
                _width = value;
                _needToResample = true;
                SetVerticesDirty();
                UpdateTrueShadowCustomHash();
            }
        }

        public UVMode uvMode
        {
            get => _uvMode;
            set
            {
                _uvMode = value;
                SetMaterialDirty();
                UpdateTrueShadowCustomHash();
            }
        }

        public Vector2 uvMultiplier
        {
            get => _uvMultiplier;
            set
            {
                _uvMultiplier = value;
                SetMaterialDirty();
                UpdateTrueShadowCustomHash();
            }
        }

        public Vector2 uvOffset
        {
            get => _uvOffset;
            set
            {
                _uvOffset = value;
                SetVerticesDirty();
                SetMaterialDirty();
                UpdateTrueShadowCustomHash();
            }
        }

        public Vector2 clipRange
        {
            get => _clipRange;
            set
            {
                _clipRange = value;
                _needToResample = true;
                SetVerticesDirty();
                UpdateTrueShadowCustomHash();
            }
        }


        public SplineContainer splineContainer
        {
            get
            {
                if (_splineContainer == null) _splineContainer = GetComponent<SplineContainer>();
                return _splineContainer;
            }
            set
            {
                var isNew = _splineContainer != value;
                _splineContainer = value;
                if (isNew)
                {
                    SetAllDirty();
                    UpdateRaycastTargetRect();
                }
            }
        }
        public List<Image> startImages = new();
        public List<Image> endImages = new();

        [Tooltip("Splines to render. \n\nIf you want to have multiple UISplineRenderers in one SplineContainer, create a child object under the SplineContainer object, then reference the SplineContainer in this property.")]
        [FormerlySerializedAs("splineContainer")][SerializeField] SplineContainer _splineContainer;
        [Tooltip("Color tint multiplied to the color. 0 is the first point and 1 is the  last point.")]
        [SerializeField] Gradient _colorGradient = new Gradient();
        [Tooltip("Multiplier for the width. 0 is the first point and 1 is the last point.")]
        [SerializeField] AnimationCurve _widthCurve = AnimationCurve.Linear(0, 1, 1, 1);


        [Tooltip("If the fitPosition field is set to true, each renderer will be aligned exactly to the spline's position, and if it's false, they will be rendered at the position of each renderer on the spline.\nIf the SplineContainer and UISplineRenderer are on the same object, this field has no effect.")]
        [SerializeField] bool _fitPosition;
        [SerializeField] bool _smooth;
        [SerializeField] bool _roundEnds;
        [Tooltip("Keep all z positions and transform.localPosition.z as 0. This keeps the spline flatten on canvas.")]
        [SerializeField] bool _keepZeroZ = true;
        [Tooltip("Keep all normals of the vertices pointing towards the screen direction. This ensures that the spline is rendered as a billboard, even if you set odd rotations to knots")]
        [SerializeField] bool _keepBillboard = true;
        [SerializeField] float _width = 10;
        [Tooltip("Line texture’s UV mode. Texture is applied in the Y-direction.\n\nTile\t\nTexture is tiled along spline length. The wrap mode of the texture should \tbe Repeat.\n\nRepeat Per Segment\t\nTexture is placed per segment(spline knot). The wrap mode of the texture should be Repeat.\n\nStretch\t\nTexture is stretched along splines.")]
        [SerializeField] UVMode _uvMode;
        [SerializeField] Vector2 _uvMultiplier = new Vector2(1, 1);
        [SerializeField] Vector2 _uvOffset;
        [Tooltip("Range to render splines.")]
        [SerializeField] Vector2 _clipRange = new Vector2(0, 1);

        [Tooltip("If true, this renderer's maskable value will also be applied to startImages and endImages.")]
        [SerializeField] bool _recursiveMaterial = true;
        [SerializeField] bool _recursiveColor = true;
        [SerializeField] int _resolution = 5;
        [SerializeField] Texture m_Texture;
        [SerializeField] Sprite _startImageSprite;
        [SerializeField] Sprite _endImageSprite;
        [SerializeField] float _startImageSize = 32;
        [Tooltip("How to calculate the offset of start images. In the case of Distance, the absolute value of the length is used as the offset. When Normalized, an offset expressed as 0..1 is used.")]
        [SerializeField] OffsetMode _startImageOffsetMode;
        [Tooltip("Offset from the first point of spline. If the value is greater than 0, it moves along the spline. Otherwise, it moves backward relative to current rotation.")]
        [SerializeField] float _startImageOffset;
        [Tooltip("Normalized offset value. 0 means the beginning of the spline, and 1 means the end of the spline.")]
        [SerializeField] float _normalizedStartImageOffset;
        [SerializeField] float _endImageSize = 32;
        [Tooltip("How to calculate the offset of end images. In the case of Distance, the absolute value of the length is used as the offset. When Normalized, an offset expressed as 0..1 is used.")]
        [SerializeField] OffsetMode _endImageOffsetMode;
        [Tooltip("Offset from the last point of the spline. If the value is smaller than 0, it moves along the spline. Otherwise, it moves forward relative to current rotation.")]
        [SerializeField] float _endImageOffset;
        [Tooltip("Normalized offset value. 0 means the beginning of the spline, and 1 means the end of the spline.")]
        [SerializeField] float _normalizedEndImageOffset = 1;
        [SerializeField] bool _defaultTextureInitialized;

#pragma warning disable CS0414
        bool _needToResample;
#pragma warning restore CS0414


        VertexHelper _vh;
        HashSet<IDisposable> _disposables = new HashSet<IDisposable>();
#if LETAI_TRUESHADOW
        TrueShadow _trueShadow;
#endif

        #region override Methods

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            if (_splineContainer == null) _splineContainer = GetComponent<SplineContainer>();
            SetVerticesDirty();
            SetMaterialDirty();
            UpdateTrueShadowCustomHash();
            UpdateGraphicColors();
        }
#endif


        protected override void OnEnable()
        {
            base.OnEnable();
            Spline.Changed += OnSplineChanged;
            SplineContainer.SplineAdded += OnSplineAddedOrRemoved;
            SplineContainer.SplineRemoved += OnSplineAddedOrRemoved;

            if (!_defaultTextureInitialized && m_Texture == null)
            {
                m_Texture = UISplineRendererSettings.Instance.defaultLineTexture;
            }

            SetVerticesDirty();
            SetMaterialDirty();
        }

        protected override void Start()
        {
#if UNITY_EDITOR
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null && prefabStage.IsPartOfPrefabContents(gameObject))
            {
                UpdateRaycastTargetRect();
            }
#endif
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            Spline.Changed -= OnSplineChanged;
            SplineContainer.SplineAdded -= OnSplineAddedOrRemoved;
            SplineContainer.SplineRemoved -= OnSplineAddedOrRemoved;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            var startImageCount = startImages.Count;
            for (int i = 0; i < startImageCount; i++)
            {
                if (startImages[i] != null)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(startImages[i].gameObject);
                    }
                    else
                    {
                        DestroyImmediate(startImages[i].gameObject);
                    }
                }
            }

            var endImageCount = endImages.Count;
            for (int i = 0; i < endImageCount; i++)
            {
                if (endImages[i] != null)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(endImages[i].gameObject);
                    }
                    else
                    {
                        DestroyImmediate(endImages[i].gameObject);
                    }
                }
            }
        }


        public override void SetMaterialDirty()
        {
            if (splineContainer == null) return;
            base.SetMaterialDirty();
            ManipulateOtherGraphics(x => x.maskable = maskable);
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            Profiler.BeginSample("UISplineRenderer.OnPopulateMesh", this);
            _vh ??= vh;
            vh.Clear();
            DoExtrudeSplineJobAll();
            Profiler.EndSample();
        }

        protected override void OnDidApplyAnimationProperties()
        {
            base.OnDidApplyAnimationProperties();
            UpdateStartEndImages(true);
            UpdateStartEndImages(false);
        }

        public override bool Raycast(Vector2 sp, Camera eventCamera)
        {
            if (!base.Raycast(sp, eventCamera)) return false;
            return SplineRaycast(sp, eventCamera);
        }

        bool SplineRaycast(Vector2 sp, Camera eventCamera)
        {
            Vector3 point = default;
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                var flatten = new Vector3(sp.x, sp.y, transform.position.z);
                point = transform.InverseTransformPoint(flatten);
            }
            else
            {
                var wp = eventCamera.ScreenToWorldPoint(new Vector3(sp.x, sp.y, transform.position.z - eventCamera.transform.position.z));
                point = transform.InverseTransformPoint(wp);
                point.z = 0;
            }



            foreach (var spline in splineContainer.Splines)
            {
                var distance = SplineUtility.GetNearestPoint(spline, point, out var nearest, out var t);
                if (distance <= GetWidthAt(t))
                {
                    return true;
                }
            }

            return false;
        }


        public override void CrossFadeAlpha(float alpha, float duration, bool ignoreTimeScale)
        {
            base.CrossFadeAlpha(alpha, duration, ignoreTimeScale);

            ManipulateOtherGraphics(x => x.CrossFadeAlpha(alpha, duration, ignoreTimeScale));
        }

        public override void CrossFadeColor(Color targetColor, float duration, bool ignoreTimeScale, bool useAlpha)
        {
            base.CrossFadeColor(targetColor, duration, ignoreTimeScale, useAlpha);
            ManipulateOtherGraphics(x => x.CrossFadeColor(targetColor, duration, ignoreTimeScale, useAlpha));
        }

        public override void CrossFadeColor(Color targetColor, float duration, bool ignoreTimeScale, bool useAlpha, bool useRGB)
        {
            base.CrossFadeColor(targetColor, duration, ignoreTimeScale, useAlpha, useRGB);
            ManipulateOtherGraphics(x => x.CrossFadeColor(targetColor, duration, ignoreTimeScale, useAlpha, useRGB));
        }


        #endregion

        void OnSplineChanged(Spline spline, int knotIndex, SplineModification modification)
        {
            if (spline == null) return;
            var isMySpline = false;
            if (splineContainer == null) return;
            for (int i = 0; i < splineContainer.Splines.Count; i++)
            {
                if (splineContainer.Splines[i] == spline)
                {
                    isMySpline = true;

                    if (modification == SplineModification.KnotModified)
                    {
                        var knot = spline[knotIndex];
                        var pos = knot.Position;
                        if (keepZeroZ) knot.Position = new float3(pos.x, pos.y, 0);

                        spline.SetKnotNoNotify(knotIndex, knot);
                    }

                    break;
                }
            }

            if (!isMySpline) return;
            if (keepZeroZ) transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, 0);


            SetVerticesDirty();
            SetMaterialDirty();

            UpdateRaycastTargetRect();

            UpdateTrueShadowCustomHash();

        }

        void OnSplineAddedOrRemoved(SplineContainer container, int i)
        {
            if (container != splineContainer) return;
            SetVerticesDirty();
            SetMaterialDirty();

            UpdateRaycastTargetRect();

            UpdateTrueShadowCustomHash();

        }

        public void UpdateRaycastTargetRect()
        {
            if (splineContainer == null) return;
            if (splineContainer.Splines.Count == 0) return;
            if (splineContainer.Splines.Count == 1 && splineContainer.Spline.Count < 2) return;

            if (_fitPosition && splineContainer.transform != transform)
            {
                transform.position = splineContainer.transform.position;
            }

            var p0 = splineContainer.EvaluatePosition(0);
            var scale = transform.lossyScale.x;
            var bounds = new Bounds(p0, Vector3.one * (GetWidthAt(0) * scale));

            for (int i = 0; i < splineContainer.Splines.Count; i++)
            {
                var spline = splineContainer[i];

                var sampleCount = vertexCount / 2;
                for (int j = 0; j < sampleCount; j++)
                {
                    var t = (float)j / (sampleCount - 1);
                    var p = splineContainer.EvaluatePosition(spline, t);
                    var b = new Bounds(p, Vector3.one * (GetWidthAt(t) * scale));

                    bounds.Encapsulate(b);
                }
            }

            if (splineContainer.Splines.Count == 1)
            {
                var p1 = splineContainer.EvaluatePosition(1);
                var b1 = new Bounds(p1, Vector3.one * (GetWidthAt(1) * scale));
                bounds.Encapsulate(b1);
            }
            if (float.IsNaN(bounds.center.x) || float.IsNaN(bounds.center.y) || float.IsNaN(bounds.center.z) ||
                float.IsNaN(bounds.size.x) || float.IsNaN(bounds.size.y) || float.IsNaN(bounds.size.z)) return;
            var pos = transform.position;
            bounds.center = new Vector3(bounds.center.x, bounds.center.y, pos.z);
            {
                var a = bounds.size;
                var b = canvas.transform.lossyScale;
                bounds.size = new Vector3(a.x / b.x, a.y / b.y, 1);
            }
            var parent = rectTransform.parent as RectTransform;

            rectTransform.sizeDelta = bounds.size;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.position = bounds.center;
            rectTransform.SetPivotWithoutRect(pos);

            // 이 쓸모없어보이는 라인을 지우지 말 것.
            // 유니티의 RectTransform은 프로퍼티를 읽을때 초기화되는 병신같은 기믹이 있어서
            // 이 라인을 지우면 오브젝트를 복제할때 이 게임오브젝트의 위치가 변경됨 
            var r = parent.rect;

            UpdateStartEndImages(true);
            UpdateStartEndImages(false);
        }

        public void UpdateTrueShadowCustomHash()
        {
#if LETAI_TRUESHADOW
            if (_trueShadow == null) _trueShadow = GetComponent<TrueShadow>();
            if (_trueShadow == null) return;

            var knotCountToHash = 0;
            var verticesToHash = 0;
            for (int i = 0; i < splineContainer.Splines.Count; i++)
            {
                var spline = splineContainer.Splines[i];
                knotCountToHash += spline.Count;
                for (int j = 0; j < spline.Count; j++)
                {
                    var knot = spline[j];
                    verticesToHash += knot.Position.GetHashCode();
                    verticesToHash += knot.Rotation.GetHashCode();
                    verticesToHash += knot.TangentIn.GetHashCode();
                    verticesToHash += knot.TangentOut.GetHashCode();
                }
            }

            _trueShadow.CustomHash = LeTai.HashUtils.CombineHashCodes(
                texture == null ? 0 : texture.GetHashCode(),
                material == null ? 0 : material.GetHashCode(),
                color.GetHashCode(),
                _colorGradient.GetHashCode(),
                uvOffset.GetHashCode(),
                uvMultiplier.GetHashCode(),
                knotCountToHash,
                verticesToHash
            );
#endif
        }

        void DoExtrudeSplineJobAll()
        {
            if (splineContainer == null)
            {
                // 그리기 과정에서 콜백으로 인해 루프에 빠지는 것을 방지하기 위해 꼭 _splineContainer에 값을 넣어야함.
                _splineContainer = GetComponent<SplineContainer>();
            }
            if (splineContainer == null) return;
            var splineCount = splineContainer.Splines.Count;
            if (splineCount <= 0) return;
            if (width == 0) return;
#if !UNITY_EDITOR && !UNITY_WEBGL
            GarbageCollector.GCMode = GarbageCollector.Mode.Disabled;      
#endif
            var vertices = new NativeList<UIVertex>(Allocator.TempJob);
            var triangles = new NativeList<int3>(Allocator.TempJob);
            _disposables.Add(vertices);
            _disposables.Add(triangles);

            var gradient = _colorGradient.ToNative();
            _disposables.Add(gradient);

            var widthCurve = new NativeCurve(_widthCurve, Allocator.TempJob);
            _disposables.Add(widthCurve);


            var handle = DoExtrudeSplinesJob(widthCurve, gradient, vertices, triangles);


            handle.Complete();
            for (int i = 0; i < vertices.Length; i++)
            {
                _vh.AddVert(vertices[i]);
            }

            for (int i = 0; i < triangles.Length; i++)
            {
                var tri = triangles[i];
                _vh.AddTriangle(tri.x, tri.y, tri.z);
            }


            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }
            _disposables.Clear();
            _needToResample = false;
#if !UNITY_EDITOR && !UNITY_WEBGL
            GarbageCollector.GCMode = GarbageCollector.Mode.Enabled;
#endif
        }


        JobHandle DoExtrudeSplinesJob(
            NativeCurve widthCurve, NativeColorGradient gradient,
            NativeList<UIVertex> vertices, NativeList<int3> triangles)
        {
            JobHandle handle = default;
            for (int i = 0; i < splineContainer.Splines.Count; i++)
            {
                var nSpline = new NativeSpline(splineContainer[i], Allocator.TempJob);
                _disposables.Add(nSpline);
                var job = new SplineExtrudeJob
                {
                    spline = nSpline,
                    widthCurve = widthCurve,
                    resolution = resolution,
                    smooth = smooth,
                    keepBillboard = keepBillboard,
                    keepZeroZ = keepZeroZ,
                    roundEnds = roundEnds,
                    clipRange = clipRange,
                    uvMultiplier = uvMultiplier,
                    uvOffset = uvOffset,
                    color = color,
                    colorGradient = gradient,
                    uvMode = uvMode,
                    width = width,
                    vertices = vertices,
                    triangles = triangles
                };
                handle = job.Schedule(handle);
            }

            return handle;
        }


        void UpdateGraphicColors()
        {
            if (splineContainer == null) return;
            if (splineContainer.Splines.Count == 0) return;
            if (splineContainer.Splines.Count == 1 && splineContainer.Spline.Count < 2) return;

            if (!_recursiveColor) return;

            var startImageOffsetT = _startImageOffset / splineContainer.CalculateLength();
            for (int i = 0; i < startImages.Count; i++)
            {
                startImages[i].color = GetColorAt(startImageOffsetT);
            }

            var endImageOffsetT = 1f - (_endImageOffset / splineContainer.CalculateLength());
            for (int i = 0; i < endImages.Count; i++)
            {
                endImages[i].color = GetColorAt(endImageOffsetT);
            }
        }

        void ManipulateOtherGraphics(Action<MaskableGraphic> graphic)
        {
            RemoveInvalidOtherGraphics();
            for (int i = 0; i < startImages.Count; i++)
            {
                graphic.Invoke(startImages[i]);
            }

            for (int i = 0; i < endImages.Count; i++)
            {
                graphic.Invoke(endImages[i]);
            }
        }

        void RemoveInvalidOtherGraphics()
        {
            startImages.RemoveAll(x => x == null);
            endImages.RemoveAll(x => x == null);
        }
        internal void UpdateStartEndImages(bool isStartImage)
        {
            var sprite = isStartImage ? _startImageSprite : _endImageSprite;
            var images = isStartImage ? startImages : endImages;
            var offset = isStartImage ? _startImageOffset : _endImageOffset;
            var size = isStartImage ? _startImageSize : _endImageSize;
            var offsetMode = isStartImage ? _startImageOffsetMode : _endImageOffsetMode;
            var nOffset = isStartImage ? _normalizedStartImageOffset : _normalizedEndImageOffset;

            RemoveInvalidOtherGraphics();
            if (sprite == null)
            {
                while (images.Count > 0)
                {
                    if (Application.isPlaying)
                        Destroy(images[^1].gameObject);
                    else
                        DestroyImmediate(images[^1].gameObject);

                    images.RemoveAt(images.Count - 1);
                }
            }
            else
            {
                var validSplineCount = splineContainer.Splines.Count(x => x.Count > 1);
                if (images.Count > validSplineCount)
                {
                    var diff = images.Count - validSplineCount;
                    for (int i = 0; i < diff; i++)
                    {
                        if (Application.isPlaying)
                            Destroy(images[^1].gameObject);
                        else
                            DestroyImmediate(images[^1].gameObject);

                        images.RemoveAt(images.Count - 1);
                    }
                }
                else if (images.Count < validSplineCount)
                {
                    var diff = validSplineCount - images.Count;
                    for (int i = 0; i < diff; i++)
                    {
                        var GO = new GameObject($"Spline UI Renderer - {(isStartImage ? "StartImage" : "EndImage")}[{i}]");
                        GO.transform.SetParent(transform);
                        GO.layer = LayerMask.NameToLayer("UI");
                        var img = GO.AddComponent<Image>();
                        img.SetNativeSize();
                        images.Add(img);
                    }
                }

                for (int i = 0; i < splineContainer.Splines.Count; i++)
                {
                    var spline = splineContainer[i];
                    if (spline.Count < 2) continue;

                    if (i > images.Count - 1) continue;
                    var img = images[i];
                    img.rectTransform.sizeDelta = Vector2.one * size;
                    img.sprite = sprite;

                    float3 pos;
                    quaternion rot;
                    float t;

                    var length = spline.GetLength();

                    if (offsetMode == OffsetMode.Distance)
                    {
                        t = isStartImage ? offset / length : 1 + offset / length;
                    }
                    else
                    {
                        t = nOffset;
                    }


                    var outward = t is < 0 or > 1;

                    if (outward)
                    {
                        var tt = isStartImage ? 0f : 1f;
                        if (isStartImage && t > 1) tt = 1;
                        else if (!isStartImage && t < 0) tt = 0;
                        if (offsetMode == OffsetMode.Normalized) tt = t;

                        pos = splineContainer.EvaluatePosition(tt);
                        var tan = splineContainer.EvaluateTangent(tt);

                        // resolve tangent
                        if ((Vector3)tan == Vector3.zero)
                        {
                            var p = splineContainer.EvaluatePosition(spline, isStartImage ? 0.01f : 0.99f);
                            tan = isStartImage ? p - pos : pos - p;
                        }


                        if (keepBillboard) rot = quaternion.LookRotation(new float3(0, 0, 1), tan);
                        else
                        {
                            var up = splineContainer.EvaluateUpVector(tt);
                            rot = quaternion.LookRotation(up, tan);
                        }


                        var outwardOffset = offset;
                        if (offsetMode == OffsetMode.Distance)
                        {
                            if (isStartImage && t > 1)
                            {
                                outwardOffset -= length;
                            }
                            else if (!isStartImage && t < 0)
                            {
                                outwardOffset += length;
                            }
                        }
                        else
                        {
                            outwardOffset = length * (t > 1 ? t - 1 : t);
                        }

                        pos = (Vector3)pos + (Quaternion)rot * (Vector3.up * outwardOffset);

                        if (keepZeroZ) pos.z = transform.position.z;

                        if (recursiveColor) img.color = GetColorAt(isStartImage ? 0 : 1);
                        if (recursiveMaterial) img.material = material;
                    }
                    else
                    {
                        splineContainer.Evaluate(spline, t, out pos, out var tan, out var up);

                        // resolve tangent
                        if ((Vector3)tan == Vector3.zero)
                        {
                            var acc = spline.EvaluateAcceleration(t);
                            tan = isStartImage ? acc - pos : pos - acc;
                        }

                        if (keepZeroZ) pos.z = transform.position.z;

                        if (keepBillboard) rot = quaternion.LookRotation(new float3(0, 0, 1), tan);
                        else rot = quaternion.LookRotation(up, tan);

                        if (recursiveColor) img.color = GetColorAt(t);
                        if (recursiveMaterial) img.material = material;
                    }

                    images[i].transform.SetPositionAndRotation(
                        pos,
                        rot
                    );
                }
            }
        }


        public Color GetColorAt(float t)
        {
            t = Mathf.Clamp01(t);
            return color * _colorGradient.Evaluate(t);
        }

        public Gradient GetColorGradient()
        {
            return _colorGradient;
        }

        public float GetWidthAt(float t)
        {
            return _widthCurve.Evaluate(t) * width;
        }

        /// <summary>
        /// Change Width Animation Curve.
        /// </summary>
        /// <param name="curve"></param>
        public void SetWidthCurve(AnimationCurve curve)
        {
            _widthCurve = curve;
            SetVerticesDirty();
        }

        /// <summary>
        /// Change a single keyframe of widthCurve.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="key"></param>
        public void ChangeWidthCurveKey(int index, Keyframe key)
        {
            _widthCurve.MoveKey(index, key);
            SetVerticesDirty();
        }

        /// <summary>
        /// Change Color Gradient.
        /// </summary>
        /// <param name="gradient"></param>
        public void SetColorGradient(Gradient gradient)
        {
            _colorGradient = gradient;
            SetVerticesDirty();
            UpdateGraphicColors();
        }

        /// <summary>
        /// Change a single alpha key of colorGradient;
        /// </summary>
        public void ChangeColorGradientAlphaKey(int index, GradientAlphaKey key)
        {
            _colorGradient.alphaKeys[index] = key;
            SetVerticesDirty();
            UpdateGraphicColors();
        }
        /// <summary>
        /// Change a single color key of colorGradient;
        /// </summary>
        public void ChangeColorGradientAlphaKey(int index, GradientColorKey key)
        {
            _colorGradient.colorKeys[index] = key;
            SetVerticesDirty();
            UpdateGraphicColors();
        }

        public void ForceUpdate()
        {
            DoExtrudeSplineJobAll();
            Rebuild(CanvasUpdate.Layout);
        }

        /// <summary>
        /// Rotate all knots to screen direction.
        /// </summary>
        [ContextMenu("ReorientKnots")]
        public void ReorientKnots()
        {
            splineContainer.ReorientKnots();
        }

        #region static API

        /// <summary>
        /// Create SplineContainer and UISplineRenderer at once using presets.
        /// All tangent mode of the knots are TangentMode.AutoSmooth.
        /// </summary>
        /// <param name="positions">World positions of knots. all rotations of the knots are -forward</param>
        /// <param name="parent">A UI gameObject to belong to</param>
        /// <param name="isLocal">True if the positions are local positions. False if the positions are world positions</param>
        /// <param name="lineTexture">A preset of the line texture. If you want to use custom texture, change texture property after this object is created</param>
        /// <param name="startImage">A preset of the start image (first point). If you want to use custom sprite, change startImageSprite property after this object is created</param>
        /// <param name="endImage">A preset of the end image (last point). If you want to use custom sprite, change endImageSprite property after this object is created</param>
        /// <returns></returns>
        public static UISplineRenderer Create(
            IEnumerable<Vector3> positions,
            RectTransform parent,
            bool isLocal = false,
            LineTexturePreset lineTexture = LineTexturePreset.Default,
            StartEndImagePreset startImage = StartEndImagePreset.None,
            StartEndImagePreset endImage = StartEndImagePreset.None)
        {
            var t = new GameObject("New UI Spline Renderer").AddComponent<RectTransform>();
            t.SetParent(parent, false);
            var container = t.gameObject.AddComponent<SplineContainer>();

            foreach (var p in positions)
            {
                var localP = isLocal ? p : container.transform.InverseTransformPoint(p);
                var knot = new BezierKnot(localP);
                container.Spline.Add(knot);
            }

            container.ReorientKnotsAndSmooth();

            var renderer = container.gameObject.AddComponent<UISplineRenderer>();
            renderer.lineTexturePreset = lineTexture;
            renderer.startImagePreset = startImage;
            renderer.endImagePreset = endImage;

            return renderer;
        }

        public static UISplineRenderer Create(
            IEnumerable<Vector3> positions,
            RectTransform parent,
            bool isLocal,
            Texture lineTexture,
            Sprite startImage,
            Sprite endImage)
        {
            var t = new GameObject("New UI Spline Renderer").AddComponent<RectTransform>();
            t.SetParent(parent, false);
            var container = t.gameObject.AddComponent<SplineContainer>();

            foreach (var p in positions)
            {
                var localP = isLocal ? p : container.transform.InverseTransformPoint(p);
                var knot = new BezierKnot(localP);
                container.Spline.Add(knot);
            }

            container.ReorientKnotsAndSmooth();

            var renderer = container.gameObject.AddComponent<UISplineRenderer>();
            renderer.texture = lineTexture;
            renderer.startImageSprite = startImage;
            renderer.endImageSprite = endImage;

            return renderer;
        }

        #endregion
    }
}