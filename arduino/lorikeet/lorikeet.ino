#include <Adafruit_NeoPixel.h>

#ifdef __AVR__
 #include <avr/power.h> // Required for 16 MHz Adafruit Trinket
#endif

#define PACKET_BUFFER_SIZE 255

const byte LED_PIN = 6;

byte packet_buffer_pos = 0;
byte packet_buffer[PACKET_BUFFER_SIZE];

bool packet_available = false;

Adafruit_NeoPixel strip;

void setup() {  
  Serial.begin(115200);
  
  strip = Adafruit_NeoPixel(255, LED_PIN, NEO_GRB + NEO_KHZ800);
  strip.begin();
  strip.clear();
  strip.show();
  
  memset(packet_buffer, 0, PACKET_BUFFER_SIZE);
    
  pinMode(LED_PIN,OUTPUT);  
  
#ifdef __AVR__
  clock_prescale_set(clock_div_1);
#endif  
}

void loop() {  
  //Serial packet loop
  while (Serial.available() > 0) {
    byte incoming_byte = Serial.read();

    if (packet_buffer_pos < PACKET_BUFFER_SIZE - 1) {
      packet_buffer[packet_buffer_pos++] = incoming_byte;
    }

    if (incoming_byte == '/') {
      packet_available = true;      
      packet_buffer_pos = 0; 
      break;
    }
  }

  if (!packet_available) return;

  //handle peepee poopoo handshake
  if (packet_buffer[0] == 'P' && packet_buffer[1] == 'E' && packet_buffer[2] == 'E' 
   && packet_buffer[3] == 'P' && packet_buffer[4] == 'E' && packet_buffer[5] == 'E') {    
    Serial.print("POOPOO");    
    goto done;
  }       

  //clear strip
  else if (packet_buffer[0] == 'C') {
    strip.clear();
    strip.show();
    goto done;
  }

  //set brightness
  else if (packet_buffer[0] == 'B') {
    strip.setBrightness(packet_buffer[1]);
    goto done;
  }

  //fill LED zones
  else if (packet_buffer[0] == 'Z') {
    byte current_led = 0;

    for (int z = 0; z < packet_buffer[1]; z++) {
      byte len = packet_buffer[2 + (z * 4)];
      strip.fill(
        strip.Color(
          packet_buffer[2 + (z * 4) + 1], 
          packet_buffer[2 + (z * 4) + 2], 
          packet_buffer[2 + (z * 4) + 3]), 
        current_led, len);
      
      current_led += len;          
    }
    
    if (current_led < 255) {
      strip.fill(strip.Color(0,0,0), current_led, 255 - current_led);
    }
  
    strip.show();
    goto done;
  }

  //reset packet buffer and prep for next serial packet
  done:   
    packet_buffer_pos = 0;
    packet_available = false;
    memset(packet_buffer, 0, PACKET_BUFFER_SIZE);
}
