#include <Adafruit_NeoPixel.h>

#ifdef __AVR__
 #include <avr/power.h> // Required for 16 MHz Adafruit Trinket
#endif

#define PACKET_BUFFER_SIZE 900

const byte LED_PIN = 6;
const byte LED_COUNT = 24;

int packet_buffer_pos = 0;
int packet_size = 0;

byte packet_buffer[PACKET_BUFFER_SIZE];

bool packet_available = false;

Adafruit_NeoPixel strip;

void setup() {  
  Serial.begin(115200);
  
  strip = Adafruit_NeoPixel(LED_COUNT, LED_PIN, NEO_GRB + NEO_KHZ800);
  strip.begin();
  strip.clear();
  strip.show();
  
  memset(packet_buffer, 0, PACKET_BUFFER_SIZE);
    
  pinMode(LED_PIN,OUTPUT);  
  
#ifdef __AVR__
  clock_prescale_set(clock_div_1);
#endif  
}

const byte handshake[7] = { 'P', 'O', 'O', 'P', 'O', 'O', LED_COUNT };

void loop() {  
  //Serial packet loop
  while (Serial.available() > 0) {
    byte incoming_byte = Serial.read();

    if (packet_buffer_pos < PACKET_BUFFER_SIZE - 1) {
      packet_buffer[packet_buffer_pos++] = incoming_byte;
      packet_size++;
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
    Serial.write(handshake,7);
    Serial.flush();
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

  //set LEDs individually
  else if (packet_buffer[0] == 'I') {    
    byte leds_in_buffer = (packet_size - 1) / 3;
    
    for (int i = 0; i < leds_in_buffer; i++) {
      strip.setPixelColor(i, 
        packet_buffer[1 + (i * 3) + 0], 
        packet_buffer[1 + (i * 3) + 1], 
        packet_buffer[1 + (i * 3) + 2]);        
    }

    if (leds_in_buffer < LED_COUNT) {
      strip.fill(strip.Color(0,0,0), leds_in_buffer, LED_COUNT - leds_in_buffer);
    }
    
    strip.show();
    goto done;
  }

  //reset packet buffer and prep for next serial packet
  done:   
    packet_buffer_pos = 0;
    packet_size = 0;
    packet_available = false;
    memset(packet_buffer, 0, PACKET_BUFFER_SIZE);
}
