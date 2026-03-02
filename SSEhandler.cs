using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace blekenbleu.SimHub_Remote_menu
{
	// Source - https://stackoverflow.com/questions/28899954/net-server-sent-events-using-httphandler-not-working
	//			https://learn.microsoft.com/en-us/dotnet/api/system.io.stream?view=netframework-4.8

	partial class HttpServer
	{
		private static bool SSEtimeout = true, SSEonce = true;
		private static int foo = 0;

		// send event to each client
		public static async void SSErespond(string responseText)
		{
			SSEtimeout = false;
			if (null != clients && 0 < clients.Count)
			{
				SSEonce = true;
				byte[] togo = Encoding.UTF8.GetBytes($"{responseText}\n\n");

				foreach (var c in clients)
					if (c.Value.Tc.Connected)
					{
						try
						{
							NetworkStream clientStream = c.Value.Tc.GetStream();
							await clientStream.WriteAsync(togo, 0, togo.Length);
						}
						catch
						{ // Remove disconnected client
							clients.TryRemove(c.Key, out _);
						}
					}
					else WebMenu.Info("SSErespond():  null client!!?");

			} else if (listening) {
				if (SSEonce)
					WebMenu.Info("SSErespond():  no clients");
				SSEonce = false;
			}
		}

		// https://stackoverflow.com/questions/28899954/net-server-sent-events-using-httphandler-not-working
		public static void SSEvent(string name, string data)
		{
			SSErespond("event: " + name + "\ndata:{" + data + "}");
		}

		public async static Task SSEtimer()		// hopefully long-running
		{
			WebMenu.Info("SSEtimer(): launching");
			while (listening && 0 < clients.Count)
			{
				if (SSEtimeout)
					SSErespond($"data: keep-alive {++foo} async");
				SSEtimeout = true;
				await Task.Delay(15000);
			}
			WebMenu.Info("SSEtimer(): exiting");
		}
	}		// class
}			// namespace
