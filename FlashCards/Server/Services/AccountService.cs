using FlashCards.Server.Configuration;
using FlashCards.Server.Data;
using FlashCards.Server.Data.Models;
using FlashCards.Shared.Models;
using FlashCards.Shared.Models.Auth;
using FlashCards.Shared.Util;
using IdGen;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Security.Claims;
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

		public StandardResponse<UserHeaderInfoResponse> GetUserHeaderInfo(HttpContext context)
		{
			try
			{
				var userName = context.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;
				var email = context.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
				if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(email))
				{
					_logger.LogWarning($"Failed to get user claims: AccountService.GetUserHeaderInfo({context?.User?.Identity?.Name})");
					return new StandardResponse<UserHeaderInfoResponse>()
					{
						Success = false,
						Message = "ERROR",
						StatusCode = System.Net.HttpStatusCode.InternalServerError,
					};
				}
				return new StandardResponse<UserHeaderInfoResponse>()
				{
					Data = new UserHeaderInfoResponse()
					{
						Email = email,
						UserName = userName,
					}
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Exception: AccountService.GetUserHeaderInfo({context?.User?.Identity?.Name})");
				return new StandardResponse<UserHeaderInfoResponse>()
				{
					Success = false,
					Message = "EXCEPTION",
					StatusCode = System.Net.HttpStatusCode.InternalServerError,
				};
			}
		}

		public StandardResponse<CanRegisterResponse> CanRegister()
		{
			if (_appSettings.UserSettings.RegistrationOpen)
				return new StandardResponse<CanRegisterResponse>()
				{
					Data = new CanRegisterResponse()
					{
						EmailRequired = _appSettings.UserSettings.RequireEmail
					}
				};
			else
			{
				return new StandardResponse<CanRegisterResponse>()
				{
					Success = false,
					Message = "REGISTRATION_NOT_OPEN",
					StatusCode = System.Net.HttpStatusCode.Forbidden
				};
			}
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

				var exists = await _dbContext.Users.AnyAsync(x => x.UserName == request.UserName)
					|| (!string.IsNullOrWhiteSpace(request.Email) && await _dbContext.Users.AnyAsync(x => x.Email == request.Email));
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
					Disabled = false,
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
