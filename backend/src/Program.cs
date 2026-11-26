using CorsoApi.Infrastructure;
using OllamaSharp;

var builder = WebApplication.CreateBuilder(args);

// Configure logging from configuration so environment variables (e.g. on Azure App Service)
// can control minimum log levels using the Logging:LogLevel section (Logging__LogLevel__Default).
builder.Logging.ClearProviders();
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
builder.Logging.AddConsole();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddMemoryCache();
builder.Services
    .AddOptions<ChatHistoryCacheOptions>()
    .Bind(builder.Configuration.GetSection(ChatHistoryCacheOptions.SectionName))
    .Validate(options => options.MaxMessages > 0, "Chat history MaxMessages must be greater than zero.")
    .Validate(options => options.SlidingExpiration > TimeSpan.Zero, "Chat history SlidingExpiration must be greater than zero.")
    .ValidateOnStart();
builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});


//Infrastructure services
builder.Services.AddSingleton<IAccountsVault, AccountsVault>(opts =>
{
    var masterHash = builder.Configuration["masterHash"]
        ?? throw new InvalidOperationException("masterHash configuration is missing");

    return new AccountsVault("db.dat", masterHash);
});
builder.Services.AddSingleton<IChatHistoryStore, MemoryChatHistoryStore>();
builder.Services.AddScoped<IHasher, Argon2Hasher>();
builder.Services.AddScoped<CsvParser>();

builder.Services.AddChatClient(new OllamaApiClient("http://localhost:11434", "llama3.2"));

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
    options.IdleTimeout = TimeSpan.FromMinutes(15);
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

if(!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseCors("AllowCorsoWeb");
app.UseSession();
app.MapControllers();
app.MapFallbackToFile("index.html");
await app.RunAsync();
