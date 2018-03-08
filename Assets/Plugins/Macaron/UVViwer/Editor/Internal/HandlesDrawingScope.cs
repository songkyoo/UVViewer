using UnityEngine;
using UnityEditor;

namespace Macaron.UVViewer.Editor.Internal
{
    internal class HandlesDrawingScope : GUI.Scope
    {
        private readonly Color _color;
        private readonly Matrix4x4 _matrix;

        public HandlesDrawingScope(Color color, Matrix4x4 matrix)
        {
            _color = Handles.color;
            _matrix = Handles.matrix;

            Handles.color = color;
            Handles.matrix = matrix;
        }

        public HandlesDrawingScope(Color color) : this(color, Handles.matrix)
        {
        }

        public HandlesDrawingScope(Matrix4x4 matrix) : this(Handles.color, matrix)
        {
        }

        #region Overrides
        protected override void CloseScope()
        {
            Handles.color = _color;
            Handles.matrix = _matrix;
        }
        #endregion
    }
}
