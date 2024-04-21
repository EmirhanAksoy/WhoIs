using Microsoft.Extensions.Logging;
using System.Data;
using WhoIsAPI.Domain.Models;
using Dapper;
using WhoIsAPI.Domain;
using WhoIsAPI.Domain.Errors;

namespace WhoIsAPI.Persistence.Repositories.ImageProcessRepository;

public class ImageProcessRepository : IImageProcessRepository
{
    private readonly IDbConnection _dbConnection;
    private readonly ILogger<ImageProcessRepository> _logger;

    public ImageProcessRepository(IDbConnection dbConnection, ILogger<ImageProcessRepository> logger)
    {
        _dbConnection = dbConnection;
        _logger = logger;
    }


    public async Task<Response<ImageUniqueIdPair>> GetUnprocessedImage()
    {
        try
        {
            ImageUniqueIdPair imageUniqueIdPair = await _dbConnection.QueryFirstAsync<ImageUniqueIdPair>("SELECT TOP 1 UniqueId,ImagePath FROM Images WHERE IsProcessed=0 AND IsActive=1");
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
            int effectedRows = await _dbConnection.ExecuteAsync(@"UPDATE Images SET IsActive = 0 WHERE UniqueId = @UniqueId", imageId);
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
    }
}
