namespace WhoIsAPI.Domain.Models;

public class ImageProcessResponse
{
    public int Count { get; set; }
    public List<ImageProcessItem> ImageProcesses { get; set; } = [];
}

public record ImageProcessItem(float Dist,string Id);
