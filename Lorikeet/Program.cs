namespace Lorikeet;

class Program {
    private static LEDStrip strip;
    
    internal static CancellationTokenSource LED_cancellation_token_source = new CancellationTokenSource();
    internal static CancellationToken LED_cancellation_token => LED_cancellation_token_source.Token;
    
    static void Main(string[] args) {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.CancelKeyPress += delegate (object? sender, ConsoleCancelEventArgs e) { e.Cancel = true; Exit(); };
        
        strip = new LEDStrip(
            new StripSection(255,0,255, 6), 
            new StripSection(0, 255, 0, 6),
            new StripSection(255,0,255, 6), 
            new StripSection(0,255, 0, 6)
            );
        
        Start(args);
    }
    
    static void Start(string[] args) {
        Logging.Start();

        Logging.Config("Connecting to arduino...");
        
        Serial.Connect();

        if (!Serial.Connected) {
            while (!Serial.Connected && !Tasks.cancellation_token_source.IsCancellationRequested) {
                Thread.Sleep(1500);
                Serial.Connect();
            }
        }
        
        if (Serial.Connected) {
            Tasks.StartTask(LEDUpdateThread, LED_cancellation_token);
            
            while (!Tasks.cancellation_token_source.IsCancellationRequested) {
                Thread.Sleep(15);
            }
        } 
        
        Serial.Disconnect();
        Logging.Stop();
    }

    static void LEDUpdateThread() {
        byte b = 0;
        bool flipflop = true;

        if (Serial.Connected) {
            while (!LED_cancellation_token_source.IsCancellationRequested) {
                if (flipflop) b++; else b--; 
                if (b is 0 or 255) flipflop = !flipflop;
                
                strip.Brightness = b;
                    
                strip.Update();

                Thread.Sleep(15);
            }
        }
    } 
    
    static void Exit() {
        Logging.Message("Shutting down!");
        
        Tasks.cancellation_token_source.Cancel();
        LED_cancellation_token_source.Cancel();
        strip.Clear();

        while (Tasks.TaskCount > 0) { }
        
        Logging.Stop();
        Serial.Disconnect();
        
        Environment.Exit(0);
    }
}
