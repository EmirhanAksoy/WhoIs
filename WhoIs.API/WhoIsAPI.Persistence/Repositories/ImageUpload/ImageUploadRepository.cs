using Dapper;
using Microsoft.Extensions.Logging;
using System.Data;
using WhoIsAPI.Domain;
using WhoIsAPI.Domain.Errors;
using WhoIsAPI.Domain.Models;

namespace WhoIsAPI.Persistence.Repositories.ImageUpload;

public class ImageUploadRepository : IImageUploadRepository
{
    private readonly IDbConnection _dbConnection;
    private readonly ILogger<ImageUploadRepository> _logger;
    public ImageUploadRepository(IDbConnection dbConnection, ILogger<ImageUploadRepository> logger)
    {
        _dbConnection = dbConnection;
        _logger = logger;
    }
    public async Task<Response<bool>> InsertImagePaths(List<ImageUniqueIdPair> images)
    {
        try
        {
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
}
