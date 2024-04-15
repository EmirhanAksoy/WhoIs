namespace WhoIsAPI.Domain.Extensions;
public static class ImageFileExtensions
{
    public static bool IsImageFile(this string fileName)
    {
        if(string.IsNullOrEmpty(fileName)) 
            return false;
        var extensions = new[] { ".jpg", ".jpeg", ".png" };
        return extensions.Any(ext => fileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
    }
}
