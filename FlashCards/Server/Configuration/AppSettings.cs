using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;

namespace FlashCards.Server.Configuration
{
	public class AppSettings
	{
		public DbSettings DbSettings { get; set; } = new DbSettings();
		public UserSettings UserSettings { get; set; } = new UserSettings();
		public LogSettings Logging { get; set; } = new LogSettings();
	}

	public enum DbSettingsType
	{
		MySql = 0,
		SqlLite = 1
	}

	public class DbSettings
	{
		public string DbName { get; set; } = "FlashCardsTool";
		public DbSettingsType Type { get; set; } = DbSettingsType.MySql;
		public string ConnectionString { get; set; } = string.Empty;
		[JsonIgnore]
		public string FullConnectionString
		{
			get => $"{ConnectionString};Initial Catalog={DbName};";
		}
	}

	public class UserSettings
	{
		public int IdGenId { get; set; } = 0;
		public bool RequireEmail { get; set; } = false;
		public bool RegistrationOpen { get; set; } = true;
	}

	public class LogSettings
	{
		public LogLevel MinLogLevel { get; set; } = LogLevel.Information;
	}
}
