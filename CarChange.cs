namespace blekenbleu.SimHub_Remote_menu
{
    public partial class WebMenu
    {
/*--------------------------------------------------------------
 ;	  invoked for CarId changes
 ;--------------------------------------------------------------- */
		void CarChange(string cname, string gnew)
		{
            int ml = 0;

			if (0 == simValues.Count)
				return;

			if (0 < gnew?.Length)	// valid?
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
			Control.Model.StatusText = Gname + " " + CurrentCar;
			ToSlider();
			Control.Model.ButtonVisibility = System.Windows.Visibility.Visible;	// ready
		}
	}
}
