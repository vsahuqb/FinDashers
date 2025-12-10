using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace FinDashers.API.Features.Webhooks.Adyen.Services;

public interface IAdyenHmacValidationService
{
    bool ValidateHmacSignature(string rawBody, string hmacSignature, string hmacKey);
}

public class AdyenHmacValidationService : IAdyenHmacValidationService
{
    private readonly ILogger<AdyenHmacValidationService> _logger;

    public AdyenHmacValidationService(ILogger<AdyenHmacValidationService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Validates the HMAC signature of an Adyen webhook according to Adyen's specification.
    /// Reference: https://docs.adyen.com/development-resources/webhooks/verify-hmac-signatures
    /// </summary>
    public bool ValidateHmacSignature(string rawBody, string hmacSignature, string hmacKey)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(rawBody) || string.IsNullOrWhiteSpace(hmacSignature) || string.IsNullOrWhiteSpace(hmacKey))
            {
                _logger.LogWarning("Invalid input: rawBody, hmacSignature, or hmacKey is empty");
                return false;
            }

            // Decode the HMAC signature from Base64
            byte[] decodedSignature;
            try
            {
                decodedSignature = Convert.FromBase64String(hmacSignature);
            }
            catch (FormatException ex)
            {
                _logger.LogWarning($"Invalid Base64 format for HMAC signature: {ex.Message}");
                return false;
            }

            // Decode the HMAC key from Base64
            byte[] decodedKey;
            try
            {
                decodedKey = Convert.FromBase64String(hmacKey);
            }
            catch (FormatException ex)
            {
                _logger.LogWarning($"Invalid Base64 format for HMAC key: {ex.Message}");
                return false;
            }

            // Calculate HMAC-SHA256 of the raw body using the decoded key
            using (var hmac = new HMACSHA256(decodedKey))
            {
                byte[] bodyBytes = Encoding.UTF8.GetBytes(rawBody);
                byte[] calculatedSignature = hmac.ComputeHash(bodyBytes);

                // Compare the calculated signature with the provided signature
                bool isValid = ConstantTimeComparison(calculatedSignature, decodedSignature);

                if (!isValid)
                {
                    _logger.LogWarning("HMAC signature validation failed: signature mismatch");
                }
                else
                {
                    _logger.LogInformation("HMAC signature validation successful");
                }

                return isValid;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error during HMAC validation: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Performs constant-time comparison to prevent timing attacks.
    /// </summary>
    private bool ConstantTimeComparison(byte[] a, byte[] b)
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
