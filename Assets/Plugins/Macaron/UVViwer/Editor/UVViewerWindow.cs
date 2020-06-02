using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using Macaron.UVViewer.Editor.Internal;
using Macaron.UVViewer.Editor.Internal.ExtensionMethods;

namespace Macaron.UVViewer.Editor
{
    public partial class UVViewerWindow : EditorWindow
    {
        private const string _prefsKeyPrefix = "Macaron.UVViewer.Editor.UVViewerWindow.";
        private const string _viewHeightKey = _prefsKeyPrefix + "ViewHeight";
        private const string _foldoutViewSettingsKey = _prefsKeyPrefix + "FoldoutViewSettings";

#if UNITY_2018_2_OR_NEWER
        private static PropertyInfo _editorScreenPointOffset;
#else
        private static FieldInfo _editorScreenPointOffset;
#endif
        private static Material _textureMaterial;
        private static Material _lineMaterial;
        private static Material _vertexMaterial;

        [SerializeField] private Mesh _uvLineMesh;
        [SerializeField] private Mesh _uvVertexMesh;
        [SerializeField] private Mesh _uv2LineMesh;
        [SerializeField] private Mesh _uv2VertexMesh;
        [SerializeField] private Mesh _uv3LineMesh;
        [SerializeField] private Mesh _uv3VertexMesh;
        [SerializeField] private Mesh _uv4LineMesh;
        [SerializeField] private Mesh _uv4VertexMesh;
        [SerializeField] private Texture2D _textureCopy;

        [SerializeField] private MeshSettings _meshSettings;
        [SerializeField] private UnityEngine.Object _selectedObject;
        [SerializeField] private Mesh _mesh; // 실행취소 시 뷰 메시를 재구성해야 하는지 확인하기 위해서 사용한다.
        [SerializeField] private TextureSettings _textureSettings;
        [SerializeField] private Texture2D _texture; // 실행취소 시 텍스처 사본을 다시 생성해야 하는지 확인하기 위해서 사용한다.
        [SerializeField] private ViewSettings _viewSettings;

        private int _subMeshIndex;
        private int _uvIndex;

        private Vector2 _scrollPosition;

        private float _viewHeight = -1.0f;
        private float _lastViewHeight;
        private bool _viewResizing;
        private Vector2 _viewResizingMouseMovement;
        private Vector2Double _viewPivot;
        private double _viewScale;
        private bool _viewDrag;
        private bool _viewMoved;
        private bool _waitingResetView;

        private bool _foldoutViewSettings = true;
        private SerializedObject _viewSettingsObject;

        #region EditorWindow Messages
        private void Awake()
        {
            Func<Mesh> createMesh = () =>
            {
                return new Mesh()
                {
                    hideFlags = HideFlags.DontSave,
#if UNITY_2017_3_OR_NEWER
                    indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
#endif
                };
            };

            _uvLineMesh = createMesh();
            _uvVertexMesh = createMesh();
            _uv2LineMesh = createMesh();
            _uv2VertexMesh = createMesh();
            _uv3LineMesh = createMesh();
            _uv3VertexMesh = createMesh();
            _uv4LineMesh = createMesh();
            _uv4VertexMesh = createMesh();
            _textureCopy = new Texture2D(0, 0) { hideFlags = HideFlags.DontSave };

            _meshSettings = ScriptableObject.CreateInstance<MeshSettings>();
            _meshSettings.hideFlags = HideFlags.DontSave;
            _textureSettings = ScriptableObject.CreateInstance<TextureSettings>();
            _textureSettings.hideFlags = HideFlags.DontSave;
            _viewSettings = ScriptableObject.CreateInstance<ViewSettings>();
            _viewSettings.hideFlags = HideFlags.DontSave;

            _subMeshIndex = _meshSettings.SubMeshIndex;
            _uvIndex = _meshSettings.UVIndex;

            switch (_meshSettings.SourceType)
            {
            case MeshSourceType.SelectedObject:
                OnSelectionChange();
                break;

            case MeshSourceType.Custom:
                SetMeshInfo(_meshSettings.CustomMesh, null);
                break;
            }

            if (_textureSettings.SourceType == TextureSourceType.Custom)
            {
                SetTextureCopy(_textureSettings.CustomTexture);
            }
        }

        private void OnEnable()
        {
            Undo.undoRedoPerformed += Repaint;
            Undo.undoRedoPerformed += OnUndoRedoPerformed;

            if (_editorScreenPointOffset == null)
            {
#if UNITY_2018_2_OR_NEWER
                _editorScreenPointOffset = typeof(GUIUtility).GetProperty(
                    "s_EditorScreenPointOffset",
                    BindingFlags.Static | BindingFlags.NonPublic);
#else
                _editorScreenPointOffset = typeof(GUIUtility).GetField(
                    "s_EditorScreenPointOffset",
                    BindingFlags.Static | BindingFlags.NonPublic);
#endif
            }

            if (_textureMaterial == null)
            {
                _textureMaterial = new Material(Shader.Find("Hidden/Macaron/UVViewer/Editor/DrawTexture"))
                {
                    hideFlags = HideFlags.DontSave
                };
            }

            if (_lineMaterial == null)
            {
                _lineMaterial = new Material(Shader.Find("Hidden/Macaron/UVViewer/Editor/DrawLine"))
                {
                    hideFlags = HideFlags.DontSave
                };
            }

            if (_vertexMaterial == null)
            {
                _vertexMaterial = new Material(Shader.Find("Hidden/Macaron/UVViewer/Editor/DrawVertex"))
                {
                    hideFlags = HideFlags.DontSave
                };
            }

            titleContent = new GUIContent("UV Viewer", EditorResources.TitleIcon);

            _foldoutViewSettings = EditorPrefs.GetBool(_foldoutViewSettingsKey, true);
            _viewSettingsObject = new SerializedObject(_viewSettings);
        }

        private void OnGUI()
        {
            // OnEnable에서 position의 값이 확정되지 않기 때문에 여기서 처리한다.
            if (_viewHeight < 0.0f)
            {
                float viewHeight = EditorPrefs.GetFloat(_viewHeightKey, -1.0f);
                _viewHeight = viewHeight < _minViewHeight ? ClampViewHeight(position.width) : viewHeight;
                _lastViewHeight = _viewHeight == MaxViewHeight ? position.width : _viewHeight;

                ResetView(new Vector2(position.width - 2.0f, _viewHeight));
            }

            // 뷰 영역.
            Rect viewRect = DrawViewBackground();

            _viewMoved = false;
            ProcessViewInput(viewRect);

            // 뷰 이동 시 OnGUI 재호출 횟수가 많기 때문에 나머지 부분을 그리지 않도록 한다.
            if (_viewMoved)
            {
                GUIUtility.ExitGUI();
            }

            DrawTexture(viewRect);
            DrawViewGrid(viewRect);
            DrawViewMesh(viewRect);

            // 인스펙터 영역.
            // 레이아웃의 기본 여백을 제거해서 스크롤 영역의 시작 위치를 맞춘다.
            GUILayout.Space(-EditorGUIUtility.standardVerticalSpacing);

            using (var scrollView = new EditorGUILayout.ScrollViewScope(_scrollPosition))
            {
                _scrollPosition = scrollView.scrollPosition;

                // 상단 여백.
                GUILayout.Space(8.0f);

                using (new EditorGUILayout.VerticalScope(new GUIStyle { padding = new RectOffset(14, 0, 0, 0) }))
                {
                    EditorGUIUtility.labelWidth += 3.0f;

                    DrawMeshSettings();
                    EditorGUILayout.Space();

                    DrawTextureSettings();
                    EditorGUILayout.Space();

                    DrawViewSettings();
                }

                // 하단 여백.
                EditorGUILayout.Space();
            }
        }

        private void OnInspectorUpdate()
        {
            Repaint();
        }

        private void OnSelectionChange()
        {
            // _meshSettings이 생성되지 않은 상태에서 호출될 수 있기 때문에 null 검사가 필요하다.
            if (_meshSettings == null || _meshSettings.SourceType != MeshSourceType.SelectedObject)
            {
                return;
            }

            UnityEngine.Object[] objs = Selection.objects;

            if (objs.Length != 1)
            {
                _selectedObject = null;
                SetMeshInfo(null, null);
                return;
            }

            UnityEngine.Object obj = objs[0];

            if (obj == _selectedObject)
            {
                return;
            }

            Action<Mesh, Renderer> setMeshInfo = (m, r) =>
            {
                _selectedObject = obj;
                SetMeshInfo(m, r);
            };

            Mesh mesh = obj as Mesh;
            if (mesh != null)
            {
                setMeshInfo(mesh, null);
                return;
            }

            GameObject go = obj as GameObject;
            if (go != null)
            {
                MeshFilter meshFilter = go.GetComponent<MeshFilter>();
                if (meshFilter != null)
                {
                    setMeshInfo(meshFilter.sharedMesh, go.GetComponent<MeshRenderer>());
                    return;
                }

                SkinnedMeshRenderer skinnedMeshRenderer = go.GetComponent<SkinnedMeshRenderer>();
                if (skinnedMeshRenderer != null && skinnedMeshRenderer.sharedMesh != null)
                {
                    setMeshInfo(skinnedMeshRenderer.sharedMesh, skinnedMeshRenderer);
                    return;
                }
            }

            setMeshInfo(null, null);
        }

        private void OnFocus()
        {
            // 플레이 모드에 진입할 때 선택된 오브젝트가 없는 상태로 OnFocus가 호출되기 때문에 해당 상황을 무시하지 않으면 아무 것도
            // 선택되지 않은 상태가 된다.
            if (!EditorApplication.isPlaying && EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            OnSelectionChange();
        }

        private void OnDisable()
        {
            EditorPrefs.SetFloat(_viewHeightKey, _viewHeight);
            EditorPrefs.SetBool(_foldoutViewSettingsKey, _foldoutViewSettings);

            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
            Undo.undoRedoPerformed -= Repaint;
        }

        private void OnDestroy()
        {
            DestroyImmediate(_uvLineMesh);
            DestroyImmediate(_uvVertexMesh);
            DestroyImmediate(_uv2LineMesh);
            DestroyImmediate(_uv2VertexMesh);
            DestroyImmediate(_uv3LineMesh);
            DestroyImmediate(_uv3VertexMesh);
            DestroyImmediate(_uv4LineMesh);
            DestroyImmediate(_uv4VertexMesh);
            DestroyImmediate(_textureCopy);

            DestroyImmediate(_meshSettings);
            DestroyImmediate(_textureSettings);
            DestroyImmediate(_viewSettings);
        }
        #endregion

        #region Public Properties
        public Mesh CustomMesh
        {
            get { return _meshSettings.CustomMesh; }
        }

        public Texture2D CustomTexture
        {
            get { return _textureSettings.CustomTexture; }
        }
        #endregion

        private bool EnableGeometryShader
        {
            get
            {
                return
                    SystemInfo.graphicsShaderLevel >= 40 &&
                    SystemInfo.graphicsDeviceType != GraphicsDeviceType.Metal;
            }
        }

        private float MaxViewHeight
        {
            get { return position.height - 17.0f; }
        }

        #region Public Methods
        public void SetCustomMesh(Mesh mesh)
        {
            _meshSettings.SourceType = MeshSourceType.Custom;
            _meshSettings.CustomMesh = mesh;
            SetMeshInfo(mesh, null);

            Repaint();
        }

        public void SetCustomTexture(Texture2D texture)
        {
            _textureSettings.SourceType = TextureSourceType.Custom;
            _textureSettings.CustomTexture = texture;
            SetTextureCopy(texture);

            Repaint();
        }
        #endregion

        #region Private Methods
        private float ClampViewHeight(float height)
        {
            return Mathf.Min(Mathf.Max(_minViewHeight, height), MaxViewHeight);
        }

        private void OnUndoRedoPerformed()
        {
            MeshInfo meshInfo = _meshSettings.MeshInfo;
            if (meshInfo.Mesh != _mesh)
            {
                SetMeshInfo(meshInfo.Mesh, meshInfo.Renderer);
            }

            Texture2D texture = _textureSettings.Texture;
            if (texture != _texture)
            {
                SetTextureCopy(texture);
            }
        }

        private void SetMeshInfo(Mesh mesh, Renderer renderer)
        {
            MeshInfo meshInfo = null;

#if UNITY_5_5_OR_NEWER
            UnityEngine.Profiling.Profiler.BeginSample("UVViewerWindow.SetMeshInfo", this);
#endif
            try
            {
                _uvLineMesh.Clear();
                _uvVertexMesh.Clear();
                _uv2LineMesh.Clear();
                _uv2VertexMesh.Clear();
                _uv3LineMesh.Clear();
                _uv3VertexMesh.Clear();
                _uv4LineMesh.Clear();
                _uv4VertexMesh.Clear();

                if (mesh != null && mesh.vertexCount > 0)
                {
                    int subMeshCount = mesh.subMeshCount;
                    int[][] subMeshSegmentIndices;
                    HashSet<int>[] subMeshVertexIndices;
                    SubMeshInfo[] subMeshInfos;
                    BuildSegmentIndices(mesh, out subMeshSegmentIndices, out subMeshVertexIndices, out subMeshInfos);

                    Action<Vector2[], Mesh, Mesh> setViewMesh;

                    if (EnableGeometryShader)
                    {
                        setViewMesh = (uv, lineMesh, vertexMesh) =>
                        {
                            SetViewMeshForGeometryShader(
                                uv,
                                mesh.colors32,
                                subMeshCount,
                                subMeshSegmentIndices,
                                subMeshVertexIndices,
                                lineMesh,
                                vertexMesh);
                        };
                    }
                    else
                    {
                        Color32[] meshColors = mesh.colors32;
                        Color32[] colors = new Color32[meshColors.Length * 4];

                        for (int i = 0; i < meshColors.Length; ++i)
                        {
                            int index = i * 4;

                            colors[index + 0] = meshColors[i];
                            colors[index + 1] = meshColors[i];
                            colors[index + 2] = meshColors[i];
                            colors[index + 3] = meshColors[i];
                        }

                        setViewMesh = (uv, lineMesh, vertexMesh) =>
                        {
                            SetViewMesh(
                                uv,
                                colors,
                                subMeshCount,
                                subMeshSegmentIndices,
                                subMeshVertexIndices,
                                lineMesh,
                                vertexMesh);
                        };
                    }

                    setViewMesh(mesh.uv, _uvLineMesh, _uvVertexMesh);
                    setViewMesh(mesh.uv2, _uv2LineMesh, _uv2VertexMesh);
                    setViewMesh(mesh.uv3, _uv3LineMesh, _uv3VertexMesh);
                    setViewMesh(mesh.uv4, _uv4LineMesh, _uv4VertexMesh);

                    int uvChannelFlag =
                        (_uvLineMesh.vertexCount > 0 ? 0x01 : 0) |
                        (_uv2LineMesh.vertexCount > 0 ? 0x02 : 0) |
                        (_uv3LineMesh.vertexCount > 0 ? 0x04 : 0) |
                        (_uv4LineMesh.vertexCount > 0 ? 0x08 : 0);
                    meshInfo = new MeshInfo(mesh, renderer, uvChannelFlag, subMeshInfos);
                }
                else
                {
                    meshInfo = new MeshInfo(null, null, 0, new SubMeshInfo[0]);
                }
            }
            finally
            {
#if UNITY_5_5_OR_NEWER
                UnityEngine.Profiling.Profiler.EndSample();
#endif
            }

            if (_subMeshIndex > meshInfo.SubMeshCount)
            {
                _subMeshIndex = -1;
            }

            if (!meshInfo.HasUVChannel(_uvIndex))
            {
                bool[] uvChannels = Enumerable.Range(0, 4).Select(i => meshInfo.HasUVChannel(i)).ToArray();
                _uvIndex = Array.FindIndex(uvChannels, ch => ch);
            }

            _meshSettings.MeshInfo = meshInfo;
            _mesh = mesh;
        }

        private void SetTextureCopy(Texture2D texture)
        {
            DestroyImmediate(_textureCopy);

            _texture = texture;

            if (texture == null)
            {
                _textureCopy = new Texture2D(0, 0) { hideFlags = HideFlags.DontSave };
                return;
            }

            _textureCopy = new Texture2D(texture.width, texture.height, texture.format, texture.mipmapCount > 1)
            {
                hideFlags = HideFlags.DontSave,
                filterMode = (FilterMode)_textureSettings.FilterMode
            };
            _textureCopy.wrapMode = TextureWrapMode.Repeat;

            Graphics.CopyTexture(texture, _textureCopy);
        }

        private Rect DrawViewBackground()
        {
            Rect controlRect = EditorGUILayout.GetControlRect(false, _viewHeight);

            var backgroundRect = new Rect(
                controlRect.x - 3.0f,
                controlRect.y - 1.0f,
                controlRect.width + 7.0f,
                controlRect.height + 1.0f);
            var resizeHandleBackgroundRect = new Rect(backgroundRect)
            {
                yMin = backgroundRect.yMax,
                yMax = backgroundRect.yMax + 17.0f
            };

            var previewLabelStyle = new GUIStyle("preLabel");
            previewLabelStyle.normal.textColor = new Color32(222, 222, 222, 255);
            var labelContent = new GUIContent(_mesh != null ? _mesh.name : string.Empty);
            Vector2 labelSize = previewLabelStyle.CalcSize(labelContent);
            var labelRect = new Rect(resizeHandleBackgroundRect)
            {
                xMin = backgroundRect.xMin + 3.0f,
                xMax = backgroundRect.xMin + 3.0f + labelSize.x
            };

            var previewButtonStyle = new GUIStyle("preButton");
            var resetButtonContent = new GUIContent(EditorResources.ZoomIcon, "Reset View");
            Vector2 resetButtonSize = previewButtonStyle.CalcSize(resetButtonContent);
            var resetButtonRect = new Rect(resizeHandleBackgroundRect)
            {
                xMin = backgroundRect.xMax - 5.0f - resetButtonSize.x,
                xMax = backgroundRect.xMax - 5.0f
            };
            var resizeHandleRect = new Rect(resizeHandleBackgroundRect)
            {
                xMin = labelRect.xMax - 1.0f,
                xMax = resetButtonRect.xMin
            };

            EditorGUI.DrawRect(backgroundRect, new Color32(49, 49, 49, 255));
            GUI.Box(resizeHandleBackgroundRect, string.Empty, EditorResources.ResizeHandleBackgroundStyle);
            GUI.Box(resizeHandleRect, string.Empty, EditorResources.ResizeHandleStyle);
            GUI.Label(labelRect, labelContent, previewLabelStyle);

            var resizeControlRect = new Rect(resizeHandleBackgroundRect)
            {
                xMin = backgroundRect.xMin + 4.0f,
                xMax = resetButtonRect.xMin
            };

            EditorGUIUtility.AddCursorRect(resizeControlRect, MouseCursor.ResizeVertical);
            ProcessResizeView(resizeControlRect);

            GUILayout.Space(17.0f);

            var viewRect = new Rect(
                backgroundRect.x + 1.0f,
                backgroundRect.y + 1.0f,
                backgroundRect.width - 2.0f,
                backgroundRect.height - 1.0f);

            if (GUI.Button(resetButtonRect, resetButtonContent, previewButtonStyle))
            {
                ResetView(viewRect.size);
            }

            return viewRect;
        }

        private void ProcessResizeView(Rect resizeControlRect)
        {
            Event evt = Event.current;

            if (_viewResizing)
            {
                if (evt.rawType == EventType.MouseUp && evt.button == 0)
                {
                    _viewResizing = false;

                    // 드래그가 아닌 클릭 시의 처리.
                    // 뷰의 크기를 최대화하거나 이전 크기로 되돌린다.
                    if (_viewResizingMouseMovement == Vector2.zero)
                    {
                        _viewHeight = ClampViewHeight(_viewHeight == MaxViewHeight ? _lastViewHeight : position.height);
                        Repaint();
                    }
                    else if (_viewHeight != MaxViewHeight)
                    {
                        _lastViewHeight = _viewHeight;
                    }

                    evt.Use();
                    return;
                }

                EditorGUIUtility.AddCursorRect(new Rect(Vector2.zero, position.size), MouseCursor.ResizeVertical);

                if (evt.type == EventType.MouseDrag)
                {
                     _viewResizingMouseMovement += new Vector2(Mathf.Abs(evt.delta.x), Mathf.Abs(evt.delta.y));

                    float viewHeight = ClampViewHeight(_viewHeight + evt.delta.y);
                    if (viewHeight != _viewHeight)
                    {
                        _viewHeight = viewHeight;
                        Repaint();
                    }

                    evt.Use();
                    return;
                }
            }

            if (evt.type == EventType.MouseDown && evt.button == 0 && resizeControlRect.Contains(evt.mousePosition))
            {
                _viewResizing = true;
                _viewResizingMouseMovement = Vector2.zero;

                evt.Use();
            }
        }

        private void ProcessViewInput(Rect viewRect)
        {
            ProcessDragAndDrop(viewRect);
            ProcessDragMove(viewRect);
            ProcessWheelZoom(viewRect);

            Event evt = Event.current;

            if (_waitingResetView && evt.rawType == EventType.MouseUp && evt.button == 1)
            {
               if (viewRect.Contains(evt.mousePosition))
               {
                   ResetView(viewRect.size);
               }

                _waitingResetView = false;
               evt.Use();
            }

            if (evt.type == EventType.MouseDown && evt.button == 1 && viewRect.Contains(evt.mousePosition))
            {
                _waitingResetView = true;
                evt.Use();
            }
        }

        private void ProcessDragAndDrop(Rect viewRect)
        {
            Event evt = Event.current;

            if (!viewRect.Contains(evt.mousePosition))
            {
                return;
            }

            if (evt.type != EventType.DragUpdated && evt.type != EventType.DragPerform)
            {
                return;
            }

            UnityEngine.Object obj = DragAndDrop.objectReferences.FirstOrDefault();

            if (obj == null)
            {
                return;
            }

            Mesh mesh = null;
            MeshFilter meshFilter = null;
            SkinnedMeshRenderer skinnedMeshRenderer = null;
            Texture2D texture = null;
            DragObjectType dragObjectType = DragObjectType.None;

            Func<bool> isAcceptableDrag = () =>
            {
                mesh = obj as Mesh;
                if (mesh != null)
                {
                    dragObjectType = DragObjectType.Mesh;
                    return true;
                }

                var go = obj as GameObject;
                if (go != null)
                {
                    meshFilter = go.GetComponent<MeshFilter>();
                    if (meshFilter != null)
                    {
                        if (meshFilter.sharedMesh == null)
                        {
                            return false;
                        }

                        dragObjectType = DragObjectType.MeshFilter;
                        return true;
                    }

                    skinnedMeshRenderer = go.GetComponent<SkinnedMeshRenderer>();
                    if (skinnedMeshRenderer != null)
                    {
                        if (skinnedMeshRenderer.sharedMesh == null)
                        {
                            return false;
                        }

                        dragObjectType = DragObjectType.SkinnedMeshFilter;
                        return true;
                    }
                }

                texture = obj as Texture2D;
                if (texture != null)
                {
                    dragObjectType = DragObjectType.Texture2D;
                    return true;
                }

                return false;
            };

            if (!isAcceptableDrag())
            {
                return;
            }

            DragAndDrop.AcceptDrag();
            DragAndDrop.activeControlID = GUIUtility.GetControlID(FocusType.Passive, viewRect);

            if (evt.type == EventType.DragPerform)
            {
                Action<Mesh> setCustomMesh = customMesh =>
                {
                    Undo.RecordObject(_meshSettings, "Mesh Change");
                    SetCustomMesh(customMesh);
                };

                switch (dragObjectType)
                {
                case DragObjectType.Mesh:
                    setCustomMesh(mesh);
                    break;

                case DragObjectType.MeshFilter:
                    setCustomMesh(meshFilter.sharedMesh);
                    break;

                case DragObjectType.SkinnedMeshFilter:
                    setCustomMesh(skinnedMeshRenderer.sharedMesh);
                    break;

                case DragObjectType.Texture2D:
                    Undo.RecordObject(_textureSettings, "Texture Change");
                    SetCustomTexture(texture);
                    break;
                }
            }
            else
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
            }

            evt.Use();
        }

        private void ProcessDragMove(Rect viewRect)
        {
            Event evt = Event.current;

            if (_viewDrag && evt.rawType == EventType.MouseUp && evt.button == 0)
            {
                _viewDrag = false;
                evt.Use();

                return;
            }

            if (_viewDrag && evt.delta != Vector2.zero)
            {
                // 컨트롤 키(맥에서는 Cmd)가 눌려있을 경우 상하 이동을 확대/축소로 처리한다.
                if (!EditorGUI.actionKey)
                {
                    Vector2Double delta = (Vector2Double)evt.delta / _viewScale * 0.5;
                    delta.y *= -1.0;
                    _viewPivot -= delta;
                }
                else
                {
                    double zoomFactor = Math.Exp(-evt.delta.y * _zoomIntensity);
                    double oldViewScale = _viewScale;
                    double newViewScale = Math.Min(Math.Max(_minViewScale, oldViewScale * zoomFactor), _maxViewScale);
                    _viewScale = newViewScale;
                }

                _viewMoved = true;
                Repaint();
            }

            if (evt.type == EventType.MouseDown && evt.button == 0 && viewRect.Contains(evt.mousePosition))
            {
                _viewDrag = true;
                evt.Use();
            }
        }

        private void ProcessWheelZoom(Rect viewRect)
        {
            Event evt = Event.current;

            if (evt.type != EventType.ScrollWheel || !viewRect.Contains(evt.mousePosition))
            {
                return;
            }

            double zoomFactor = Math.Exp(-evt.delta.y * _zoomIntensity);
            double oldViewScale = _viewScale;
            double newViewScale = Math.Min(Math.Max(_minViewScale, oldViewScale * zoomFactor), _maxViewScale);

            // 커서 위치를 기준으로 확대/축소.
            Vector2Double cursorPos = viewRect.center - evt.mousePosition;
            Vector2Double oldCursorPivot = cursorPos / oldViewScale;
            Vector2Double newCursorPivot = cursorPos / newViewScale;
            Vector2Double pivotDiff = newCursorPivot - oldCursorPivot;
            pivotDiff.y *= -1.0;

            _viewPivot += pivotDiff;
            _viewScale = newViewScale;

            evt.Use();
            Repaint();
        }

        private void ResetView(Vector2 viewSize)
        {
            _viewScale = Math.Max(_minViewScale, Math.Min(viewSize.x - 30.0f, viewSize.y - 20.0f));

            var offset = new Vector2Double(10.0, 5.0) / _viewScale;
            _viewPivot = _defaultViewPivot - offset;
        }

        private void DrawTexture(Rect viewRect)
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            if (!_textureSettings.HasTexture)
            {
                return;
            }

            Matrix4x4 mat =
                Matrix4x4Ext.Translate(viewRect.size * 0.5f) *
                Matrix4x4.Scale(new Vector3((float)_viewScale, -(float)_viewScale, 1.0f)) *
                Matrix4x4Ext.Translate(-(Vector2)_viewPivot);
            Vector3 topLeft = mat.MultiplyPoint3x4(Vector3.up);
            Vector3 bottomRight = mat.MultiplyPoint3x4(Vector3.right);

            var rect = new Rect(topLeft, bottomRight - topLeft);

            float xMin;
            float xMax;
            float yMin;
            float yMax;

            if (_textureSettings.Repeating)
            {
                xMin = viewRect.xMin;
                xMax = viewRect.xMax;
                yMin = viewRect.yMin;
                yMax = viewRect.yMax;
            }
            else
            {
                if (rect.xMin > viewRect.xMax || rect.xMax < viewRect.xMin ||
                    rect.yMin > viewRect.yMax || rect.yMax < viewRect.yMin)
                {
                    return;
                }

                xMin = Mathf.Max(rect.xMin, viewRect.xMin);
                xMax = Mathf.Min(rect.xMax, viewRect.xMax);
                yMin = Mathf.Max(rect.yMin, viewRect.yMin);
                yMax = Mathf.Min(rect.yMax, viewRect.yMax);
            }

            var screenRect = new Rect
            {
                xMin = xMin,
                xMax = xMax,
                yMin = yMin,
                yMax = yMax
            };
            var sourceRect = new Rect
            {
                xMin = (xMin - rect.xMin) / (rect.xMax - rect.xMin),
                xMax = (xMax - rect.xMin) / (rect.xMax - rect.xMin),
                yMin = 1.0f - ((yMax - rect.yMin) / (rect.yMax - rect.yMin)),
                yMax = 1.0f - ((yMin - rect.yMin) / (rect.yMax - rect.yMin))
            };

            UpdateTextureMaterial();

            Graphics.DrawTexture(
                screenRect,
                _textureCopy,
                sourceRect,
                0,
                0,
                0,
                0,
                _textureSettings.Color,
                _textureMaterial);
        }

        private void UpdateTextureMaterial()
        {
            Func<TextureChannelFlags, bool> hasChannel = channel => _textureSettings.ChannelFlag.Has(channel);

            bool hasR = hasChannel(TextureChannelFlags.R);
            bool hasG = hasChannel(TextureChannelFlags.G);
            bool hasB = hasChannel(TextureChannelFlags.B);
            bool hasA = hasChannel(TextureChannelFlags.A);
            int rgbCount = (hasR ? 1 : 0) + (hasG ? 1 : 0) + (hasB ? 1 : 0);
            int count = rgbCount + (hasA ? 1 : 0);

            Vector4 colorMaskR;
            Vector4 colorMaskG;
            Vector4 colorMaskB;
            Vector4 colorMaskA;
            float additiveAlpha;

            if (rgbCount == 0)
            {
                colorMaskR = Vector4.zero;
                colorMaskG = Vector4.zero;
                colorMaskB = Vector4.zero;
            }
            else if (rgbCount == 1)
            {
                colorMaskR = hasR ? new Vector4(1.0f, 1.0f, 1.0f, 0.0f) : Vector4.zero;
                colorMaskG = hasG ? new Vector4(1.0f, 1.0f, 1.0f, 0.0f) : Vector4.zero;
                colorMaskB = hasB ? new Vector4(1.0f, 1.0f, 1.0f, 0.0f) : Vector4.zero;
            }
            else
            {
                colorMaskR = hasR ? new Vector4(1.0f, 0.0f, 0.0f, 0.0f) : Vector4.zero;
                colorMaskG = hasG ? new Vector4(0.0f, 1.0f, 0.0f, 0.0f) : Vector4.zero;
                colorMaskB = hasB ? new Vector4(0.0f, 0.0f, 1.0f, 0.0f) : Vector4.zero;
            }

            if (count == 0)
            {
                colorMaskA = Vector4.zero;
                additiveAlpha = 0.0f;
            }
            else if (rgbCount > 0)
            {
                colorMaskA = hasA ? new Vector4(0.0f, 0.0f, 0.0f, 1.0f) : Vector4.zero;
                additiveAlpha = hasA ? 0.0f : 1.0f;
            }
            else
            {
                colorMaskA = new Vector4(1.0f, 1.0f, 1.0f, 0.0f);
                additiveAlpha = 1.0f;
            }

            _textureMaterial.SetVector("_ColorMaskR", colorMaskR);
            _textureMaterial.SetVector("_ColorMaskG", colorMaskG);
            _textureMaterial.SetVector("_ColorMaskB", colorMaskB);
            _textureMaterial.SetVector("_ColorMaskA", colorMaskA);
            _textureMaterial.SetFloat("_AdditiveAlpha", additiveAlpha);
        }

        private void DrawViewGrid(Rect viewRect)
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            if (!_viewSettings.DrawGrid)
            {
                return;
            }

            Matrix4x4 mat =
                Matrix4x4Ext.Translate((viewRect.size * 0.5f) - viewRect.position) *
                Matrix4x4.Scale(new Vector3((float)_viewScale, -(float)_viewScale, 1.0f)) *
                Matrix4x4Ext.Translate(-(Vector2)_viewPivot);

            using (new GUI.ClipScope(viewRect))
            using (new HandlesGUIScope())
            using (new HandlesDrawingScope(Handles.matrix * mat))
            {
                Func<double, double, byte> getAlpha = (begin, end) =>
                {
                    double t = (_viewScale - begin) / (end - begin);
                    double a = Math.Max(0.0, Math.Min(t, 1.0));
                    return (byte)Math.Round(255.0 * a);
                };

                // 확대율에 따른 그룹의 투명도. 각 수치는 적절한 값을 수동으로 입력한 것으로 디스플레이 환경에 따라 적절하게 표시되지
                // 않을 수 있음.
                byte alpha0 = getAlpha(20.0, 40.0);
                byte alpha1 = getAlpha(40.0, 80.0);
                byte alpha2 = getAlpha(200.0, 400.0);

                var lineGroups = new[]
                {
                    new { Segments = _viewGridGroup3LineSegments, Color = new Color32(70, 70, 70, alpha1) },
                    new { Segments = _viewGridGroup2LineSegments, Color = new Color32(90, 90, 90, alpha0) },
                    new { Segments = _viewGridGroup1LineSegments, Color = new Color32(110, 110, 110, 255) },
                    new { Segments = _viewGridGroup0LineSegments, Color = new Color32(130, 130, 130, 255) }
                };

                foreach (var lineGroup in lineGroups)
                {
                    using (new HandlesDrawingScope(lineGroup.Color))
                    {
                        Handles.DrawLines(lineGroup.Segments);
                    }
                }

                var textOffset = new Vector3(-20.0f, -1.0f) / (float)_viewScale;
                var textGroups = new[]
                {
                    new { TextInfos = _viewGridGroup3TextInfos, Color = new Color32(180, 180, 180, alpha2) },
                    new { TextInfos = _viewGridGroup2TextInfos, Color = new Color32(180, 180, 180, alpha1) },
                    new { TextInfos = _viewGridGroup1TextInfos, Color = new Color32(180, 180, 180, alpha0) },
                    new { TextInfos = _viewGridGroup0TextInfos, Color = new Color32(180, 180, 180, alpha0) }
                };

                foreach (var textGroup in textGroups)
                {
                    var style = new GUIStyle
                    {
                        normal = new GUIStyleState { textColor = textGroup.Color },
                        alignment = TextAnchor.UpperRight
                    };

                    GridTextInfo[] infos = textGroup.TextInfos;
                    for (int i = 0; i < infos.Length; ++i)
                    {
                        GridTextInfo info = infos[i];
                        Handles.Label(info.Position + textOffset, info.Text, style);
                    }
                }
            }
        }

        private void DrawViewMesh(Rect viewRect)
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            float scaleFactor = EnableGeometryShader ? 1.0f : EditorGUIUtility.pixelsPerPoint;
            float invScaleFactor = 1.0f / scaleFactor;

            using (new DrawMeshScope(viewRect, EnableGeometryShader ? EditorGUIUtility.pixelsPerPoint : 1.0f))
            {
                var offset = (Vector2)_editorScreenPointOffset.GetValue(null);
                offset -= GUIUtility.GUIToScreenPoint(Vector2.zero);

                Matrix4x4 viewMat =
                    Matrix4x4Ext.Translate(offset) *
                    Matrix4x4.Scale(new Vector3(invScaleFactor, invScaleFactor, 1.0f)) *
                    Matrix4x4Ext.Translate((viewRect.size * 0.5f) - viewRect.position) *
                    Matrix4x4.Scale(new Vector3((float)_viewScale, -(float)_viewScale, 1.0f)) *
                    Matrix4x4Ext.Translate(-(Vector2)_viewPivot);

                Color lineColor = _viewSettings.LineColor;

                if (_viewSettings.DrawOutline)
                {
                    Color outlineColor = _viewSettings.OutlineColor;
                    outlineColor.a *= lineColor.a;

                    _lineMaterial.SetColor("_Color", outlineColor);
                    _lineMaterial.SetFloat("_Thickness", _viewSettings.LineThickness + 2);
                    _lineMaterial.SetPass(0);

                    if (EnableGeometryShader)
                    {
                        DrawLineMesh(viewMat);
                    }
                    else
                    {
                        var lineOffsets = new[]
                        {
                            Vector2.right * invScaleFactor,
                            Vector2.left * invScaleFactor,
                            Vector2.up * invScaleFactor,
                            Vector2.down * invScaleFactor
                        };
                        foreach (var lineOffset in lineOffsets)
                        {
                            Matrix4x4 mat = Matrix4x4Ext.Translate(lineOffset) * viewMat;
                            DrawLineMesh(mat);
                        }
                    }
                }

                _lineMaterial.SetColor("_Color", lineColor);
                _lineMaterial.SetFloat("_Thickness", _viewSettings.LineThickness);
                _lineMaterial.SetPass(0);

                DrawLineMesh(viewMat);

                if (_viewSettings.DrawVertex)
                {
                    Func<int, float> sizeToRadius = size =>
                    {
                        return Mathf.Sqrt((size * size) / 2.0f) * invScaleFactor;
                    };

                    if (_viewSettings.DrawVertexOutline)
                    {
                        _vertexMaterial.SetColor("_Color", _viewSettings.VertexOutlineColor);
                        _vertexMaterial.SetFloat("_VertexColorRatio", 0.0f);
                        _vertexMaterial.SetFloat("_Radius", sizeToRadius(_viewSettings.VertexSize + 2));
                        _vertexMaterial.SetPass(0);

                        DrawVertexMesh(viewMat);
                    }

                    _vertexMaterial.SetColor("_Color", Color.white);
                    _vertexMaterial.SetFloat("_VertexColorRatio", 1.0f);
                    _vertexMaterial.SetFloat("_Radius", sizeToRadius(_viewSettings.VertexSize));
                    _vertexMaterial.SetPass(0);

                    DrawVertexMesh(viewMat);
                }
            }
        }

        private void DrawLineMesh(Matrix4x4 matrix)
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            Mesh lineMesh;

            switch (_uvIndex)
            {
            case 0: lineMesh = _uvLineMesh; break;
            case 1: lineMesh = _uv2LineMesh; break;
            case 2: lineMesh = _uv3LineMesh; break;
            case 3: lineMesh = _uv4LineMesh; break;

            default:
                return;
            }

            int subMeshCount = _meshSettings.MeshInfo.SubMeshCount;

            if (_subMeshIndex == -1)
            {
                for (int i = 0; i < subMeshCount; ++i)
                {
                    Graphics.DrawMeshNow(lineMesh, matrix, i);
                }
            }
            else if (_subMeshIndex >= 0 && _subMeshIndex < subMeshCount)
            {
                Graphics.DrawMeshNow(lineMesh, matrix, _subMeshIndex);
            }
        }

        private void DrawVertexMesh(Matrix4x4 matrix)
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            Mesh pointMesh;

            switch (_uvIndex)
            {
            case 0: pointMesh = _uvVertexMesh; break;
            case 1: pointMesh = _uv2VertexMesh; break;
            case 2: pointMesh = _uv3VertexMesh; break;
            case 3: pointMesh = _uv4VertexMesh; break;

            default:
                return;
            }

            int subMeshCount = _meshSettings.MeshInfo.SubMeshCount;

            if (_subMeshIndex == -1)
            {
                for (int i = 0; i < subMeshCount; ++i)
                {
                    Graphics.DrawMeshNow(pointMesh, matrix, i);
                }
            }
            else if (_subMeshIndex >= 0 && _subMeshIndex < subMeshCount)
            {
                Graphics.DrawMeshNow(pointMesh, matrix, _subMeshIndex);
            }
        }

        private void DrawMeshSettings()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                var meshSourceType = (MeshSourceType)EditorGUILayout.EnumPopup(
                "Mesh",
                _meshSettings.SourceType);

                if (meshSourceType != _meshSettings.SourceType)
                {
                    Undo.RecordObject(_meshSettings, "Mesh Settings Change");
                    _meshSettings.SourceType = meshSourceType;

                    switch (meshSourceType)
                    {
                    case MeshSourceType.SelectedObject:
                        OnSelectionChange();
                        break;

                    case MeshSourceType.Custom:
                        _selectedObject = null;
                        SetMeshInfo(_meshSettings.CustomMesh, null);
                        break;
                    }
                }

                using (new EditorGUI.DisabledGroupScope(!_meshSettings.MeshInfo.HasMesh))
                {
                    if (GUILayout.Button("Reload", EditorStyles.miniButton, GUILayout.ExpandWidth(false)))
                    {
                        MeshInfo meshInfo = _meshSettings.MeshInfo;
                        SetMeshInfo(meshInfo.Mesh, meshInfo.Renderer);
                    }
                }
            }

            if (_meshSettings.SourceType == MeshSourceType.Custom)
            {
                using (new EditorGUIIndentLevelScope())
                {
                    Mesh oldMesh = _meshSettings.CustomMesh;
                    Mesh newMesh = (Mesh)EditorGUILayout.ObjectField("Source", oldMesh, typeof(Mesh), false);

                    if (newMesh != oldMesh)
                    {
                        Undo.RecordObject(_meshSettings, "Mesh Change");

                        _meshSettings.CustomMesh = newMesh;
                        SetMeshInfo(newMesh, null);
                    }
                }
            }

            if (_meshSettings.MeshInfo.SubMeshInfos.Count(info => info.Topology != MeshTopology.Triangles) > 0)
            {
                using (new EditorGUIIndentLevelScope())
                {
                    EditorGUILayout.HelpBox(
                        "Some sub meshes has non triangles topology. " +
                        "UV will be drawn only sub meshes that has triangles topology.",
                        MessageType.Warning);
                }
            }

            DrawSubMeshSelector();
            DrawUVChannelSelector();
        }

        private void DrawSubMeshSelector()
        {
            MeshInfo meshInfo = _meshSettings.MeshInfo;

            if (!meshInfo.HasMesh)
            {
                using (new EditorGUI.DisabledGroupScope(true))
                using (new EditorGUIIndentLevelScope())
                {
                    EditorGUILayout.Popup("Sub Mesh", 0, new[] { "-" });
                }
                return;
            }

            Mesh mesh = meshInfo.Mesh;
            int oldSubMeshIndex = _meshSettings.SubMeshIndex;

            if (oldSubMeshIndex < -1 || oldSubMeshIndex >= mesh.subMeshCount)
            {
                oldSubMeshIndex = -1;
            }

            Func<int, string> buildSubMeshText = i =>
            {
                SubMeshInfo info = meshInfo.SubMeshInfos[i];
                uint elementCount = 0;

                switch (info.Topology)
                {
                case MeshTopology.Points: elementCount = info.IndexCount; break;
                case MeshTopology.Lines: elementCount = info.IndexCount / 2; break;
                case MeshTopology.LineStrip: elementCount = info.IndexCount -1; break;
                case MeshTopology.Triangles: elementCount = info.IndexCount / 3; break;
                case MeshTopology.Quads: elementCount = info.IndexCount / 4; break;
                }

                return string.Format("{0} ({1}, {2})", i + 1, info.Topology, elementCount);
            };

            var range = Enumerable.Range(0, mesh.subMeshCount);
            string[] subMeshList = new[] { string.Format("All ({0})", mesh.subMeshCount) }
                .Concat(range.Select(buildSubMeshText))
                .ToArray();
            int[] subMeshIndexList = new[] { -1 }.Concat(range).ToArray();

            using (new EditorGUIIndentLevelScope())
            {
                int newSubMeshIndex = EditorGUILayout.IntPopup(
                    "Sub Mesh",
                    oldSubMeshIndex,
                    subMeshList,
                    subMeshIndexList);
                if (newSubMeshIndex != oldSubMeshIndex)
                {
                    Undo.RecordObject(_meshSettings, "Mesh Settings Change");
                    _meshSettings.SubMeshIndex = newSubMeshIndex;
                }

                _subMeshIndex = newSubMeshIndex;
            }
        }

        private void DrawUVChannelSelector()
        {
            MeshInfo meshInfo = _meshSettings.MeshInfo;

            using (new EditorGUI.DisabledGroupScope(!meshInfo.HasMesh))
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(15.0f);
                GUILayout.Label("UV Channel", GUILayout.Width(EditorGUIUtility.labelWidth - 19.0f));

                var toggleWidth = GUILayout.Width(36.0f);
                var toggles = new[]
                {
                    new { Index = 0, Text = "UV", Style = "buttonLeft", Enable = meshInfo.HasUVChannel(0) },
                    new { Index = 1, Text = "UV2", Style = "buttonMid", Enable = meshInfo.HasUVChannel(1) },
                    new { Index = 2, Text = "UV3", Style = "buttonMid", Enable = meshInfo.HasUVChannel(2) },
                    new { Index = 3, Text = "UV4", Style = "buttonRight", Enable = meshInfo.HasUVChannel(3) }
                };
                int oldUVIndex = meshInfo.UVChannelFlag == 0 ? -1 : _meshSettings.UVIndex;

                if (oldUVIndex != -1 && !toggles[oldUVIndex].Enable)
                {
                    oldUVIndex = toggles.FirstOrDefault(t => t.Enable).Index;
                }

                int newUVIndex = oldUVIndex;

                for (int i = 0; i < toggles.Length; ++i)
                {
                    var toggle = toggles[i];
                    using (new EditorGUI.DisabledGroupScope(!toggle.Enable))
                    {
                        if (GUILayout.Toggle(
                            newUVIndex == toggle.Index,
                            toggle.Text,
                            toggle.Style,
                            toggleWidth))
                        {
                            newUVIndex = toggle.Index;
                        }
                    }
                }

                if (oldUVIndex != newUVIndex)
                {
                    Undo.RecordObject(_meshSettings, "Mesh Settings Change");
                    _meshSettings.UVIndex = newUVIndex;
                }

                _uvIndex = newUVIndex;
            }
        }

        private void DrawTextureSettings()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                TextureSourceType oldSourceType = _textureSettings.SourceType;
                TextureSourceType newSourceType = (TextureSourceType)EditorGUILayout.EnumPopup(
                    "Texture",
                    oldSourceType);

                using (new EditorGUI.DisabledGroupScope(!_textureSettings.HasTexture))
                {
                    if (GUILayout.Button("Reload", EditorStyles.miniButton, GUILayout.ExpandWidth(false)))
                    {
                        SetTextureCopy(_textureSettings.Texture);
                    }
                }


                if (newSourceType != oldSourceType)
                {
                    Undo.RecordObject(_textureSettings, "Texture Settings Change");
                    _textureSettings.SourceType = newSourceType;
                }
            }

            using (new EditorGUIIndentLevelScope())
            {
                switch (_textureSettings.SourceType)
                {
                case TextureSourceType.None:
                    break;

                case TextureSourceType.Materials:
                    DrawMaterialSelector();
                    break;

                case TextureSourceType.Custom:
                    Texture2D oldTexture = _textureSettings.CustomTexture;
                    Texture2D newTexture = (Texture2D)EditorGUILayout.ObjectField(
                        "Source",
                        oldTexture,
                        typeof(Texture2D),
                        false,
                        GUILayout.Height(EditorGUIUtility.singleLineHeight));

                    if (newTexture != oldTexture)
                    {
                        Undo.RecordObject(_textureSettings, "Texture Change");

                        _textureSettings.CustomTexture = newTexture;
                        SetTextureCopy(newTexture);
                    }
                    break;
                }

                // 보간 방법.
                int oldFilterMode = _textureSettings.FilterMode;
                int newFilterMode = EditorGUILayout.IntPopup(
                    "Filter Mode",
                    oldFilterMode,
                    new[] { "Point", "Bilinear" },
                    new[] { 0, 1 });

                if (newFilterMode != oldFilterMode)
                {
                    Undo.RecordObject(_textureSettings, "Texture Filter Mode Change");

                    _textureSettings.FilterMode = newFilterMode;
                    _textureCopy.filterMode = (FilterMode)newFilterMode;
                }

                // 반복 여부.
                bool oldRepeating = _textureSettings.Repeating;
                bool newRepeating = EditorGUILayout.Toggle("Repeating", oldRepeating);

                if (newRepeating != oldRepeating)
                {
                    Undo.RecordObject(_textureSettings, "Texture Repeating Change");
                    _textureSettings.Repeating = newRepeating;
                }

                // 색.
                Color oldTextureColor = _textureSettings.Color;
                Color newTextureColor = EditorGUILayout.ColorField("Color", oldTextureColor);

                if (newTextureColor != oldTextureColor)
                {
                    Undo.RecordObject(_textureSettings, "Texture Color Change");
                    _textureSettings.Color = newTextureColor;
                }

                // 채널.
                DrawTextureChannelSelector();
            }
        }

        private void DrawMaterialSelector()
        {
            Material[] materials = _meshSettings.MeshInfo.BuildMaterials();

            if (materials.IsNullOrEmpty())
            {
                _textureSettings.MaterialTexture = null;
                EditorGUILayout.HelpBox("Object has no material.", MessageType.Info);

                return;
            }

            string[] materialNames = materials.Select(mat => mat.name).ToArray();
            int[] materialIds = materials.Select(mat => mat.GetInstanceID()).ToArray();
            int oldMaterialId = _textureSettings.MaterialId;

            if (!materialIds.Contains(oldMaterialId))
            {
                oldMaterialId = materialIds[0];
                _textureSettings.MaterialTexture = null;
            }

            int newMaterialId = EditorGUILayout.IntPopup("Material", oldMaterialId, materialNames, materialIds);
            if (newMaterialId != oldMaterialId)
            {
                Undo.RecordObject(_textureSettings, "Texture Settings Change");

                _textureSettings.MaterialId = newMaterialId;
                _textureSettings.MaterialTexture = null;
            }

            Material material = Array.Find(materials, mat => mat.GetInstanceID() == newMaterialId);
            Shader shader = material.shader;

            var props = Enumerable
                .Range(0, ShaderUtil.GetPropertyCount(shader))
                .Where(i => ShaderUtil.GetPropertyType(shader, i) == ShaderUtil.ShaderPropertyType.TexEnv)
                .Select(i =>
                    {
                        string propName = ShaderUtil.GetPropertyName(shader, i);
                        return new
                        {
                            Name = propName,
                            Description = ShaderUtil.GetPropertyDescription(shader, i),
                            Texture = material.GetTexture(propName) as Texture2D
                        };
                    })
                .Where(prop => prop.Texture != null);

            if (!props.Any())
            {
                EditorGUILayout.HelpBox("Material has no texture.", MessageType.Info);
                return;
            }

            string[] propNames = props.Select(prop => prop.Name).ToArray();
            string[] propDescriptions = props.Select(prop => prop.Description).ToArray();
            Texture2D[] propTextures = props.Select(prop => prop.Texture).ToArray();

            int oldPropIndex = Array.FindIndex(
                propNames,
                propName => propName.Equals(_textureSettings.PropertyName, StringComparison.Ordinal));

            if (oldPropIndex == -1)
            {
                oldPropIndex = 0;
                _textureSettings.MaterialTexture = propTextures[0];

                if (propTextures[0] != _texture)
                {
                    SetTextureCopy(propTextures[0]);
                }
            }

            int newPropIndex = EditorGUILayout.Popup("Property", oldPropIndex, propDescriptions);

            if (newPropIndex != oldPropIndex)
            {
                Undo.RecordObject(_textureSettings, "Texture Settings Change");

                _textureSettings.PropertyName = propNames[newPropIndex];
                _textureSettings.MaterialTexture = propTextures[newPropIndex];
                SetTextureCopy(propTextures[newPropIndex]);
            }
        }

        private void DrawTextureChannelSelector()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PrefixLabel("Channel");

                var toggleWidth = GUILayout.Width(25.0f);
                Action<TextureChannelFlags, string, string> toggle = (channel, label, style) =>
                {
                    bool oldValue = _textureSettings.ChannelFlag.Has(channel);
                    bool newValue= GUILayout.Toggle(oldValue, label, style, toggleWidth);
                    if (oldValue != newValue)
                    {
                        Undo.RecordObject(_textureSettings, "Texture Settings Change");

                        _textureSettings.ChannelFlag = newValue
                            ? _textureSettings.ChannelFlag.Set(channel)
                            : _textureSettings.ChannelFlag.Reset(channel);
                    }
                };

                toggle(TextureChannelFlags.R, "R", "buttonLeft");
                toggle(TextureChannelFlags.G, "G", "buttonMid");
                toggle(TextureChannelFlags.B, "B", "buttonMid");
                toggle(TextureChannelFlags.A, "A", "buttonRight");
            }
        }

        private void DrawViewSettings()
        {
            var settingsFoldOutRect = EditorGUILayout.GetControlRect();
            settingsFoldOutRect.xMin -= 12.0f;

            _foldoutViewSettings = EditorGUI.Foldout(settingsFoldOutRect, _foldoutViewSettings, "Settings");

            if (_foldoutViewSettings)
            {
                _viewSettingsObject.Update();

                EditorGUI.BeginChangeCheck();
                using (new EditorGUIIndentLevelScope())
                {
                    SerializedProperty drawGridProp = _viewSettingsObject.FindProperty("_drawGrid");
                    SerializedProperty lineThicknessProp = _viewSettingsObject.FindProperty("_lineThickness");
                    SerializedProperty lineColorProp = _viewSettingsObject.FindProperty("_lineColor");
                    SerializedProperty drawOutlineProp = _viewSettingsObject.FindProperty("_drawOutline");
                    SerializedProperty outlineColorProp = _viewSettingsObject.FindProperty("_outlineColor");
                    SerializedProperty drawVertexProp = _viewSettingsObject.FindProperty("_drawVertex");
                    SerializedProperty vertexSizeProp = _viewSettingsObject.FindProperty("_vertexSize");
                    SerializedProperty drawVertexOutlineProp = _viewSettingsObject.FindProperty("_drawVertexOutline");
                    SerializedProperty vertexOutlineColorProp = _viewSettingsObject.FindProperty("_vertexOutlineColor");

                    drawGridProp.boolValue = EditorGUILayout.Toggle("Draw Grid", drawGridProp.boolValue);

                    if (EnableGeometryShader)
                    {
                        lineThicknessProp.intValue = EditorGUILayout.IntSlider(
                            "Line Thickness",
                            lineThicknessProp.intValue,
                            1,
                            5);
                    }

                    lineColorProp.colorValue = EditorGUILayout.ColorField("Line Color", lineColorProp.colorValue);
                    drawOutlineProp.boolValue = EditorGUILayout.Toggle("Draw Outline", drawOutlineProp.boolValue);
                    outlineColorProp.colorValue = EditorGUILayout.ColorField(
                        "Outline Color",
                        outlineColorProp.colorValue);

                    drawVertexProp.boolValue = EditorGUILayout.Toggle("Draw Vertex", drawVertexProp.boolValue);
                    vertexSizeProp.intValue = EditorGUILayout.IntSlider("Vertex Size", vertexSizeProp.intValue, 1, 9);
                    drawVertexOutlineProp.boolValue = EditorGUILayout.Toggle(
                        "Draw Vertex Outline",
                        drawVertexOutlineProp.boolValue);
                    vertexOutlineColorProp.colorValue = EditorGUILayout.ColorField(
                        "Vertex Outline Color",
                        vertexOutlineColorProp.colorValue);
                }

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.SetCurrentGroupName("Settings Change");
                }

                _viewSettingsObject.ApplyModifiedProperties();
            }
        }
        #endregion
    }
}
