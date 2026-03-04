## WebMenu property handling
This paradigm is largely inherited from the [JSONio](https://github.com/blekenbleu/JSONio) plugin..  

There are 3 types of properties, determined by [.ini](https://github.com/blekenbleu/SimHub-Remote-menu/blob/main/NCalcScripts/WebMenu.ini) content
- per-car
- per-game
- global

That .ini may change between games or plugin relaunches...

Each property will have 3 values, all numeric
- current
- previous
- default

Given many possible games and cars, lots of settings may accumulate.  
Instead of saving that property dictionary internally to SimHub,  
it is save in a .json file located per .ini `WebMenu.file`.  
This facilitates sharing settings among users e.g. sharing a ShakeIt configuration.

New (to WebMenu) cars and games typically inherit properties and values from the most recent car (and game)...  
Any property value changes are updated in a memory image  
of the current game dictionary for each car change within a game session,  
and, when plugin `End()`s, e.g. for game changes,   
that game dictionary is added to or updated in the .json file containing global properties and all game dictionaries.

Loading a new car for unchanged game will use most recent per-car values.
- if for an existing game *other than most recent*, values will be that game's defaults, *not most-recent* 
	- *most-recent* values are available as **Previous**.

However, .ini and .json files may change at any time...  
- an NCalcScripts .ini file may be changed to indicate a different `WebMenu.file`.  
- Property names may change or move between types.  
- a different .json file (with the same or different name) may be swapped  
- users typically will not alter the internally saved instance of most recent property values

WebMenu must sort incompatibilities among .ini, .json and interally saved property sets.
- In all cases, .ini is considered definitive for *property type* classifications.
- internally saved instance *values* are considered definitive for global and current car and game
	- for example, if .ini moves `propertyA` from per-car to `global`,  
		then that per-car default value becomes the new global default.
		- changing that moved property default value, e.g. to match the .ini,
			will require using the plugin UI.
