## WebMenu MIDI
Directly support MIDI device inputs for controlling property change actions.
- alternative to mouse and joystick events
- instead of queueing [BoundedChannel](Channel.md) MIDI payloads,
  queue [`SemaphoreSlim SemaphoreQueue` Tasks](SemaphoreSlim.md) for `PayloadHandler()`

SimHub **Controls and events** handles **Controllers** joystick button, but not axis events.
#### To do:
- restore learned MIDI and `Resume()` after game changes
- Stop unused `mMidiIn` devices after learn
