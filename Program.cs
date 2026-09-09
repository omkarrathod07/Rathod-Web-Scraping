using RathodWebScraping.Components;
using Microsoft.Extensions.AI;
using OpenAI;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

string? openAIKey = builder.Configuration["OpenAI:ApiKey"];
string? openAIModel = builder.Configuration["OpenAI:GptModel"];
OpenAIClient openAIClient = new OpenAIClient(openAIKey);
builder.Services.AddChatClient(new OpenAIChatClient(openAIClient,openAIModel));

var app = builder.Build();

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
