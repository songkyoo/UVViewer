using System;
using UnityEngine;
using UnityEditor;

namespace Macaron.UVViewer.Editor.Internal
{
    internal class MeshSettings : ScriptableObject
    {
        private const string _prefsKeyPrefix = "Macaron.UVViewer.Editor.Internal.MeshSettings.";
        private const string _sourceTypeKey = _prefsKeyPrefix + "SourceType";
        private const string _customMeshKey = _prefsKeyPrefix + "CustomMesh";
        private const string _subMeshIndexKey = _prefsKeyPrefix + "SubMeshIndex";
        private const string _uvIndexKey = _prefsKeyPrefix + "UVIndex";

        [SerializeField] private MeshSourceType _sourceType;
        [SerializeField] private MeshInfo _meshInfo = new MeshInfo();
        [SerializeField] private Mesh _customMesh;
        [SerializeField] private int _subMeshIndex = -1;
        [SerializeField] private int _uvIndex;

        #region ScriptableObject Messages
        private void OnEnable()
        {
            _sourceType = EditorPrefsExt.GetEnum(_sourceTypeKey, MeshSourceType.SelectedObject);
            _customMesh = EditorPrefsExt.GetAsset<Mesh>(_customMeshKey);
            _subMeshIndex = EditorPrefs.GetInt(_subMeshIndexKey, -1);
            _uvIndex = Mathf.Clamp(EditorPrefs.GetInt(_uvIndexKey, 0), 0, 4);
        }

        private void OnDisable()
        {
            EditorPrefsExt.SetEnum(_sourceTypeKey, _sourceType);
            EditorPrefsExt.SetAsset(_customMeshKey, _customMesh);
            EditorPrefs.SetInt(_subMeshIndexKey, _subMeshIndex);
            EditorPrefs.SetInt(_uvIndexKey, _uvIndex);
        }
        #endregion

        public MeshSourceType SourceType
        {
            get { return _sourceType; }
            set { _sourceType = value; }
        }

        public MeshInfo MeshInfo
        {
            get { return _meshInfo; }
            set { _meshInfo = value; }
        }

        public Mesh CustomMesh
        {
            get { return _customMesh; }
            set { _customMesh = value; }
        }

        public int SubMeshIndex
        {
            get { return _subMeshIndex; }
            set { _subMeshIndex = value; }
        }

        public int UVIndex
        {
            get { return _uvIndex; }
            set
            {
                if (value < 0 || value > 3)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                _uvIndex = value;
            }
        }
    }
}
