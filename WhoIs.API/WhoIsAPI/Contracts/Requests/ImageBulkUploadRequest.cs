namespace WhoIsAPI.Contracts.Requests;

public class ImageBulkUploadRequest
{
    public required IFormFile ZipFile { get; set; }
}
