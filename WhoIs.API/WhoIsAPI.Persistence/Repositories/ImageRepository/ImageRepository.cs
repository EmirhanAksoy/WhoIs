using Microsoft.Extensions.Logging;
using System.Data;
using WhoIsAPI.Domain.Models;
using Dapper;
using WhoIsAPI.Domain;
using WhoIsAPI.Domain.Errors;
using static System.Net.Mime.MediaTypeNames;
using System.Xml.Linq;

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

    public async Task<Response<ImageUniqueIdPair>> GetUnprocessedImage()
    {
        try
        {
            ImageUniqueIdPair? imageUniqueIdPair = await _dbConnection.QueryFirstOrDefaultAsync<ImageUniqueIdPair>("SELECT TOP 1 UniqueId,ImagePath FROM Images WHERE IsProcessed=0 AND IsActive=1");
            return Response<ImageUniqueIdPair>.SuccessResult(imageUniqueIdPair);
        }
        catch (Exception ex)
        {
            IError error = new ImageRetrieveError();
            _logger.LogError(error.EventId, ex, "{Code} {Message}", error.ErrorCode, error.ErrorMessage);
            return Response<ImageUniqueIdPair>.ErrorResult(error, ex);
        }
    }

    public async Task<Response<bool>> DeleteImage(string imageId)
    {
        try
        {
            int effectedRows = await _dbConnection.ExecuteAsync(@"UPDATE Images SET IsActive = 0 WHERE UniqueId = @ImageId", new { ImageId = imageId });
            return Response<bool>.SuccessResult(effectedRows > 0);
        }
        catch (Exception ex)
        {
            IError error = new ImageDeleteError();
            _logger.LogError(error.EventId, ex, "{Code} {Message}", error.ErrorCode, error.ErrorMessage);
            return Response<bool>.ErrorResult(error, ex);
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
            IError error = new ImageFaceMappingInsertError();
            _logger.LogError(error.EventId, ex, "{Code} {Message} {@images}", error.ErrorCode, error.ErrorMessage, imageFaceMappings);
            return Response<bool>.ErrorResult(error, ex);
        }
        finally
        {
            _dbConnection.Close();
        }
    }

    public async Task<Response<bool>> SetImageAsProcessed(string imageId)
    {
        try
        {
            _dbConnection.Open();
            using IDbTransaction transaction = _dbConnection.BeginTransaction();
            int effectedRows = await _dbConnection.ExecuteAsync(@"UPDATE  Images SET IsProcessed = 1 WHERE UniqueId = @ImageId",
                new { ImageId = imageId }, transaction: transaction);
            transaction.Commit();

            return Response<bool>.SuccessResult(effectedRows > 0);
        }
        catch (Exception ex)
        {
            IError error = new ImageSetAsProcessedError();
            _logger.LogError(error.EventId, ex, "{Code} {Message}", error.ErrorCode, error.ErrorMessage);
            return Response<bool>.ErrorResult(error, ex);
        }
        finally
        {
            _dbConnection.Close();
        }
    }

    public async Task<Response<List<FaceInfo>>> GetFaces()
    {
        try
        {
            IEnumerable<FaceInfo> faces = await _dbConnection.QueryAsync<FaceInfo>("SELECT UniqueId AS FaceId,FaceName FROM Faces WHERE IsActive=1");
            return Response<List<FaceInfo>>.SuccessResult(faces.ToList());
        }
        catch (Exception ex)
        {
            IError error = new FacesRetrieveError();
            _logger.LogError(error.EventId, ex, "{Code} {Message}", error.ErrorCode, error.ErrorMessage);
            return Response<List<FaceInfo>>.ErrorResult(error, ex);
        }
    }

    public async Task<Response<string>> GetFaceImagePath(string imageId)
    {
        try
        {
            string facePath = (await _dbConnection.QueryFirstOrDefaultAsync<string>("SELECT FacePath FROM Faces WHERE UniqueId=@UniqueId", new { UniqueId = imageId })) ?? string.Empty;
            if (string.IsNullOrEmpty(facePath))
            {
                facePath = facePath.Replace(@"/root", ".");
            }
            return Response<string>.SuccessResult(facePath);
        }
        catch (Exception ex)
        {
            IError error = new FaceImagePathRetrieveError();
            _logger.LogError(error.EventId, ex, "{Code} {Message}", error.ErrorCode, error.ErrorMessage);
            return Response<string>.ErrorResult(error, ex);
        }
    }

    public async Task<Response<bool>> InsertImagePaths(List<ImageUniqueIdPair> images)
    {
        try
        {
            _dbConnection.Open();
            using IDbTransaction transaction = _dbConnection.BeginTransaction();
            int effectedRows = await _dbConnection.ExecuteAsync(@"
                INSERT INTO  Images (UniqueId, ImagePath)
                VALUES(@UniqueId, @ImagePath)", images, transaction: transaction);
            transaction.Commit();
            return Response<bool>.SuccessResult(effectedRows > 0);
        }
        catch (Exception ex)
        {
            IError error = new ImageBulkInsertError();
            _logger.LogError(error.EventId, ex, "{Code} {Message} {@images}", error.ErrorCode, error.ErrorMessage, images);
            return Response<bool>.ErrorResult(error, ex);
        }
    }

    public async Task<Response<bool>> CheckIfFaceNameExists(string imageId, string name)
    {
        try
        {
            bool facePath = (await _dbConnection.QueryFirstOrDefaultAsync<bool>("SELECT 1 FROM Faces WHERE UniqueId<>@UniqueId AND FaceName=@FaceName AND IsActive=1", new { UniqueId = imageId, FaceName = name }));
            return Response<bool>.SuccessResult(facePath);
        }
        catch (Exception ex)
        {
            IError error = new FaceImagePathRetrieveError();
            _logger.LogError(error.EventId, ex, "{Code} {Message}", error.ErrorCode, error.ErrorMessage);
            return Response<bool>.ErrorResult(error, ex);
        }
    }

    public async Task<Response<bool>> UpdateFaceName(string imageId, string name)
    {
        try
        {
            int effectedRows = (await _dbConnection.ExecuteAsync("UPDATE Faces SET FaceName=@FaceName WHERE UniqueId=@UniqueId", new { UniqueId = imageId, FaceName = name }));
            return Response<bool>.SuccessResult(effectedRows > 0);
        }
        catch (Exception ex)
        {
            IError error = new FaceImagePathRetrieveError();
            _logger.LogError(error.EventId, ex, "{Code} {Message}", error.ErrorCode, error.ErrorMessage);
            return Response<bool>.ErrorResult(error, ex);
        }
    }

    public async Task<Response<List<string>>> GetImageIdsByFaceName(string faceNameSearchText)
    {
        try
        {
            var imageIds = (await _dbConnection.QueryAsync<string>("""
                SELECT IMF.ImageId FROM ImageFaceMapping IMF
                LEFT JOIN Faces F on IMF.FaceId = F.UniqueId
                WHERE F.FaceName LIKE '%' + @Search + '%'
                """, new { Search = faceNameSearchText })).ToList();
            return Response<List<string>>.SuccessResult(imageIds);
        }
        catch (Exception ex)
        {
            IError error = new FaceImagePathRetrieveError();
            _logger.LogError(error.EventId, ex, "{Code} {Message}", error.ErrorCode, error.ErrorMessage);
            return Response<List<string>>.ErrorResult(error, ex);
        }
    }
}
