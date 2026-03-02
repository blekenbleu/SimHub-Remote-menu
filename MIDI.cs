using System;
using System.Collections.Generic;
using System.Text;
using NAudio.Midi;

namespace blekenbleu.SimHub_Remote_menu
{
	public class MidiDev			// must be public for Settings.cs
	{
		public string devName, butName;
		public int devMessage;	// {4-bit dev = 1-bit button | 3-bit lMidiIn index} | data2 | data 1 | status
	}

	internal class Device			// NAudio MidiIn lacks device name
	{
		internal string id;
		internal MidiIn m;
	}

	// https://github.com/naudio/NAudio/blob/master/Docs/MidiInAndOut.md
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

		internal static bool Start(ViewModel m, Control c)
		{
			Model = m;
			View = c;
			if (null == lMidiIn)
			{
				lMidiIn = new List<Device> { };
				InputMidiDevices();
			}
			return true;
		}

		// populate Control.midi.cs SortedList click from Settings.cs midiDevs
		// Update MidiDev devMessage 3-bit lMidiIn indices to (j)
		// for devName matching MidiIn.DeviceInfo(j).ProductName
		internal static void Resume(ViewModel m, Control c)
		{
			Control.click.Clear();
			for (int i = 0; i < OKSHmenu.Settings.midiDevs.Count; i++)
			{
				for (int j = 0; j < MidiIn.NumberOfDevices; j++)
				{
					if (OKSHmenu.Settings.midiDevs[i].devName == MidiIn.DeviceInfo(j).ProductName)
					{
						int recent = OKSHmenu.Settings.midiDevs[i].devMessage;
						int mDev = j << 24;		// updated 3-bit lMidiIn index
						int dev = recent;

						dev &= 0x07000000;				// breaks if 7 < NumberOfDevices
						recent &= 0xFFFF;
						recent |= mDev;
						Control.Add(recent, OKSHmenu.Settings.midiDevs[i].butName);
						for (int k = 1 + i; k < OKSHmenu.Settings.midiDevs.Count; k++)
						{
							if (OKSHmenu.Settings.midiDevs[k].devMessage == OKSHmenu.Settings.midiDevs[i].devMessage)
							{
								i = k;
								continue;	// ignore duplicate midiDevs
							}
							// likely multiple OKSHmenu.Settings.midiDevs per dev
							recent = OKSHmenu.Settings.midiDevs[k].devMessage;
							if (dev == (0x07000000 & recent))
							{
								i = k;
								recent &= 0xFFFF;
								recent |= mDev;
								Control.Add(recent, OKSHmenu.Settings.midiDevs[i].butName);
							}
							else break;
						}
						break;
					}
				}
			}
//			OKSHmenu.Info($"Resume():  {Control.click.Count} configured clicks");
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

		internal static bool Stop()			// called by OKSHmenu.cs End()
		{

			for (int j = lMidiIn.Count -1 ; j >= 0; j--)
				Stop(j);
			lMidiIn = null;
			return Control.Stop();
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

		static void InputMidiDevices()		// called by Start()
		{
			StringBuilder s = new StringBuilder($"\nNAudio MidiIn device count {MidiIn.NumberOfDevices}");

			for (int i = 0; i < MidiIn.NumberOfDevices; i++)
			{
				string input = MidiIn.DeviceInfo(i).ProductName;

				s.Append("\n\t").Append(input);
				if (input.StartsWith("loopMIDI") || input.StartsWith("AudioBox"))
					s.Append(" ignored");
				else s.Append(InputMidiSetup(i, input) ? " handled" : " failed");
			}
			Model.MidiStatus = s.ToString();
		}

		// e.MidiEvent = FromRawMessage(e.RawMessage);
		static async void MidiIn0(object sender, MidiInMessageEventArgs e)
		{
			await View.EventHandler("MIDI", e.RawMessage);
		}

		static async void MidiIn1(object sender, MidiInMessageEventArgs e)
		{
			await View.EventHandler("MIDI", e.RawMessage | (1 << 24));
		}

		static async void MidiIn2(object sender, MidiInMessageEventArgs e)
		{
			await View.EventHandler("MIDI", e.RawMessage | (2 << 24));
		}

		static void MidiIn_ErrorReceived(object sender, MidiInMessageEventArgs e)
		{
			OKSHmenu.Info(String.Format("MidiIn_ErrorReceived():  Message 0x{0:X8} Event {1}",
				e.RawMessage, e.MidiEvent));
		}
	}
}
