using System;

namespace LorikeetUI;

public static class ColorUtils {
    public static (byte R, byte G, byte B) HSVtoRGB(float h, float s, float v) {
        h = (h % 1f) * 6f;
        int i = (int)Math.Floor(h);
        float f = h - i;
        float p = v * (1f - s);
        float q = v * (1f - f * s);
        float t = v * (1f - (1f - f) * s);

        return i switch
        {
            0 => ((byte)(v * 255), (byte)(t * 255), (byte)(p * 255)),
            1 => ((byte)(q * 255), (byte)(v * 255), (byte)(p * 255)),
            2 => ((byte)(p * 255), (byte)(v * 255), (byte)(t * 255)),
            3 => ((byte)(p * 255), (byte)(q * 255), (byte)(v * 255)),
            4 => ((byte)(t * 255), (byte)(p * 255), (byte)(v * 255)),
            _ => ((byte)(v * 255), (byte)(p * 255), (byte)(q * 255)),
        };
    }

    public static (float h, float s, float v) RGBtoHSV(byte r, byte g, byte b)
    {
        float rf = r / 255f;
        float gf = g / 255f;
        float bf = b / 255f;

        float max = Math.Max(rf, Math.Max(gf, bf));
        float min = Math.Min(rf, Math.Min(gf, bf));
        float delta = max - min;

        float h = 0f;
        if (delta > 0f)
        {
            if (max == rf)
                h = (gf - bf) / delta;
            else if (max == gf)
                h = 2f + (bf - rf) / delta;
            else
                h = 4f + (rf - gf) / delta;

            h /= 6f;
            if (h < 0f) h += 1f;
        }

        float s = max == 0f ? 0f : delta / max;
        float v = max;

        return (h, s, v);
    }
}