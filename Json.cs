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

			// this should be unnecessary if Reconcile() works..
			if (gCount != data.gList[gndx].cList[0].Vlist.Count
			 || pCount != data.gList[gndx].cList[cndx].Vlist.Count)
				changed = true;
			else for (int p = 0; p < gCount; p++)
				if (simValues[p].Default != data.gList[gndx].cList[0].Vlist[p]			// per-game default change?
		 		 || p < pCount && simValues[p].Current != data.gList[gndx].cList[cndx].Vlist[p]) // per-car change?
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

		List<string> DefaultCopy()		// called in SaveSlim(), End()
		{
			int i;
			List<string> New = new List<string> { };
			for (i = 0; i < gCount; i++)
				New.Add(simValues[i].Default);
			return New;
		}

		List<string> CurrentCopy()		// called in SaveSlim()
		{
			int i;
			List<string> New = new List<string> { };
			for (i = 0; i < pCount; i++)
				New.Add(simValues[i].Current);
			return New;
		}

		int GameIndex(string gnew)
		{
			if (1 > gnew.Length)
				return gndx;										// should be unlikely

			for (int g = 0; g < data.gList.Count; g++)
				if (0 == data.gList[g].cList.Count
				 || null == data.gList[g].cList[0].Name)
					data.gList.RemoveAt(g--);					// Reconcile() failure
				else if (gnew == data.gList[g].cList[0].Name)
					gndx = g;
			return gndx;
		}

		bool SaveSlim()	// called in End(), CarChange() and maybe Changed()
		{
			if (null == CurrentCar || 0 == gCount)
				return false;			// nothing to save

			if (0 > GameIndex(Gname))	// first car for this game?
			{
				write = true;			// first car
				gndx = data.gList.Count;
				data.gList.Add(new GameList
					{ cList = new List<CarL>
						{ new CarL { Name = Gname,
									 Vlist = DefaultCopy()
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
					  Vlist = CurrentCopy()
					}
				);
			} else {								// property value changes?
				for (int i = 0; i < gCount; i++)
					if (data.gList[gndx].cList[0].Vlist[i] != simValues[i].Default)
					{
						data.gList[gndx].cList[0].Vlist = DefaultCopy();
						write = true;	// per-game property change
						break;
					}
				for (int i = 0; i < pCount; i++)
					if (data.gList[gndx].cList[cndx].Vlist[i] != simValues[i].Current)
					{
						data.gList[gndx].cList[cndx].Vlist = CurrentCopy();
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

			if (null !=cname && 0 < cname.Length && null != gnew && 0 < gnew.Length)	// valid?
			{
				GameList game = null;
				int i, count = 0, vcount = 0;


				Msg = "Current Car: " + cname;
				if (0 < Gname.Length && SaveSlim())		// do not save first instance
					Msg += $";  {CurrentCar} saved";
				ml = Msg.Length;

				for (i = 0; i < simValues.Count; i++)			// copy Current to previous
					Previous(i, simValues[i].Current);

				// indices for new car
				if (0 <= GameIndex(gnew))						// sets gndx
				{
					game = data.gList[gndx];
					cndx = game.cList.FindIndex(c => c.Name == cname);
					vcount = game.cList[0].Vlist.Count;
					count = gCount > vcount ? vcount : gCount;
				}
				else cndx = -1;

				if (0 > cndx && null != game)
				{
					NewCar = "true";
					if (0 <= gndx)									// set at line 132
					{												// not a new game
						if (gnew != Settings.game)
						{											// different game
							count = pCount > vcount ? vcount : pCount;
							for (i = 0; i < count; i++)				// per-car defaults
								Default(i, game.cList[0].Vlist[i]);
						}
						for (i = pCount; i < count; i++)			// per-game defaults
							Default(i, game.cList[0].Vlist[i]);	// perhaps altered since .ini
					}
				}
				else
				{													// existing car
						NewCar = "false";
						if (cname != Settings.carid && null != game)				// previous car?
							for (i = 0; i < pCount; i++)
								Current(i, game.cList[cndx].Vlist[i]);
						if (null == CurrentCar && null != game)						// first in this game instance?
						{											// restore game defaults
							count = pCount > vcount ? vcount : pCount;
							for (i = 0; i < count; i++)
								Default(i, game.cList[0].Vlist[i]);
							count = gCount > vcount ? vcount : gCount;
							for(i = pCount; i < count; i++)
								Current(i, Default(i, game.cList[0].Vlist[i]));
						}
				}
				Settings.carid = CurrentCar = cname;
				Changed();
			}
			else if (null == cname)
				Msg = "null CarID";
			else if (0 == cname.Length)
				Msg = "empty CarID";

			if (null == gnew)
				Msg += ", null CurrentGame Name, ";
			else if (0 == gnew.Length)
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
