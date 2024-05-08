using Microsoft.Extensions.Logging;
using System.Data;
using WhoIsAPI.Domain.Models;
using Dapper;
using WhoIsAPI.Domain;
using WhoIsAPI.Domain.Errors;

namespace WhoIsAPI.Persistence.Repositories.ImageRepository;

public class ImageRepository : IImageRepository
{
    private readonly IDbConnection _dbConnection;
    private readonly ILogger<ImageRepository> _logger;

    public ImageRepository(IDbConnection dbConnection, ILogger<ImageRepository> logger)
    {
        _dbConnection = dbConnection;
        _logger = logger;
    }

    public async Task<Response<ImagePathPair>> GetUnprocessedImage()
    {
        try
        {
            ImagePathPair? imageUniqueIdPair = await _dbConnection.QueryFirstOrDefaultAsync<ImagePathPair>("SELECT TOP 1 ImageId,ImagePath FROM Images WHERE IsProcessed=0 AND IsActive=1");
            return Response<ImagePathPair>.SuccessResult(imageUniqueIdPair);
        }
        catch (Exception ex)
        {
            return Response<ImagePathPair>.HandleException<ImageRetrieveError>(ex, _logger);
        }
    }

    public async Task<Response<List<FaceInfo>>> GetFaces()
    {
        try
        {
            IEnumerable<FaceInfo> faces = await _dbConnection.QueryAsync<FaceInfo>("SELECT FaceId,FaceName FROM Faces WHERE IsActive=1");
            return Response<List<FaceInfo>>.SuccessResult(faces.ToList());
        }
        catch (Exception ex)
        {
            return Response<List<FaceInfo>>.HandleException<FacesRetrieveError>(ex, _logger);
        }
    }

    public async Task<Response<List<string>>> GetImageIdsByFaceName(string faceNameSearchText)
    {
        try
        {
            var imageIds = (await _dbConnection.QueryAsync<string>("""
                SELECT IMF.ImageId FROM ImageFaceMapping IMF
                LEFT JOIN Faces F on IMF.FaceId = F.FaceId
                WHERE IMF.IsActive = 1 AND F.FaceName LIKE '%' + @Search + '%'
                """, new { Search = faceNameSearchText })).ToList();
            return Response<List<string>>.SuccessResult(imageIds);
        }
        catch (Exception ex)
        {
            return Response<List<string>>.HandleException<GetImageIdsByFaceNameError>(ex, _logger);
        }
    }

    public async Task<Response<string>> GetImagePath(string imageId, bool isFaceImage)
    {
        try
        {
            string query = isFaceImage ? "SELECT FacePath FROM Faces WHERE FaceId=@ImageId" : "SELECT ImagePath FROM Images WHERE ImageId=@ImageId";

            string imagePath = (await _dbConnection.QueryFirstOrDefaultAsync<string>(query, new { ImageId = imageId })) ?? string.Empty;

            if (!string.IsNullOrEmpty(imagePath) && isFaceImage)
            {
                imagePath = imagePath.Replace(@"/root", ".");
            }
            return Response<string>.SuccessResult(imagePath);
        }
        catch (Exception ex)
        {
            return Response<string>.HandleException<FaceImagePathRetrieveError>(ex, _logger);
        }
    }

    public async Task<Response<bool>> CheckIfFaceNameExists(string faceId, string name)
    {
        try
        {
            bool facePath = (await _dbConnection.QueryFirstOrDefaultAsync<bool>("SELECT 1 FROM Faces WHERE FaceId<>@FaceId AND FaceName=@FaceName AND IsActive=1", new { FaceId = faceId, FaceName = name }));
            return Response<bool>.SuccessResult(facePath);
        }
        catch (Exception ex)
        {
            return Response<bool>.HandleException<CheckIfFaceNameExistsError>(ex, _logger);
        }
    }

    public async Task<Response<bool>> InsertImageFaceMappings(List<ImageFaceMapping> imageFaceMappings)
    {
        try
        {
            _dbConnection.Open();
            using IDbTransaction transaction = _dbConnection.BeginTransaction();
            int effectedRows = await _dbConnection.ExecuteAsync(@"
                INSERT INTO  ImageFaceMapping (FaceId, ImageId)
                VALUES(@FaceId, @ImageId)", imageFaceMappings, transaction: transaction);
            transaction.Commit();

            return Response<bool>.SuccessResult(effectedRows > 0);
        }
        catch (Exception ex)
        {
            return Response<bool>.HandleException<ImageFaceMappingInsertError>(ex, _logger);
        }
        finally
        {
            _dbConnection.Close();
        }
    }

    public async Task<Response<bool>> InsertImagePaths(List<ImagePathPair> images)
    {
        try
        {
            _dbConnection.Open();
            using IDbTransaction transaction = _dbConnection.BeginTransaction();
            int effectedRows = await _dbConnection.ExecuteAsync(@"
                INSERT INTO  Images (ImageId, ImagePath)
                VALUES(@ImageId, @ImagePath)", images, transaction: transaction);
            transaction.Commit();
            return Response<bool>.SuccessResult(effectedRows > 0);
        }
        catch (Exception ex)
        {
            return Response<bool>.HandleException<ImageBulkInsertError>(ex, _logger);
        }
    }

    public async Task<Response<bool>> UpdateImageAsProcessed(string imageId)
    {
        try
        {
            _dbConnection.Open();
            using IDbTransaction transaction = _dbConnection.BeginTransaction();
            int effectedRows = await _dbConnection.ExecuteAsync(@"UPDATE  Images SET IsProcessed = 1 WHERE ImageId = @ImageId",
                new { ImageId = imageId }, transaction: transaction);
            transaction.Commit();
            return Response<bool>.SuccessResult(effectedRows > 0);
        }
        catch (Exception ex)
        {
            return Response<bool>.HandleException<ImageSetAsProcessedError>(ex, _logger);
        }
        finally
        {
            _dbConnection.Close();
        }
    }

    public async Task<Response<bool>> UpdateFaceName(string faceId, string name)
    {
        try
        {
            int effectedRows = (await _dbConnection.ExecuteAsync("UPDATE Faces SET FaceName=@FaceName WHERE FaceId=@FaceId", new { FaceId = faceId, FaceName = name }));
            return Response<bool>.SuccessResult(effectedRows > 0);
        }
        catch (Exception ex)
        {
            return Response<bool>.HandleException<UpdateFaceNameError>(ex, _logger);
        }
    }

    public async Task<Response<bool>> DeleteImage(string imageId)
    {
        try
        {
            int effectedRows = await _dbConnection.ExecuteAsync(@"UPDATE Images SET IsActive = 0 WHERE ImageId = @ImageId", new { ImageId = imageId });
            return Response<bool>.SuccessResult(effectedRows > 0);
        }
        catch (Exception ex)
        {
            return Response<bool>.HandleException<ImageDeleteError>(ex, _logger);
        }
    }
}
