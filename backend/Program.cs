using CorsoApi.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

//Infrastructure services
builder.Services.AddSingleton<IAccountsVault, AccountsVault>(opts =>
{
    var masterHash = builder.Configuration["masterHash"]
        ?? throw new InvalidOperationException("masterHash configuration is missing");

    return new AccountsVault("db.dat", masterHash);
});
builder.Services.AddScoped<IHasher, Argon2Hasher>();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddProblemDetails();

builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowCorsoWeb", policy =>
    {
        policy.WithOrigins("http://localhost:4200") // Angular dev server
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

builder.Services.AddDistributedMemoryCache();
// Add session support
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".Corso.Session";
    //TODO: Implement hard limit.
    options.IdleTimeout = TimeSpan.FromMinutes(1);
    options.Cookie.HttpOnly = true;           // XSS protection javascript cannot access
    options.Cookie.IsEssential = true;        // Essential to app
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;  //HTTPS only

    if(builder.Environment.IsDevelopment())
    {
        options.Cookie.SameSite = SameSiteMode.None;  //Allow cross-site requests for development
    }
    else
    {
        options.Cookie.SameSite = SameSiteMode.Strict;  //CSRF protection, can only be sent to same domain
    }
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

//Use section
app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseCors("AllowCorsoWeb");
app.UseSession();
app.MapControllers();
app.MapFallbackToFile("index.html");
await app.RunAsync();
