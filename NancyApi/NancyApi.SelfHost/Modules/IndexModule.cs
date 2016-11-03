using Nancy;

namespace NancyApi.SelfHost.Modules
{
	public class IndexModule : NancyModule
	{
		public IndexModule()
		{
			Get["/"] = parameters =>
			{
				return View["index"];
			};
		}
	}
}