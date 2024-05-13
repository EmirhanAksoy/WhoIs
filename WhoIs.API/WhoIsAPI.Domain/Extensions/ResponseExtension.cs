

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WhoIsAPI.Domain.Extensions;

public static class ResponseExtension
{
    public static ProblemDetails ToBadRequestProblemDetails<T>(this Response<T> response, string instance)
    {
        return new ProblemDetails()
        {
            Status = StatusCodes.Status400BadRequest,
            Title = response.ErrorCode,
            Detail = response.Errors.FirstOrDefault(),
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1",
            Instance = instance
        };
    }
}
