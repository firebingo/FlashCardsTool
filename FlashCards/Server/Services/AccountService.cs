using FlashCards.Server.Configuration;
using FlashCards.Server.Data;
using FlashCards.Server.Models;
using FlashCards.Server.Models.Auth;
using FlashCards.Shared.Util;
using IdGen;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace FlashCards.Server.Services
{
	public class AccountService
	{
		readonly AppSettings _appSettings;
		readonly ILogger<AccountService> _logger;
		readonly ServiceDbContext _dbContext;
		readonly IdGenerator _idGen;

		public AccountService(IOptions<AppSettings> appSettings, ILogger<AccountService> logger, ServiceDbContext dbContext, IdGenerator idGen)
		{
			_appSettings = appSettings.Value;
			_logger = logger;
			_dbContext = dbContext;
			_idGen = idGen;
		}

		public async Task<StandardResponse> Register(RegisterRequest request)
		{
			try
			{
				if (!_appSettings.UserSettings.RegistrationOpen)
				{
					return new StandardResponse()
					{
						Message = "REGISTRATION_NOT_OPEN",
						StatusCode = System.Net.HttpStatusCode.Forbidden,
						Success = false
					};
				}

				request.Email = request.Email?.Trim();
				request.UserName = request.UserName.Trim();
				request.Password = request.Password.Trim();

				if (_appSettings.UserSettings.RequireEmail && string.IsNullOrWhiteSpace(request.Email))
				{
					return new StandardResponse()
					{
						Message = "EMAIL_REQUIRED",
						StatusCode = System.Net.HttpStatusCode.BadRequest,
						Success = false
					};
				}

				if ((!string.IsNullOrWhiteSpace(request.Email) && request.Email.Length > 127) ||
					(!string.IsNullOrWhiteSpace(request.Email) && !RegexUtil.SimpleEmailRegex().IsMatch(request.Email)))
				{
					return new StandardResponse()
					{
						Message = "INVALID_EMAIL",
						StatusCode = System.Net.HttpStatusCode.BadRequest,
						Success = false
					};
				}

				if (request.UserName.Length > 127)
				{
					return new StandardResponse()
					{
						Message = "INVALID_USERNAME",
						StatusCode = System.Net.HttpStatusCode.BadRequest,
						Success = false
					};
				}

				var exists = await _dbContext.Users.AnyAsync(x => x.UserName == request.UserName);
				if (exists)
				{
					return new StandardResponse()
					{
						Message = "USER_EXISTS",
						StatusCode = System.Net.HttpStatusCode.BadRequest,
						Success = false
					};
				}

				var salt = HashUtil.GenerateSalt();
				await _dbContext.Users.AddAsync(new User()
				{
					Id = _idGen.CreateId(),
					Email = request.Email,
					UserName = request.UserName,
					Password = HashUtil.HashPassword(request.Password, salt),
					Salt = salt,
					RequiresNewPassword = false,
					CreatedTime = DateTime.UtcNow,
					ModifiedTime = DateTime.UtcNow
				});

				await _dbContext.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Exception: AccountService.Register()");
				return new StandardResponse()
				{
					Message = "EXCEPTION",
					StatusCode = System.Net.HttpStatusCode.InternalServerError,
					Success = false
				};
			}

			return new StandardResponse();
		}
	}
}
