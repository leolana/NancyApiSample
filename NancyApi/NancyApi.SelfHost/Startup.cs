using System.Diagnostics;
using Microsoft.Owin;
using Microsoft.Owin.Extensions;
using Nancy;
using Nancy.Owin;
using NancyApi.SelfHost;
using Owin;

[assembly: OwinStartup(typeof(Startup))]
namespace NancyApi.SelfHost
{
	public class Startup
	{
		public void Configuration(IAppBuilder app)
		{

			var errorLog = Elmah.ErrorLog.GetDefault(null);
			errorLog.ApplicationName = "/LM/W3SVC/2/ROOT/NancyApi.SelfHost";

			app.UseNancy(options =>
			{
				options.Bootstrapper = new Bootstrapper();
				options.PassThroughWhenStatusCodesAre(HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
			})
				.UseStageMarker(PipelineStage.MapHandler);
		}
	}
}