﻿using WhoIsAPI.Domain;
using WhoIsAPI.Domain.Models;

namespace WhoIsAPI.Persistence.Repositories.ImageProcessRepository;

public interface IImageProcessRepository
{
    Task<Response<ImageUniqueIdPair>> GetUnprocessedImage();

    Task<Response<bool>> DeleteImage(string imageId);

    Task<Response<bool>> InsertImageFaceMappings(List<ImageFaceMapping> imageFaceMappings);

    Task<Response<bool>> SetImageAsProcessed(string imageId);

    Task<Response<List<FaceInfo>>> GetFaces();

    Task<Response<bool>> InsertImagePaths(List<ImageUniqueIdPair> images);

    Task<Response<string>> GetFaceImagePath(string imageId);
}
