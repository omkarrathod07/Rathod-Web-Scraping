using RathodWebScraping.Components;
using Microsoft.Extensions.AI;
using OpenAI;
using RathodWebScraping.Services;
using RathodWebScraping.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

string? openAIKey = builder.Configuration["OpenAI:ApiKey"];
string? openAIModel = builder.Configuration["OpenAI:GptModel"];
OpenAIClient openAIClient = new OpenAIClient(openAIKey);
builder.Services.AddChatClient(new OpenAIChatClient(openAIClient, openAIModel));

builder.Services.AddHttpClient();
builder.Services.AddScoped<ThemeState>();
builder.Services.AddScoped<HtmlScraperService>();
builder.Services.AddScoped<VoiceSpeechService>();
builder.Services.AddDbContextFactory<ChatDbContext>(options => options.UseSqlite("Data Source=chats.db"));

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ChatDbContext>>();
    using var db = dbContextFactory.CreateDbContext();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();