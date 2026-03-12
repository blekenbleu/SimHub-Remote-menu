using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace blekenbleu.SimHub_Remote_menu
{
	public partial class Control
	{
		readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);		// Only 1 PayloadHandler() at a time

		// WPF events by name;  MIDI events by payload
		internal async Task SemaphoreQueue(string name, int payload)
		{
			await semaphore.WaitAsync();
			try
			{
				PayloadHandler(name, payload);
			}
			catch
			{
				System.Windows.Forms.MessageBox.Show($"WebMenu.SemaphoreQueue({name}, {payload})", "Exception");
			}
			finally
			{
				semaphore.Release();
			}
		}

		void PayloadHandler(string name, int payload)
		{
			if ("MIDI" == name)
				ProcessMIDI(payload);		// Control.midi.cs
			else if (-1 == payload)			// WPF RoutedEvent
			{
				if ("bm" == name)			// [MIDI learn] button
					NotEarn();
				else if (Earn)				// learning events
					Learn(name);
				else ClickHandle(name);		// Control.xaml.cs "live" events
			}
			else if (Earn)					// System.Windows.Input.Mouse event
			{								// learn slider map
				if (button)					// only 0 or 127 values?
					Model.MidiStatus = "\nMIDI control >>only<< for button; ignored";
				else ListClick(name);		// Control.midi.cs
			}
			else OK.FromSlider(0.1 * payload);
		}

		// handle all button events in one method
		internal async void ButEvent(object sender, RoutedEventArgs e)
		{
			string butName = (e.OriginalSource as FrameworkElement).Name;

			await SemaphoreQueue(butName, -1);
		}

		// handle slider changes
		private async void Slider_DragCompleted(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			await SemaphoreQueue("SL", (int)(0.5 + 10 * SL.Value));
		}
	}
}
