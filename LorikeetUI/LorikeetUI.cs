using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using LorikeetServer;
using LorikeetUI.UIElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Gtk;
using Button = LorikeetUI.UIElements.Button;

namespace LorikeetUI;

public class LorikeetUI : Game {
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    
    public LorikeetUI() {
        _graphics = new GraphicsDeviceManager(this);
        
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Window.AllowUserResizing = true;
        
        Window.FileDrop += WindowOnFileDrop;
    }

    private void WindowOnFileDrop(object sender, FileDropEventArgs e) {
        foreach (var file in e.Files) {
            Debug.WriteLine("F: " + file);    
        }
        
    }

    protected override void Initialize() {
        Drawing.content = Content;
        
        this.TargetElapsedTime = TimeSpan.FromMilliseconds(1000/180);
        this.InactiveSleepTime = TimeSpan.Zero;

        Application.Init();
        
        base.Initialize();
        
    }
    
    protected override void LoadContent() {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        SDF.load(Content);
        State.Initialize(this, _graphics, GraphicsDevice, Content, Window);
        
        MappedLEDStrip.OpenMap();
        MappedLEDStrip.SetBrightness(255);
        
        Drawing.load(GraphicsDevice, _graphics, Content, State.WindowBounds);

        UI.AddElement("groups", 
            new LEDGroupManager(Vector2.One , Vector2.One * 1000,
            new LEDGroupManager.LEDGroup(5, new LED(255,0,255), true),
            new LEDGroupManager.LEDGroup(5, new LED(0,255,0)),
            new LEDGroupManager.LEDGroup(5, new LED(255,0,255), true),
            new LEDGroupManager.LEDGroup(5, new LED(0,255,0)),
            new LEDGroupManager.LEDGroup(5, new LED(255,0,255), true)
            ));

        UI.AddElement("brightness", new Slider(new Vector2(90f, 1f), new Vector2(50f, 14), true));
        ((Slider)UI.Elements["brightness"]).ValueChanged = () => {
            MappedLEDStrip.SetBrightness((byte)(((Slider)UI.Elements["brightness"]).Value * 255));
        };
        var close_button_size = new Vector2(20, 20);
        UI.AddElement("close_button", new Button(
            (Vector2.UnitX * (Window.ClientBounds.Size.X - close_button_size.X)) , close_button_size, "x"));
        ((Button)UI.Elements["close_button"]).Clicked = () => {
            Exit();
        };
        
        UI.AddElement("image_file_pick_button", new Button(
                Vector2.Zero + Vector2.UnitX, new Vector2(70,20), "Pick File"));
        UI.Elements["image_file_pick_button"].Actions.OnClick = () => {
            string file_picked = null;
            using (var dialog = new FileChooserDialog(
                       "Choose image",
                       null, FileChooserAction.Open, 
                       "Cancel", ResponseType.Cancel,
                           "Open", ResponseType.Accept)) {
                if (dialog.Run() == (int)ResponseType.Accept) {
                    file_picked = dialog.Filename;
                    ((LEDGroupManager)UI.Elements["groups"]).AddImage(file_picked);
                }
                dialog.Destroy();
            }
            
            while (Gtk.Application.EventsPending())
                Gtk.Application.RunIteration();
            
            
        };

        UI.Elements["groups"].Size = State.WindowBounds.ToVector2();
        
    }
    
    protected override unsafe void Update(GameTime gameTime) {
        State.Update(gameTime);
        
        if (Input.keyboard.IsKeyDown(Keys.Escape)) Exit();
        
        UI.Update();
        UI.Elements["groups"].Size = State.WindowBounds.ToVector2();
        UI.Elements["close_button"].Position = Vector2.UnitX * (Window.ClientBounds.Width - 20);
        
        Clock.TickRateUpdate(gameTime.ElapsedGameTime.TotalMilliseconds);
        base.Update(gameTime);
    }
    
    protected override unsafe void Draw(GameTime gameTime) {
        GraphicsDevice.Clear(UI.BackgroundColor);
        GraphicsDevice.SetRenderTarget(null);
        UI.PreDraw();
        GraphicsDevice.SetRenderTarget(null);
        
        UI.Draw();
        
        var b = MappedLEDStrip.strip->brightness;

        float brightness = b / 255f;
        
        string tps = Clock.update_thread_tick_rate.ToString();
        var fps = $"{Clock.frame_rate.ToString()} FPS :: {tps} Ticks/sec";
        
        var fps_width = Drawing.measure_string_profont(fps).X;

        string title = $"Lorikeet";
        
        var title_width = Drawing.measure_string_profont(title).X;
        Drawing.text(title, Vector2.UnitX * ((State.WindowBounds.X / 2f) - (title_width / 2f)) + (Vector2.UnitY * 2f), UI.BackgroundColor);
        Drawing.text(fps, Vector2.UnitX * (State.WindowBounds.X - (fps_width+30)) + (Vector2.UnitY * 2f), UI.BackgroundColor);

        Clock.FrameRateUpdate(gameTime.ElapsedGameTime.TotalMilliseconds);
        base.Draw(gameTime);
    }
    
    ~LorikeetUI() {
        MappedLEDStrip.CloseMap();
    }
}