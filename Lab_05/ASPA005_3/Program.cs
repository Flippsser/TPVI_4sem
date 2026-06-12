using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseExceptionHandler("/Error");

app.MapGet("/", () => Results.Redirect("/A/3"));

// --- A ----------
app.MapGet("/A/{x:int:max(100)}", (HttpContext context, [FromRoute] int? x) =>
    Results.Ok(new { path = context.Request.Path.Value, x }));
app.MapPost("/A/{x:int:range(0,100)}", (HttpContext context, [FromRoute] int? x) =>
    Results.Ok(new { path = context.Request.Path.Value, x }));
app.MapPut("/A/{x:int:min(1)}/{y:int:min(1)}", (HttpContext context, [FromRoute] int x, [FromRoute] int y) =>
    Results.Ok(new { path = context.Request.Path.Value, x, y }));
app.MapDelete("/A/{x:int:min(1)}-{y:int:range(1,100)}", (HttpContext context, [FromRoute] int x, [FromRoute] int y) =>
    Results.Ok(new { path = context.Request.Path.Value, x, y }));

// --- B ----------
app.MapGet("/B/{x:float}", (HttpContext context, [FromRoute] float? x) =>
    Results.Ok(new { path = context.Request.Path.Value, x }));
app.MapPost("/B/{x:float}/{y:float}", (HttpContext context, [FromRoute] float x, [FromRoute] float y) =>
    Results.Ok(new { path = context.Request.Path.Value, x, y }));
app.MapDelete("/B/{x:float}-{y:float}", (HttpContext context, [FromRoute] float x, [FromRoute] float y) =>
    Results.Ok(new { path = context.Request.Path.Value, x, y }));

// --- C ----------
app.MapGet("/C/{x:bool}", (HttpContext context, [FromRoute] bool? x) =>
    Results.Ok(new { path = context.Request.Path.Value, x }));
app.MapPost("/C/{x:bool},{y:bool}", (HttpContext context, [FromRoute] bool x, [FromRoute] bool y) =>
    Results.Ok(new { path = context.Request.Path.Value, x, y }));

// --- D ----------
app.MapGet("/D/{x:datetime}", (HttpContext context, [FromRoute] DateTime? x) =>
    Results.Ok(new { path = context.Request.Path.Value, x }));
app.MapPost("/D/{x:datetime}|{y:datetime}", (HttpContext context, [FromRoute] DateTime x, [FromRoute] DateTime y) =>
    Results.Ok(new { path = context.Request.Path.Value, x, y }));

// --- E ----------
app.MapGet("/E/12-{x:required}", (HttpContext context, [FromRoute] string x) =>
    Results.Ok(new { path = context.Request.Path.Value, x }));
app.MapPut("/E/{x:alpha:length(2,12)}", (HttpContext context, [FromRoute] string x) =>
    Results.Ok(new { path = context.Request.Path.Value, x }));

// --- F ----------
app.MapGet("/F/{x:regex(^[\\w.%+-]+@[\\w.-]+\\.by$)}", (HttpContext context, [FromRoute] string x) =>
    Results.Ok(new { path = context.Request.Path.Value, x }));

app.MapFallback((HttpContext ctx) =>
    Results.NotFound(new { message = $"path {ctx.Request.Path.Value} not supported" }));

app.Map("/Error", (HttpContext ctx) =>
{
    Exception? ex = ctx.Features.Get<IExceptionHandlerFeature>()?.Error;
    return Results.Ok(new { message = ex?.Message });
});

app.Run("http://localhost:5000");
