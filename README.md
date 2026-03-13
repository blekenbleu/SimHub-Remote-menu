## [SimHub](https://www.simhubdash.com)-[Remote-menu](https://github.com/blekenbleu/SimHub-Remote-menu/wiki) for [OpenKneeboard](https://openkneeboard.com/)
*[SimHub plugin properties](https://github.com/blekenbleu/JSONio) HTTP table for e.g.
[OpenKneeboard](https://github.com/OpenKneeboard/OpenKneeboard)*  
![](example.png)  
WPF plugin user interface employs 7 buttons and one slider:  
![](plugin.png)  

### background
Currently, access from SteamVR to SimHub (and its plugin menus) is by e.g.
- [SteamVR's Desktop](https://store.steampowered.com/news/app/250820/view/2898585530113863169)
- [Desktop+](https://steamcommunity.com/app/1494460)

SimHub dash overlays lack VR support, but [OpenKneeboard can display SimHub
overlays](https://www.madmikeplays.com/free-downloads#block-yui_3_17_2_1_1742822224076_6340) via HTTP.

### wanted
A dedicated menu display always visible in VR  
for tweaking e.g. harness tensioner or haptics settings (properties).  
- instead of invoking a computationally expensive overlay GUI,
	- update HTML table properties and values
	- table navigation and changes by e.g. rotary encoders, sliders, buttons

HTML table updates should have lower processing overhead than graphical overlay..  

### resources
- from [JSONio](https://github.com/blekenbleu/JSONio):
	- in `SimHub-Remote-menu.csproj`:  
	 could not get [ReferencePath](https://learn.microsoft.com/en-us/troubleshoot/developer/visualstudio/project-build/troubleshooting-broken-references)
	 working;&nbsp; copied JSONio `HintPath`s  
	- SimHub confused `WebMenu` with `JSONio` plugin until renaming `class JSONio`.  
	`KSHmenu.ChangeProperties` needs its own `ExternalScript.CarChange` event trigger setting  
	in **SimHub Controls and events**.
- [**TcpMultiClient**](https://github.com/blekenbleu/TcpMultiClient) - multiple `keep-alive` TCP connections
	- TcpListener AcceptTcpClientAsync, ConcurrentDictionary, NetworkStream WriteAsync, StreamReader
	- a better starting point and learning exercise than either:
		- [A Simple HTTP SSE server in C#](SSE.md) - proof of concept -
  			[Gist](https://gist.github.com/define-private-public/d05bc52dd0bed1c4699d49e2737e80e7)
			- user-space `HttpListener` server can serve only to browsers on the same PC
		- [**TCPListener - based HttpServer**](https://github.com/blekenbleu/HttpServer) avoided localhost limitation,  
			but potentially problematic SSE `keep-alive`  

## plan
- generate an [HTML `<table>`](HTML.md) from `NCalcScripts/WebMenu.ini` JSON properties during `Init()`
- hand-code [JavaScript](JavaScript.md) for `<table>` updates by Server-Sent Events
- make [HTML](HTML.cs) + [JavaScript](JavaScript.cs) page available to client browsers
- [Server-Sent Events](SSE.md) for `<table>` cell property values and e.g. scroll actions - **working**
	- *replaced* TCPserver.cs content with [TcpMultiClient](https://github.com/blekenbleu/TcpMultiClient) `Program.cs` Main() + MultiClientTcpServer()
	- *replaced* [HTTPserver](https://github.com/blekenbleu/HttpServer) content with `IsHttp()` and `ClientTask()`
- current HTML scroll and slider set with car changes, not waiting for WPF menu open
- [multiple MIDI input device support](SemaphoreSlim.md) *with learning* to make `<table>` property value changes
- maintain client web sessions across game changes, when SimHub does not exit.
- *to do*:&nbsp; [improve MIDI click list handling](MIDI.md)

## bug
- `Expression error:The type initializer for 'Jint.Native.Global.GlobalObject' threw an exception.`
	- any Javascript ShakeIt Custom Effect formula returns this until SimHub is reinstalled.
	- loading a new build of this plugin provokes it again...?
	- `JavaScript` name collision perhaps provoked this?&nbsp; but *different namespace*
### new-to-me tricks  
- handle all button events in one method by [`(e.OriginalSource as FrameworkElement).Name`](https://stackoverflow.com/a/26938950)
- [NAudio `MidiIn.NumberOfDevices`, `MidiIn(deviceNumber)`](https://github.com/naudio/NAudio/blob/master/NAudioDemo/MidiInDemo/MidiInPanel.cs#L24)
	- [`ConcurrentDictionary<>`](https://www.dotnetperls.com/concurrentdictionary)  
	- [System.Threading.Channels FIFO queue](Channel.md)  
- [`TcpListener` Web Server](TcpListener.md)
- [scrolling TextBlock](https://stackoverflow.com/a/40626596)
- Click handler:&nbsp; [`var jumptbl = new SortedList<uint, Func<string, string> >();`](https://stackoverflow.com/a/7181866)  
	- [SortedList Class](https://learn.microsoft.com/en-us/dotnet/api/system.collections.sortedlist?view=netframework-4.8)
- [SemaphoreSlim](https://learn.microsoft.com/en-us/dotnet/api/system.threading.semaphoreslim?view=netframework-4.8)
	- replaces [`System.Threading.Channels BoundedChannel`](Channel.md), queuing tasks instead of MIDI payloads
	- avoids race condition between XAML `RoutedEvents` and NAudio `MidiInMessageEvents`
---
#### [SimHub plugins](https://github.com/SHWotever/SimHub/wiki/Plugin-and-extensions-SDKs) are .NET Framework 4.8 WPF User Control libraries
- [SimHub plugin build process](https://blekenbleu.github.io/static/SimHub/)  
	- [.NET Framework v4.8](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net48)  
		Windows-only version of .NET for building client and server applications;  
		latest supported version is `4.8.1` (*August 9th, 2022*), but SimHub uses `4.8`  
	- Visual Studio [`new project > WPF User Control Library (.NET Framework)`](https://learn.microsoft.com/en-us/dotnet/desktop/winforms/advanced/walkthrough-creating-new-wpf-content-on-windows-forms-at-design-time) has xaml
