using CorsoApi.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

//Infrastructure services
builder.Services.AddSingleton<ISecretStore, KeyringSecretStore>();
builder.Services.AddSingleton<IAccountsVault, AccountsVault>(opts =>
{
    var secretStore = opts.GetRequiredService<ISecretStore>();
    var masterPassword = secretStore.GetSecretAsync("master").GetAwaiter().GetResult()
        ?? throw new InvalidOperationException("Master password not found in secret store. Did you forget to set it up?");

    var salt = secretStore.GetSecretAsync("salt").GetAwaiter().GetResult()
        ?? throw new InvalidOperationException("Salt not found in secret store. Did you forget to set it up?");
    
    return new AccountsVault("db.dat", masterPassword, salt);
});

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
app.UseCors("AllowCorsoWeb");
app.MapControllers();
await app.RunAsync();
