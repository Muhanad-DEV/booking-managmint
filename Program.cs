using BookingManagmint;
using BookingManagmint.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages().AddMvcOptions(o =>
{
    o.Filters.Add(new Microsoft.AspNetCore.Mvc.IgnoreAntiforgeryTokenAttribute());
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<DbConnectionFactory>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
    {
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();

app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value ?? string.Empty;
    var method = context.Request.Method;
    var isAuthPath = path.StartsWith("/Auth", StringComparison.OrdinalIgnoreCase);
    var isStatic = path.StartsWith("/css", StringComparison.OrdinalIgnoreCase) || 
                   path.StartsWith("/js", StringComparison.OrdinalIgnoreCase) || 
                   path.StartsWith("/images", StringComparison.OrdinalIgnoreCase) ||
                   path.StartsWith("/dist", StringComparison.OrdinalIgnoreCase);
    
    Console.WriteLine($"Middleware: {method} {path}, isAuthPath: {isAuthPath}, isStatic: {isStatic}, hasSession: {!string.IsNullOrEmpty(context.Session.GetString("UserId"))}");
    
    if (!isAuthPath && !isStatic && string.IsNullOrEmpty(context.Session.GetString("UserId")))
    {
        Console.WriteLine($"Middleware: Redirecting to /Auth/Login");
        context.Response.Redirect("/Auth/Login");
        return;
    }
    await next();
});

app.UseStatusCodePagesWithReExecute("/Error", "?code={0}");

app.MapRazorPages();

_ = Task.Run(async () =>
{
    try
    {
        await Task.Delay(TimeSpan.FromSeconds(5));
        DatabaseInitializer.EnsureCreatedAndSeed(app.Services, builder.Configuration);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database initialization error: {ex.Message}");
    }
});

app.Run();
