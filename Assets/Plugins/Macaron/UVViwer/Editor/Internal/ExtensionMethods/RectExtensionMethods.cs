using UnityEngine;

namespace Macaron.UVViewer.Editor.Internal.ExtensionMethods
{
    public static class RectExtensionMethods
    {
        public static bool Contains(this Rect rect, Rect other)
        {
            float xMin1 = rect.xMin;
            float xMin2 = other.xMin;

            if (xMin2 < xMin1)
            {
                return false;
            }

            float xMax1 = rect.xMax;
            float xMax2 = other.xMax;

            if (xMax2 > xMax1)
            {
                return false;
            }

            float yMin1 = rect.yMin;
            float yMin2 = other.yMin;

            if (yMin2 < yMin1)
            {
                return false;
            }

            float yMax1 = rect.yMax;
            float yMax2 = other.yMax;

            if (yMax2 > yMax1)
            {
                return false;
            }

            return true;
        }
    }
}
