namespace Lorikeet;

public struct StripSection {
    public byte R, G, B;
    public byte Length;

    internal LEDStrip strip;
    
    public StripSection(byte r, byte g, byte b, byte length) {
        Length = length;
        SetColor(r,g,b);
    }

    public void SetColor(byte r, byte g, byte b) {
        R = r; G = g; B = b;
    }
}

public class LEDStrip {
    private byte brightness = 255;
    public byte Brightness {
        get => brightness;
        set {
            brightness = value; 
            SetBrightness(brightness);
        }
    }
    
    private byte total_length = 0;
    public byte TotalLength => total_length;
    
    public StripSection[] Sections;
    public byte SectionCount => (byte)Sections.Length;

    public LEDStrip(params StripSection[] sections) {
        Sections = sections;

        for (var index = 0; index < sections.Length; index++) {
            total_length += sections[index].Length;
            sections[index].strip = this;
        }
    }

    private static byte[] buffer = new byte[255];

    private byte current_section = 0;
    private byte current_byte = 0;

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
    
    public void SetBrightness(byte brightness) {
        buffer[0] = (byte)'B';
        buffer[1] = brightness;
        buffer[2] = (byte)'/';
        
        WriteData(buffer, 0, 3);
    }
    
    public void Update() {
        if (!Serial.Connected) return;
        
        buffer[0] = (byte)'Z';
        buffer[1] = SectionCount;
        
        current_section = 0;
        current_byte = 2;
        
        foreach (StripSection section in Sections) {
            buffer[2 + (current_section * 4)] = section.Length;
            
            buffer[2 + (current_section * 4) + 1] = (byte)(section.R == 47 ? 48 : section.R);
            buffer[2 + (current_section * 4) + 2] = (byte)(section.G == 47 ? 48 : section.G);
            buffer[2 + (current_section * 4) + 3] = (byte)(section.B == 47 ? 48 : section.B);

            current_section++;
            current_byte += 4;
        }

        buffer[current_byte] = (byte)'/';
        
        WriteData(buffer, 0, (SectionCount * 4) + 3);
    }
}