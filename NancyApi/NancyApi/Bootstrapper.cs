using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using Nancy.Elmah;

namespace NancyApi
{
	public class Bootstrapper : DefaultNancyBootstrapper
	{
		protected override void ApplicationStartup(Nancy.TinyIoc.TinyIoCContainer container, Nancy.Bootstrapper.IPipelines pipelines)
		{
			base.ApplicationStartup(container, pipelines);
			Elmahlogging.Enable(pipelines, "elmah", new string[0], new[] { HttpStatusCode.NotFound, HttpStatusCode.InsufficientStorage });
		}
	}
}
