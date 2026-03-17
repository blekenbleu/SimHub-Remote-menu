using System;
using System.Collections.Generic;
using System.Windows;

namespace blekenbleu.SimHub_Remote_menu
{
	public partial class WebMenu
	{
		// check whether current properties differ from JSON
		bool Changed()
		{
			bool changed = false;
			
			if (0 > gndx || 0 > cndx)
				return changed;

			for (int p = 0; p < Settings.Name.Count; p++)
				if (simValues[p].Current != Settings.Value[p]
				 || simValues[p].Default != Settings.defaults[p])
				{
					changed = true;
					break;
				}

			Control.Model.ChangedVisibility = changed ? Visibility.Visible : Visibility.Hidden;
			return changed;
		}

		public void SliderButtton()			// List<GameList> Glist) "SelectedAsSlider" AddAction
		{
			slider = View.Selection;
			if (0 > slider || slider >= simValues.Count)
				return;

			Control.Model.SliderProperty = HttpServer.SliderProperty = simValues[slider].Name;
			/* slider View.SL.Maximum = 100; scale property to it, based on Steps[slider]
			 ; Steps		Guestimated range
			 ; 1 (0.01)		0 - 2
			 ; 10 (0.10)	0 - 10
			 ; 100 (1)		0 - 100
			 ; 1000 (10)	0 - 1000
			 */
			if (100 < Convert.ToDouble(simValues[slider].Default))
			{
				SliderFactor[0] = 10;	// slider to value
				SliderFactor[1] = 0.1;	// value to slider
			}
			else if (0 != Steps[slider] % 10)
			{
				SliderFactor[0] = 0.02;	// slider to value
				SliderFactor[1] = 50;	// value to slider
			} else if (0 != Steps[slider] % 100) {
				SliderFactor[0] = 0.1;	// slider to value
				SliderFactor[1] = 10;	// value to slider
			} else {
				SliderFactor[0] = 1;	// slider to value
				SliderFactor[1] = 1;	// value to slider
			}
			ToSlider();
		}

		// called in End() and CarChange()
		// Update game default, game change, (perhaps new) car change values
		// return whether JSON should be saved to disk
		bool UpdateGame()	// called in End(), CarChange()
		{
			if (0 == CurrentCar?.Length || 0 == GamePropCount)
				return write;							// nothing to save

			// simValues.Current gets data.gList[gndx].rList in (Init)
			// Update game default, game change, car change values 
			List<string> vList = new List<string> {};	// current game
			List<string> Car = new List<string> {};	 // current car (may be new)

			for (int i = 0; i < GamePropCount; i++)
			{
				string Current = simValues[i].Current,
						Default = simValues[i].Default;

				if (Current != data.gList[gndx].rList[i])
					write = true;
				if (i < CarPropCount)
					Car.Add(Current);
				vList.Add(Current);	// Current per-car+game

				// game defaults
				if (data.gList[gndx].cList[0].vList[i] != Default)
				{
					write = true;
					data.gList[gndx].cList[0].vList[i] = Default;
				}
			}
			if (write)
				data.gList[gndx].rList = vList;	// for game changes

			if (0 > (cndx = data.gList[gndx].cList.FindIndex(c => c.Name == CurrentCar)))
			{
				write = true;		// add car to game
				cndx = data.gList[gndx].cList.Count;
				data.gList[gndx].cList.Add(new CarL { Name = CurrentCar, vList = Car });
			}
			else if (write)
				data.gList[gndx].cList[cndx].vList = Car;

			return write;			// UpdateGame()
		}
	}		// public partial class WebMenu
}
