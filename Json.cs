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

			for (int p = 0; p < simValues.Count; p++)
			
				if (simValues[p].Default != data.gList[gndx].cList[0].vList[p]							// per-game default
		 		 || (p < CarPropCount && simValues[p].Current != data.gList[gndx].cList[cndx].vList[p])	// per-car
				 || (p >= GamePropCount && simValues[p].Default != Settings.gDefaults[p - GamePropCount].Value)	// global
				)
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
			 ; Steps	   Guestimated range
			 ; 1  (0.01)	0 - 2
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

		List<string> GameDefaults()		// called in SaveSlim(), End()
		{
			int i;
			List<string> New = new List<string> { };
			for (i = 0; i < GamePropCount; i++)
				New.Add(simValues[i].Default);
			return New;
		}

		List<string> CurrentCarCopy()		// called in SaveSlim()
		{
			int i;
			List<string> New = new List<string> { };
			for (i = 0; i < CarPropCount; i++)
				New.Add(simValues[i].Current);
			return New;
		}

        // add or update car and per-game default values;
		// return whether JSON needs to be saved to disk
        bool SaveSlim()	// called in End(), CarChange()
		{
			if (0 == CurrentCar?.Length || 0 == GamePropCount)
				return false;			// nothing to save

			if (0 > (cndx = data.gList[gndx].cList.FindIndex(c => c.Name == CurrentCar)))
			{	// add car to game
				write = true;			// add car
				cndx = data.gList[gndx].cList.Count;
				data.gList[gndx].cList.Add(new CarL
					{ Name = CurrentCar,
					  vList = CurrentCarCopy()
					}
				);
			} else {								// property value changes?
				for (int i = 0; i < GamePropCount; i++)
					if (data.gList[gndx].cList[0].vList[i] != simValues[i].Default)
					{
						data.gList[gndx].cList[0].vList = GameDefaults();
						write = true;	// per-game property change
						break;
					}
				for (int i = 0; i < GamePropCount; i++)
					if (data.gList[gndx].cList[cndx].vList[i] != simValues[i].Current)
					{
						data.gList[gndx].cList[cndx].vList = CurrentCarCopy();
						write = true;	// per-car property change
						break;
					}
			}
			return write;			// SaveSlim()
		}
	}		// public partial class WebMenu
}
