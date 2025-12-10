using System.Text;

namespace FinDashers.API.Features.Webhooks.Adyen.Services;

public interface IAdyenBasicAuthorizationService
{
    Task<bool> ValidateCredentialsAsync(string username, string password);
}

public class AdyenBasicAuthorizationService : IAdyenBasicAuthorizationService
{
    private readonly IAdyenDatabaseService _databaseService;
    private readonly ILogger<AdyenBasicAuthorizationService> _logger;

    public AdyenBasicAuthorizationService(
        IAdyenDatabaseService databaseService,
        ILogger<AdyenBasicAuthorizationService> logger)
    {
        _databaseService = databaseService;
        _logger = logger;
    }

    /// <summary>
    /// Validates the provided username and password against the webhook_credentials table.
    /// </summary>
    public async Task<bool> ValidateCredentialsAsync(string username, string password)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                _logger.LogWarning("Empty username or password provided for authentication");
                return false;
            }

            // Retrieve the stored password from database
            var storedPassword = await _databaseService.GetWebhookCredentialAsync(username);

            if (string.IsNullOrWhiteSpace(storedPassword))
            {
                _logger.LogWarning($"User '{username}' not found in webhook_credentials");
                return false;
            }

            // Compare passwords using constant-time comparison to prevent timing attacks
            bool isValid = ConstantTimeComparison(password, storedPassword);

            if (!isValid)
            {
                _logger.LogWarning($"Invalid password for user '{username}'");
            }
            else
            {
                _logger.LogInformation($"Successfully authenticated user '{username}'");
            }

            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error during credential validation: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Performs constant-time comparison to prevent timing attacks.
    /// </summary>
    private bool ConstantTimeComparison(string a, string b)
    {
        if (a.Length != b.Length)
            return false;

        int result = 0;
        for (int i = 0; i < a.Length; i++)
        {
            result |= a[i] ^ b[i];
        }

        return result == 0;
    }
}
