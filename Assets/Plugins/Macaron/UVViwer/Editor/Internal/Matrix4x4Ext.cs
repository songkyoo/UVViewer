using UnityEngine;

namespace Macaron.UVViewer.Editor.Internal
{
    public static class Matrix4x4Ext
    {
        public static Matrix4x4 Translate(Vector3 vector)
        {
            Matrix4x4 mat = Matrix4x4.identity;
            mat.SetColumn(3, new Vector4(vector.x, vector.y, vector.z, 1.0f));

            return mat;
        }
    }
}
