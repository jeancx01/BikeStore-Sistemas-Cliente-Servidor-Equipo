using System.Net.Http.Json;
using System.Text.Json;
using BikeStore.Web.Models;

namespace BikeStore.Web.Services;

public sealed class BicycleApiClient(HttpClient httpClient)
    : IBicycleApiClient
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyList<BicycleViewModel>>
        GetBicyclesAsync(
            string? name = null,
            int? categoryId = null,
            string? brand = null,
            CancellationToken cancellationToken = default)
    {
        var parameters = new List<string>();

        if (!string.IsNullOrWhiteSpace(name))
        {
            parameters.Add(
                $"name={Uri.EscapeDataString(name.Trim())}");
        }

        if (categoryId.HasValue)
        {
            parameters.Add(
                $"categoryId={categoryId.Value}");
        }

        if (!string.IsNullOrWhiteSpace(brand))
        {
            parameters.Add(
                $"brand={Uri.EscapeDataString(brand.Trim())}");
        }

        var url = "api/bicicletas";

        if (parameters.Count > 0)
        {
            url += $"?{string.Join("&", parameters)}";
        }

        return await GetListAsync<BicycleViewModel>(
            url,
            cancellationToken);
    }

    public Task<IReadOnlyList<BicycleViewModel>>
        GetLowStockAsync(
            CancellationToken cancellationToken = default)
    {
        return GetListAsync<BicycleViewModel>(
            "api/bicicletas/stock-bajo",
            cancellationToken);
    }

    public Task<IReadOnlyList<BicycleViewModel>>
        GetOutOfStockAsync(
            CancellationToken cancellationToken = default)
    {
        return GetListAsync<BicycleViewModel>(
            "api/bicicletas/agotadas",
            cancellationToken);
    }

    public Task<IReadOnlyList<CategoryOptionViewModel>>
        GetCategoriesAsync(
            CancellationToken cancellationToken = default)
    {
        return GetListAsync<CategoryOptionViewModel>(
            "api/bicicletas/categorias",
            cancellationToken);
    }

    public async Task<BicycleViewModel> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        using var response = await ExecuteAsync(
            () => httpClient.GetAsync(
                $"api/bicicletas/{id}",
                cancellationToken),
            cancellationToken);

        await EnsureSuccessAsync(
            response,
            cancellationToken);

        return await ReadResponseAsync<BicycleViewModel>(
            response,
            "La API no devolvió la bicicleta.",
            cancellationToken);
    }

    public async Task<BicycleViewModel> CreateAsync(
        BicycleFormViewModel model,
        CancellationToken cancellationToken = default)
    {
        using var response = await ExecuteAsync(
            () => httpClient.PostAsJsonAsync(
                "api/bicicletas",
                model,
                JsonOptions,
                cancellationToken),
            cancellationToken);

        await EnsureSuccessAsync(
            response,
            cancellationToken);

        return await ReadResponseAsync<BicycleViewModel>(
            response,
            "La API no devolvió la bicicleta registrada.",
            cancellationToken);
    }

    public async Task UpdateAsync(
        int id,
        BicycleFormViewModel model,
        CancellationToken cancellationToken = default)
    {
        using var response = await ExecuteAsync(
            () => httpClient.PutAsJsonAsync(
                $"api/bicicletas/{id}",
                model,
                JsonOptions,
                cancellationToken),
            cancellationToken);

        await EnsureSuccessAsync(
            response,
            cancellationToken);
    }

    public async Task DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        using var response = await ExecuteAsync(
            () => httpClient.DeleteAsync(
                $"api/bicicletas/{id}",
                cancellationToken),
            cancellationToken);

        await EnsureSuccessAsync(
            response,
            cancellationToken);
    }

    private async Task<IReadOnlyList<T>> GetListAsync<T>(
        string url,
        CancellationToken cancellationToken)
    {
        using var response = await ExecuteAsync(
            () => httpClient.GetAsync(
                url,
                cancellationToken),
            cancellationToken);

        await EnsureSuccessAsync(
            response,
            cancellationToken);

        return await response.Content
            .ReadFromJsonAsync<List<T>>(
                JsonOptions,
                cancellationToken)
            ?? [];
    }

    private static async Task<T> ReadResponseAsync<T>(
        HttpResponseMessage response,
        string emptyResponseMessage,
        CancellationToken cancellationToken)
    {
        return await response.Content
            .ReadFromJsonAsync<T>(
                JsonOptions,
                cancellationToken)
            ?? throw new BicycleApiException(
                emptyResponseMessage,
                502);
    }

    private static async Task<HttpResponseMessage> ExecuteAsync(
        Func<Task<HttpResponseMessage>> operation,
        CancellationToken cancellationToken)
    {
        try
        {
            return await operation();
        }
        catch (OperationCanceledException exception)
            when (!cancellationToken.IsCancellationRequested)
        {
            throw new BicycleApiException(
                "La API tardó demasiado en responder.",
                504,
                exception);
        }
        catch (HttpRequestException exception)
        {
            throw new BicycleApiException(
                "No fue posible conectarse con BikeStore.Api.",
                503,
                exception);
        }
    }

    private static async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var content = await response.Content
            .ReadAsStringAsync(cancellationToken);

        var message =
            "No fue posible completar la solicitud.";

        if (!string.IsNullOrWhiteSpace(content))
        {
            try
            {
                using var document =
                    JsonDocument.Parse(content);

                var root = document.RootElement;

                if (root.TryGetProperty(
                        "detail",
                        out var detail))
                {
                    message =
                        detail.GetString()
                        ?? message;
                }
                else if (root.TryGetProperty(
                             "title",
                             out var title))
                {
                    message =
                        title.GetString()
                        ?? message;
                }
            }
            catch (JsonException)
            {
                message = content;
            }
        }

        throw new BicycleApiException(
            message,
            (int)response.StatusCode);
    }
}

public sealed class BicycleApiException : Exception
{
    public BicycleApiException(
        string message,
        int statusCode,
        Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }

    public int StatusCode { get; }
}