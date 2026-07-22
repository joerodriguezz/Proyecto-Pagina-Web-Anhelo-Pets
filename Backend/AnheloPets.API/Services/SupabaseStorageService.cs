using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace AnheloPets.API.Services;

public class SupabaseStorageService : ISupabaseStorageService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public SupabaseStorageService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<string> UploadPublicAsync(string bucket, string path, Stream content, string contentType, CancellationToken ct = default)
    {
        await UploadAsync(bucket, path, content, contentType, ct);
        var supabaseUrl = _configuration["Supabase:Url"];
        return $"{supabaseUrl}/storage/v1/object/public/{bucket}/{path}";
    }

    public async Task<string> UploadPrivateAsync(string bucket, string path, Stream content, string contentType, CancellationToken ct = default)
    {
        await UploadAsync(bucket, path, content, contentType, ct);
        return path;
    }

    public async Task<string> CreateSignedUrlAsync(string bucket, string path, int expiresInSeconds = 3600, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("Supabase");
        var response = await client.PostAsJsonAsync(
            $"/storage/v1/object/sign/{bucket}/{path}",
            new { expiresIn = expiresInSeconds },
            ct);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"No se pudo generar la URL firmada para {bucket}/{path}: {(int)response.StatusCode}");
        }

        var body = await response.Content.ReadFromJsonAsync<SignedUrlResponse>(cancellationToken: ct);
        if (body?.SignedURL == null)
        {
            throw new InvalidOperationException($"Respuesta inesperada de Supabase Storage al firmar {bucket}/{path}.");
        }

        // signedURL viene relativo (ej: "/object/sign/{bucket}/{path}?token=..."), hay que
        // anteponer solo el host + "/storage/v1" — NO volver a agregar "/object/..." aparte.
        var supabaseUrl = _configuration["Supabase:Url"];
        return $"{supabaseUrl}/storage/v1{body.SignedURL}";
    }

    public async Task<bool> DeleteAsync(string bucket, string path, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("Supabase");
        var response = await client.DeleteAsync($"/storage/v1/object/{bucket}/{path}", ct);
        return response.IsSuccessStatusCode;
    }

    private async Task UploadAsync(string bucket, string path, Stream content, string contentType, CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient("Supabase");

        using var streamContent = new StreamContent(content);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        var response = await client.PutAsync($"/storage/v1/object/{bucket}/{path}", streamContent, ct);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException($"Error al subir a Supabase Storage ({bucket}/{path}): {(int)response.StatusCode} {body}");
        }
    }

    private class SignedUrlResponse
    {
        [JsonPropertyName("signedURL")]
        public string? SignedURL { get; set; }
    }
}
