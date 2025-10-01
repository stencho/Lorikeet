using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace LorikeetUI;

public static class State {
    public static Game game;
    public static GameWindow window;
    
    public static GraphicsDeviceManager graphics;
    public static GraphicsDevice graphics_device;
    public static ContentManager content;

    public static KeyboardState keyboard_state;
    public static KeyboardState keyboard_state_previous;
    
    public static MouseState mouse;
    public static MouseState mouse_state_previous;

    public static Vector2 mouse_pos => mouse.Position.ToVector2();
    
    public static void Initialize(Game game, GraphicsDeviceManager graphics, GraphicsDevice graphics_device, ContentManager content, GameWindow window) {
        State.graphics = graphics;
        State.graphics_device = graphics_device;
        State.content = content;
        State.window = window;
        State.game = game;
    }

    public static void Update() {
        mouse_state_previous = mouse;
        mouse = Mouse.GetState();

        keyboard_state_previous = keyboard_state;
        keyboard_state = Keyboard.GetState();
        
        
    }
}