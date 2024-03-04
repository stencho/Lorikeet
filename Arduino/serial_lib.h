#include "Arduino.h"

class serial_handler {
  public:
    serial_handler();    
    void read_serial();
    void write_serial(String s);
    void write_serial(byte b);
    void flush_serial();
    void clear_buffers();
    char* buffer_r;
    char* buffer_w;
    byte buffer_length_r;
    byte buffer_length_w;
};
