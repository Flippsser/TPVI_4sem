using ASPA008_1.Services;
using DAL_Celebrity_MSSQL;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ASPA008_1.Filters;

public sealed class InfoAsyncActionFilter : Attribute, IAsyncActionFilter
{
    public const string Wikipedia = "WIKI";

    private readonly string infoType;

    public InfoAsyncActionFilter(string infoType = "")
    {
        this.infoType = infoType.ToUpperInvariant();
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (infoType.Contains(Wikipedia, StringComparison.OrdinalIgnoreCase) &&
            TryGetId(context, out int id))
        {
            IRepository? repository = context.HttpContext.RequestServices.GetService<IRepository>();
            Celebrity? celebrity = repository?.GetCelebrityById(id);

            if (celebrity is not null)
            {
                WikiInfoCelebrity wiki = context.HttpContext.RequestServices.GetRequiredService<WikiInfoCelebrity>();
                context.HttpContext.Items[Wikipedia] = await wiki.GetReferencesAsync(celebrity.FullName);
            }
        }

        await next();
    }

    private static bool TryGetId(ActionExecutingContext context, out int id)
    {
        id = 0;

        if (context.ActionArguments.TryGetValue("id", out object? value) &&
            value is int actionId)
        {
            id = actionId;
            return id > 0;
        }

        string? routeId = context.RouteData.Values["id"]?.ToString();
        return int.TryParse(routeId, out id) && id > 0;
    }
}
