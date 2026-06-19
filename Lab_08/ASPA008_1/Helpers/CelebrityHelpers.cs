using System.Globalization;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ASPA008_1.Helpers;

public static class CelebrityHelpers
{
    public static IHtmlContent CelebrityPhoto(this IHtmlHelper html, int id, string title, string src)
    {
        string href = id == 0 ? "/0" : $"/{id.ToString(CultureInfo.InvariantCulture)}";
        TagBuilder image = new("img");

        image.Attributes["id"] = id.ToString(CultureInfo.InvariantCulture);
        image.Attributes["class"] = "celebrity-photo";
        image.Attributes["title"] = title;
        image.Attributes["alt"] = title;
        image.Attributes["src"] = src;
        image.Attributes["onclick"] = $"location.href='{href}'";

        return image;
    }
}
