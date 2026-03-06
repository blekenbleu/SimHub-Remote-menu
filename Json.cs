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
				if (!SaveSlim())
					return changed;

			for (int p = 0; p < GamePropCount; p++)
				if (simValues[p].Default != data.gList[gndx].cList[0].Vlist[p]			// per-game default change?
		 		 || p < CarPropCount && simValues[p].Current != data.gList[gndx].cList[cndx].Vlist[p]) // per-car change?
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

		int GameIndex(string gnew)
		{
			if (1 > gnew?.Length)
				return gndx;										// should be unlikely

			for (int g = 0; g < data.gList.Count; g++)
				if (gnew == data.gList[g]?.cList[0]?.Name)
					return gndx = g;

			return gndx;
		}

        // add or update car and per-game default values;
		// return whether JSON needs to be saved to disk
        bool SaveSlim()	// called in End(), CarChange() and maybe Changed()
		{
			if (null == CurrentCar || 0 == GamePropCount)
				return false;			// nothing to save

			if (0 > GameIndex(Gname))	// first car for this game?
			{
				gndx = data.gList.Count;
				data.gList.Add(new GameList
					{ cList = new List<CarL>
						{ new CarL { Name = Gname,
									 Vlist = GameDefaults()
								   }
						}
					}
				);
			}

			if (0 > (cndx = data.gList[gndx].cList.FindIndex(c => c.Name == CurrentCar)))
			{	// add car to game
				write = true;			// add car
				cndx = data.gList[gndx].cList.Count;
				data.gList[gndx].cList.Add(new CarL
					{ Name = CurrentCar,
					  Vlist = CurrentCarCopy()
					}
				);
			} else {								// property value changes?
				for (int i = 0; i < GamePropCount; i++)
					if (data.gList[gndx].cList[0].Vlist[i] != simValues[i].Default)
					{
						data.gList[gndx].cList[0].Vlist = GameDefaults();
						write = true;	// per-game property change
						break;
					}
				for (int i = 0; i < GamePropCount; i++)
					if (data.gList[gndx].cList[cndx].Vlist[i] != simValues[i].Current)
					{
						data.gList[gndx].cList[cndx].Vlist = CurrentCarCopy();
						write = true;	// per-car property change
						break;
					}
			}
			return write;			// SaveSlim()
		}

/*--------------------------------------------------------------
 ;	  invoked for CarId changes, based on this `NCalcScripts/WebMenu.ini` entry:
 ;		  [ExportEvent]
 ;		  name='CarChange'
 ;		  trigger=changed(200, [DataCorePlugin.GameData.CarId])
 ;--------------------------------------------------------------- */
		void CarChange(string cname, string gnew)
		{
            int ml = 0;

			if (0 == simValues.Count)
				return;

			if (0 == cname?.Length)
				Msg = "empty CarID";
			else if (0 < gnew?.Length)	// valid?
			{
				GameList game = null;
				int i, count = 0, vcount = 0;


				Msg = "Current Car: " + cname;
				if (0 < Gname.Length && SaveSlim())		// do not save first instance
					Msg += $";  {CurrentCar} saved";
				ml = Msg.Length;

				for (i = 0; i < simValues.Count; i++)			// copy Current to previous
					SetPrevious(i, simValues[i].Current);

				// indices for new car
				if (0 <= GameIndex(gnew))						// sets gndx
				{
					game = data.gList[gndx];
					cndx = game.cList.FindIndex(c => c.Name == cname);
					vcount = game.cList[0].Vlist.Count;
					count = GamePropCount > vcount ? vcount : GamePropCount;
				}
				else cndx = -1;

				if (0 > cndx && null != game)
				{
					NewCar = "true";
					if (0 <= gndx)
					{                                   // not a new game
						if (gnew != Settings.game)
							i = 0;                      // different game, also set per-car
						else i = CarPropCount;			// ONLY per-game defaults
						for (; i < GamePropCount; i++)
							SetDefault(i);				// perhaps altered since .ini
					}
				}
				else
				{													// existing car
					NewCar = "false";
					if (cname != Settings.carid && null != game)	// previous car?
						for (i = 0; i < GamePropCount; i++)
							SetCurrent(i, game.cList[cndx].Vlist[i]);
					if (null == CurrentCar && null != game)			// first in this game instance?
					{												// restore game defaults
						for (i = 0; i < CarPropCount; i++)
							SetDefault(i);
						for(; i < GamePropCount; i++)
							SetCurrent(i, SetDefault(i, game.cList[0].Vlist[i]));
					}
				}
				Settings.carid = CurrentCar = cname;
				Changed();
			}

			if (0 == gnew?.Length)
				Msg += ", empty CurrentGame Name, ";
			else Gname = gnew;

			if (ml < Msg.Length)
			{
				if (once)
				{
					OOpsMB();
					once = false;
				}
				return;
			}
			else Msg = "";
			Control.Model.StatusText = Gname + " " + CurrentCar;					// CarChange()
			ToSlider();
			Control.Model.ButtonVisibility = System.Windows.Visibility.Visible;	// ready
		}	// CarChange()
	}		// public partial class WebMenu
}
