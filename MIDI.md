[*back*](README.md#plan)

## WebMenu MIDI
Directly support MIDI device inputs for controlling property change actions.
- alternative to mouse and joystick events
- instead of queueing [BoundedChannel](Channel.md) MIDI payloads,  
  queue [`SemaphoreSlim SemaphoreQueue` Tasks](SemaphoreSlim.md) for `PayloadHandler()`
- restore learned MIDI from `Settings.midiDevs` and `Resume()` after game changes

SimHub **Controls and events** handles **Controllers** joystick button, but not axis events.
#### To do:
- Stop unused `mMidiIn` devices after learn
- log active MIDI devices
- refactor Resume()
	- keep all ProductName in click list
		- preserve learnings for ProductNames currently unavailable
- click list edit screen - replacing `dg` DataGrid for review / deletions

#### MIDI Init
Available devices may change at any time;  
- check `MidiIn.DeviceInfo` on every `Init()`;
	- extract `available` and `used` ProductName Lists
- sort `Settings.midiDevs` into `lMidiIn` and `unused`

#### MISI Stop
if changed,
- convert `lMidiIn` back to `Settings.midiDevs`
	- then Concat `unused` to `Settings.midiDevs`
