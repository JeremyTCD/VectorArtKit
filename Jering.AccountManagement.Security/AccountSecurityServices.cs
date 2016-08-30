﻿using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Jering.AccountManagement.DatabaseInterface;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Jering.AccountManagement.Security
{
    /// <summary>
    /// Provides an API for managing Account security.
    /// </summary>
    public class AccountSecurityServices<TAccount> : IAccountSecurityServices<TAccount> where TAccount : IAccount
    {
        private ClaimsPrincipalServices<TAccount> _claimsPrincipalServices { get; }
        private IAccountRepository<TAccount> _accountRepository { get; }
        private HttpContext _httpContext { get; }
        private AccountSecurityOptions _securityOptions { get; }
        private Dictionary<string, ITokenService<TAccount>> _tokenServices { get; } = new Dictionary<string, ITokenService<TAccount>>();
        private IEmailSender _emailSender { get; }

        /// <summary>
        /// The data protection purpose used for email confirmation related methods.
        /// </summary>
        protected const string _confirmEmailTokenPurpose = "EmailConfirmation";
        /// <summary>
        /// 
        /// </summary>
        protected const string _twoFactorTokenPurpose = "TwoFactor";

        /// <summary>
        /// Constructs a new instance of <see cref="AccountSecurityServices{TAccount}"/>.
        /// </summary>
        /// <param name="claimsPrincipalServices"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="securityOptionsAccessor"></param>
        /// <param name="accountRepository"></param>
        /// <param name="serviceProvider"></param>
        /// <param name="emailSender"></param>
        public AccountSecurityServices(ClaimsPrincipalServices<TAccount> claimsPrincipalServices,
            IHttpContextAccessor httpContextAccessor,
            IOptions<AccountSecurityOptions> securityOptionsAccessor,
            IAccountRepository<TAccount> accountRepository,
            IServiceProvider serviceProvider,
            IEmailSender emailSender)
        {
            _claimsPrincipalServices = claimsPrincipalServices;
            _httpContext = httpContextAccessor?.HttpContext;
            _securityOptions = securityOptionsAccessor?.Value;
            _accountRepository = accountRepository;
            _emailSender = emailSender;

            if (serviceProvider != null)
            {
                foreach (string tokenServiceName in _securityOptions.TokenServiceOptions.TokenServiceMap.Keys)
                {
                    ITokenService<TAccount> tokenService = (ITokenService<TAccount>)serviceProvider.
                        GetRequiredService(_securityOptions.TokenServiceOptions.TokenServiceMap[tokenServiceName]);
                    RegisterTokenProvider(tokenServiceName, tokenService);                    
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tokenServiceName"></param>
        /// <param name="tokenService"></param>
        public virtual void RegisterTokenProvider(string tokenServiceName, ITokenService<TAccount> tokenService)
        {
            if(tokenService == null)
            {
                throw new ArgumentNullException(nameof(tokenService));
            }
            _tokenServices[tokenServiceName] = tokenService;
        }

        /// <summary>
        /// Signs in specified <paramref name="account"/> using specified <paramref name="authenticationProperties"/>.
        /// </summary>
        /// <param name="account"></param>
        /// <param name="authenticationProperties"></param>
        /// <returns>A <see cref="Task"/>.</returns>
        public virtual async Task SignInAsync(TAccount account, AuthenticationProperties authenticationProperties)
        {
            ClaimsPrincipal claimsPrincipal = await _claimsPrincipalServices.CreateClaimsPrincipalAsync(account, _securityOptions.CookieOptions.ApplicationCookieOptions.AuthenticationScheme);

            await _httpContext.Authentication.SignInAsync(
                    _securityOptions.CookieOptions.ApplicationCookieOptions.AuthenticationScheme,
                    claimsPrincipal,
                    authenticationProperties);
        }

        /// <summary>
        /// Signs in account with specified <paramref name="email"/> and <paramref name="password"/> using 
        /// specified <paramref name="authenticationProperties"/>.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="authenticationProperties"></param>
        /// <returns>
        /// <see cref="PasswordSignInResult.Failed"/> if credentials are invalid. 
        /// <see cref="PasswordSignInResult.TwoFactorRequired"/> if two factor is required. 
        /// <see cref="PasswordSignInResult.Succeeded"/> if application sign in is complete. 
        /// </returns>
        public virtual async Task<PasswordSignInResult> PasswordSignInAsync(string email, string password, AuthenticationProperties authenticationProperties)
        {
            TAccount account = await _accountRepository.GetAccountByEmailAndPasswordAsync(email, password);
            if (account != null)
            {
                if (account.TwoFactorEnabled)
                {
                    await CreateTwoFactorCookieAsync(account);
                    await SendTwoFactorTokenEmailAsync(account);
                    return PasswordSignInResult.TwoFactorRequired;
                }

                await SignInAsync(account, authenticationProperties);
                return PasswordSignInResult.Succeeded;
            }

            return PasswordSignInResult.Failed;
        }

        /// <summary>
        /// Signs out account that sent request. 
        /// </summary>
        /// <returns>A <see cref="Task"/>.</returns>
        public virtual async Task SignOutAsync()
        {
            await _httpContext.Authentication.SignOutAsync(_securityOptions.CookieOptions.ApplicationCookieOptions.AuthenticationScheme);
            await _httpContext.Authentication.SignOutAsync(_securityOptions.CookieOptions.TwoFactorCookieOptions.AuthenticationScheme);
        }

        /// <summary>
        /// Gets signed in account for <see cref="HttpContext.User"/>.
        /// </summary>
        /// <returns>
        /// An account if there is a signed in account.
        /// Null otherwise.
        /// </returns>
        public virtual async Task<TAccount> GetSignedInAccount()
        {
            return await GetSignedInAccount(_httpContext.User);
        }

        /// <summary>
        /// Gets signed in account for <param name="claimsPrincipal"></param>. This overload must be used if <see cref="HttpContext.User"/> 
        /// has not been set, for example before authentication is complete.
        /// </summary>
        /// <returns>
        /// An account if there is a signed in account.
        /// Null otherwise.
        /// </returns>
        public virtual async Task<TAccount> GetSignedInAccount(ClaimsPrincipal claimsPrincipal)
        {
            if (claimsPrincipal.Identity.AuthenticationType == _securityOptions.CookieOptions.ApplicationCookieOptions.AuthenticationScheme)
            {
                System.Security.Claims.Claim accountIdClaim = claimsPrincipal.FindFirst(_securityOptions.ClaimsOptions.AccountIdClaimType);
                if (accountIdClaim != null)
                {
                    return await _accountRepository.GetAccountAsync(Convert.ToInt32(accountIdClaim.Value));
                }
            }
            return default(TAccount);
        }

        /// <summary>
        /// Validates <paramref name="token"/>. If valid, sets EmailConfirmed to true for the associated account.
        /// </summary>
        /// <param name="token"></param>
        /// <returns>
        /// <see cref="ConfirmEmailResult.Failed"/> if there is no signed in account.
        /// <see cref="ConfirmEmailResult.InvalidToken"/> if token is invalid.
        /// <see cref="ConfirmEmailResult.Failed"/> if unable to update account email confirmed. 
        /// <see cref="ConfirmEmailResult.Succeeded"/> if <paramref name="token"/> is valid and EmailConfirmed updates successfully.
        /// </returns>
        public virtual async Task<ConfirmEmailResult> ConfirmEmailAsync(string token)
        {
            TAccount account = await GetSignedInAccount();

            if(account == null)
            {
                return ConfirmEmailResult.Failed;
            }

            if(!await _tokenServices[TokenServiceOptions.DataProtectionTokenService].ValidateTokenAsync(_confirmEmailTokenPurpose, token, account))
            {
                return ConfirmEmailResult.InvalidToken;
            }

            if (!await _accountRepository.UpdateAccountEmailConfirmedAsync(account.AccountId))
            {
                return ConfirmEmailResult.Failed;
            }

            return ConfirmEmailResult.Succeeded;
        }

        /// <summary>
        /// Sends confirmation email to account with specified <paramref name="accountId"/>.
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns>A <see cref="Task"/> that returns true if confirmation email is sent successfully.</returns>
        public virtual async Task SendConfirmationEmailAsync(int accountId)
        {
            TAccount account = await _accountRepository.GetAccountAsync(accountId);
            await SendConfirmationEmailAsync(account);
        }

        /// <summary>
        /// Sends confirmation email to specified <paramref name="account"/>.
        /// </summary>
        /// <param name="account"></param>
        /// <returns>A <see cref="Task"/> that returns true if confirmation email is sent successfully.</returns>
        public virtual async Task SendConfirmationEmailAsync(TAccount account)
        {
            string token = await _tokenServices[TokenServiceOptions.DataProtectionTokenService].GenerateTokenAsync(_confirmEmailTokenPurpose, account);

            await _emailSender.SendEmailAsync("your link:" + token, account.Email, "confirmation email");           
        }

        /// <summary>
        /// Validates <paramref name="token"/>. If valid, updates PasswordHash for account.
        /// </summary>
        /// <param name="account"></param>
        /// <param name="password"></param>
        /// <param name="token"></param>
        /// <returns>A <see cref="Task"/> that returns true if <paramref name="token"/> is valid and PasswordHash 
        /// updates successfully, false otherwise.</returns>
        public virtual async Task<bool> UpdatePasswordAsync(TAccount account, string password, string token)
        {
            return await _tokenServices[TokenServiceOptions.DataProtectionTokenService].ValidateTokenAsync(_confirmEmailTokenPurpose, token, account) &&
                await _accountRepository.UpdateAccountPasswordHashAsync(account.AccountId, password);
        }

        /// <summary>
        /// Generates and sends a two factor token to the email address associated with <paramref name="account"/>.
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public virtual async Task SendTwoFactorTokenEmailAsync(TAccount account)
        {
            
            string token = await _tokenServices[TokenServiceOptions.TotpTokenService].GenerateTokenAsync(_twoFactorTokenPurpose, account);

            await _emailSender.SendEmailAsync("Your security code is: " + token, account.Email, "security code");
        }

        /// <summary>
        /// Gets account using two factor cookie's account Id value. 
        /// </summary>
        /// <returns>
        /// Null if two factor cookie is invalid.
        /// Null if two factor cookie does not have an account Id value.
        /// An account if two factor cookie's account Id value exists.
        /// </returns>
        public virtual async Task<TAccount> GetTwoFactorAccountAsync()
        {
            ClaimsPrincipal claimsPrincipal = await _httpContext.Authentication.AuthenticateAsync(_securityOptions.CookieOptions.TwoFactorCookieOptions.AuthenticationScheme);

            if(claimsPrincipal == null)
            {
                return default(TAccount);
            }

            System.Security.Claims.Claim accountIdClaim = claimsPrincipal.FindFirst(_securityOptions.ClaimsOptions.AccountIdClaimType);

            if(accountIdClaim == null)  
            {
                return default(TAccount);
            }

            int accountId = Convert.ToInt32(accountIdClaim.Value);

            return await _accountRepository.GetAccountAsync(accountId);
        }

        /// <summary>
        /// Instructs cookie authentication middleware to add two factor cookie to <see cref="HttpResponse"/>.
        /// </summary>
        /// <param name="account"></param>
        public virtual async Task CreateTwoFactorCookieAsync(TAccount account)
        {
            ClaimsPrincipal claimsPrincipal = _claimsPrincipalServices.CreateClaimsPrincipal(account.AccountId, _securityOptions.CookieOptions.TwoFactorCookieOptions.AuthenticationScheme);

            await _httpContext.Authentication.SignInAsync(
                _securityOptions.CookieOptions.TwoFactorCookieOptions.AuthenticationScheme,
                claimsPrincipal);
        }

        /// <summary>
        /// Validates two factor token. If valid, peforms application sign for user specified by two factor cookie.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="isPersistent"></param>
        /// <returns>
        /// <see cref="TwoFactorSignInResult.Failed"/> if unable to retrieve two factor account.
        /// <see cref="TwoFactorSignInResult.Succeeded"/> if <paramref name="token"/> is valid. 
        /// <see cref="TwoFactorSignInResult.Failed"/> if <paramref name="token"/> is invalid. 
        /// </returns>
        public virtual async Task<TwoFactorSignInResult> TwoFactorSignInAsync(string token, bool isPersistent)
        {
            TAccount account = await GetTwoFactorAccountAsync();
            if (account == null)
            {
                return TwoFactorSignInResult.Failed;
            }

            if (await _tokenServices[TokenServiceOptions.TotpTokenService].ValidateTokenAsync(_twoFactorTokenPurpose, token, account))
            {
                // Cleanup two factor cookie
                await _httpContext.Authentication.SignOutAsync(_securityOptions.CookieOptions.TwoFactorCookieOptions.AuthenticationScheme);
                await SignInAsync(account, new AuthenticationProperties() { IsPersistent = isPersistent });

                return TwoFactorSignInResult.Succeeded;
            }

            return TwoFactorSignInResult.Failed;
        }

        /// <summary>
        /// Creates an account with the specified <paramref name="email"/> and <paramref name="password"/>. If successful, signs in account and sends
        /// confirmation email.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns>
        /// <see cref="CreateAccountResult"/>. 
        /// </returns>
        public virtual async Task<CreateAccountResult> CreateAccountAsync(string email, string password)
        {
            try
            {
                TAccount account = await _accountRepository.CreateAccountAsync(email, password);

                if (account == null)
                {
                    throw new NullReferenceException(nameof(account));
                }

                await SendConfirmationEmailAsync(account);

                await SignInAsync(account, new AuthenticationProperties { IsPersistent = false });

                return CreateAccountResult.Succeeded;
            }
            catch (SqlException sqlException)
            {
                if (sqlException.Number == 51000)
                {
                    return CreateAccountResult.Failed;
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
