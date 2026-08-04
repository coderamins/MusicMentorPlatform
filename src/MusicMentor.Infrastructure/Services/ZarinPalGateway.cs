using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MusicMentor.Application.Interfaces;

namespace MusicMentor.Infrastructure.Services;

public class ZarinPalGateway : IZarinPalGateway
{
    private readonly HttpClient _httpClient;
    private readonly ZarinPalSettings _settings;
    private readonly ILogger<ZarinPalGateway> _logger;

    public ZarinPalGateway(HttpClient httpClient, IOptions<ZarinPalSettings> settings, ILogger<ZarinPalGateway> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<ZarinPalRequestResult> RequestPaymentAsync(
        decimal amountToman,
        string description,
        string? mobile,
        string? email,
        CancellationToken cancellationToken = default)
    {
        var payload = new ZarinPalRequestPayload
        {
            MerchantId = _settings.MerchantId,
            Amount = amountToman,
            Currency = "IRT", // تومان - در صورت نیاز به ریال این مقدار را به "IRR" تغییر دهید
            Description = description,
            CallbackUrl = _settings.CallbackUrl,
            Metadata = (mobile is null && email is null)
                ? null
                : new ZarinPalMetadata { Mobile = mobile, Email = email },
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"{_settings.ApiBaseUrl}request.json", payload, cancellationToken);

            var body = await response.Content.ReadFromJsonAsync<ZarinPalRequestEnvelope>(cancellationToken: cancellationToken);

            if (body?.Data is { Code: 100 } data && !string.IsNullOrEmpty(data.Authority))
            {
                var paymentUrl = $"{_settings.StartPayBaseUrl}{data.Authority}";
                return new ZarinPalRequestResult(true, data.Authority, paymentUrl, null);
            }

            var errorMessage = TryExtractErrorMessage(body?.Errors)
                ?? body?.Data?.Message
                ?? "درخواست ایجاد تراکنش در زرین‌پال ناموفق بود.";
            _logger.LogWarning("ZarinPal payment request failed: {Error}", errorMessage);
            return new ZarinPalRequestResult(false, null, null, errorMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while calling ZarinPal payment request API");
            return new ZarinPalRequestResult(false, null, null, "خطا در ارتباط با درگاه پرداخت.");
        }
    }

    public async Task<ZarinPalVerifyResult> VerifyPaymentAsync(
        decimal amountToman,
        string authority,
        CancellationToken cancellationToken = default)
    {
        var payload = new ZarinPalVerifyPayload
        {
            MerchantId = _settings.MerchantId,
            Amount = amountToman,
            Authority = authority,
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"{_settings.ApiBaseUrl}verify.json", payload, cancellationToken);

            var body = await response.Content.ReadFromJsonAsync<ZarinPalVerifyEnvelope>(cancellationToken: cancellationToken);

            // code 100: تایید موفق برای اولین بار / code 101: قبلاً هم تایید شده (idempotent - همچنان موفق است)
            if (body?.Data is { } data && (data.Code == 100 || data.Code == 101))
            {
                return new ZarinPalVerifyResult(true, data.RefId?.ToString(), null);
            }

            var errorMessage = TryExtractErrorMessage(body?.Errors) ?? body?.Data?.Message ?? "تایید تراکنش در زرین‌پال ناموفق بود.";
            _logger.LogWarning("ZarinPal payment verify failed: {Error}", errorMessage);
            return new ZarinPalVerifyResult(false, null, errorMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while calling ZarinPal payment verify API");
            return new ZarinPalVerifyResult(false, null, "خطا در ارتباط با درگاه پرداخت.");
        }
    }

    // --- مدل‌های داخلی درخواست/پاسخ زرین‌پال (REST v4) ---

    private class ZarinPalRequestPayload
    {
        [JsonPropertyName("merchant_id")] public string MerchantId { get; set; } = default!;
        [JsonPropertyName("amount")] public decimal Amount { get; set; }
        [JsonPropertyName("currency")] public string Currency { get; set; } = "IRT";
        [JsonPropertyName("description")] public string Description { get; set; } = default!;
        [JsonPropertyName("callback_url")] public string CallbackUrl { get; set; } = default!;
        [JsonPropertyName("metadata")] public ZarinPalMetadata? Metadata { get; set; }
    }

    private class ZarinPalMetadata
    {
        [JsonPropertyName("mobile")] public string? Mobile { get; set; }
        [JsonPropertyName("email")] public string? Email { get; set; }
    }

    private class ZarinPalVerifyPayload
    {
        [JsonPropertyName("merchant_id")] public string MerchantId { get; set; } = default!;
        [JsonPropertyName("amount")] public decimal Amount { get; set; }
        [JsonPropertyName("authority")] public string Authority { get; set; } = default!;
    }

    private class ZarinPalRequestEnvelope
    {
        [JsonPropertyName("data")] public ZarinPalRequestData? Data { get; set; }
        [JsonPropertyName("errors")] public JsonElement? Errors { get; set; }
    }

    private class ZarinPalRequestData
    {
        [JsonPropertyName("code")] public int Code { get; set; }
        [JsonPropertyName("message")] public string? Message { get; set; }
        [JsonPropertyName("authority")] public string? Authority { get; set; }
    }

    private class ZarinPalVerifyEnvelope
    {
        [JsonPropertyName("data")] public ZarinPalVerifyData? Data { get; set; }
        [JsonPropertyName("errors")] public JsonElement? Errors { get; set; }
    }

    private class ZarinPalVerifyData
    {
        [JsonPropertyName("code")] public int Code { get; set; }
        [JsonPropertyName("message")] public string? Message { get; set; }
        [JsonPropertyName("ref_id")] public long? RefId { get; set; }
    }

    /// <summary>
    /// فیلد errors در پاسخ زرین‌پال شکل ثابتی ندارد: روی موفقیت یک آرایه خالی [] است،
    /// روی خطا معمولاً یک آبجکت با فیلد message. این متد بدون پرتاب Exception هر دو حالت را پوشش می‌دهد.
    /// </summary>
    private static string? TryExtractErrorMessage(JsonElement? errors)
    {
        if (errors is not { } el || el.ValueKind != JsonValueKind.Object)
            return null;

        return el.TryGetProperty("message", out var msg) && msg.ValueKind == JsonValueKind.String
            ? msg.GetString()
            : null;
    }
}
