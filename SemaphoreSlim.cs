using System.Threading;
using System.Threading.Tasks;

namespace blekenbleu.SimHub_Remote_menu
{
	public partial class Control
	{
		SemaphoreSlim semaphore = new SemaphoreSlim(1);		// Only 1 task at a time

		// WPF events by name;  MIDI events by payload
		internal async Task EventHandler(string name, int payload)
		{
			await semaphore.WaitAsync();
			try
			{
				await Task.Run(() => PayloadHandler(name, payload));
			}
			catch
			{
                System.Windows.Forms.MessageBox.Show($"Remote-menu.EventHandler({name}, {payload})", "Exception");
			}
			finally
			{
				semaphore.Release();
			}
		}

		void PayloadHandler(string name, int payload)
		{
			if ("MIDI" == name)
				ProcessMIDI(payload);
			else if (-1 == payload)			// WPF RoutedEvent
			{
				if ("bm" == name)			// [MIDI learn] button
					NotEarn();
				else if (Earn)				// learning events
					Learn(name);
				else ClickHandle(name);		// "live" events
			}
			else if (Earn)					// System.Windows.Input.Mouse event
			{								// learn slider map
				if (button)					// only 0 or 127 values?
					Model.MidiStatus = "\nMIDI control >>only<< for button; ignored";
				else ListClick(name);		// Control.midi.cs
			}
			else OK.FromSlider(0.1 * payload);
		}
	}
}
