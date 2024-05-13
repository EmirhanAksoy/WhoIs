using System.Data;
using WhoIsAPI.Domain.Models;
using Dapper;

namespace WhoIsAPI.Persistence.Repositories.ImageRepository;

public class ImageRepository : IImageRepository
{
    private readonly IDbConnection _dbConnection;

    public ImageRepository(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task<ImagePathPair?> GetUnprocessedImage()
    {
        ImagePathPair? imageUniqueIdPair = await _dbConnection.QueryFirstOrDefaultAsync<ImagePathPair>("SELECT TOP 1 ImageId,ImagePath FROM Images WHERE IsProcessed=0 AND IsActive=1");
        return imageUniqueIdPair;
    }

    public async Task<List<FaceInfo>> GetFaces()
    {
        IEnumerable<FaceInfo> faces = await _dbConnection.QueryAsync<FaceInfo>("SELECT FaceId,FaceName FROM Faces WHERE IsActive=1");
        return faces.ToList();
    }

    public async Task<List<string>> GetImageIdsByFaceName(string faceNameSearchText)
    {
        var imageIds = (await _dbConnection.QueryAsync<string>("""
                SELECT IMF.ImageId FROM ImageFaceMapping IMF
                LEFT JOIN Faces F on IMF.FaceId = F.FaceId
                WHERE IMF.IsActive = 1 AND F.FaceName LIKE '%' + @Search + '%'
                """, new { Search = faceNameSearchText })).ToList();
        return imageIds;
    }

    public async Task<string> GetImagePath(string imageId, bool isFaceImage)
    {
        string query = isFaceImage ? "SELECT FacePath FROM Faces WHERE FaceId=@ImageId" : "SELECT ImagePath FROM Images WHERE ImageId=@ImageId";

        string imagePath = (await _dbConnection.QueryFirstOrDefaultAsync<string>(query, new { ImageId = imageId })) ?? string.Empty;

        if (!string.IsNullOrEmpty(imagePath) && isFaceImage)
        {
            imagePath = imagePath.Replace(@"/root", ".");
        }
        return imagePath;
    }

    public async Task<bool> CheckIfFaceNameExists(string faceId, string name)
    {
        bool facePath = (await _dbConnection.QueryFirstOrDefaultAsync<bool>("SELECT 1 FROM Faces WHERE FaceId<>@FaceId AND FaceName=@FaceName AND IsActive=1", new { FaceId = faceId, FaceName = name }));
        return facePath;
    }

    public async Task<bool> InsertImageFaceMappings(List<ImageFaceMapping> imageFaceMappings)
    {
        try
        {
            _dbConnection.Open();
            using IDbTransaction transaction = _dbConnection.BeginTransaction();
            int effectedRows = await _dbConnection.ExecuteAsync(@"
                INSERT INTO  ImageFaceMapping (FaceId, ImageId)
                VALUES(@FaceId, @ImageId)", imageFaceMappings, transaction: transaction);
            transaction.Commit();
            return effectedRows > 0;
        }
        finally
        {
            _dbConnection.Close();
        }
    }

    public async Task<bool> InsertImagePaths(List<ImagePathPair> images)
    {
        try
        {
            _dbConnection.Open();
            using IDbTransaction transaction = _dbConnection.BeginTransaction();
            int effectedRows = await _dbConnection.ExecuteAsync(@"
                INSERT INTO  Images (ImageId, ImagePath)
                VALUES(@ImageId, @ImagePath)", images, transaction: transaction);
            transaction.Commit();
            return effectedRows > 0;
        }
        finally
        {
            _dbConnection.Close();
        }
    }

    public async Task<bool> UpdateImageAsProcessed(string imageId)
    {
        try
        {
            _dbConnection.Open();
            using IDbTransaction transaction = _dbConnection.BeginTransaction();
            int effectedRows = await _dbConnection.ExecuteAsync(@"UPDATE  Images SET IsProcessed = 1 WHERE ImageId = @ImageId",
                new { ImageId = imageId }, transaction: transaction);
            transaction.Commit();
            return effectedRows > 0;
        }
        finally
        {
            _dbConnection.Close();
        }
    }

    public async Task<bool> UpdateFaceName(string faceId, string name)
    {
        int effectedRows = (await _dbConnection.ExecuteAsync("UPDATE Faces SET FaceName=@FaceName WHERE FaceId=@FaceId", new { FaceId = faceId, FaceName = name }));
        return effectedRows > 0;
    }

    public async Task<bool> DeleteImage(string imageId)
    {
        int effectedRows = await _dbConnection.ExecuteAsync(@"UPDATE Images SET IsActive = 0 WHERE ImageId = @ImageId", new { ImageId = imageId });
        return effectedRows > 0;

    }
}
