using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio.Midi;

namespace blekenbleu.SimHub_Remote_menu
{
	public class MidiDev		// must be public for Settings.cs
	{
		public string devName, butName;
		public int devMessage;	// (3-bit lMidiIn index) | data2 | data 1 | status
	}

	internal class Device		// NAudio MidiIn lacks device name
	{
		internal string id;
		internal MidiIn m;		// https://github.com/naudio/NAudio/blob/master/Docs/MidiInAndOut.md
	}

	// https://deepwiki.com/naudio/NAudio/7-midi-support#midi-io-operations
	partial class MIDI
	{
		// https://truelogic.org/wordpress/2021/01/28/using-midi-with-naudio/
        static List<Device> lMidiIn;

		// an array of MidiIn message event handlers
		// to distinguish which device sourced each message
		// https://github.com/naudio/NAudio/NAudio.Midi/Midi/MidiInMessageEventArgs.cs
		static readonly EventHandler<MidiInMessageEventArgs>[] RcvArray
			= new EventHandler<MidiInMessageEventArgs>[3] { MidiIn0, MidiIn1, MidiIn2 };
		static ViewModel Model;
		static Control View;
		static string status;
		static List<string> available, used;
		static List<MidiDev> unused;        // Settings.midiDevs for devName currently unavailable
		static List<int> devIndex;
		static internal readonly SortedList<string, string> buttonList = new SortedList<string, string>
		{
			{ "b0", "Scroll Up" },
			{ "b1", "Down" },
			{ "b2", "Increment" },
			{ "b3", "Decrement" },
			{ "b4", "Swap" },
			{ "b5", "Default" },
			{ "bf", "Forget" },
			{ "bm", "Learn" },
			{ "SB", "Set Slider" },
			{ "SL", "Slider" }
		};

		internal static MMvalues MMvalue(int devMessage, string butName)
		{
			return new MMvalues
			{
				MidiIn = available[devMessage >> 24],
				Word = $"{devMessage:X8}",
				Button = buttonList[butName]
			};
		}

		internal static MMvalues MMvalue(MidiDev md)
		{
			return new MMvalues
			{
				MidiIn = available[md.devMessage >> 24],
				Word = $"{md.devMessage:X8}",
				Button = buttonList[md.butName]
			};
		}

		internal static bool Start(ViewModel m, Control c)
		{
			Model = m;
			View = c;
			if (0 < status.Length)
			{
				Model.MidiStatus = status;
				status = "";
			}
			return true;
		}

		// populate Control.midi.cs SortedList click from Settings.cs midiDevs
		// Update MidiDev devMessage 3-bit lMidiIn indices to (j)
		// for devName matching MidiIn.DeviceInfo(j).ProductName
		internal static void Resume(ViewModel m, Control c, WebMenu w)
		{
			Control.click.Clear();
			for (int i = 0; i < w.Settings.midiDevs.Count; i++)
			{
				int j;

				if (0 > (j = available.FindIndex(s => s == w.Settings.midiDevs[i].devName)))
				{
					unused.Add(w.Settings.midiDevs[i]);
					continue;
				}

				int mDev = devIndex[j] << 24;		// current 3-bit lMidiIn index
				int recent = mDev | (0xFFFF & w.Settings.midiDevs[i].devMessage);

				if (!used.Contains(w.Settings.midiDevs[i].devName))
					used.Add(w.Settings.midiDevs[i].devName);
				else if (Control.click.ContainsKey(recent))
					continue;						// avoid duplicate devMessages

				Control.click.Add(recent, w.Settings.midiDevs[i].butName);
			}
//			WebMenu.Info($"Resume():  {Control.click.Count} configured clicks");
			Start(m, c);
		}

// shutting down and restarting between games
// https://github.com/naudio/NAudio/blob/master/NAudioDemo/MidiInDemo/MidiInPanel.cs#L67
		internal static void Stop(int i)
		{
			lMidiIn[i].m.Stop();
			lMidiIn[i].m.Dispose();
			lMidiIn[i].m.MessageReceived -= RcvArray[i];
			lMidiIn[i].m.ErrorReceived -= MidiIn_ErrorReceived;
			lMidiIn.RemoveAt(i);
		}

		internal static bool Stop(WebMenu wm)			// called by WebMenu.cs End()
		{
			if (0 < lMidiIn?.Count)
				for (int j = lMidiIn.Count -1 ; j >= 0; j--)
					Stop(j);
			lMidiIn = null;

            if (Control.changed)	// convert click List to midiDevs
			{
				wm.Settings.midiDevs = Control.click.Select(md => new MidiDev
				{
					butName = md.Value,
					devName = MidiIn.DeviceInfo((0x07000000 & md.Key) >> 24).ProductName,
					devMessage = md.Key & 0xFFFF
				}).ToList();
				if (0 < unused.Count)
					wm.Settings.midiDevs.Concat(unused);
			}
			return Control.changed;
		}
		static bool InputMidiSetup(int deviceNumber, string ProductName)	// called by InputMidiDevices
		{
			int j = lMidiIn.Count;
			bool ok = j < RcvArray.Length;

			if (ok)
			{
				MidiIn mMidiIn = new MidiIn(deviceNumber);
				mMidiIn.MessageReceived += RcvArray[j];
				mMidiIn.ErrorReceived += MidiIn_ErrorReceived;
				mMidiIn.Start();
				lMidiIn.Add(new Device { id = ProductName, m = mMidiIn });
			}
			return ok;
		}

		static string InputMidiDevices()		// called by Start()
		{
			StringBuilder s = new StringBuilder($"\nNAudio MidiIn device count {MidiIn.NumberOfDevices}"),
			t = new StringBuilder("\nNAudio MidiIn device:  ");
			bool b = false;

			lMidiIn = new List<Device> {};
			for (int i = 0; i < MidiIn.NumberOfDevices; i++)
			{
				string input = MidiIn.DeviceInfo(i).ProductName;

				s.Append("\n\t").Append(input);
				if (input.StartsWith("loopMIDI") || input.StartsWith("AudioBox"))
					s.Append(" ignored");
				else {
					bool a = InputMidiSetup(i, input);

					available.Add(input);
					devIndex.Add(i);
					s.Append(a ? " handled" : " ignored");
					if (a) {
						if (b)
							t.Append(",\n\t");
						t.Append(input);
						b = true;
					}
				}
			}
			status = t.ToString();
			return s.ToString();
		}

		// e.MidiEvent = FromRawMessage(e.RawMessage);
		static async void MidiIn0(object sender, MidiInMessageEventArgs e)
		{
			await View.SemaphoreQueue("MIDI", e.RawMessage);
		}

		static async void MidiIn1(object sender, MidiInMessageEventArgs e)
		{
			await View.SemaphoreQueue("MIDI", e.RawMessage | (1 << 24));
		}

		static async void MidiIn2(object sender, MidiInMessageEventArgs e)
		{
			await View.SemaphoreQueue("MIDI", e.RawMessage | (2 << 24));
		}

		static void MidiIn_ErrorReceived(object sender, MidiInMessageEventArgs e)
		{
			WebMenu.Info(String.Format("MidiIn_ErrorReceived():  Message 0x{0:X8} Event {1}",
				e.RawMessage, e.MidiEvent));
		}

		internal static string Init()
		{
			available = new List<string> {};
			used = new List<string> {};
			unused = new List<MidiDev> {};
			devIndex = new List<int> {};
			return "\t" + InputMidiDevices();
		}
	}
}
