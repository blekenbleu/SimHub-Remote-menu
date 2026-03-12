[*back*](README.md)  
## WebMenu [MIDI](https://www.hobbytronics.co.uk/wp-content/uploads/2023/07/9_MIDI_code.pdf) input `BoundedChannel` support - *replaced by* [`SemaphoreSlim`](SemaphoreSlim.md)

### [Channel programming](https://github.com/blekenbleu/MIDIio/blob/UI/docs/map.md#queue-multiple-midi-device-inputs-by-systemthreadingchannels) reference links
- [An Introduction](https://devblogs.microsoft.com/dotnet/an-introduction-to-system-threading-channels/)  
`while (await channelReader.WaitToReadAsync())	//` [false when channel is closed](https://learn.microsoft.com/en-us/dotnet/api/system.threading.channels.channelreader-1.waittoreadasync?view=net-10.0&viewFallbackFrom=netframework-4.8)  
`    while (channelReader.TryRead(out T item))	//` empty the queue  
`        Use(item);`

- [How to Build](https://oneuptime.com/blog/post/2026-01-30-dotnet-custom-channels/view)
```
// Bounded with drop: drops items when full (useful for telemetry)
var boundedDrop = Channel.CreateBounded(new BoundedChannelOptions(100)
{
    FullMode = BoundedChannelFullMode.DropOldest
});
```

- [Explained - Medium](https://medium.com/@abhirajgawai/c-channels-explained-from-producer-consumer-basics-to-high-performance-net-systems-f8ab610c0639) - [`Channel.CreateBounded()`](https://learn.microsoft.com/en-us/dotnet/core/extensions/channels#bounded-creation-patterns)  
```
var channel = Channel.CreateBounded<Event>(new BoundedChannelOptions(100)
{
    FullMode = BoundedChannelFullMode.Wait,
    SingleReader = true,
    SingleWriter = false
});
```
While advertised as thread-safe and high performing, seemingly requires `System.Runtime.CompilerServices.Unsafe`...  
Practically, [MIDI payload queue](Channel.cs) was more cumbersome than [`SemaphoreSlim`](SemaphoreSlim.cs) MIDI event task queue
