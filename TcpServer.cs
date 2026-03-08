// https://stackoverflow.com/a/26562695

using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace blekenbleu.SimHub_Remote_menu
{
	internal class SsClient
	{
		internal TcpClient Tc;
		internal bool Ht;
	}

	// https://dev.to/nickproud/net-tcplistener-accepting-multiple-client-connections-108d
	partial class HttpServer	// works in .NET Framework 4.8 WPF User Control library (SimHub plugin)
	{
		internal static string SliderProperty;
		internal static double SliderValue;
		static TcpListener server = null;		// works for any IP addresses
		internal static ConcurrentDictionary<string, SsClient> clients;
		static bool listening = false;
		static string localIP;

		// from https://github.com/blekenbleu/TcpMultiClient
		internal static void Stop()
		{
			if (listening)
			{
				listening = false;
				server?.Stop();
			}
		}

		internal static async Task OpenAsync()
		{
			var fun = Task.Run(() => MultiClientTcpServer());
			using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
			{
				socket.Connect("8.8.8.8", 65530);
				IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
				localIP = endPoint.Address.ToString();
				socket.Close();
			}
			await fun;
		}

		// TcpClient creates a Socket to send and receive data, accessible as TcpClient.Client
		// Each TcpClient.Client connection seemingly requires its own Task
		// https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.tcpclient.client?view=netframework-4.8
		public static async Task MultiClientTcpServer(int port = 8765)
		{
			clients = new ConcurrentDictionary<string, SsClient>();
			urls = new string[] { $"http://localhost:{port}/", $"http://127.0.0.1:{port}/", $"http://{localIP}:{port}" };

			try
			{
				server = new TcpListener(IPAddress.Any, port);
				server.Start();
				WebMenu.Info($"MultiClientTcpServer(): " + urls[2]);

				// Accept clients continuously
				for (listening = true; listening;)
				{
					TcpClient client = await server.AcceptTcpClientAsync();
					string clientId = $"Client_{DateTime.Now:HHmmss}_{client.Client.RemoteEndPoint}";

					WebMenu.Info($"MultiClientTcpServer():  New client {clientId}");
					_ = Task.Run(() => ClientTask(client, clientId));
				}
			}
			catch (Exception ex)
			{
				WebMenu.Info("MultiClientTcpServer() " + (listening ? $"error: {ex}" : "halted"));
			}
			finally
			{
				server?.Stop();
			}
		}
	}	   // class	HttpServer
} 			// namespace
