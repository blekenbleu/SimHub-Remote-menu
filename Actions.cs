using SimHub.Plugins;

namespace blekenbleu.SimHub_Remote_menu
{
	public partial class WebMenu
	{
		/// <summary>
		/// Helper functions used in Init() AddAction()s and Control.xaml.cs Button Clicks
		/// </summary>
		void Actions()
		{
			this.AddAction("IncrementSelectedProperty", (a, b) => Ment(1));
			this.AddAction("DecrementSelectedProperty", (a, b) => Ment(-1));
			this.AddAction("NextProperty",				(a, b) => Select(true)	);
			this.AddAction("PreviousProperty",			(a, b) => Select(false)	);
			this.AddAction("SwapCurrentPrevious",		(a, b) => Swap()		);
			this.AddAction("CurrentAsDefaults",			(a, b) => SetDefault());
			this.AddAction("SelectedAsSlider",			(a, b) => SliderButtton());		// Json.cs
		}

		/// <param name="sign"></param> should be 1 or -1
		/// <param name="prefix"></param> should be "in" or "de"
		public void Ment(int sign)
		{
			if (0 == Gname.Length || 0 == CurrentCar.Length)
				return;

			int step = Steps[View.Selection];
			int iv = (int)(0.004 + 100 * float.Parse(simValues[View.Selection].Current));

			iv += sign * step;
			if (0 <= iv)
			{
				SetCurrent(View.Selection, (0 != step % 100) ? $"{(float)(0.01 * iv)}"
										: $"{(int)(0.004 + 0.01 * iv)}");
				Changed();
				if (slider == View.Selection)
					ToSlider();
			}
		}

		/// <summary>
		/// Select next or prior property; exception if invoked on other than UI thread
		/// </summary>
		/// <param name="next"></param> false for prior
		public void Select(bool next)
		{
			if (0 == Gname.Length || 0 == CurrentCar.Length)
				return;

			if (next)
			{
				if (++View.Selection >= simValues.Count)
					View.Selection = 0;
			}
			else if (0 < View.Selection)	// prior
				View.Selection--;
			else View.Selection = (byte)(simValues.Count - 1);
			SelectedStatus();		// Select()
		}

		public void Swap()
		{
			string temp;
			for (int i = 0; i < simValues.Count; i++)
			{
				temp = simValues[i].Previous;
				SetPrevious(i, simValues[i].Current);
				SetCurrent(i, temp);
			}
			ToSlider();		// Swap()
			Changed();
		}

		internal void SetDefault()						// List<GameList> Glist) "CurrentAsDefaults" AddAction
		{
			if (0 == Gname.Length)
				OOps("SetDefault: no Gname");
			else {
				int p = View.Selection;

				SetDefault(p, simValues[p].Current);	// End() sorts per-game changes
				Changed();
			}
		}

		// supporting cast ===================================================
		string SetCurrent(int i, string value)	// Ment(), Swap(), FromSlider(), Slider_DragCompleted()
		{
			simValues[i].Current = value;
			HttpServer.SSEcell(1 + i, 1, value);
			return value;
		}

		void CurrentSlider(double value)
		{
			SetCurrent(slider, (SliderFactor[0] * (int)(0.5 + value)).ToString());
		}

		string SetPrevious(int i, string value)	// Swap()
		{
			simValues[i].Previous = value;
			HttpServer.SSEcell(1 + i, 2, value);
			return value;
		}

		private void SelectedStatus()			// Select(), CarChange()
		{
			Control.Model.SelectedProperty = (0 > View.Selection) ? "unKnown"
											: simValues[View.Selection].Name;
		}

		// simValues set methods
		string SetDefault(int i, string value)	// SetDefault(), SetDefault(i)
		{
			simValues[i].Default = value;
			HttpServer.SSEcell(1 + i, 3, value);
			return value;
		}

		string SetDefault(int i)				// CarChange()
		{
			return SetDefault(i, data.gList[gndx].cList[0].vList[i]);
		}

		internal void ToSlider()				// Ment(), Swap(), SetSlider(), CarChange()
		{
			if(0 > slider)
				return;

			double stuff = 0;

			Control.Model.SliderProperty = HttpServer.SliderProperty
							= simValues[slider].Name + ":  " + simValues[slider].Current;
			try
			{
				stuff = System.Convert.ToDouble(simValues[slider].Current);
			}
			catch
			{
				OOps("ToSlider(): bad Current value '{simValues[slider].Current}'");
			}
			finally
			{
				Control.Model.SliderValue = HttpServer.SliderValue = SliderFactor[1] * stuff;
			}
		}

		// Control.xaml action -------------------------------------------------
		internal void FromSlider(double value)	// Slider_DragCompleted()
		{
			Control.Model.MidiStatus = " ";
			CurrentSlider(value);
			Changed();
			Control.Model.SliderProperty =  simValues[slider].Name + ":  " + simValues[slider].Current;
		}

		internal void ClickHandle(string butName)	// used by ButEvent(), Process(MidiMessage)
		{
			Control.Model.MidiStatus = " ";
			switch(butName)
			{
				case "b0":
					Select(false);
					break;
				case "b1":
					Select(true);
					break;
				case "b2":
					Ment(1);
					break;
				case "b3":
					Ment(-1);
					break;
				case "b4":
					Swap();
					break;
				case "b5":
					SetDefault();
					break;
				case "SB":
					SliderButtton();
					break;
				default:
                    Msg = "ClickHandle(): unconfigured click '{butName)'";
					OOpsMB();	// tested 1 Mar 2026
					break;
			}
		}
	}
}
