using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using Hangfire;
using Microsoft.Owin;
using Microsoft.Owin.Extensions;
using Nancy;
using Nancy.Owin;
using NancyApi;
using Owin;

[assembly: OwinStartup(typeof(Startup))]
namespace NancyApi
{
	public class Startup
	{
		public void Configuration(IAppBuilder app)
		{
			GlobalConfiguration.Configuration
			.UseSqlServerStorage(ConfigurationManager.ConnectionStrings["scheduler"].ConnectionString);

			app.UseHangfireDashboard("/hangfire");
			app.UseHangfireServer();

			RecurringJob.AddOrUpdate(
						() => Debug.WriteLine("Recurring Job completed successfully!"),
						Cron.Minutely);

			app.UseNancy(options =>
				options.PassThroughWhenStatusCodesAre(HttpStatusCode.NotFound, HttpStatusCode.InternalServerError)
			)
				.UseStageMarker(PipelineStage.MapHandler);
		}
	}
}