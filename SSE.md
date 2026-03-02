[*back*](README.md)  
## .NET Framework 4.8 `HttpListener` Server-Sent Event handler
 &emsp; *SimHub plugins use* `.NET Framework 4.8`

Based on [A Simple HTTP server in C#](https://16bpp.net/tutorials/csharp-networking/02/)  

### [Razor seems doomed](Doomed.md) for .NET Framework
 &emsp; if only for lack of support in recent Visual Studio  

[**`HttpListener.Prefixes.Add()` works only for `localhost` and `127.0.0.1`**](urlacl.md)
- which does not allow access by e.g. smartphone browser

- send current Properties `<table>` in response to initial GET [*(done)*](HTML.cs)
	- also send [JavaScript](JavaScript.md) to request SSE and update `<table>` cells [(*keep-alive works*)](JavaScript.cs)
		- browser ignores JavaScript with syntax errors
		- [In Chrome, CTRL + SHIFT + C launches Chrome Console, then select `Network` Tab](https://stackoverflow.com/questions/25291052/how-to-view-the-whole-raw-http-request)
	- update SSEhandler.cs with methods for custom SSE events `table`, `scroll`

For `req.Url.AbsolutePath.StartsWith("/SSE")` in `HandleIncomingConnections()`
- kick off a SSEkeep-alive Timer task (`Task keepalive = KeepAliveAsync();`)
```
    {
        await SSEtimer();   // keep-alive
        WebMenu.Info("KeepAliveAsync(): SSEtimer() ended.");
    }
```

`SSEtimer()` method periodically checks for SSEtimeout flag to be `true`  
- set `false` by `SSErespond()` table update data events  
```
	while (null != SSEcontext)
    {
        if (SSEtimeout)
        {
            SSErespond($"foo {++foo} async");
            if (null == SSEcontext)
                return;
        }
        SSEtimeout = true;
        await Task.Delay(5000);
    }
```
* `SSErespond()` sets `SSEcontext = null` for `HttpListenerException`s.

<hr>

### (SSE) [`response.OutputStream.WriteAsync(data, 0, data.Length);` exceptions](https://github.com/jeremybytes/channel-exceptions)  
 &emsp; *detecting SSE client disconnects otherwise not possible?*  

- [`WriteAsync(Byte[], Int32, Int32, CancellationToken)`](https://learn.microsoft.com/en-us/dotnet/api/system.io.stream.writeasync?view=netframework-4.8#system-io-stream-writeasync(system-byte()-system-int32-system-int32-system-threading-cancellationtoken))
- [`CancellationTokenSource.CancelAfter()`](https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/cancel-async-tasks-after-a-period-of-time#complete-example)
- [`await Task.WhenAny(DoSomethingAsync(), Task.Delay(TimeSpan.FromSeconds(1)));`](https://devblogs.microsoft.com/oldnewthing/20220505-00/?p=106585)
- [Using CancellationTokenSource](https://www.webdevtutor.net/blog/c-sharp-kill-running-task)
- [How to: Cancel a Task and Its Children](https://learn.microsoft.com/en-us/dotnet/standard/parallel-programming/how-to-cancel-a-task-and-its-children)
- [Cancel async tasks after a period of time](https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/cancel-async-tasks-after-a-period-of-time)
- [`Task.Delay()` is non-blocking](https://www.w3tutorials.net/blog/understanding-the-use-of-task-run-wait-async-await-used-in-one-line/)
- [`Task.Run()` vs `await`](https://dev.to/stevsharp/taskrun-vs-await-what-every-c-developer-should-know-1mmi)
	- use `Task.Run()` only when absolutely needed.
