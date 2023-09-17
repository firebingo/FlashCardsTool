using FlashCards.Server.Configuration;
using FlashCards.Shared.Util;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FlashCards.Server.Data
{
	public class CreateMySqlDb
	{
		private static readonly Regex _dateRegex = new Regex(@"^FlashCards.Server.Data.Scripts.MySQL.(\d{0,})_(\d{0,})", RegexOptions.Compiled);

		public async Task<bool> CreateDatabase(AppSettings settings, ServiceDbContext context, ILogger logger)
		{
			MySqlConnection? connection = null;
			MySqlTransaction? transaction = null;
			try
			{
				connection = new MySqlConnection(settings.DbSettings.ConnectionString);
				await connection.OpenAsync();
				transaction = await connection.BeginTransactionAsync();
				string? nameCheck = null;
				using (var command = connection.CreateCommand())
				{
					command.CommandText = $"SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{settings.DbSettings.DbName}'";
					nameCheck = (await command.ExecuteScalarAsync()) as string;
				}
				var assembly = Assembly.GetExecutingAssembly();
				if (string.IsNullOrWhiteSpace(nameCheck))
				{
					var initialScript = await FileUtil.ReadEmbeddedResourceLinesAsync(assembly, "FlashCards.Server.Data.Scripts.MySQL.init.sql");
					using var command = connection.CreateCommand();
					command.Transaction = transaction;
					command.CommandText = string.Join('\n', initialScript.Select(x => x.Replace("{{dbname}}", settings.DbSettings.DbName)));
					await command.ExecuteNonQueryAsync();
				}
				var resources = assembly.GetManifestResourceNames()
					.Where(x => _dateRegex.IsMatch(x))
					.OrderBy(x => x)
					.ToList();

				var meta = await context.MetaData.FirstOrDefaultAsync();
				if (meta == null)
				{
					logger.LogError("Failed to select meta row from DB");
					return false;
				}
				//Check for and run update scripts if there are any.
				resources = resources
					.Where(x => DateTime.ParseExact(_dateRegex.Match(x).Groups[1].Value, "yyyyMMdd", CultureInfo.InvariantCulture) > meta.VersionDate ||
								(DateTime.ParseExact(_dateRegex.Match(x).Groups[1].Value, "yyyyMMdd", CultureInfo.InvariantCulture) == meta.VersionDate && int.Parse(_dateRegex.Match(x).Groups[2].Value) > meta.Version))
					.ToList();
				foreach (var resource in resources)
				{
					var reg = _dateRegex.Match(resource);
					var yearUpdate = DateTime.ParseExact(reg.Groups[1].Value, "yyyyMMdd", CultureInfo.InvariantCulture);
					var versionUpdate = int.Parse(reg.Groups[2].Value);
					var script = await FileUtil.ReadEmbeddedResourceLinesAsync(assembly, resource);
					var scriptText = string.Join('\n', script.Select(x => x.Replace("{{dbname}}", settings.DbSettings.DbName)));
					if (!string.IsNullOrWhiteSpace(scriptText))
					{
						using var command = connection.CreateCommand();
						command.CommandText = scriptText;
						command.Transaction = transaction;
						await command.ExecuteNonQueryAsync();
					}
					using var metaCommand = connection.CreateCommand();
					metaCommand.CommandText = $"USE {settings.DbSettings.DbName};\nUPDATE metaData SET versionDate = @VersionDate, version = @Version, modifiedTime = @ModifiedDate WHERE mkey = @mkey;";
					metaCommand.Parameters.Add(new MySqlParameter("@VersionDate", yearUpdate));
					metaCommand.Parameters.Add(new MySqlParameter("@Version", versionUpdate));
					metaCommand.Parameters.Add(new MySqlParameter("@ModifiedDate", DateTime.UtcNow));
					metaCommand.Parameters.Add(new MySqlParameter("@mkey", meta.Key));
					metaCommand.Transaction = transaction;
					await metaCommand.ExecuteNonQueryAsync();
				}

				await transaction.CommitAsync();
			}
			catch (Exception ex)
			{
				try
				{
					if (transaction != null)
						await transaction.RollbackAsync();
				}
				catch (Exception iex)
				{
					logger.LogError(iex, "Exception rolling back transaction");
				}

				logger.LogError(ex, "Exception creating DB");
				return false;
			}
			finally
			{
				if (transaction != null)
				{
					await transaction.DisposeAsync();
				}
				if (connection != null)
				{
					await connection.DisposeAsync();
				}
			}

			return true;
		}
	}
}
