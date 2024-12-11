using Microsoft.JSInterop;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Presentation.Services;
using Presentation.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace Presentation;

public class AuthHeaderHandler : DelegatingHandler
{
    private readonly ILogger<AuthHeaderHandler> _logger;
    private readonly TokenService _tokenService;
    private const int MaxRetryCount = 3;

    public AuthHeaderHandler(TokenService tokenService, ILogger<AuthHeaderHandler> logger)
    {
        _tokenService = tokenService;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        string? accessToken = await _tokenService.GetAccessTokenAsync();
        if (!string.IsNullOrEmpty(accessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        return await RetryRequestAsync(request, cancellationToken);
    }

    private async Task<HttpResponseMessage> RetryRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        int retryCount = 0;

        while (retryCount < MaxRetryCount)
        {
            try
            {
                var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    var newAccessToken = await _tokenService.RefreshAccessTokenAsync();
                    if (!string.IsNullOrWhiteSpace(newAccessToken))
                    {
                        // Retry with new access token
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newAccessToken);
                        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
                    }
                }

                return response;
            }
            catch (Exception ex)
            {
                retryCount++;
                // Log or handle the exception if necessary
                _logger.LogError($"Request failed, retry attempt {retryCount}. Error: {ex.Message}");

                if (retryCount >= MaxRetryCount)
                {
                    return new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
                }

                await Task.Delay(1000 * retryCount, cancellationToken);
            }
        }

        return new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
    }
}