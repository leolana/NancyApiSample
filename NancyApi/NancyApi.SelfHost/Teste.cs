using System;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Text;
using System.Web;
using Elmah;

namespace NancyApi.SelfHost
{
	/// <summary>
	/// Handler for the "elmah/errorEventLog" section of the configuration file.
	/// </summary>
	public class ElmahErrorEventLogSectionHandler : SingleTagSectionHandler { }

	/// <summary>
	/// HTTP module that writes Elmah logged error to the Windows Application EventLog whenever an unhandled exception occurs in an ASP.NET web application.
	/// </summary>
	public class ElmahErrorEventLogModule : HttpModuleBase, IExceptionFiltering
	{
		private string eventLogSource;

		public event Elmah.ExceptionFilterEventHandler Filtering;

		/// <summary>
		/// Initializes the module and prepares it to handle requests.
		/// </summary>
		protected override void OnInit(HttpApplication application)
		{
			if (application == null)
				throw new ArgumentNullException("application");

			// Get the configuration section of this module.
			// If it's not there then there is nothing to initialize or do.
			// In this case, the module is as good as mute.
			IDictionary config = (IDictionary)ElmahConfiguration.GetSubsection("errorEventLog");
			if (config == null)
				return;

			// Get settings.
			eventLogSource = ElmahConfiguration.GetSetting(config, "eventLogSource", string.Empty);
			if (string.IsNullOrEmpty(eventLogSource))
				return;

			// Register an event source in the Application log.
			try
			{
				if (!EventLog.SourceExists(eventLogSource))
					EventLog.CreateEventSource(eventLogSource, "Application");
			}
			catch
			{
				// Don't register event handlers if it's not possible to register an EventLog source.
				// Most likely an application hasn't rights to register a new source in the EventLog.
				// Administration rights are required for this. Please register a new source manually.
				// Or maybe eventLogSource is not valid.
				return;
			}

			// Hook into the Error event of the application.
			application.Error += new EventHandler(OnError);
			Elmah.ErrorSignal.Get(application).Raised += new Elmah.ErrorSignalEventHandler(OnErrorSignaled);
		}

		/// <summary>
		/// Determines whether the module will be registered for discovery in partial trust environments or not.
		/// </summary>
		protected override bool SupportDiscoverability
		{
			get { return true; }
		}

		/// <summary>
		/// The handler called when an unhandled exception bubbles up to the module.
		/// </summary>
		protected virtual void OnError(object sender, EventArgs e)
		{
			HttpContext context = ((HttpApplication)sender).Context;
			OnError(context.Server.GetLastError(), context);
		}

		/// <summary>
		/// The handler called when an exception is explicitly signaled.
		/// </summary>
		protected virtual void OnErrorSignaled(object sender, Elmah.ErrorSignalEventArgs args)
		{
			OnError(args.Exception, args.Context);
		}

		/// <summary>
		/// Reports the exception.
		/// </summary>
		protected virtual void OnError(Exception e, HttpContext context)
		{
			if (e == null)
				throw new ArgumentNullException("e");

			// Fire an event to check if listeners want to filter out reporting of the uncaught exception.
			Elmah.ExceptionFilterEventArgs args = new Elmah.ExceptionFilterEventArgs(e, context);
			OnFiltering(args);

			if (args.Dismissed)
				return;

			// Get the last error and then write it to the EventLog.
			Elmah.Error error = new Elmah.Error(e, context);
			ReportError(error);
		}

		/// <summary>
		/// Raises the <see cref="Filtering"/> event.
		/// </summary>
		protected virtual void OnFiltering(Elmah.ExceptionFilterEventArgs args)
		{
			Elmah.ExceptionFilterEventHandler handler = Filtering;

			if (handler != null)
				handler(this, args);
		}

		/// <summary>
		/// Writes the error to the EventLog.
		/// </summary>
		protected virtual void ReportError(Elmah.Error error)
		{
			// Compose an error message.
			StringBuilder sb = new StringBuilder();
			sb.Append(error.Message);
			sb.AppendLine();
			sb.AppendLine();
			sb.Append("Date and Time: " + error.Time.ToString("dd.MM.yyyy HH.mm.ss"));
			sb.AppendLine();
			sb.Append("Host Name: " + error.HostName);
			sb.AppendLine();
			sb.Append("Error Type: " + error.Type);
			sb.AppendLine();
			sb.Append("Error Source: " + error.Source);
			sb.AppendLine();
			sb.Append("Error Status Code: " + error.StatusCode.ToString());
			sb.AppendLine();
			sb.Append("Error Request Url: " + HttpContext.Current.Request.Url.AbsoluteUri);
			sb.AppendLine();
			sb.AppendLine();
			sb.Append("Error Details:");
			sb.AppendLine();
			sb.Append(error.Detail);
			sb.AppendLine();

			string messageString = sb.ToString();
			if (messageString.Length > 32765)
			{
				// Max limit of characters that EventLog allows for an event is 32766.
				messageString = messageString.Substring(0, 32765);
			}

			// Write the error entry to the event log.
			try
			{
				EventLog.WriteEntry(eventLogSource, messageString, EventLogEntryType.Error, error.StatusCode);
			}
			catch
			{
				// Nothing to do if it is not possible to write an error message to the EventLog.
				// Most likely an application hasn't rights to write to the EventLog.
				// Or maybe eventLogSource is not valid.
			}
		}

	}

	/// <summary>
	/// Get the configuration from the "elmah" section of the configuration file.
	/// </summary>
	public class ElmahConfiguration
	{
		internal const string GroupName = "elmah";
		internal const string GroupSlash = GroupName + "/";

		public ElmahConfiguration() { }

		public static NameValueCollection AppSettings
		{
			get
			{
				return ConfigurationManager.AppSettings;
			}
		}

		public static object GetSubsection(string name)
		{
			return GetSection(GroupSlash + name);
		}

		public static object GetSection(string name)
		{
			return ConfigurationManager.GetSection(name);
		}

		public static string GetSetting(IDictionary config, string name)
		{
			return GetSetting(config, name, null);
		}

		public static string GetSetting(IDictionary config, string name, string defaultValue)
		{
			string value = NullString((string)config[name]);

			if (value.Length == 0)
			{
				if (defaultValue == null)
				{
					throw new Elmah.ApplicationException(string.Format(
						"The required configuration setting '{0}' is missing for the error eventlog module.", name));
				}

				value = defaultValue;
			}

			return value;
		}

		public static string NullString(string s)
		{
			return s == null ? string.Empty : s;
		}
	}
}