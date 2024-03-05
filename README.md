# Lorikeet

A small program for setting the color of sets of WS2812 RGB LED strips

Mostly incomplete. It has a plugin system, but it also can't even save or load settings yet. Uses NAudio and SlimDX for the plugin system.

I use this literally every day, my settings are hardcoded in MainStripForm.cs


#### Contains
- LorikeetUI: The actual program, tries to do a handshake with any serial devices and if successful, will start to send commands
- LorikeetLib: The plugin library
- LorikeetLib/plugins/LorikeetPlugins: The base plugin library
- Arduino: The Arduino code
