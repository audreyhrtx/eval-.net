using System.Net.Http.Json;
using System.Text.Json;
using Pipeline.Models;

namespace Pipeline;

public sealed class PipelineProvider(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<PipelineModel> GetPipelineAsync(
        string url,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || uri.Scheme == Uri.UriSchemeFile)
        {
            var filePath = uri?.LocalPath ?? Path.GetFullPath(url, AppContext.BaseDirectory);
            var json = await File.ReadAllTextAsync(filePath, cancellationToken);
            return JsonSerializer.Deserialize<PipelineModel>(json, JsonOptions)
                ?? throw new JsonException("Le fichier JSON ne contient aucun pipeline.");
        }

        using var response = await httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var pipeline = await response.Content.ReadFromJsonAsync<PipelineModel>(
            JsonOptions,
            cancellationToken);

        return pipeline ?? throw new JsonException("La reponse JSON ne contient aucun pipeline.");
    }
}
