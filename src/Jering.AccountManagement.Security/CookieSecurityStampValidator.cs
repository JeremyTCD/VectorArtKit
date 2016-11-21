﻿using Jering.AccountManagement.DatabaseInterface;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Jering.AccountManagement.Security
{
    /// <summary>
    /// 
    /// </summary>
    public class CookieSecurityStampValidator<TAccount> : ICookieSecurityStampValidator where TAccount : IAccount
    {
        private readonly AccountSecurityOptions _securityOptions;
        private readonly IAccountRepository<TAccount> _accountRepository;
        private readonly IAccountSecurityService<TAccount> _accountSecurityService;
        private readonly ClaimsPrincipalService<TAccount> _claimsPrincipalService;

        /// <summary>
        /// Constructs an instance of <see cref="CookieSecurityStampValidator{TAccount}"/> .
        /// </summary>
        /// <param name="securityOptionsAccessor"></param>
        /// <param name="accountRepository"></param>
        /// <param name="accountSecurityService"></param>
        /// <param name="claimsPrincipalService"></param>
        public CookieSecurityStampValidator(IOptions<AccountSecurityOptions> securityOptionsAccessor,
                        IAccountRepository<TAccount> accountRepository,
                        IAccountSecurityService<TAccount> accountSecurityService,
                        ClaimsPrincipalService<TAccount> claimsPrincipalService)
        {
            _accountRepository = accountRepository;
            _securityOptions = securityOptionsAccessor.Value;
            _accountSecurityService = accountSecurityService;
            _claimsPrincipalService = claimsPrincipalService;
        }

        /// <summary>
        /// Validates <see cref="ClaimsPrincipal"/> security stamp claim. Logs account out and rejects <see cref="ClaimsPrincipal"/>  if security stamp
        /// is invalid.
        /// </summary>
        /// <param name="context"></param>
        public virtual async Task ValidateAsync(CookieValidatePrincipalContext context)
        {
            if (context.Principal != null) {
                TAccount account = await _accountSecurityService.GetLoggedInAccountAsync(context.Principal);

                if (account == null || account.SecurityStamp.ToString() != context.Principal.FindFirst(_securityOptions.ClaimsOptions.SecurityStampClaimType)?.Value) {
                    context.RejectPrincipal();
                    await _accountSecurityService.LogOffAsync();
                }
            }
        }
    }

    /// <summary>
    /// Static helper class used to configure a CookieAuthenticationNotifications to validate a cookie against a user's security
    /// stamp.
    /// </summary>
    public static class CookieSecurityStampValidator
    {
        /// <summary>
        /// Validates a principal against a user's stored security stamp.
        /// the identity.
        /// </summary>
        /// <param name="context">The context containing the <see cref="System.Security.Claims.ClaimsPrincipal"/>
        /// and <see cref="Microsoft.AspNetCore.Http.Authentication.AuthenticationProperties"/> to validate.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous validation operation.</returns>
        public static Task ValidatePrincipalAsync(CookieValidatePrincipalContext context)
        {
            if (context.HttpContext.RequestServices == null)
            {
                throw new InvalidOperationException("RequestServices is null.");
            }

            ICookieSecurityStampValidator validator = context.HttpContext.RequestServices.GetRequiredService<ICookieSecurityStampValidator>();
            return validator.ValidateAsync(context);
        }
    }
}
