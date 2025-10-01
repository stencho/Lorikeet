using System;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Xna.Framework;

namespace LorikeetUI.UIElements;

public class Button : UIElement {
    public Vector2 Position { get; set; }
    public Vector2 Size { get; set; }
    public Vector2 MousePosRelative { get; set; }
    public bool MouseOver { get; set; }
    public bool LeftMouseDown { get; set; }

    public string Text { get; set; }
    
    public Button(Vector2 position, Vector2 size, string text) {
        Position = position;
        Size = size;
        Text = text;
    }
    
    public void Draw() {
        string text = MouseOver.ToString();
        Vector2 string_size = Drawing.measure_string_profont(text);

        var text_pos = Position + (Size / 2f) - (string_size / 2f); 

        if (MouseOver && !LeftMouseDown) {
            Drawing.fill_rect(Position, Position + Size, UI.TextColor);
            Drawing.text(text, text_pos, UI.BackgroundColor);
        } else {
            Drawing.fill_rect(Position, Position + Size, UI.BackgroundColor);
            Drawing.text(text, text_pos, UI.TextColor);
        }
        
        Drawing.rect(Position, Position + Size, UI.ForegroundColor, 1f);
    }

    public void Update() {  
        
    }

    public Action OnMouseEnter { get; set; }
    public Action OnMouseLeave { get; set; }
    public Action OnMouseDown { get; set; }
    public Action OnMouseUp { get; set; }
    public Action OnMouseMove { get; set; }
    public Action OnClick { get; set; }
}