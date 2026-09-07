using cc_dotnet_demo.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // HSTS et TLS gérés par le proxy Clever Cloud : pas de UseHsts() ni de UseHttpsRedirection() ici.
}

// En-têtes de sécurité minimaux (pas de CSP : Blazor Server/SignalR, à tester en Report-Only avant activation).
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    await next();
});

app.UseStaticFiles();
app.UseAntiforgery();

// Health check (CC_HEALTH_CHECK_PATH=/health côté Clever Cloud).
app.MapGet("/health", () => Results.Text("ok"));

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
