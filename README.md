# Lorikeet

A small program for setting the color of a WS2812 RGB LED strip connected to an Arduino.

Mostly incomplete. It has a plugin system, but it also can't even save or load settings or even actually change the color of LEDs manually yet. Uses NAudio and SlimDX for the plugin system.

I use this literally every day, my settings are hardcoded in MainStripForm.cs

It uses a list of 'zone' objects which specify a color, brightness, start and length. Plugins have those zones passed to them, and do things to them, before they're sent to the microcontroller. This lets plugins make the LEDs do things like blink for alerts, react to music, or change depending on screen color (some day I'll make this work)

![](doody_alert.mp4)

#### Contains
- LorikeetUI: The actual program, tries to do a handshake with any serial devices and if successful, will start to send commands
- LorikeetLib: The plugin library
- LorikeetLib/plugins/LorikeetPlugins: A plugin containing a set of different default plugins
- Arduino: The Arduino code
