using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using LorikeetUI;

namespace LorikeetServer;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct LED {
    public byte R, G, B;
    
    public LED(byte r, byte g, byte b) {
        SetColor(r,g,b);
    }

    public void SetColor(byte r, byte g, byte b) {
        R = r; G = g; B = b;
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct StripIndividual {
    public int version = 0;
    public byte brightness = 255;
    public byte LEDCount;

    public LED* LEDs {
        get {
            fixed (StripIndividual* self = &this) {
                return (LED*) ((byte*)self + sizeof(StripIndividual));
            }
        }
    }
    
    public StripIndividual(byte brightness) => this.brightness = brightness;
    
    public void increment_version() => version++;
}

public unsafe class LEDStrip {
    private StripIndividual* strip;
    
    public void IncrementVersion() => strip->version++;
    
    public byte LEDCount => (byte)strip->LEDCount;
    
    public const string MappedMemoryName = "LorikeetMappedMemory";
    
    public const byte MaxLEDCount = 255;
    public static int MappedMemorySize = sizeof(StripIndividual) + (MaxLEDCount * sizeof(LED));
    
    private static MemoryMappedFile memory_map;

    public LEDStrip(byte led_count) {
        Setup(led_count, 0,0,0);    
    }
    
    public LEDStrip(byte led_count, byte R, byte G, byte B) {
        Setup(led_count, R, G, B);
    }

    void Setup(byte led_count, byte R, byte G, byte B) {
        File.Delete($"/dev/shm/{MappedMemoryName}");
        
        memory_map =  MemoryMappedFile.CreateFromFile($"/dev/shm/{MappedMemoryName}",
            FileMode.OpenOrCreate,
            null,
            MappedMemorySize,
            MemoryMappedFileAccess.ReadWrite
        );

        byte* ptr = null;
        using var accessor = memory_map.CreateViewAccessor(0, MappedMemorySize, MemoryMappedFileAccess.ReadWrite);
        
        accessor.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);
        strip = (StripIndividual*)ptr;
        
        AddLEDs(led_count, R, G, B);
        
        strip->increment_version();
    }

    private void AddLEDs(byte count, byte R, byte G, byte B ) {
        strip->LEDCount = count;

        for (int i = 0; i < count; i++) {
            *(strip->LEDs + i) = new LED(R, G, B);
        }
    }
    
    public void Fill(byte offset, byte length, byte R, byte G, byte B) {
        for (int i = offset; i < offset + length; i++) {
            SetLEDColor((byte)i, R, G, B);
        }
    }

    public void SetLEDColor(byte led_index, byte R, byte G, byte B) {
        if (led_index >= strip->LEDCount) return;
        
        strip->LEDs[led_index].R = R;
        strip->LEDs[led_index].G = G;
        strip->LEDs[led_index].B = B;
    }

    private static byte[] buffer = new byte[900];

    private byte current_section = 0;
    private byte current_byte = 0;

    private int last_version = 0;

    private byte last_brightness = 255;
    
    void WriteData(byte[] buffer, int offset, int count) {
        if (!Serial.Connected && Serial.Reconnecting) return;
            
        try {
            Serial.serial_stream.Write(buffer, offset, count);
            Serial.serial_stream.Flush();
            
        } catch {
            Serial.Disconnect();
        }
        
        if (!Serial.Connected)
            Serial.Reconnect();
    }
    
    public void Clear() {
        buffer[0] = (byte)'C';
        buffer[1] = (byte)'/';
        
        WriteData(buffer, 0, 2);
    }
    
    public void UpdateBrightness() {
        if (last_brightness == strip->brightness) return;
        
        buffer[0] = (byte)'B';
        buffer[1] = strip->brightness;
        buffer[2] = (byte)'/';
        
        WriteData(buffer, 0, 3);
        
        last_brightness = strip->brightness;
    }
    
    public void Update() {
        if (!Serial.Connected) Serial.Reconnect();
        
        if (strip->version == last_version) return;
        
        UpdateBrightness();
        
        buffer[0] = (byte)'I';
        
        current_section = 0;
        current_byte = 1;
        
        for (int i = 0; i < strip->LEDCount; i++) {
            var LED = strip->LEDs[i];
            
            buffer[current_byte + 0] = (byte)(LED.R == 47 ? 48 : LED.R);
            buffer[current_byte + 1] = (byte)(LED.G == 47 ? 48 : LED.G);
            buffer[current_byte + 2] = (byte)(LED.B == 47 ? 48 : LED.B);

            current_byte += 3;
        }

        buffer[current_byte] = (byte)'/';
        
        WriteData(buffer, 0, (LEDCount * 3) + 2);
        
        last_version = strip->version;
    }
}


public static unsafe class MappedLEDStrip {
    private static MemoryMappedFile memory_map;
    public static StripIndividual* strip;
    public static LED* LEDs;

    public static void ModifyLEDs(params LED[] LEDs) {
        strip->LEDCount = (byte)LEDs.Length;

        for (int i = 0; i < LEDs.Length; i++) {
            *(strip->LEDs + i) = LEDs[i];
        }

        strip->increment_version();
    }
    
    public static void ModifyLEDs(byte count, LED color) {
        strip->LEDCount = count;

        for (int i = 0; i < count; i++) {
            *(strip->LEDs + i) = color;
        }

        strip->increment_version();
    }

    public static void SetLEDColor(byte led_index, byte R, byte G, byte B) {
        if (led_index >= strip->LEDCount) return;
        
        strip->LEDs[led_index].R = R;
        strip->LEDs[led_index].G = G;
        strip->LEDs[led_index].B = B;
        
        strip->increment_version();
    }
    
    public static void SetLEDColor(byte led_index, float h, float s, float v) {
        if (led_index >= strip->LEDCount) return;

        var (R, G, B) = ColorUtils.HSVtoRGB(h, s, v);
        
        SetLEDColor(led_index, R, G, B);
    }
    
    public static void FillSection(byte offset, byte length, byte R, byte G, byte B) {
        for (byte i = offset; i < offset + length; i++) {
            SetLEDColor(i, R, G, B);
        }
    }
    
    public static void FillSection(byte offset, byte length, float h, float s, float v) {
        var (R, G, B) = ColorUtils.HSVtoRGB(h, s, v);
        FillSection(offset, length, R, G, B);

    }

    public static void Gradient(byte offset, byte length, byte fromR, byte fromG, byte fromB, byte toR, byte toG, byte toB) {
        var from = ColorUtils.RGBtoHSV(fromR, fromG, fromB);
        var to = ColorUtils.RGBtoHSV(toR, toG, toB);
        
        var diffH = to.h - from.h;
        var diffS = to.s - from.s;
        var diffV = to.v - from.v;
        
        for (byte i = 0; i < length; i++) {
            float prog = (float)i / (float)length;
            
            var rgb = ColorUtils.HSVtoRGB(from.h + (diffH * prog), from.s + (diffS * prog), from.v + (diffV * prog));
            SetLEDColor(i, rgb.R, rgb.G, rgb.B);
        }
    }

    public static void SetBrightness(byte brightness) {
        strip->brightness = brightness;
        strip->increment_version();
    }
    
    public static void OpenMap() {
        memory_map =  MemoryMappedFile.CreateFromFile($"/dev/shm/{LorikeetServer.LEDStrip.MappedMemoryName}",
            FileMode.Open,
            null,
            LorikeetServer.LEDStrip.MappedMemorySize,
            MemoryMappedFileAccess.ReadWrite
        );
        
        
        byte* ptr = null;
        using var accessor = memory_map.CreateViewAccessor(0, LorikeetServer.LEDStrip.MappedMemorySize, MemoryMappedFileAccess.ReadWrite);
        
        accessor.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);
        strip = (StripIndividual*)ptr;
    }

    public static void CloseMap() {
        memory_map.Dispose();
    }
}