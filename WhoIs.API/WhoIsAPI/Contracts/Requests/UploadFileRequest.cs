namespace WhoIsAPI.Contracts.Requests;

public class UploadFileRequest
{
    public required IFormFile File { get; set; }
    public required string Person { get; set; } 
}
