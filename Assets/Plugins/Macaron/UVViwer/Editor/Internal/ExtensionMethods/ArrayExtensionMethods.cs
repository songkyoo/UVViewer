namespace Macaron.UVViewer.Editor.Internal.ExtensionMethods
{
    public static class ArrayExtensionMethods
    {
        public static bool IsNullOrEmpty<T>(this T[] array)
        {
            return array == null || array.Length == 0;
        }
    }
}
