using DAL004;

namespace ASPA005_2;

public static class Validation
{
    public sealed class SurnameFilter : IEndpointFilter
    {
        public static IRepository? repository;

        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            try
            {
                Celebrity? celebrity = context.GetArgument<Celebrity?>(0);
                if (celebrity == null ||
                    string.IsNullOrWhiteSpace(celebrity.Surname) ||
                    celebrity.Surname.Length < 2)
                {
                    throw new Exception("POST /Celebrities error, Surname is wrong");
                }

                if (repository?.getCelebritiesBySurname(celebrity.Surname).Length > 0)
                {
                    throw new Exception("POST /Celebrities error, Surname is doubled");
                }

                return await next(context);
            }
            catch (Exception ex)
            {
                return Results.Conflict($"Value:{ex.Message}");
            }
        }
    }

    public sealed class PhotoExistFilter : IEndpointFilter
    {
        public static IRepository? repository;

        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            try
            {
                Celebrity? celebrity = context.GetArgument<Celebrity?>(0);
                if (celebrity == null)
                {
                    throw new Exception("POST /Celebrities error, Celebrity is null");
                }

                string fileName = Path.GetFileName(celebrity.PhotoPath);
                if (!File.Exists(Path.Combine(repository?.BasePath ?? string.Empty, fileName)))
                {
                    context.HttpContext.Response.Headers["X-Celebrity"] = $"NotFound={fileName}";
                }

                return await next(context);
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    title: "ASPA005_2/PhotoExistFilter",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }
    }

    public sealed class PutFilter : IEndpointFilter
    {
        public static IRepository? repository;

        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            int id = context.GetArgument<int>(0);
            Celebrity? celebrity = context.GetArgument<Celebrity?>(1);

            if (repository?.getCelebrityById(id) == null)
            {
                return Results.NotFound($"Value:PUT /Celebrities error, Id = {id}");
            }

            if (celebrity == null ||
                string.IsNullOrWhiteSpace(celebrity.Firstname) ||
                string.IsNullOrWhiteSpace(celebrity.Surname) ||
                string.IsNullOrWhiteSpace(celebrity.PhotoPath))
            {
                return Results.Conflict($"Value:PUT /Celebrities error, invalid celebrity");
            }

            return await next(context);
        }
    }

    public sealed class DeleteFilter : IEndpointFilter
    {
        public static IRepository? repository;

        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            int id = context.GetArgument<int>(0);
            if (repository?.getCelebrityById(id) == null)
            {
                return Results.NotFound($"Value:DELETE /Celebrities error, Id = {id}");
            }

            return await next(context);
        }
    }
}
