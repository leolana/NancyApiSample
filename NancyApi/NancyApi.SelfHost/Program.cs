using System;
using System.Web;
using Elmah;
using Nancy.Hosting.Self;

namespace NancyApi.SelfHost
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var uri = new Uri("http://localhost:3579");

			using (var host = new NancyHost(uri))
			{
				host.Start();

				Console.WriteLine("Your application is running on " + uri);
				Console.WriteLine("Press any [Enter] to close the host.");
				Console.ReadLine();
			}
		}
	}
}
