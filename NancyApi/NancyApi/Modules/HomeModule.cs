using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NancyApi.Modules
{
    public class HomeModule : Nancy.NancyModule
    {
		public HomeModule()
		{
			Get["/"] = _ => "Hello World!";
		}
	}
}
