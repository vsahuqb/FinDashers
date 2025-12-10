using Microsoft.AspNetCore.Mvc.Filters;
using System.Text;
using FinDashers.API.Features.Webhooks.Adyen.Services;

namespace FinDashers.API.Features.Webhooks.Adyen.Filters;

public class AdyenBasicAuthenticationFilter : IAsyncAuthorizationFilter
{
    private readonly IAdyenBasicAuthorizationService _authorizationService;
    private readonly ILogger<AdyenBasicAuthenticationFilter> _logger;

    public AdyenBasicAuthenticationFilter(
        IAdyenBasicAuthorizationService authorizationService,
        ILogger<AdyenBasicAuthenticationFilter> logger)
    {
        _authorizationService = authorizationService;
        _logger = logger;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        try
        {
            // Extract Authorization header
            if (!context.HttpContext.Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                _logger.LogWarning("Authorization header not found");
                context.Result = new Microsoft.AspNetCore.Mvc.UnauthorizedResult();
                return;
            }

            // Parse Basic authentication
            var authHeaderValue = authHeader.ToString();
            if (!authHeaderValue.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Invalid Authorization header format");
                context.Result = new Microsoft.AspNetCore.Mvc.UnauthorizedResult();
                return;
            }

            // Decode Base64 credentials
            var encodedCredentials = authHeaderValue.Substring("Basic ".Length).Trim();
            string decodedCredentials;

            try
            {
                var credentialsBytes = Convert.FromBase64String(encodedCredentials);
                decodedCredentials = Encoding.UTF8.GetString(credentialsBytes);
            }
            catch (FormatException ex)
            {
                _logger.LogWarning($"Invalid Base64 format in Authorization header: {ex.Message}");
                context.Result = new Microsoft.AspNetCore.Mvc.UnauthorizedResult();
                return;
            }

            // Split username and password
            var credentials = decodedCredentials.Split(':', 2);
            if (credentials.Length != 2)
            {
                _logger.LogWarning("Invalid credentials format");
                context.Result = new Microsoft.AspNetCore.Mvc.UnauthorizedResult();
                return;
            }

            var username = credentials[0];
            var password = credentials[1];

            // Validate credentials
            var isValid = await _authorizationService.ValidateCredentialsAsync(username, password);

            if (!isValid)
            {
                _logger.LogWarning($"Authentication failed for user '{username}'");
                context.Result = new Microsoft.AspNetCore.Mvc.UnauthorizedResult();
                return;
            }

            _logger.LogInformation($"User '{username}' authenticated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unexpected error during authentication: {ex.Message}");
            context.Result = new Microsoft.AspNetCore.Mvc.UnauthorizedResult();
        }
    }
}
