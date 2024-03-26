namespace WhoIsAPI.Contracts.Responses;

public class UploadFileResponse
{
    public bool Success { get; set; }
    public string? FilePath { get; set; }
    public string? ExternalAPIResponse { get; set; }
}
