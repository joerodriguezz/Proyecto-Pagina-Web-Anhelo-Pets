namespace AnheloPets.API.Services;

public interface ISupabaseStorageService
{
    /// <summary>Sube un archivo a un bucket público y devuelve la URL pública permanente.</summary>
    Task<string> UploadPublicAsync(string bucket, string path, Stream content, string contentType, CancellationToken ct = default);

    /// <summary>Sube un archivo a un bucket privado y devuelve el path guardado (no una URL).</summary>
    Task<string> UploadPrivateAsync(string bucket, string path, Stream content, string contentType, CancellationToken ct = default);

    /// <summary>Genera una URL firmada temporal para un objeto de un bucket privado.</summary>
    Task<string> CreateSignedUrlAsync(string bucket, string path, int expiresInSeconds = 3600, CancellationToken ct = default);

    /// <summary>Elimina un objeto de un bucket (público o privado).</summary>
    Task<bool> DeleteAsync(string bucket, string path, CancellationToken ct = default);
}
