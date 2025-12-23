using BookingManagmint;
using BookingManagmint.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

var builder = WebApplication.CreateBuilder(args);

// Add MVC and Razor Pages
builder.Services.AddControllersWithViews().AddMvcOptions(o =>
{
    o.Filters.Add(new Microsoft.AspNetCore.Mvc.IgnoreAntiforgeryTokenAttribute());
});

builder.Services.AddRazorPages().AddMvcOptions(o =>
{
    o.Filters.Add(new Microsoft.AspNetCore.Mvc.IgnoreAntiforgeryTokenAttribute());
});

// Add Web API
builder.Services.AddEndpointsApiExplorer();

// Add CORS for remote client
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
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
app.UseCors();
app.UseSession();

app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value ?? string.Empty;
    var method = context.Request.Method;
    var isAuthPath = path.StartsWith("/Auth", StringComparison.OrdinalIgnoreCase);
    var isApiPath = path.StartsWith("/api", StringComparison.OrdinalIgnoreCase);
    var isRemoteClient = path.StartsWith("/remote-client", StringComparison.OrdinalIgnoreCase);
    var isStatic = path.StartsWith("/css", StringComparison.OrdinalIgnoreCase) || 
                   path.StartsWith("/js", StringComparison.OrdinalIgnoreCase) || 
                   path.StartsWith("/images", StringComparison.OrdinalIgnoreCase) ||
                   path.StartsWith("/dist", StringComparison.OrdinalIgnoreCase);
    
    Console.WriteLine($"Middleware: {method} {path}, isAuthPath: {isAuthPath}, isApiPath: {isApiPath}, isStatic: {isStatic}, hasSession: {!string.IsNullOrEmpty(context.Session.GetString("UserId"))}");
    
    // Allow API and remote client access without authentication (API handles its own auth)
    // Allow static files and auth pages
    if (!isAuthPath && !isStatic && !isApiPath && !isRemoteClient && string.IsNullOrEmpty(context.Session.GetString("UserId")))
    {
        Console.WriteLine($"Middleware: Redirecting to /Auth/Login");
        context.Response.Redirect("/Auth/Login");
        return;
    }
    await next();
});

app.UseStatusCodePagesWithReExecute("/Error", "?code={0}");

// Map Razor Pages first - these handle Auth, Dashboard, Events pages, etc.
app.MapRazorPages();

// Map Web API controllers (with [ApiController] and [Route] attributes)
app.MapControllers();

// Map MVC controllers using conventional routing (fallback for controllers without [Route])
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Attendee}/{action=Index}/{id?}");

// Map root to Attendee/Index
app.MapGet("/", async (HttpContext context) =>
{
    context.Response.Redirect("/Attendee");
    return Task.CompletedTask;
});

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
