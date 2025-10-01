using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace LorikeetUI.UI;

public static class UIElementManager {
    public static Dictionary<string, UIElement> Elements = new Dictionary<string, UIElement>();

    public static void AddElement(string name, UIElement element) {
        Elements.Add(name, element);
    }
}

public abstract class UIElement {
    public Vector2 position { get; set; }
    public Vector2 size { get; set; }

    public void Draw() { }
    public void Update() { }

    public UIElement() {
        
    }
}