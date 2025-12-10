using Microsoft.AspNetCore.Mvc;
using FinDashers.API.Features.Webhooks.Adyen.Filters;

namespace FinDashers.API.Features.Webhooks.Adyen.Attributes;

/// <summary>
/// Custom attribute for Adyen Basic Authentication.
/// Inherits from TypeFilterAttribute to apply AdyenBasicAuthenticationFilter.
/// </summary>
public class AdyenBasicAuthenticationAttribute : TypeFilterAttribute
{
    public AdyenBasicAuthenticationAttribute() : base(typeof(AdyenBasicAuthenticationFilter))
    {
    }
}
