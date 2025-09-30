using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace LorikeetServer;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct StripSection {
    public byte R, G, B;
    public byte Length;
    
    public StripSection(byte r, byte g, byte b, byte length) {
        Length = length;
        SetColor(r,g,b);
    }

    public void SetColor(byte r, byte g, byte b) {
        R = r; G = g; B = b;
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct Strip {
    public int version = 0;
    public byte brightness = 255;
    public byte section_count;

    public StripSection* sections {
        get {
            fixed (Strip* self = &this) {
                return (StripSection*) ((byte*)self + sizeof(Strip));
            }
        }
    }
    
    public Strip(byte brightness) => this.brightness = brightness;
    
    public void increment_version() => version++;
}

public unsafe class LEDStrip {
    private Strip* strip;
    
    public void increment_version() => strip->version++;
    
    private byte total_length = 0;
    public byte TotalLength => total_length;
    
    public byte SectionCount => (byte)strip->section_count;
    
    public const string MappedMemoryName = "LorikeetMappedMemory";
    public const byte MaxSectionCount = 255;
    public static int MappedMemorySize = sizeof(Strip) + (MaxSectionCount * sizeof(StripSection));
    
    private static MemoryMappedFile memory_map;
    
    public LEDStrip(params StripSection[] sections) {
        memory_map =  MemoryMappedFile.CreateFromFile($"/dev/shm/{LEDStrip.MappedMemoryName}",
            FileMode.OpenOrCreate,
            null,
            LEDStrip.MappedMemorySize,
            MemoryMappedFileAccess.ReadWrite
        );

        byte* ptr = null;
        using var accessor = memory_map.CreateViewAccessor(0, MappedMemorySize, MemoryMappedFileAccess.ReadWrite);
        
        accessor.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);
        strip = (Strip*)ptr;
        
        ModifySections(sections);
    }

    
    public void ModifySections(params StripSection[] sections) {
        strip->section_count = (byte)sections.Length;

        for (int i = 0; i < sections.Length; i++) {
            *(strip->sections + i) = sections[i];
        }
        
        for (var index = 0; index < sections.Length; index++) {
            total_length += sections[index].Length;
        }
    }
    

    public void SetSectionColor(byte section_index, byte R, byte G, byte B) {
        if (section_index >= strip->section_count) return;
        strip->sections[section_index].R = R;
        strip->sections[section_index].G = G;
        strip->sections[section_index].B = B;
    }

    private static byte[] buffer = new byte[255];

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
        if (!Serial.Connected) return;
        
        if (strip->version == last_version) return;
        
        UpdateBrightness();
        
        for (var index = 0; index < strip->section_count; index++) {
            total_length += strip->sections[index].Length;
        }
        
        buffer[0] = (byte)'Z';
        buffer[1] = SectionCount;
        
        current_section = 0;
        current_byte = 2;
        
        for (int i = 0; i < strip->section_count; i++) {
            var section = strip->sections[i];
            
            buffer[2 + (current_section * 4)] = section.Length;
            
            buffer[2 + (current_section * 4) + 1] = (byte)(section.R == 47 ? 48 : section.R);
            buffer[2 + (current_section * 4) + 2] = (byte)(section.G == 47 ? 48 : section.G);
            buffer[2 + (current_section * 4) + 3] = (byte)(section.B == 47 ? 48 : section.B);

            current_section++;
            current_byte += 4;
        }

        buffer[current_byte] = (byte)'/';
        
        WriteData(buffer, 0, (SectionCount * 4) + 3);
        
        last_version = strip->version;
    }
}