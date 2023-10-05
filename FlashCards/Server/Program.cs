using FlashCards.Server.Auth;
using FlashCards.Server.Configuration;
using FlashCards.Server.Data;
using FlashCards.Server.Services;
using IdGen.DependencyInjection;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using NLog.Targets;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace FlashCards
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);
			if (builder.Environment.IsProduction() && Environment.GetEnvironmentVariable("IS_LOCAL_DEV") == "1")
				builder.WebHost.UseStaticWebAssets();

			builder.Configuration.AddJsonFile("appsettings.json");
			builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName.ToLower()}.json");
			//builder.Configuration.AddJsonFile($"appsettings.production.json");
			var isLocal = Environment.GetEnvironmentVariable("IS_LOCAL_DEV");
			if (isLocal == "1")
				builder.Configuration.AddJsonFile($"appsettings.local.json");
			var settings = builder.Configuration.GetSection("AppSettings").Get<AppSettings>()!;

			var nlogConfig = new NLogLoggingConfiguration(builder.Configuration.GetSection("NLog"));
			builder.Logging.ClearProviders();
			var dbTarget = (DatabaseTarget)nlogConfig.FindTargetByName("serviceDb");
			dbTarget.ConnectionString = settings.DbSettings.FullConnectionString;
			builder.Logging.AddNLog(nlogConfig);

			if (!Debugger.IsAttached)
			{
				builder.Services.AddResponseCompression(options =>
				{
					options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
						new[] { "application/x-font-ttf" });
					options.EnableForHttps = true;
				});
			}

			builder.Services.AddAuthentication(
			CookieAuthenticationDefaults.AuthenticationScheme
			).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme,
				options =>
				{
					options.LoginPath = PathString.Empty;
					options.LogoutPath = PathString.Empty;
					options.Cookie.SameSite = SameSiteMode.Strict;
					options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
					options.EventsType = typeof(CookieAuthEvents);
					options.SlidingExpiration = true;
				});
			builder.Services.AddAuthentication(options =>
			{
				options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
			});

			builder.Services.AddControllersWithViews().AddJsonOptions(options =>
			{
				options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
			});
			builder.Services.AddRazorPages();

			builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
			builder.Services.AddIdGen(settings.UserSettings.IdGenId);
			if (settings.DbSettings.Type == DbSettingsType.MySql)
				builder.Services.AddDbContext<ServiceDbContext>(options => options.UseMySQL(settings.DbSettings.FullConnectionString));
			else if (settings.DbSettings.Type == DbSettingsType.SqlLite)
				builder.Services.AddDbContext<ServiceDbContext>(options => options.UseSqlite(settings.DbSettings.FullConnectionString));

			builder.Services.AddTransient<UserManager>();
			builder.Services.AddScoped<AccountService>();
			builder.Services.AddScoped<CardService>();
			builder.Services.AddScoped<CookieAuthEvents>();

			var app = builder.Build();

			using (var serviceScope = app.Services.GetService<IServiceScopeFactory>()!.CreateScope())
			{
				var context = serviceScope.ServiceProvider.GetRequiredService<ServiceDbContext>();
				var logger = serviceScope.ServiceProvider.GetRequiredService<ILogger<Program>>();
				var createRes = false;
				if (settings.DbSettings.Type == DbSettingsType.MySql)
				{
					var creator = new CreateMySqlDb();
					createRes = await creator.CreateDatabase(settings, context, logger);
				}
				else if (settings.DbSettings.Type == DbSettingsType.SqlLite)
				{
					//TODO
				}
				if (!createRes)
				{
					Console.WriteLine("Unable to connect to/create database. Aborting.");
					return;
				}
			}

			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				app.UseWebAssemblyDebugging();
			}
			else
			{
				app.UseExceptionHandler("/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			app.UseHttpsRedirection();

			if (!Debugger.IsAttached)
			{
				app.UseResponseCompression();
			}

			app.UseBlazorFrameworkFiles();
			app.UseStaticFiles();

			app.UseRouting();

			app.UseAuthorization();

			app.MapRazorPages();
			app.MapControllers();
			app.MapFallbackToFile("index.html");

			app.Run();
		}
	}
}