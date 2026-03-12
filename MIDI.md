## WebMenu MIDI
Directly support MIDI device inputs for controlling property change actions.
- alternative to mouse and joystick events
- instead of queueing [BoundedChannel](Channel.md) MIDI payloads,
  queue [`SemaphoreSlim SemaphoreQueue` Tasks](SemaphoreSlim.md) for `PayloadHandler()`

SimHub handles joystick button, but not axes events.
