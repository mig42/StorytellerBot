using Microsoft.EntityFrameworkCore;
using StorytellerBot.Data;
using StorytellerBot.Services;
using StorytellerBot.Services.Conversations;
using StorytellerBot.Settings;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);

#region Logging

builder.Logging.AddLog4Net();

#endregion

#region Services

builder.Services.AddDbContext<AdventureContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("AdventureContextSQLite")));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddHostedService<ConfigureWebhook>();
builder.Services.AddHostedService<ConfigureCommands>();


builder.Services.Configure<BotConfiguration>(builder.Configuration);

var botToken = builder.Configuration[nameof(BotConfiguration.BotToken)];
builder.Services
    .AddHttpClient("tgwebhook")
    .AddTypedClient<ITelegramBotClient>(
        httpClient => new TelegramBotClient(botToken, httpClient));

builder.Services.AddScoped<AdventureRepository>();
builder.Services.AddScoped<IConversationFactory, ConversationFactory>();
builder.Services.AddScoped<IResponseSender, ResponseSender>();
builder.Services.AddScoped<IAdventureWriter, AdventureWriter>();
builder.Services.AddScoped<IMediaLocator, MediaLocator>();

builder.Services.AddTransient<CallbackConversation>();
builder.Services.AddTransient<TextConversation>();
builder.Services.AddTransient<StartCommandConversation>();
builder.Services.AddTransient<RestartCommandConversation>();
builder.Services.AddTransient<ListCommandConversation>();
builder.Services.AddTransient<NoopConversation>();

builder.Services
    .AddControllers()
    .AddNewtonsoftJson(setup => setup.UseCamelCasing(true));

#endregion

#region Application

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseRouting();
app.UseCors();

app.UseEndpoints(endpoints => endpoints.MapControllers());

app.Run();

#endregion
