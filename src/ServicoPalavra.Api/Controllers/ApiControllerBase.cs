using Microsoft.AspNetCore.Mvc;
using ServicoPalavra.Application.Abstractions;
using ServicoPalavra.Application.Common;

namespace ServicoPalavra.Api.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult OkResponse<T>(T data, string? message = null) => Ok(ApiResponse<T>.Ok(data, message));
    protected IActionResult OkResponse(string? message = null) => Ok(ApiResponse.Ok(message));

    protected Guid CurrentUserId(ICurrentUser currentUser)
    {
        return currentUser.UserId ?? throw new AppException("Usuario nao autenticado.", 401);
    }
}
