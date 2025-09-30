using System.IO;
using System.IO.MemoryMappedFiles;
using LorikeetServer;

namespace LorikeetUI;

public static unsafe class MappedLEDStrip {
    private static MemoryMappedFile memory_map;
    public static Strip* strip;
    public static StripSection* sections;

    public static void ModifySections(params StripSection[] sections) {
        strip->section_count = (byte)sections.Length;

        for (int i = 0; i < sections.Length; i++) {
            *(strip->sections + i) = sections[i];
        }

        strip->increment_version();
    }

    public static void SetSectionColor(byte section_index, byte R, byte G, byte B) {
        if (section_index >= strip->section_count) return;
        strip->sections[section_index].R = R;
        strip->sections[section_index].G = G;
        strip->sections[section_index].B = B;
        
        strip->increment_version();
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
        strip = (Strip*)ptr;
    }

    public static void CloseMap() {
        memory_map.Dispose();
    }
}