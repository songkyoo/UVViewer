using System;
using UnityEngine;
using UnityEditor;

namespace Macaron.UVViewer.Editor.Internal
{
    internal static class EditorResources
    {
        private const int _titleIconWidth = 14;
        private const int _titleIconHeight = 14;
        private const TextureFormat _titleIconFormat = TextureFormat.RGBA32;
        private const string _titleIconData = "bm5uAG5ubgBfX18AX19fAF9fXwBfX18AX19fAF9fXwBfX18AX19fAF9fXwBubm4Abm5uAG5ubgBubm4Abm5uAF9fXwBfX18AX19fAF9fXwBfX18AX19fAF9fXwBfX18AX19fAG5ubgBubm4Abm5uAG5ubgBubm7/X19f/19fX/9fX1//X19f/19fX/9fX1//X19f/19fX/9fX1//bm5u/25ubgBubm4AX19fAF9fX/9fX18Atra2/19fXwBfX18AiomJ/19fXwBfX18Atra2/19fXwBfX1//X19fAF9fXwBfX18AX19f/7a2tv+2trb/tra2/7a2tv+KiYn/tra2/7a2tv+2trb/tra2/19fX/9fX18AX19fAF9fXwBfX1//tra2ALa2tv+2trYAtra2AIqJif+2trYAtra2ALa2tv+2trYAX19f/19fXwBfX18AX19fAF9fX/9fX18Atra2/7a2tgCKiYkAiomJ/4qJiQC2trYAtra2/7a2tgBfX1//X19fAF9fXwBfX18AX19f/4qJif+KiYn/iomJ/4qJif+KiYn/iomJ/4qJif+KiYn/iomJ/19fX/9fX18AX19fAF9fXwBfX1//iomJALa2tv+KiYkAiomJAIqJif+KiYkAiomJALa2tv+KiYkAX19f/19fXwBfX18AX19fAF9fX/9fX18Atra2/7a2tgCKiYkAiomJ/4qJiQC2trYAtra2/7a2tgBfX1//X19fAF9fXwBfX18AX19f/7a2tv+2trb/tra2/7a2tv+KiYn/tra2/7a2tv+2trb/tra2/19fX/9fX18AX19fAF9fXwBfX1//tra2ALa2tv+2trYAtra2AIqJif+2trYAtra2ALa2tv+2trYAX19f/19fXwBfX18Abm5uAG5ubv9fX1//X19f/19fX/9fX1//X19f/19fX/9fX1//X19f/19fX/9ubm7/bm5uAG5ubgBubm4Abm5uAF9fXwBfX18AX19fAF9fXwBfX18AX19fAF9fXwBfX18AX19fAG5ubgBubm4Abm5uAA==";
        private const int _zoomIconWidth = 14;
        private const int _zoomIconHeight = 14;
        private const TextureFormat _zoomIconFormat = TextureFormat.RGBA32;
        private const string _zoomIconData = "////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///9mMzMz/21tbbP///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////ZjAwMP8wMDD/GBgY/////wD///8A////AP///wD///8A////AP///wD///8A////AP///2YtLS3/LS0t/xcXF/8YGBgA////AP///wH///88////W////2X///9l////W////zz///9mKSkp/ykpKf8VFRX/FxcXABgYGAD///8B+fn5TlRUVMEvLy/uJiYm/iYmJv4vLy/uVFRUwSUlJf8lJSX/ExMT/xUVFQAXFxcAGBgYAPj4+D47OzvZICAg/hoaGv8UFBT/FBQU/xoaGv8gICD+IiIi/xsbG/8TExMAFRUVABcXFwAYGBgASUlJvB4eHv4cHBzqDw8Pfw8PDyoPDw8qDw8PfxwcHOoeHh7+SUlJvElJSQBJSUkASUlJAElJSQAkJCTuGRkZ/x8fH4gPDw8ADw8PAA8PDwAPDw8AHx8fiBkZGf8kJCTuJCQkACQkJAAkJCQAJCQkABYWFv4VFRX/SUlJOElJSQAPDw8ADw8PAElJSQBJSUk4FRUV/xYWFv4WFhYAFhYWABYWFgAWFhYAEhIS/hISEv+IiIhViIiIAP///wD///8AiIiIAIiIiFUSEhL/EhIS/hISEgASEhIAEhISABISEgATExPqDg4O/05OTq3///8z////Ef///xH///8zTk5OrQ4ODv8TExPqExMTABMTEwATExMAExMTAAUFBZcJCQn+FRUV71BQULKurq5/rq6uf1BQULIVFRXvCQkJ/gUFBZcFBQUABQUFAAUFBQAFBQUABAQEAgQEBMAGBgb+BwcH/wcHB/8HBwf/BwcH/wYGBv4EBATABAQEAgQEBAAEBAQABAQEAAQEBAAEBAQAAgICAgICApcCAgLjAgIC/QICAv0CAgLjAgIClwICAgIEBAQABAQEAAQEBAAEBAQABAQEAA==";
        private const int _resizeHandleBackgroundWidth = 2;
        private const int _resizeHandleBackgroundHeight = 17;
        private const TextureFormat _resizeHandleBackgroundFormat = TextureFormat.RGB24;
        private const string _resizeHandleBackgroundData = "QUFBQUFBVFRUVFRUVlZWVlZWWFhYWFhYWlpaWlpaXFxcXFxcX19fX19fYWFhYWFhZGRkZGRkZmZmZmZmaWlpaWlpa2tra2trbW1tbW1tb29vb29vcXFxcXFxgYGBgYGBTU1NTU1N";
        private const int _resizeHandleWidth = 8;
        private const int _resizeHandleHeight = 17;
        private const TextureFormat _resizeHandleFormat = TextureFormat.RGBA32;
        private const string _resizeHandleData = "Pj4+AD4+PgA+Pj4APj4+AD4+PgA+Pj4APj4+AD4+PgA+Pj4APj4+AD4+PgA+Pj4APj4+AD4+PgA+Pj4APj4+AD4+PgA+Pj4APj4+AD4+PgA+Pj4APj4+AD4+PgA+Pj4APj4+AD4+PgA+Pj4APj4+AD4+PgA+Pj4APj4+AD4+PgA+Pj4APj4+AD4+PgA+Pj4APj4+AD4+PgA+Pj4APj4+AD4+PgA+Pj4APj4+AD4+Pv8+Pj7/Pj4+AD4+PgA+Pj4AsbGxALGxsQCxsbEAsbGx/7Gxsf+xsbEAsbGxALGxsQCxsbEAsbGxALGxsQCxsbEAsbGxALGxsQCxsbEAsbGxAEFBQQBBQUEAQUFBAEFBQf9BQUH/QUFBAEFBQQBBQUEAtLS0ALS0tAC0tLQAtLS0/7S0tP+0tLQAtLS0ALS0tAC0tLQAtLS0ALS0tAC0tLQAtLS0ALS0tAC0tLQAtLS0ALS0tAC0tLQAtLS0ALS0tAC0tLQAtLS0ALS0tAC0tLQAtLS0ALS0tAC0tLQAtLS0ALS0tAC0tLQAtLS0ALS0tAC0tLQAtLS0ALS0tAC0tLQAtLS0ALS0tAC0tLQAtLS0ALS0tAC0tLQAtLS0ALS0tAC0tLQAtLS0ALS0tAC0tLQAtLS0ALS0tAC0tLQAtLS0ALS0tAC0tLQAtLS0ALS0tAC0tLQAtLS0ALS0tAC0tLQAtLS0ALS0tAC0tLQAtLS0AA==";

        private static Texture2D _titleIcon;
        private static Texture2D _zoomIcon;
        private static Texture2D _resizeHandleBackground;
        private static GUIStyle _resizeHandleBackgroundStyle;
        private static Texture2D _resizeHandle;
        private static GUIStyle _resizeHandleStyle;

        public static Texture2D TitleIcon
        {
            get
            {
                if (_titleIcon == null)
                {
                    _titleIcon = CreateTexture(_titleIconWidth, _titleIconHeight, _titleIconFormat, _titleIconData);
                }

                return _titleIcon;
            }
        }

        public static Texture2D ZoomIcon
        {
            get
            {
                if (_zoomIcon == null)
                {
                    _zoomIcon = CreateTexture(_zoomIconWidth, _zoomIconHeight, _zoomIconFormat, _zoomIconData);
                }

                return _zoomIcon;
            }
        }

        public static GUIStyle ResizeHandleBackgroundStyle
        {
            get
            {
                if (_resizeHandleBackground == null)
                {
                    _resizeHandleBackground = CreateTexture(
                        _resizeHandleBackgroundWidth,
                        _resizeHandleBackgroundHeight,
                        _resizeHandleBackgroundFormat,
                        _resizeHandleBackgroundData);
                }

                if (_resizeHandleBackgroundStyle == null)
                {
                    _resizeHandleBackgroundStyle = new GUIStyle
                    {
                        normal = new GUIStyleState { background = _resizeHandleBackground },
                        border = new RectOffset(1, 1, 0, 0)
                    };
                }

                return _resizeHandleBackgroundStyle;
            }
        }

        public static GUIStyle ResizeHandleStyle
        {
            get
            {
                if (_resizeHandle == null)
                {
                    _resizeHandle = CreateTexture(
                        _resizeHandleWidth,
                        _resizeHandleHeight,
                        _resizeHandleFormat,
                        _resizeHandleData);
                }

                if (_resizeHandleStyle == null)
                {
                    _resizeHandleStyle = new GUIStyle
                    {
                        normal = new GUIStyleState { background = _resizeHandle },
                        border = new RectOffset(4, 4, 0, 0)
                    };
                }

                return _resizeHandleStyle;
            }
        }

        private static Texture2D CreateTexture(int width, int height, TextureFormat format, string data)
        {
            var tex = new Texture2D(width, height, format, false)
            {
                hideFlags = HideFlags.DontSave,
                filterMode = FilterMode.Point
            };
            tex.LoadRawTextureData(Convert.FromBase64String(data));
            tex.Apply();

            return tex;
        }
    }
}
