using Azure_Bot_Cloner.Components;
using Azure.Identity;
using Azure.ResourceManager;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton<ErrorHandlingService>();

builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var credential = new ClientSecretCredential(
        config["AzureAd:TenantId"],
        config["AzureAd:ClientId"],
        config["AzureAd:ClientSecret"]);
    return new ArmClient(credential);
});

builder.Services.AddSingleton<Azure_Bot_Cloner.Services.AzureBotServiceHelper>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();