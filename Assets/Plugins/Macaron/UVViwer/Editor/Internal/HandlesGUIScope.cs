using UnityEngine;
using UnityEditor;

namespace Macaron.UVViewer.Editor.Internal
{
    internal class HandlesGUIScope : GUI.Scope
    {
        public HandlesGUIScope()
        {
            Handles.BeginGUI();
        }

        #region Overrides
        protected override void CloseScope()
        {
            Handles.EndGUI();
        }
        #endregion
    }
}
