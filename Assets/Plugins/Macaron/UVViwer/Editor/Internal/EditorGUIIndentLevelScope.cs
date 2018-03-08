using UnityEngine;
using UnityEditor;

namespace Macaron.UVViewer.Editor.Internal
{
    internal class EditorGUIIndentLevelScope : GUI.Scope
    {
        public EditorGUIIndentLevelScope()
        {
            EditorGUI.indentLevel += 1;
        }

        #region Overrides
        protected override void CloseScope()
        {
            EditorGUI.indentLevel -= 1;
        }
        #endregion
    }
}
