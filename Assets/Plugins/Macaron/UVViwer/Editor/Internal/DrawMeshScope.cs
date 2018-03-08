using UnityEngine;
using UnityEditor;

namespace Macaron.UVViewer.Editor.Internal
{
    internal class DrawMeshScope : GUI.Scope
    {
        private readonly Rect _rect;
        private RenderTexture _renderTexture;

        public DrawMeshScope(Rect rect, float scaleFactor)
        {
            _rect = rect;
            _renderTexture = RenderTexture.GetTemporary(
                (int)(rect.width * scaleFactor),
                (int)(rect.height * scaleFactor),
                0);
            _renderTexture.filterMode = FilterMode.Point;

            RenderTexture.active = _renderTexture;
            GUI.matrix = Matrix4x4.identity;

            GL.Clear(true, true, new Color());
        }

        #region Overrides
        protected override void CloseScope()
        {
            RenderTexture.active = null;
            GUI.matrix = Matrix4x4.identity;

            Graphics.DrawTexture(
                _rect,
                _renderTexture,
                new Rect(0.0f, 0.0f, 1.0f, 1.0f),
                0,
                0,
                0,
                0);

            RenderTexture.ReleaseTemporary(_renderTexture);
            _renderTexture = null;
        }
        #endregion
    }
}
