using System;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Elmah;
using Nancy.TinyIoc;

namespace NancyApi.SelfHost
{
	public class Bootstrapper : DefaultNancyBootstrapper
	{
		// The bootstrapper enables you to reconfigure the composition of the framework,
		// by overriding the various methods and properties.
		// For more information https://github.com/NancyFx/Nancy/wiki/Bootstrapper
		protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
		{
			pipelines.OnError.AddItemToEndOfPipeline((ctx, exception) =>
			{
				exception.LogToElmah();
				return null;
			});

			var errorLog = Elmah.ErrorLog.GetDefault(null);
			errorLog.ApplicationName = "/LM/W3SVC/2/ROOT/NancyApi.SelfHost";

			base.ApplicationStartup(container, pipelines);
			Elmahlogging.Enable(pipelines, "elmah", new string[0], new[] { HttpStatusCode.NotFound, HttpStatusCode.InsufficientStorage });
		}

		protected override void RequestStartup(TinyIoCContainer container, IPipelines pipelines, NancyContext context)
		{
			var errorLog = Elmah.ErrorLog.GetDefault(null);
			errorLog.ApplicationName = "/LM/W3SVC/2/ROOT/NancyApi.SelfHost";
			base.RequestStartup(container, pipelines, context);
		}
	}
}