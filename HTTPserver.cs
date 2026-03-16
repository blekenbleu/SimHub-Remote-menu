using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace blekenbleu.SimHub_Remote_menu
{
	partial class HttpServer	// works in .NET Framework 4.8 WPF User Control library (SimHub plugin)
	{
		static Control View;

		// adapted from https://github.com/blekenbleu/TcpMultiClient
		internal static void Start(Control v)
		{
			if (null == View)	// continue running over game changes
				Task.Run(() => HttpServer.OpenAsync());
			View = v;			// JavaScript() sets RowColorSlider({1 + View.Selection}
		}

		static readonly byte[] ok = Encoding.UTF8.GetBytes(
			"HTTP/1.1 200 OK\nContent-Type: text/event-stream; charset=UTF-8\n\n\n"
			+ ViewModel.SSEtext(true)
		);
		static readonly byte[] not = Encoding.UTF8.GetBytes("HTTP/1.1 404 NOT FOUND\n\n");

		// handle HTTP traffic, except SSE
		// null return wants caller to handle other responses
		static byte[] IsHttp(string msg, StreamReader sr)
		{
			byte[] which = null;

			if (null != msg)
			{
				string[] actionLine = msg?.Split(new char[] { ' ' }, 3);
				if (null != actionLine && "POST" == actionLine[0] || "GET" == actionLine[0])
				{
					which = ( "/sse" == actionLine[1]) ? ok
							: "/SSE" == actionLine[1] ? ok
							: "/" == actionLine[1] ? Encoding.UTF8.GetBytes(Table())
							: not;
					for (string line = sr.ReadLine(); null != line && 0 < line.Length; line = sr.ReadLine());
				}
				if (not == which)
					WebMenu.Info("IsHttp:\tHTTP/1.1 404 NOT FOUND\n");
			}
			return which;
		}

		static byte[] Welcome(string id, int count)
		{
			return Encoding.UTF8.GetBytes($"Welcome, {id}!  Connected clients: {count}\n");
		}

		// Served page is passive; only SSE from JavaScript is supported.
		// Any other request gets table<>
		static async Task ClientTask(TcpClient client, string clientId)	// invoked by TcpServer.cs MultiClientTcpServer()
		{
			try
			{
				using (client)
				using (NetworkStream stream = client.GetStream())
				using (StreamReader sr = new StreamReader(stream))
				{
					byte[] response;
					bool connected = false;

					string msg = await sr.ReadLineAsync();

					WebMenu.Info($"ClientTask: {clientId} first: {msg}");
					if (null == msg )
						stream.Close();
					else {									// Test for HTTP
						bool ht = false;

						response = IsHttp(msg, sr);
						if (null == response)
							response = Welcome(clientId, clients.Count);
						else ht = true;
						

						clients[clientId] = new SsClient() { Tc = client, Ht = ht };
						
						await stream.WriteAsync(response, 0, response.Length);

						WebMenu.Info($"ClientTask:\n---- {clientId} StreamReader connected ---");
						connected = true;
					}

					while (connected)
					{
						string request = await sr.ReadLineAsync();

						if (null == request)							// browser disconnect?
							break;

						WebMenu.Info($"ClientTask:  {clientId}:  {request}");
						response = IsHttp(request, sr);
						if(null != response)							// HTTP client request?
						{
							await stream.WriteAsync(response, 0, response.Length);
							continue;
						}

						// Send non-HTTP request to all clients (except sender)
						string tcpMsg = $"{clientId}: {request}\n";
						byte[] tcpBytes = Encoding.UTF8.GetBytes(tcpMsg);		// non-HTTP
						byte[] reqB = Encoding.UTF8.GetBytes($"{request}\n");	// SSE

						foreach (var c in clients)
						{
							if (c.Key == clientId || !c.Value.Tc.Connected)
								continue;

							byte[] togo = c.Value.Ht ? reqB : tcpBytes;
							try
							{
								NetworkStream clientStream = c.Value.Tc.GetStream();
								await clientStream.WriteAsync(togo, 0, togo.Length);
							}
							catch
							{
								// Remove disconnected client
								clients.TryRemove(c.Key, out _);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				WebMenu.Info($"ClientTask:  Error handling client {clientId}: {ex}");
			}
			finally
			{
				clients.TryRemove(clientId, out _);
				WebMenu.Info($"ClientTask:  {clientId} disconnected;  count = {clients.Count}");
			}
		}

//		specific to SimHub-Remote-menu
		public static string[] urls;
		public static int pageViews = 0;
		public static int requestCount = 0;
		public static string end = "</body></html>";
		public static string head = 
			"<!DOCTYPE>" +
			"<html>" +
			  "<head><style>th, td {padding-right: 30px;}</style>" +
				"\n<title>HttpListener Example</title>" +
				"\n<link rel='icon' href=" +
					"'https://media.geeksforgeeks.org/wp-content/cdn-uploads/gfg_200X200.png'" +
					" type='image/x-icon'>" +
			  "\n</head>\n" +
			  "\n<body style='font-size: 30px; margin-left: 50px;'>";

		public static string pageData =
			"\n<form method='post' action='shutdown'>" +
				"\n<input type='submit' value='Shutdown'{2}>" +
			"\n</form>" +
			"\n<p> &emsp; Page Views: {0};&nbsp; Request Count: {1}</p>\n";

		static string Table ()
		{
			string data = head + HTMLtable() + end;
			string sw =
			"HTTP/1.1 200 OK\n"
			+ "Content-Type:text/html; charset=UTF-8\n"
			+ "Server: TcpMultiClient\n"
			// response payload
			+ $"Content-Length: {data.Length}\n\n"
			+ data;
			return sw;
		}
	}		// class
}			// namespace
