
using System;
using System.Runtime.CompilerServices;
using LorikeetServer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace LorikeetUI.UIElements;

public class Slider : UIElement {
    public Vector2 Position { get; set; }
    public Vector2 Size { get; set; }
    public InputState InputState { get; set; }
    public UIActions Actions { get; set; }
    public RenderState RenderState { get; set; } 
    public bool Visible { get; set; }

    public float Value { get; set; } = 0.5f;
    private float ValuePrevious;

    public float Minimum { get; set; } = 0.0f;
    public float Maximum { get; set; } = 1.0f;

    private float ClickedValue { get; set; } = 0.5f;
    private bool clicked = false;

    public Action ValueChanged { get; set; }
    
    public Color Outline { get; set; } = UI.ForegroundColor;
    public Color Background { get; set; } = UI.BackgroundColor;
    public string Text { get; set; } = ""; 
    
    public Slider(Vector2 position, Vector2 size, bool visible) {
        this.Position = position;
        this.Size = size;
        this.Visible = visible;
        ValuePrevious = Value;
    }

    public void PreDraw() {
        
    }

    public void Draw() {
        Drawing.fill_rect_outline(Position, Position+Size, Background, Outline, 1f);
        
        if (clicked) {
            Drawing.fill_rect(Position + (Vector2.One + Vector2.UnitY), ClickedValue * Size.X - 3, Size.Y-3, UI.ForegroundColor);
        } else {
            Drawing.fill_rect(Position + (Vector2.One + Vector2.UnitY), Value * Size.X - 3, Size.Y - 3, UI.ForegroundColor);
        }
        
        Drawing.rect(Position, Position+Size, Outline, 1f);
    }

    public void Update() {
        if (InputState.JustClicked) clicked = true;
        
        if (clicked) {
            var xv = InputState.MousePosRelative.X / Size.X;
            if (InputState.MousePosRelative.X < 0) ClickedValue = 0f;
            if (InputState.MousePosRelative.X > Size.X) ClickedValue = 1.0f;
            else ClickedValue = xv;

            if (ValuePrevious != ClickedValue) {
                ValueChanged?.Invoke();
            }
            
            if (Input.MouseDelta != Vector2.Zero) {
                ValueChanged?.Invoke();
            }
            
            ValuePrevious = ClickedValue;
        }


        if (InputState.JustReleasedLeftClick) {
            if (InputState.MousePosRelative.Y > 0 || InputState.MousePosRelative.Y < Size.Y) {
                Value = ClickedValue;
                ValueChanged?.Invoke();
            }
            clicked = false;
        }
    }
}