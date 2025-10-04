using System;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Xna.Framework;

namespace LorikeetUI.UIElements;

public class Button : UIElement {
    public Vector2 Position { get; set; }
    public Vector2 Size { get; set; }
    public InputState InputState { get; set; }
    public UIActions Actions { get; set; }
    public RenderState RenderState { get; set; }
    public Vector2 MousePosRelative { get; set; }
    
    public bool Visible { get; set; } = true;

    public string Text { get; set; }
    public Action Clicked { get; set; }

    public Button(Vector2 position, Vector2 size, string text) {
        Position = position;
        Size = size;
        Text = text;
    }

    public void PreDraw() {
        
    }

    public void Draw() {
        Vector2 string_size = Drawing.measure_string_profont(Text);

        var text_pos = Position + (Size / 2f) - (string_size / 2f); 

        if (InputState.MouseOver && !InputState.LeftMouseDown) {
            Drawing.fill_rect(Position, Position + Size, UI.TextColor);
            Drawing.text(Text, text_pos, UI.BackgroundColor);
        } else {
            Drawing.fill_rect(Position, Position + Size, UI.BackgroundColor);
            Drawing.text(Text, text_pos, UI.TextColor);
        }
        
        if (Clicked != null && InputState.JustClicked) Clicked();
        
        Drawing.rect(Position, Position + Size, UI.ForegroundColor, 1f);
    }

    public void Update() { }
}