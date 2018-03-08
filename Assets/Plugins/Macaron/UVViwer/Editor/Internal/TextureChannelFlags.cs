using System;

namespace Macaron.UVViewer.Editor.Internal
{
    [Flags]
    internal enum TextureChannelFlags
    {
        None = 0x00,
        R = 0x01,
        G = 0x02,
        B = 0x04,
        A = 0x08,
        RGBA = 0x0F,
    }

    internal static class TextureChannelFlagsExtensionMethods
    {
        public static bool Has(this TextureChannelFlags state, TextureChannelFlags flag)
        {
            return (state & flag) != TextureChannelFlags.None;
        }

        public static TextureChannelFlags Set(this TextureChannelFlags state, TextureChannelFlags flag)
        {
            return state | flag;
        }

        public static TextureChannelFlags Reset(this TextureChannelFlags state, TextureChannelFlags flag)
        {
            return state & ~flag;
        }

        public static TextureChannelFlags Invert(this  TextureChannelFlags state, TextureChannelFlags flag)
        {
            return state ^ flag;
        }
    }
}
