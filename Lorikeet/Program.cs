namespace Lorikeet;

class Program {
    private static LEDStrip strip;
    
    static void Main(string[] args) {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.CancelKeyPress += delegate (object? sender, ConsoleCancelEventArgs e) { e.Cancel = true; Exit(); };
        
        strip = new LEDStrip(
            new StripSection(255,0,255, 2), 
            new StripSection(0, 255, 0, 2));
        Start(args);
    }

    private static DateTime start_time;
    private static DateTime current_time;
    
    static void Start(string[] args) {
        Logging.Start();

        Logging.Config("Connecting to arduino...");
        
        Serial.Connect();
        
        int c = 0;
        byte b = 0;
        bool flipflop = true;
        if (Serial.Connected) {
            
            while (!Tasks.cancellation_token_source.IsCancellationRequested) {
                c++;

                if (flipflop) b++; else b--;
                if (b == 255 || b == 0) flipflop = !flipflop;
                
                strip.Brightness = b;
                    
                if (c > 100 && strip.SectionCount == 2) {
                    strip = new LEDStrip(
                        new StripSection(100,100,255, 1), //blue 
                        new StripSection(200, 60, 200, 1), //pink
                        new StripSection(200,200,200, 1), //white
                        new StripSection(200, 60, 200, 1), //pink
                        new StripSection(100,100,255, 1) //blue
                        );
                }
                
                strip.Update();
                
                start_time = DateTime.UtcNow;
                while ((current_time - start_time).TotalMilliseconds < 1000.0/120.0) {
                    current_time = DateTime.UtcNow;
                    Thread.Sleep(1);
                }
            } 
        }
        
        Serial.Disconnect();
        Logging.Stop();
    }

    static void Exit() {
        Logging.Message("Shutting down!");
        
        Tasks.cancellation_token_source.Cancel();
        strip.Clear();


        while (Tasks.TaskCount > 0) { }
        
        Logging.Stop();
        Serial.Disconnect();
        
        System.Environment.Exit(0);
    }
}
