using System;
using UnityEngine;
using UnityEditor;

namespace Macaron.UVViewer.Editor.Internal
{
    public class ViewSettings : ScriptableObject
    {
        private const string _prefsKeyPrefix = "Macaron.UVViewer.Editor.Internal.ViewSettings.";
        private const string _drawGridKey = _prefsKeyPrefix + "DrawGrid";
        private const string _lineThicknessKey = _prefsKeyPrefix + "LineThickness";
        private const string _lineColorKey = _prefsKeyPrefix + "LineColor";
        private const string _drawOutlineKey = _prefsKeyPrefix + "DrawOutline";
        private const string _outlineColorKey = _prefsKeyPrefix + "OutlineColor";
        private const string _drawVertexKey = _prefsKeyPrefix + "DrawVertex";
        private const string _vertexSizeKey = _prefsKeyPrefix + "VertexSize";
        private const string _drawVertexOutlineKey = _prefsKeyPrefix + "DrawVertexOutline";
        private const string _vertexOutlineColorKey = _prefsKeyPrefix + "VertexOutlineColor";

        [SerializeField] private bool _drawGrid = true;
        [SerializeField] private int _lineThickness = 1;
        [SerializeField] private Color _lineColor = Color.white;
        [SerializeField] private bool _drawOutline = true;
        [SerializeField] private Color _outlineColor = new Color(0.0f, 0.0f, 0.0f, 0.5f);
        [SerializeField] private bool _drawVertex = true;
        [SerializeField] private int _vertexSize = 5;
        [SerializeField] private bool _drawVertexOutline = true;
        [SerializeField] private Color _vertexOutlineColor = new Color(0.0f, 0.0f, 0.0f, 0.5f);

        #region ScriptableObject Messages
        private void OnEnable()
        {
            _drawGrid = EditorPrefs.GetBool(_drawGridKey, true);
            _lineThickness = Mathf.Clamp(EditorPrefs.GetInt(_lineThicknessKey, 1), 1, 5);
            _lineColor = EditorPrefsExt.GetColor(_lineColorKey, Color.white);
            _drawOutline = EditorPrefs.GetBool(_drawOutlineKey, true);
            _outlineColor =  EditorPrefsExt.GetColor(_outlineColorKey, new Color(0.0f, 0.0f, 0.0f, 0.5f));
            _drawVertex = EditorPrefs.GetBool(_drawVertexKey, true);
            _vertexSize = Mathf.Clamp(EditorPrefs.GetInt(_vertexSizeKey, 5), 1, 9);
            _drawVertexOutline = EditorPrefs.GetBool(_drawVertexOutlineKey, true);
            _vertexOutlineColor =  EditorPrefsExt.GetColor(_vertexOutlineColorKey, new Color(0.0f, 0.0f, 0.0f, 0.5f));
        }

        private void OnDisable()
        {
            EditorPrefs.SetBool(_drawGridKey, _drawGrid);
            EditorPrefs.SetInt(_lineThicknessKey, _lineThickness);
            EditorPrefsExt.SetColor(_lineColorKey, _lineColor);
            EditorPrefs.SetBool(_drawOutlineKey, _drawOutline);
            EditorPrefsExt.SetColor(_outlineColorKey, _outlineColor);
            EditorPrefs.SetBool(_drawVertexKey, _drawVertex);
            EditorPrefs.SetInt(_vertexSizeKey, _vertexSize);
            EditorPrefs.SetBool(_drawVertexOutlineKey, _drawVertexOutline);
            EditorPrefsExt.SetColor(_vertexOutlineColorKey, _vertexOutlineColor);
        }
        #endregion

        public bool DrawGrid
        {
            get { return _drawGrid; }
            set { _drawGrid = value; }
        }

        public int LineThickness
        {
            get { return _lineThickness; }
            set { _lineThickness = value; }
        }

        public Color LineColor
        {
            get { return _lineColor; }
            set { _lineColor = value; }
        }

        public bool DrawOutline
        {
            get { return _drawOutline; }
            set { _drawOutline = value; }
        }

        public Color OutlineColor
        {
            get { return _outlineColor; }
            set { _outlineColor = value; }
        }

        public bool DrawVertex
        {
            get { return _drawVertex; }
            set { _drawVertex = value; }
        }

        public int VertexSize
        {
            get { return _vertexSize; }
            set
            {
                if (value < 1 || value > 9)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                _vertexSize = value;
            }
        }

        public bool DrawVertexOutline
        {
            get { return _drawVertexOutline; }
            set { _drawVertexOutline = value; }
        }

        public Color VertexOutlineColor
        {
            get { return _vertexOutlineColor; }
            set { _vertexOutlineColor = value; }
        }
    }
}
