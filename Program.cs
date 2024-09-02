using Azure_Bot_Cloner.Components;
using Azure.Identity;
using Azure.ResourceManager;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add ErrorHandlingService
builder.Services.AddSingleton<ErrorHandlingService>();

// Configure Azure services
builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var credential = new ClientSecretCredential(
        config["AzureAd:TenantId"],
        config["AzureAd:ClientId"],
        config["AzureAd:ClientSecret"]);
    return new ArmClient(credential);
});

// Add AzureBotServiceHelper
builder.Services.AddSingleton<Azure_Bot_Cloner.Services.AzureBotServiceHelper>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();