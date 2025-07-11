using System;
using UI_Spline_Renderer;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.UIElements;

namespace UISplineRendererEditor
{
    [CustomEditor(typeof(UISplineRenderer))]
    [CanEditMultipleObjects]
    public class UISplineRendererEditor : UnityEditor.Editor
    {
        
        UISplineRenderer _target;

        ObjectField _splineContainerField;
        ObjectField _materialField;
        ObjectField _textureField;
        Button _defaultLineTextureButton;
        Button _uvTestLineTextureButton;
        Button _customLineTextureButton;

        Toggle _keepZeroZField;
        Toggle _keepBillboardField;

        EnumField _uvModeField;
        SliderInt _resolutionSlider;
        Label _vertexCountField;
        
        ObjectField _startImageSpriteField;
        FloatField _startImageSizeField;
        FloatField _startImageOffsetField;
        Button _noneStartImageButton;
        Button _triangleStartImageButton;
        Button _arrowStartImageButton;
        Button _emptyCircleStartImageButton;
        Button _filledCircleStartImageButton;
        Button _customStartImageButton;
        
        ObjectField _endImageSpriteField;
        FloatField _endImageSizeField;
        FloatField _endImageOffsetField;
        Button _noneEndImageButton;
        Button _triangleEndImageButton;
        Button _arrowEndImageButton;
        Button _emptyCircleEndImageButton;
        Button _filledCircleEndImageButton;
        Button _customEndImageButton;

        bool _shouldUpdateStartEndImages;

        UISplineRendererSettings settings => UISplineRendererSettings.Instance;
        void OnEnable()
        {
            if (_target == null) _target = target as UISplineRenderer;
            Spline.Changed += SplineChangeCallback;
            SplineContainer.SplineAdded += SplineContainerChangeCallback;
            SplineContainer.SplineRemoved += SplineContainerChangeCallback;
            EditorApplication.update += _delayed_update_vertex_count;

            void _delayed_update_vertex_count()
            {
                UpdateVertexCount();
                EditorApplication.update -= UpdateVertexCount;
            }
        }
        void OnDisable()
        {
            Spline.Changed -= SplineChangeCallback;
            SplineContainer.SplineAdded -= SplineContainerChangeCallback;
            SplineContainer.SplineRemoved -= SplineContainerChangeCallback;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_splineContainer"));
            
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_fitPosition"));
                EditorGUI.indentLevel--;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Material"));
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_recursiveMaterial"));
                EditorGUI.indentLevel--;
        
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Color"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_colorGradient"));
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_recursiveColor"));
                EditorGUI.indentLevel--;
        
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RaycastTarget"));
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RaycastPadding"));
                EditorGUI.indentLevel--;
                
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Maskable"));
            
            EditorGUILayout.Space();
        
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_width"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_widthCurve"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_keepZeroZ"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_keepBillboard"));
        
            EditorGUILayout.Space();
            
            EditorGUI.BeginChangeCheck();
            var lineTexturePreset = EnumButtonField("Line Texture Preset", Enum.GetNames(typeof(LineTexturePreset)),
                (int)_target.lineTexturePreset);
            if(EditorGUI.EndChangeCheck())
            {
                _target.lineTexturePreset = (LineTexturePreset)lineTexturePreset;
            }
        
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Texture"));
            EditorGUI.indentLevel--;
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_smooth"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_roundEnds"));
        
            EditorGUI.BeginChangeCheck();
            var resolution = serializedObject.FindProperty("_resolution");
            EditorGUILayout.IntSlider(resolution, 1, 20);
            if (EditorGUI.EndChangeCheck())
            {
                _target.resolution = resolution.intValue;
            }
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField("Vertex Count", _target.vertexCount.ToString());
            EditorGUI.indentLevel--;
        
            var clipRangeSP = serializedObject.FindProperty("_clipRange");
            var clipRange = clipRangeSP.vector2Value;
            EditorGUILayout.MinMaxSlider("Clip Range", ref clipRange.x, ref clipRange.y, 0, 1);
            clipRangeSP.vector2Value = clipRange;

            EditorGUILayout.LabelField($"min: {clipRange.x} | max: {clipRange.y}");
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_uvMode"));
            
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("_uvMultiplier"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_uvOffset"));
            
            EditorGUILayout.Space();
            EditorGUI.BeginChangeCheck();
            var startImagePreset = EnumButtonField("Start Image Preset", Enum.GetNames(typeof(StartEndImagePreset)),
                (int)_target.startImagePreset, 3);
            if (EditorGUI.EndChangeCheck())
            {
                _target.startImagePreset = (StartEndImagePreset)startImagePreset;
            }

            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_startImageSprite"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_startImageSize"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_startImageOffsetMode"));
            if(_target.startImageOffsetMode == OffsetMode.Distance)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_startImageOffset"));
            else if(_target.startImageOffsetMode == OffsetMode.Normalized)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_normalizedStartImageOffset"));
            EditorGUI.indentLevel--;
                
            EditorGUILayout.Space();
            EditorGUI.BeginChangeCheck();
            var endImagePreset = EnumButtonField("End Image Preset", Enum.GetNames(typeof(StartEndImagePreset)),
                    (int)_target.endImagePreset, 3);
            if(EditorGUI.EndChangeCheck())
            {
                _target.endImagePreset = (StartEndImagePreset)endImagePreset;
            }

            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_endImageSprite"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_endImageSize"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_endImageOffsetMode"));
            if(_target.endImageOffsetMode == OffsetMode.Distance)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_endImageOffset"));
            else if(_target.endImageOffsetMode == OffsetMode.Normalized)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_normalizedEndImageOffset"));
            EditorGUI.indentLevel--;
                
            
            
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(target);
                serializedObject.ApplyModifiedProperties();
                _target.UpdateRaycastTargetRect();
                foreach (var o in targets)
                {
                    var renderer = o as UISplineRenderer;
                    renderer.UpdateStartEndImages(true);
                    renderer.UpdateStartEndImages(false);   
                }
            }
        }

        int EnumButtonField(string label, string[] names, int selected, int lineChange = 0)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(label);
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            for(var i = 0; i < names.Length; i ++)
            {
                if (i > 0 && lineChange > 0 && i % lineChange == 0)
                {
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                }
                if (i == selected) GUI.color = new Color(2,2,2);
                if (GUILayout.Button(names[i], GUILayout.Height(30)))
                {
                    selected = i;
                }
                GUI.color = Color.white;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
         
            return selected;
        }

        // public override VisualElement CreateInspectorGUI()
        // {
        //     var asset = Resources.Load<VisualTreeAsset>("UISplineRenderer Inspector");
        //     var tree = asset.CloneTree();
        //
        //     _splineContainerField = tree.Q<ObjectField>("splineContainer");
        //     _splineContainerField.SetEnabled(false);
        //
        //     _materialField = tree.Q<ObjectField>("material");
        //     _materialField.RegisterValueChangedCallback(e =>
        //     {
        //         _target.material = e.newValue as Material;
        //     });
        //
        //     _keepZeroZField = tree.Q<Toggle>("keepZeroZ");
        //     _keepZeroZField.RegisterValueChangedCallback(e =>
        //     {
        //         _target.UpdateStartEndImages(true);
        //         _target.UpdateStartEndImages(false);
        //     });
        //     _keepBillboardField = tree.Q<Toggle>("keepBillboard");
        //     _keepBillboardField.RegisterValueChangedCallback(e =>
        //     {
        //         _target.UpdateStartEndImages(true);
        //         _target.UpdateStartEndImages(false);
        //     });
        //     
        //     var lineTextureArea = tree.Q<VisualElement>("lineTextureArea");
        //     _textureField = lineTextureArea.Q<ObjectField>("texture");
        //     
        //     var textureButtons = tree.Q<VisualElement>("presetButtons");
        //     _defaultLineTextureButton = textureButtons.Q<Button>("default");
        //     _uvTestLineTextureButton = textureButtons.Q<Button>("uvTest");
        //     _customLineTextureButton = textureButtons.Q<Button>("custom");
        //
        //     _defaultLineTextureButton.clicked += () => OnLineTexturePresetChanged(LineTexturePreset.Default);
        //     _uvTestLineTextureButton.clicked += () => OnLineTexturePresetChanged(LineTexturePreset.UVTest);
        //     _customLineTextureButton.clicked += () => OnLineTexturePresetChanged(LineTexturePreset.Custom);
        //
        //     OnLineTexturePresetChanged(_target.lineTexturePreset);
        //
        //     _uvModeField = lineTextureArea.Q<EnumField>("uvMode");
        //     _uvModeField.tooltip = "Tile - Texture is tiled along spline length. The wrap mode of the texture should be Repeat\n\n" +
        //                            "Repeat per Segment - Texture is placed per segment(based on spline knots). The wrap mode of the texture should be Repeat\n\n" +
        //                            "Stretch - Texture is stretched along spline.";
        //
        //     
        //     _vertexCountField = lineTextureArea.Q<Label>("vertexCount");
        //     _resolutionSlider = lineTextureArea.Q<SliderInt>("resolution");
        //     _resolutionSlider.RegisterValueChangedCallback(e =>
        //     {
        //         _target.resolution = e.newValue;
        //         UpdateVertexCount();
        //     });
        //     UpdateVertexCount();
        //
        //     var clipRangeSlider = lineTextureArea.Q<MinMaxSlider>("clipRange");
        //     clipRangeSlider.RegisterValueChangedCallback(e =>
        //     {
        //         UpdateVertexCount();
        //     });
        //
        //     // init start images
        //     var startImageArea = tree.Q<VisualElement>("startImageArea");
        //     _startImageSpriteField = startImageArea.Q<ObjectField>("sprite");
        //     _startImageSpriteField.RegisterValueChangedCallback(e =>
        //     {
        //         _target.UpdateStartEndImages(true);
        //     });
        //     
        //     _noneStartImageButton = startImageArea.Q<Button>("none");
        //     _triangleStartImageButton = startImageArea.Q<Button>("triangle");
        //     _arrowStartImageButton = startImageArea.Q<Button>("arrow");
        //     _emptyCircleStartImageButton = startImageArea.Q<Button>("emptyCircle");
        //     _filledCircleStartImageButton = startImageArea.Q<Button>("filledCircle");
        //     _customStartImageButton = startImageArea.Q<Button>("custom");
        //     
        //     _noneStartImageButton.clicked +=         () => OnStartImagePresetChanged(StartEndImagePreset.None);
        //     _triangleStartImageButton.clicked +=     () => OnStartImagePresetChanged(StartEndImagePreset.Triangle);
        //     _arrowStartImageButton.clicked +=        () => OnStartImagePresetChanged(StartEndImagePreset.Arrow);
        //     _emptyCircleStartImageButton.clicked +=  () => OnStartImagePresetChanged(StartEndImagePreset.EmptyCircle);
        //     _filledCircleStartImageButton.clicked += () => OnStartImagePresetChanged(StartEndImagePreset.FilledCircle);
        //     _customStartImageButton.clicked +=       () => OnStartImagePresetChanged(StartEndImagePreset.Custom);
        //
        //     _startImageSizeField = startImageArea.Q<FloatField>("size");
        //     _startImageSizeField.RegisterValueChangedCallback(e =>
        //     {
        //         _target.UpdateStartEndImages(true);
        //     });
        //     
        //     _startImageOffsetField = startImageArea.Q<FloatField>("offset");
        //     _startImageOffsetField.RegisterValueChangedCallback(e =>
        //     {
        //         _target.UpdateStartEndImages(true);
        //     });
        //     
        //     OnStartImagePresetChanged(_target.startImagePreset);
        //     
        //     
        //     // init end images
        //     var endImageArea = tree.Q<VisualElement>("endImageArea");
        //     _endImageSpriteField = endImageArea.Q<ObjectField>("sprite");
        //     _endImageSpriteField.RegisterValueChangedCallback(e =>
        //     {
        //         _target.UpdateStartEndImages(false);
        //     });
        //     
        //     _noneEndImageButton = endImageArea.Q<Button>("none");
        //     _triangleEndImageButton = endImageArea.Q<Button>("triangle");
        //     _arrowEndImageButton = endImageArea.Q<Button>("arrow");
        //     _emptyCircleEndImageButton = endImageArea.Q<Button>("emptyCircle");
        //     _filledCircleEndImageButton = endImageArea.Q<Button>("filledCircle");
        //     _customEndImageButton = endImageArea.Q<Button>("custom");
        //     
        //     _noneEndImageButton.clicked +=         () => OnEndImagePresetChanged(StartEndImagePreset.None);
        //     _triangleEndImageButton.clicked +=     () => OnEndImagePresetChanged(StartEndImagePreset.Triangle);
        //     _arrowEndImageButton.clicked +=        () => OnEndImagePresetChanged(StartEndImagePreset.Arrow);
        //     _emptyCircleEndImageButton.clicked +=  () => OnEndImagePresetChanged(StartEndImagePreset.EmptyCircle);
        //     _filledCircleEndImageButton.clicked += () => OnEndImagePresetChanged(StartEndImagePreset.FilledCircle);
        //     _customEndImageButton.clicked +=       () => OnEndImagePresetChanged(StartEndImagePreset.Custom);
        //
        //     
        //     OnEndImagePresetChanged(_target.endImagePreset);
        //     _endImageSizeField = endImageArea.Q<FloatField>("size");
        //     _endImageSizeField.RegisterValueChangedCallback(e =>
        //     {
        //         _target.UpdateStartEndImages(false);
        //     });
        //     
        //     _endImageOffsetField = endImageArea.Q<FloatField>("offset");
        //     _endImageOffsetField.RegisterValueChangedCallback(e =>
        //     {
        //         _target.UpdateStartEndImages(false);
        //     });
        //     
        //
        //     return tree;
        // }
        
        
        void OnLineTexturePresetChanged(LineTexturePreset preset)
        {
            switch (preset)
            {
                case LineTexturePreset.Default:
                    _defaultLineTextureButton.style.backgroundColor = Color.gray;
                    _uvTestLineTextureButton.style.backgroundColor = StyleKeyword.Null;
                    _customLineTextureButton.style.backgroundColor = StyleKeyword.Null;
                    _textureField.value = settings.defaultLineTexture;
                    // _textureField.SetEnabled(false);
                    
                    // if not initialized yet
                    if (_target.texture == null)
                    {
                        _target.texture = settings.defaultLineTexture;
                    }
                    break;
                case LineTexturePreset.UVTest:
                    _defaultLineTextureButton.style.backgroundColor = StyleKeyword.Null;
                    _uvTestLineTextureButton.style.backgroundColor = Color.gray;
                    _customLineTextureButton.style.backgroundColor = StyleKeyword.Null;
                    _textureField.value = settings.uvTestLineTexture;
                    // _textureField.SetEnabled(false);
                    break;
                case LineTexturePreset.Custom:
                    _defaultLineTextureButton.style.backgroundColor = StyleKeyword.Null;
                    _uvTestLineTextureButton.style.backgroundColor = StyleKeyword.Null;
                    _customLineTextureButton.style.backgroundColor = Color.gray;
                    _textureField.SetEnabled(true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(preset), preset, null);
            }
        }



        void OnStartImagePresetChanged(StartEndImagePreset preset)
        {
            switch (preset)
            {
                case StartEndImagePreset.None:
                    _noneStartImageButton.style.backgroundColor = Color.gray;
                    _triangleStartImageButton.style.backgroundColor = StyleKeyword.Null;
                    _arrowStartImageButton.style.backgroundColor = StyleKeyword.Null;
                    _emptyCircleStartImageButton.style.backgroundColor = StyleKeyword.Null;
                    _filledCircleStartImageButton.style.backgroundColor = StyleKeyword.Null;
                    _customStartImageButton.style.backgroundColor = StyleKeyword.Null;

                    _startImageSpriteField.value = null;
                    // _startImageSpriteField.SetEnabled(false);
                    break;
                case StartEndImagePreset.Triangle:
                    _noneStartImageButton.style.backgroundColor = StyleKeyword.Null;
                    _triangleStartImageButton.style.backgroundColor = Color.gray;
                    _arrowStartImageButton.style.backgroundColor = StyleKeyword.Null;
                    _emptyCircleStartImageButton.style.backgroundColor = StyleKeyword.Null;
                    _filledCircleStartImageButton.style.backgroundColor = StyleKeyword.Null;
                    _customStartImageButton.style.backgroundColor = StyleKeyword.Null;

                    _startImageSpriteField.value = settings.triangleHead;
                    // _startImageSpriteField.SetEnabled(false);
                    break;
                case StartEndImagePreset.Arrow:
                    _noneStartImageButton.style.backgroundColor = StyleKeyword.Null;
                    _triangleStartImageButton.style.backgroundColor = StyleKeyword.Null;
                    _arrowStartImageButton.style.backgroundColor = Color.gray;
                    _emptyCircleStartImageButton.style.backgroundColor = StyleKeyword.Null;
                    _filledCircleStartImageButton.style.backgroundColor = StyleKeyword.Null;
                    _customStartImageButton.style.backgroundColor = StyleKeyword.Null;

                    _startImageSpriteField.value = settings.arrowHead;
                    // _startImageSpriteField.SetEnabled(false);
                    break;
                case StartEndImagePreset.EmptyCircle:
                    _noneStartImageButton.style.backgroundColor = StyleKeyword.Null;
                    _triangleStartImageButton.style.backgroundColor = StyleKeyword.Null;
                    _arrowStartImageButton.style.backgroundColor = StyleKeyword.Null;
                    _emptyCircleStartImageButton.style.backgroundColor = Color.gray;
                    _filledCircleStartImageButton.style.backgroundColor = StyleKeyword.Null;
                    _customStartImageButton.style.backgroundColor = StyleKeyword.Null;

                    _startImageSpriteField.value = settings.emptyCircleHead;
                    // _startImageSpriteField.SetEnabled(false);
                    break;
                case StartEndImagePreset.FilledCircle:
                    _noneStartImageButton.style.backgroundColor = StyleKeyword.Null;
                    _triangleStartImageButton.style.backgroundColor = StyleKeyword.Null;
                    _arrowStartImageButton.style.backgroundColor = StyleKeyword.Null;
                    _emptyCircleStartImageButton.style.backgroundColor = StyleKeyword.Null;
                    _filledCircleStartImageButton.style.backgroundColor = Color.gray;
                    _customStartImageButton.style.backgroundColor = StyleKeyword.Null;

                    _startImageSpriteField.value = settings.filledCircleHead;
                    // _startImageSpriteField.SetEnabled(false);
                    break;
                case StartEndImagePreset.Custom:
                    _noneStartImageButton.style.backgroundColor = StyleKeyword.Null;
                    _triangleStartImageButton.style.backgroundColor = StyleKeyword.Null;
                    _arrowStartImageButton.style.backgroundColor = StyleKeyword.Null;
                    _emptyCircleStartImageButton.style.backgroundColor = StyleKeyword.Null;
                    _filledCircleStartImageButton.style.backgroundColor = StyleKeyword.Null;
                    _customStartImageButton.style.backgroundColor = Color.gray;

                    _startImageSpriteField.SetEnabled(true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(preset), preset, null);
            }
        }
        
        void OnEndImagePresetChanged(StartEndImagePreset preset)
        {
            switch (preset)
            {
                case StartEndImagePreset.None:
                    _noneEndImageButton.style.backgroundColor = Color.gray;
                    _triangleEndImageButton.style.backgroundColor = StyleKeyword.Null;
                    _arrowEndImageButton.style.backgroundColor = StyleKeyword.Null;
                    _emptyCircleEndImageButton.style.backgroundColor = StyleKeyword.Null;
                    _filledCircleEndImageButton.style.backgroundColor = StyleKeyword.Null;
                    _customEndImageButton.style.backgroundColor = StyleKeyword.Null;

                    _endImageSpriteField.value = null;
                    // _endImageSpriteField.SetEnabled(false);
                    break;
                case StartEndImagePreset.Triangle:
                    _noneEndImageButton.style.backgroundColor = StyleKeyword.Null;
                    _triangleEndImageButton.style.backgroundColor = Color.gray;
                    _arrowEndImageButton.style.backgroundColor = StyleKeyword.Null;
                    _emptyCircleEndImageButton.style.backgroundColor = StyleKeyword.Null;
                    _filledCircleEndImageButton.style.backgroundColor = StyleKeyword.Null;
                    _customEndImageButton.style.backgroundColor = StyleKeyword.Null;

                    _endImageSpriteField.value = settings.triangleHead;
                    // _endImageSpriteField.SetEnabled(false);
                    break;
                case StartEndImagePreset.Arrow:
                    _noneEndImageButton.style.backgroundColor = StyleKeyword.Null;
                    _triangleEndImageButton.style.backgroundColor = StyleKeyword.Null;
                    _arrowEndImageButton.style.backgroundColor = Color.gray;
                    _emptyCircleEndImageButton.style.backgroundColor = StyleKeyword.Null;
                    _filledCircleEndImageButton.style.backgroundColor = StyleKeyword.Null;
                    _customEndImageButton.style.backgroundColor = StyleKeyword.Null;

                    _endImageSpriteField.value = settings.arrowHead;
                    // _endImageSpriteField.SetEnabled(false);
                    break;
                case StartEndImagePreset.EmptyCircle:
                    _noneEndImageButton.style.backgroundColor = StyleKeyword.Null;
                    _triangleEndImageButton.style.backgroundColor = StyleKeyword.Null;
                    _arrowEndImageButton.style.backgroundColor = StyleKeyword.Null;
                    _emptyCircleEndImageButton.style.backgroundColor = Color.gray;
                    _filledCircleEndImageButton.style.backgroundColor = StyleKeyword.Null;
                    _customEndImageButton.style.backgroundColor = StyleKeyword.Null;

                    _endImageSpriteField.value = settings.emptyCircleHead;
                    // _endImageSpriteField.SetEnabled(false);
                    break;
                case StartEndImagePreset.FilledCircle:
                    _noneEndImageButton.style.backgroundColor = StyleKeyword.Null;
                    _triangleEndImageButton.style.backgroundColor = StyleKeyword.Null;
                    _arrowEndImageButton.style.backgroundColor = StyleKeyword.Null;
                    _emptyCircleEndImageButton.style.backgroundColor = StyleKeyword.Null;
                    _filledCircleEndImageButton.style.backgroundColor = Color.gray;
                    _customEndImageButton.style.backgroundColor = StyleKeyword.Null;

                    _endImageSpriteField.value = settings.filledCircleHead;
                    // _endImageSpriteField.SetEnabled(false);
                    break;
                case StartEndImagePreset.Custom:
                    _noneEndImageButton.style.backgroundColor = StyleKeyword.Null;
                    _triangleEndImageButton.style.backgroundColor = StyleKeyword.Null;
                    _arrowEndImageButton.style.backgroundColor = StyleKeyword.Null;
                    _emptyCircleEndImageButton.style.backgroundColor = StyleKeyword.Null;
                    _filledCircleEndImageButton.style.backgroundColor = StyleKeyword.Null;
                    _customEndImageButton.style.backgroundColor = Color.gray;

                    _endImageSpriteField.SetEnabled(true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(preset), preset, null);
            }
        }

        void UpdateVertexCount()
        {
            if(_vertexCountField == null) return;
#if UNITY_2022_3_OR_NEWER
            _vertexCountField.text = $"Vertex Count : {_target.vertexCount}";      
#endif
        }

        void SplineChangeCallback(Spline spline, int idx, SplineModification modification)
        {
            for (int i = 0; i < _target.splineContainer.Splines.Count; i++)
            {
                if (_target.splineContainer.Splines[i] == spline)
                {
                    UpdateVertexCount();
                    return;
                }
            }
        }
        void SplineContainerChangeCallback(SplineContainer container, int i)
        {
            if(container == _target.splineContainer) UpdateVertexCount();
        }
    }
}