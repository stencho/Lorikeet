#include <Adafruit_NeoPixel.h>
#include "serial_lib.h"

#ifdef __AVR__
 #include <avr/power.h> // Required for 16 MHz Adafruit Trinket
#endif

const byte LED_PIN = 6;
Adafruit_NeoPixel strip;

struct zone {
  byte length; 
  byte r;
  byte g; 
  byte b;
  byte brightness;
}; 

//Adafruit_NeoPixel strip;
serial_handler sh;

zone zones[64];
bool control = false;
byte total_zones = 0;
void setup() {
  
  pinMode(LED_PIN,OUTPUT);  
  
  //strip = Adafruit_NeoPixel(255, LED_PIN, NEO_GRB + NEO_KHZ800);
  //strip.begin();
#ifdef __AVR__
  //clock_prescale_set(clock_div_1);
#endif

  strip = Adafruit_NeoPixel(255, LED_PIN, NEO_GRB + NEO_KHZ800);
  strip.begin();
  strip.fill(strip.Color(255,255,255),0,10);
  strip.show();

  sh = serial_handler();
}

typedef struct {
  byte r,g,b;
} rgb;

void set_zone_colors() {
  int pc = 0;
  //strip.fill(strip.Color(255,255,255),0,10);
  
  for(int z = 0; z < total_zones; z++) {
    strip.fill(strip.Color(zones[z].r, zones[z].g, zones[z].b), pc, zones[z].length);
    pc += zones[z].length;
  }  
  
  strip.show();
}

void loop() {
  sh.read_serial();
  
  if (sh.buffer_r[0] == 'P' && sh.buffer_r[1] == 'E' && sh.buffer_r[2] == 'E' 
   && sh.buffer_r[3] == 'P' && sh.buffer_r[4] == 'E' && sh.buffer_r[5] == 'E') {
    sh.write_serial("POOPOO");
  }       
  
  if (sh.buffer_r[0] == 'C') { 
    if (sh.buffer_r[1] == 0x00) {
      control = false;
    } else {
      control = true;
    }    
  }
  
  if (sh.buffer_r[0] == 'Z') {    
    //if (sh.buffer_length_r < (total_zones*4)+3) return;
    total_zones = sh.buffer_r[1];
    for(int z = 0; z < total_zones; z++) {
      zones[z].length = sh.buffer_r[2+(z*6)];   
         
      zones[z].r = sh.buffer_r[2+(z*6)+1];
      zones[z].g = sh.buffer_r[2+(z*6)+2];
      zones[z].b = sh.buffer_r[2+(z*6)+3];
      
      zones[z].brightness = sh.buffer_r[2+(z*6)+4];   
             
    }    
  }

  
  sh.flush_serial();  
  set_zone_colors();
  sh.clear_buffers();
}
