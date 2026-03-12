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
- review Resume()
	- should probably keep ProductName in click list
		- preserve learnings for ProductNames currently unavailable
