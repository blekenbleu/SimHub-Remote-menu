## WebMenu [MIDI](https://www.hobbytronics.co.uk/wp-content/uploads/2023/07/9_MIDI_code.pdf) input support  by `SemaphoreSlim`

If/when `MIDI learn` button is pressed, the plugin must
- set `learn = true`, disable UI event handling, then enable [`MIDI`](MIDI.cs), if not already
	- [Start() MidiIn](https://github.com/blekenbleu/MIDIio/blob/UI/docs/map.md)
	- make 'MIDI remove` button visible

- prompt for Delete or MIDI event, then associated UI
	- if Delete, then remove from event list
    - if event, then associate with UI button or slider as appropriate, replacing any previous association

When `MIDI learn` is pressed while `true == learn`
- set `learn = false`
- make 'MIDI remove` button invisible
- clear MIDI prompts
- *ToDo:* if event list is empty, kill MIDI inputs
	- else kill unused MidiIn devices
- update WebMenu UI event handling, *then* enable

---

### MIDI event list processing
- [Midi input events launch `MidiInMessageEvent` handlers](MIDI.cs)
- these queue [SemaphoreSlim tasks for `PayloadHandler()`](SemaphoreSlim.cs)
  - `PayloadHandler()` sorts MIDI vs WPF and sider vs button `Learn()` vs `ClickHandle()` methods
- *ToDo:* check if event device is in MidiOut list
  - map device to MidiOut channel number and forward it
- *ToDo:* similarly for VJoy and property/event list handling

---
### MIDI device name handling
Device names found by NAudio `MidiIn.DeviceInfo().ProductName`  
may differ from those saved/restored between plugin sessions (game changes).  
Consequently, when `MIDI learn` button is pressed while the configured list is empty,
then saved MIDI devices and channels must be sorted to the current `lMidiIn[].id` ProductName list,
and any saved MIDI devices not among current `lMidiIn[].id` should be noted.

#### reminders
- MIDI messages have 3 bytes
  - value
  - status number
  - status = 3-bit message type (`0xBn - 0xCn`) |  4-bit MIDI channel number (usually 0)
- saved mappings want product names + 3 bytes
  - mapped control
  - product status and number
- working map wants 3 byte keys with mapped control (3-bit) nibble
  - (product) index - corresponding to `lMidiIn[index].id` ProductNames
  - status and number, to match incoming MIDI messages

#### while mapping
Enable `unmap` if working map is not empty  
Prompt when MIDI input is already mapped  
Note whether 0 < value < 127
- not a button, map only to slider  
Then note whether selected control is already mapped
- change `unmap` to `replace` and prompt

#### *ToDo:* when mapping ends
- release unmapped MIDI input devices
- shutdown channel if map is empty	
