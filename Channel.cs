// https://medium.com/@abhirajgawai/c-channels-explained-from-producer-consumer-basics-to-high-performance-net-systems-f8ab610c0639
using System;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace blekenbleu.SimHub_Remote_menu
{
	internal partial class MIDI
	{
		// Multiple producers, single consumer 
		readonly static Channel<int> _channel = Channel.CreateBounded(
			new BoundedChannelOptions(100)	// Bounded to 100. If full, drop oldest.
			{
				SingleReader = true,	// Optimization hint
				SingleWriter = false,	// Multiple producers

				FullMode = BoundedChannelFullMode.DropOldest
			},
			(int dropped) =>
			{
				string drop = $"MIDI.Channel dropped: {dropped:X}" + (Control.busy ? " busy" : "");
				WebMenu.Info(drop);
				Model.MidiStatus = "\n" + drop;
			}
		);

		internal static async Task ReadAsync()
		{
			try	// check for exceptions
			{
				while (await _channel.Reader.WaitToReadAsync())
					while (_channel.Reader.TryRead(out int item))
						Control.Process(item);				// Control.midi.cs
			}
			catch (Exception ex)
			{
				WebMenu.Info($".ReadAsync() {ex}");
			}

			WebMenu.Info("ReadAsync() ended");
		}

		internal static void ReadMidiChannel() { Task.Run(() => ReadAsync()); }
		
		// https://learn.microsoft.com/en-us/dotnet/core/extensions/channels#producer-patterns
		internal static void Enque(int inDevice, int payload)
		{
			payload |= inDevice << 24;
			// Fire-and-forget
			if (!_channel.Writer.TryWrite(payload))
				WebMenu.Info(lMidiIn[inDevice].id + ".Enqueue(" + payload.ToString() + ") failed");
		}
	}
}
