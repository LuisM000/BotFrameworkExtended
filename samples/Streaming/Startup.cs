using System;
using System.ClientModel;
using Azure.AI.OpenAI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Bot.Streaming;

public class Startup
{
    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpClient().AddControllers().AddNewtonsoftJson(options =>
        {
            options.SerializerSettings.MaxDepth = HttpHelper.BotMessageSerializerSettings.MaxDepth;
        });

        // Create the Bot Framework Authentication to be used with the Bot Adapter.
        services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();

        // Create the Bot Adapter with error handling enabled.
        services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

        // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
        services.AddTransient<IBot, StreamingBot>();

        // Add IStorage to save the chat history
        services.AddSingleton<IStorage, MemoryStorage>();

        // Add OpenAI Chat client
        services.AddSingleton(sp =>
        {
            var model = sp.GetRequiredService<IConfiguration>().GetSection("AzureOpenAI:Model").Get<string>();
            var apiKey = sp.GetRequiredService<IConfiguration>().GetSection("AzureOpenAI:ApiKey").Get<string>();
            var endpoint = sp.GetRequiredService<IConfiguration>().GetSection("AzureOpenAI:Endpoint").Get<string>();

            return new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(apiKey)).GetChatClient(model);
        });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseDefaultFiles()
            .UseStaticFiles()
            .UseWebSockets()
            .UseRouting()
            .UseAuthorization()
            .UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
    }
}
