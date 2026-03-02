## `HttpListener.Prefixes.Add()` issues
- perhaps only Windows 11 Home?
- [Microsoft Oct Update KB5066133 or KB5066793](https://www.reddit.com/r/ispyconnect/comments/1o7e1zl/my_softwareagent_stopped_working_after_microsoft/)

[Reportedly](https://stackoverflow.com/questions/2564669/net-httplistener-prefix-issue-with-anything-other-than-localhost), something like this Administrator: Command Prompt should enable IP access:
```
netsh http add urlacl url=http://192.168.1.147:8765/ user=bleke

URL reservation successfully added
```
.. but instead, ALL `HttpListener.Prefixes.Add()` fails:
```
[2026-01-27 06:00:43,051] INFO - WebMenu.Serve(): HttpListenerException transaction System.Net.HttpListenerException (0x80004005): Access is denied
   at System.Net.HttpListener.AddAllPrefixes()
   at System.Net.HttpListener.Start()
   at blekenbleu.SimHub_Remote_menu.HttpServer.Serve() in https://github.com/blekenbleu/HTTPserver.cs:line 147 
```
To restore local function:
```
C:\Windows\System32>netsh http delete urlacl url=http://192.168.1.147:8765/

URL reservation successfully deleted
```
### Doomed web hits
- [How to properly set netsh http add urlacl?](https://stackoverflow.com/questions/71024816/how-to-properly-set-netsh-http-add-urlacl)
- [add urlacl](https://learn.microsoft.com/en-us/windows/win32/http/add-urlacl)
- [In what scenarios will I use `netsh http add urlacl`?](https://superuser.com/questions/1272374/in-what-scenarios-will-i-use-netsh-http-add-urlacl)
- [Windows 11 and API port blocked in normal mode](https://forum.sequencegeneratorpro.com/t/windows-11-and-api-port-blocked-in-normal-mode/17742)

## Work-arounds
- [elevate perms and add http ACL entries through netsh - stackoverflow](https://stackoverflow.com/a/10128350)
- [**Replace `HTTPListener()` with `TcpListener()`**](https://github.com/dotnet/core/issues/1413)
	- [SimpleHTTPServer using `TcpListener()` - gist uses Thread](https://gist.github.com/aksakalli/9191056?permalink_comment_id=2756082)
		- [TcpListener Class](https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.tcplistener?view=netframework-4.8)
	- [Create a TcpListener using **`IPAddress.Any`** and port](https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/sockets/tcp-classes#create-a-tcplistener)
		- [`IPAddress.Any`:&nbsp; 0.0.0.0]()
	- [asynchronous TcpListener using async/await - stackoverflow](https://stackoverflow.com/a/21831405)
	- [Task-based `TcpListener()` HTTP server](https://www.programmersought.com/article/59981249482/)
	- [TcpListener listening socket loop](https://stackoverflow.com/a/19387431)
	- [custom `TcpListener()` HTTP server - Medium](https://medium.com/@haroonayaz/mastering-c-with-a-custom-built-http-server-an-exciting-dive-into-network-programming-427792a02cd6) - [GitHub](https://github.com/Kyojur0/HTTP-Server)
	- [Mastering TCPListener in C# - YouTube](https://www.youtube.com/watch?v=TAGoid4u6PY)
	- [Simple `TcpListener()` HTTP server in C# - codeproject.com](https://www.codeproject.com/articles/Simple-HTTP-Server-in-C) handling GET and POST
	- Server task blocks awaiting clients:&nbsp; [`TcpClient client = server.AcceptTcpClient();`](https://www.aicodesnippet.com/c-sharp/networking/simple-tcp-server-and-client-with-tcplistener-and-tcpclient.html)
	- [C# HTTP server using `TcpListener()` - c-sharpcorner](https://www.c-sharpcorner.com/article/creating-your-own-web-server-using-C-Sharp/)
	= [NikolayIT/SimpleHttpServerWithTcpListener.cs - gist](https://gist.github.com/NikolayIT/91dee5fea4386199ea6171de80eb2be4)
	- [C# HTTP server using TCP `Socket()` - Medium](https://medium.com/@antoharyanto/creating-an-http-server-using-tcp-socket-in-c-without-third-party-libraries-for-a-better-a68d2102b1d0)
		- [Socket IP server Listen() and AcceptConnect()](https://stackoverflow.com/questions/15514800/client-side-ipendpoint-cant-use-ipaddress-any)
			- [Socket.EndAccept Method](https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.socket.endaccept?view=netframework-4.8)
