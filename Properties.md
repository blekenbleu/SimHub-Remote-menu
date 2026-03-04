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
Instead of storing that all-games property dictionary internally to SimHub,  
it is saved in a .json file located per .ini `WebMenu.file`.  
This facilitates sharing settings among users e.g. sharing a ShakeIt configuration.

New (to WebMenu) cars and games typically inherit properties and values from the most recent car (and game)...  
Any property value changes are updated in a memory image  
of the current game dictionary for each car change during a game session.  
When plugin `End()`s, e.g. for game changes, the all-games .json file gets updated.

A new car for unchanged game will use most recent per-car values.
- Otherwise, current and default values will be from that game dictionary. 
	- *most-recent* values are available as **Previous**.

However, .ini and .json files may change at any time...  
- an NCalcScripts .ini file may be changed to indicate a different `WebMenu.file`.  
- Property names may change or move between types.  
- a different .json file (with the same or different name) may be swapped  
- SimHub's internally saved instance of most recent car property values is unlikely to change between sessions.

WebMenu must sort incompatibilities among .ini, .json and interally saved property sets to populate simValues and Steps.
- In all cases, .ini is considered definitive for *property type* classifications.
- internally saved instance *values* are considered definitive for global and current car and game
	- for example, if .ini moves `propertyA` from per-car to `global`,  
		then that per-car default value becomes the new global default.
		- changing that moved property default value, e.g. to match the .ini,
			will require using the plugin UI.
