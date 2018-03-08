using System;
using UnityEngine;
using UnityEditor;

namespace Macaron.UVViewer.Editor.Internal
{
    internal class TextureSettings : ScriptableObject
    {
        private const string _prefsKeyPrefix = "Macaron.UVViewer.Editor.Internal.TextureSettings.";
        private const string _sourceTypeKey = _prefsKeyPrefix + "SourceType";
        private const string _materialIdKey = _prefsKeyPrefix + "MaterialId";
        private const string _propertyNameKey = _prefsKeyPrefix + "PropertyName";
        private const string _customTextureKey = _prefsKeyPrefix + "CustomTexture";
        private const string _filterModeKey = _prefsKeyPrefix + "FilterMode";
        private const string _repeatingKey = _prefsKeyPrefix + "Repeating";
        private const string _colorKey = _prefsKeyPrefix + "Color";
        private const string _channelFlagKey = _prefsKeyPrefix + "ChannelFlag";

        [SerializeField] private TextureSourceType _sourceType;
        [SerializeField] private int _materialId;
        [SerializeField] private string _propertyName = string.Empty;
        [SerializeField] private Texture2D _materialTexture;
        [SerializeField] private Texture2D _customTexture;
        [SerializeField] private int _filterMode = 1;
        [SerializeField] private bool _repeating;
        [SerializeField] private Color _color = Color.white;
        [SerializeField] private TextureChannelFlags _channelFlag = TextureChannelFlags.RGBA;

        #region ScriptableObject Messages
        private void OnEnable()
        {
            _sourceType = EditorPrefsExt.GetEnum(_sourceTypeKey, TextureSourceType.None);
            _materialId = EditorPrefs.GetInt(_materialIdKey, 0);
            _propertyName = EditorPrefs.GetString(_propertyNameKey, string.Empty);
            _filterMode = Mathf.Clamp(EditorPrefs.GetInt(_filterModeKey, 1), 0, 1);
            _customTexture = EditorPrefsExt.GetAsset<Texture2D>(_customTextureKey);
            _repeating = EditorPrefs.GetBool(_repeatingKey, false);
            _color = EditorPrefsExt.GetColor(_colorKey, Color.white);
            _channelFlag = EditorPrefsExt.GetEnum(_channelFlagKey, TextureChannelFlags.RGBA);
        }

        private void OnDisable()
        {
            EditorPrefsExt.SetEnum(_sourceTypeKey, _sourceType);
            EditorPrefs.SetInt(_materialIdKey, _materialId);
            EditorPrefs.SetString(_propertyNameKey, _propertyName);
            EditorPrefsExt.SetAsset(_customTextureKey, _customTexture);
            EditorPrefs.SetInt(_filterModeKey, _filterMode);
            EditorPrefs.SetBool(_repeatingKey, _repeating);
            EditorPrefsExt.SetColor(_colorKey, _color);
            EditorPrefsExt.SetEnum(_channelFlagKey, _channelFlag);
        }
        #endregion

        public TextureSourceType SourceType
        {
            get { return _sourceType; }
            set { _sourceType = value; }
        }

        public Texture2D MaterialTexture
        {
            get { return _materialTexture; }
            set { _materialTexture = value; }
        }

        public Texture2D CustomTexture
        {
            get { return _customTexture; }
            set { _customTexture = value; }
        }

        public int MaterialId
        {
            get { return _materialId; }
            set { _materialId = value; }
        }

        public string PropertyName
        {
            get { return _propertyName; }
            set { _propertyName = value; }
        }

        public int FilterMode
        {
            get { return _filterMode; }
            set
            {
                if (value < 0 || value > 1)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                _filterMode = value;
            }
        }

        public bool Repeating
        {
            get { return _repeating; }
            set { _repeating = value; }
        }

        public Color Color
        {
            get { return _color; }
            set { _color = value; }
        }

        public TextureChannelFlags ChannelFlag
        {
            get { return _channelFlag; }
            set { _channelFlag = value; }
        }

        public bool HasTexture
        {
            get
            {
                switch (_sourceType)
                {
                case TextureSourceType.Materials:
                    return _materialTexture != null;

                case TextureSourceType.Custom:
                    return _customTexture != null;
                }

                return false;
            }
        }

        public Texture2D Texture
        {
            get
            {
                switch (_sourceType)
                {
                case TextureSourceType.Materials:
                    return _materialTexture;

                case TextureSourceType.Custom:
                    return _customTexture;
                }

                return null;
            }
        }
    }
}
