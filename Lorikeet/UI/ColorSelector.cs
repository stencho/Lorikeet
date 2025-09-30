using System;
using Bumpo;
using Lorikeet.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Lorikeet.UI;

public class ColorSelector {
    private RenderTarget2D color_map;
    public Vector2 position = Vector2.Zero;
    private Vector2 size = Vector2.Zero;

    private bool initialized = false;
    public bool Initialized => initialized;

    public Color mouse_over_color = Color.White;
    
    public ColorSelector(Vector2 position, int width, int height) {
        size = new Vector2(width, height);
        this.position = position;
    }
    
    public static (byte R, byte G, byte B) FromSquare(float x, float y, float value) {
        // clamp inputs
        x = Math.Clamp(x, 0f, 1f);
        y = Math.Clamp(y, 0f, 1f);
        value = Math.Clamp(value, 0f, 1f);

        float hue = x;        // X maps to hue
        float sat = y;        // Y maps to saturation
        float val = value;    // slider maps to value/brightness

        var (r, g, b) = HSVtoRGB(hue, sat, val);

        // convert to 0–255 bytes
        return (
            (byte)(r * 255),
            (byte)(g * 255),
            (byte)(b * 255)
        );
    }

    public static (float R, float G, float B) HSVtoRGB(float h, float s, float v) {
        h = (h % 1f) * 6f; // map 0–1 hue into 0–6
        int i = (int)Math.Floor(h);
        float f = h - i;
        float p = v * (1f - s);
        float q = v * (1f - f * s);
        float t = v * (1f - (1f - f) * s);

        return i switch
        {
            0 => (v, t, p),
            1 => (q, v, p),
            2 => (p, v, t),
            3 => (p, q, v),
            4 => (t, p, v),
            _ => (v, p, q),
        };
    }

    public Action Clicked { get; set; } 
    
    public void Update() {
        if (Collision2D.v2_intersects_rect(State.mouse_pos, position, position + size)) {

            var relative = State.mouse_pos - position;
            var coords = relative / size;

            var RGB = FromSquare(coords.X, coords.Y, 1.0f);
            mouse_over_color = Color.FromNonPremultiplied(RGB.R, RGB.G, RGB.B, 255);


            if (State.mouse.LeftButton == ButtonState.Pressed) {
                if (Clicked != null) {
                    Clicked.Invoke();
                }
            }
        } else
            mouse_over_color = Color.White;
    }

    public void GenerateMap() {
        if (initialized) return;
        
        color_map = new RenderTarget2D(State.graphics_device, (int)size.X, (int)size.Y);
        
        State.graphics_device.SetRenderTarget(color_map);
        
        ColorMap cm = new ColorMap();
        
        cm.draw(Vector2.Zero, size);

        initialized = true;
    }
    
    public void Draw() {
        Drawing.image(color_map, Vector2.One * 100, size);
    }
}