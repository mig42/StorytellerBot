using Microsoft.EntityFrameworkCore;
using StorytellerBot.Data;
using StorytellerBot.Services;
using StorytellerBot.Settings;
using Telegram.Bot;

namespace StorytellerBot;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    private IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<AdventureContext>(options =>
            options.UseSqlite(Configuration.GetConnectionString("AdventureContextSQLite")));
        services.AddDatabaseDeveloperPageExceptionFilter();

        services.AddHostedService<ConfigureWebhook>();

        var botConfigurationSection = Configuration.GetSection(nameof(BotConfiguration));
        var botToken = botConfigurationSection
            .GetValue<string>(nameof(BotConfiguration.BotToken));

        services.Configure<BotConfiguration>(botConfigurationSection);
        services.Configure<MessageSettings>(Configuration.GetSection("Message"));

        services
            .AddHttpClient("tgwebhook")
            .AddTypedClient<ITelegramBotClient>(
                httpClient => new TelegramBotClient(botToken, httpClient));

        services.AddScoped<HandleUpdateService>();
        services.AddScoped<GameEngineService>();

        services
            .AddControllers()
            .AddNewtonsoftJson();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
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
    }
}
