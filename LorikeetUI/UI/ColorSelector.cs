using System;
using Bumpo;
using LorikeetUI.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace LorikeetUI.UIElements;

public class ColorSelector : UIElement {
    private RenderTarget2D color_map;
    public Vector2 Position { get; set; } = Vector2.Zero;
    public Vector2 Size { get; set; } = Vector2.Zero;
    public Vector2 MousePosRelative { get; set; }
    
    public bool MouseOver { get; set; }
    public bool LeftMouseDown { get; set; }
    public bool Visible { get; set; } = true;
    
    public Vector2 relativePosition { get; set; } = Vector2.Zero;
    
    private bool initialized = false;
    public bool Initialized => initialized;

    public Color mouse_over_color = Color.White;

    public Action OnMouseLeave { get; set; }
    public Action OnMouseDown { get; set; }
    public Action OnMouseUp { get; set; }
    public Action OnMouseMove { get; set; }
    public Action OnClick { get; set; }

    public ColorSelector(Vector2 position, int width, int height) {
        Size = new Vector2(width, height);
        this.Position = position;
    }
    
    public static (byte R, byte G, byte B) FromSquare(float x, float y, float value) {
        x = Math.Clamp(x, 0f, 1f);
        y = Math.Clamp(y, 0f, 1f);
        value = Math.Clamp(value, 0f, 1f);

        float hue = x;
        float sat = y;
        float val = value;

        var (r, g, b) = ColorUtils.HSVtoRGB(hue, sat, val);

        return (r, g, b);
    }
    
    public void Update() {
        if (State.game.IsActive && Collision2D.v2_intersects_rect(Input.mouse_pos, Position, Position + Size)) {

            var relative = Input.mouse_pos - Position;
            var coords = relative / Size;

            var RGB = FromSquare(coords.X, coords.Y, 1.0f);
            mouse_over_color = Color.FromNonPremultiplied(RGB.R, RGB.G, RGB.B, 255);

            if (Input.mouse.LeftButton == ButtonState.Pressed) {
                //if (Clicked != null) {
                //    Clicked.Invoke();
                //}
            }
        } else
            mouse_over_color = Color.White;
    }

    public Action OnMouseEnter { get; set; }


    public void GenerateMap() {
        if (initialized) return;
        
        color_map = new RenderTarget2D(State.graphics_device, 512, 512);
        
        State.graphics_device.SetRenderTarget(color_map);
        
        ColorMap cm = new ColorMap();
        
        cm.draw(Vector2.Zero, Vector2.One * 512);

        initialized = true;
    }

    public void Draw() {
        Drawing.image(color_map, Vector2.One * 100, Size);
    }
}