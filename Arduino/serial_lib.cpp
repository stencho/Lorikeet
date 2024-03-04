#include "Arduino.h"
#include "serial_lib.h"
    
serial_handler::serial_handler() {
  Serial.begin(115200);
  buffer_r = new char[255];
  buffer_w = new char[255];
  
  buffer_length_r = 0;
  buffer_length_w = 0;
  
  Serial.setTimeout(1000);
}

int ab = 0;

void serial_handler::read_serial() {  
  ab = Serial.available();
  
  if (ab > 0) {
    buffer_length_r = Serial.readBytesUntil('/', buffer_r,  255);
  }
}

void serial_handler::clear_buffers() {
  buffer_length_w = 0;
  buffer_length_r = 0;
    
  for (int i=0; i < 255; i++) {
    buffer_w[i] = 0;
    buffer_r[i] = 0;
  }
}

void serial_handler::write_serial(String s) {
  for (int i = 0; i < s.length(); i++) {
    if (buffer_length_w + i > 255) { buffer_length_w = 255; return; }
    buffer_w[buffer_length_w+i] = (byte)s[i];
  }
  
  buffer_length_w += s.length();
}

void serial_handler::write_serial(byte b) {
  if (buffer_length_w == 255) return;  
  buffer_w[buffer_length_w] = (char)b;
  buffer_length_w += 1;
}

void serial_handler::flush_serial() {
  Serial.write(buffer_w);
  Serial.flush();
}


/*

void serial_handler::write_serial(String s) {
  String b = "";
  char st[2];
  
  for (int i = 0; i < buffer_length; i++){
    sprintf(st, "%X", buffer[i]);
    
    b += " " + (String)st;
  }
  if (buffer_length > 0){
    Serial.println(s + " : : [" + buffer + "]");
    Serial.println(b);
    Serial.flush();
  }
}

 */
