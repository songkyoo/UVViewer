using UnityEngine;

namespace Macaron.UVViewer.Editor.Internal
{
    internal struct GridTextInfo
    {
        public readonly Vector3 Position;
        public readonly string Text;

        public GridTextInfo(float x, float y, string text)
        {
            Position = new Vector3(x, y);
            Text = text;
        }
    }
}
