using BookingManagmint;
using BookingManagmint.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages().AddMvcOptions(o =>
{
    o.Filters.Add(new Microsoft.AspNetCore.Mvc.IgnoreAntiforgeryTokenAttribute());
});

builder.Services.AddSession();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<DbConnectionFactory>();

var app = builder.Build();

// Configure the HTTP request pipeline.
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
    var isAuthPath = path.StartsWith("/Auth", StringComparison.OrdinalIgnoreCase);
    var isStatic = path.StartsWith("/css", StringComparison.OrdinalIgnoreCase) || path.StartsWith("/js", StringComparison.OrdinalIgnoreCase) || path.StartsWith("/images", StringComparison.OrdinalIgnoreCase);
    if (!isAuthPath && !isStatic && string.IsNullOrEmpty(context.Session.GetString("UserId")))
    {
        context.Response.Redirect("/Auth/Login");
        return;
    }
    await next();
});

app.UseStatusCodePagesWithReExecute("/Error", "?code={0}");

app.MapRazorPages();

// Ensure database exists and seed basic data
DatabaseInitializer.EnsureCreatedAndSeed(app.Services, builder.Configuration);

app.Run();
