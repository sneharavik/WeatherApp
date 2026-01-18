using WeatherApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<DbService>();
builder.Services.AddDistributedMemoryCache(); // Required for session
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

// ✅ Move AddSession HERE — BEFORE app is built
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true; // Recommended for login scenarios
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// ✅ UseAuthentication & UseAuthorization should come before UseSession if you ever add auth,
// but for now, this order is acceptable since you're using custom session-based auth.
app.UseAuthorization();
app.UseSession(); // Middleware goes AFTER routing and auth

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();