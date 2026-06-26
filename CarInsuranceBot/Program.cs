using CarInsuranceBot.Application.Interfaces;
using CarInsuranceBot.Application.Options;
using CarInsuranceBot.Bot.Handlers;
using CarInsuranceBot.Infrastructure.Ai;
using CarInsuranceBot.Infrastructure.Documents;
using CarInsuranceBot.Infrastructure.Files;
using CarInsuranceBot.Infrastructure.Persistence;
using CarInsuranceBot.Infrastructure.Policies;
using CarInsuranceBot.Infrastructure.Telegram;
using CarInsuranceBot.Services;
using Microsoft.Extensions.Options;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<TelegramOptions>()
    .Bind(builder.Configuration.GetSection(TelegramOptions.SectionName))
    .Validate(options => !string.IsNullOrWhiteSpace(options.BotToken), "Telegram bot token is required.")
    .ValidateOnStart();

builder.Services.AddOptions<GeminiOptions>()
    .Bind(builder.Configuration.GetSection(GeminiOptions.SectionName))
    .Validate(options => !string.IsNullOrWhiteSpace(options.ApiKey), "Gemini API key is required.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.Model), "Gemini model is required.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.SystemInstruction), "Gemini system instruction is required.")
    .ValidateOnStart();

builder.Services.AddOptions<MindeeOptions>()
    .Bind(builder.Configuration.GetSection(MindeeOptions.SectionName))
    .Validate(options => !string.IsNullOrWhiteSpace(options.ApiKey), "Mindee API key is required.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.VinEndpointName), "Mindee VIN endpoint name is required.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.VinAccountName), "Mindee VIN account name is required.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.VinEndpointVersion), "Mindee VIN endpoint version is required.")
    .ValidateOnStart();

builder.Services.AddOptions<InsuranceOptions>()
    .Bind(builder.Configuration.GetSection(InsuranceOptions.SectionName))
    .Validate(options => options.PriceAmount > 0, "Insurance price must be greater than zero.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.PriceCurrency), "Insurance currency is required.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.PolicyTemplatePath), "Policy template path is required.")
    .ValidateOnStart();

builder.Services.AddHttpClient();
builder.Services.AddSingleton<ITelegramBotClient>(serviceProvider =>
{
    var options = serviceProvider.GetRequiredService<IOptions<TelegramOptions>>().Value;
    return new TelegramBotClient(options.BotToken);
});

builder.Services.AddSingleton<IConversationStore, InMemoryConversationStore>();
builder.Services.AddSingleton<IAiAssistant, GeminiAssistant>();
builder.Services.AddSingleton<IDocumentRecognitionService, MindeeDocumentRecognitionService>();
builder.Services.AddSingleton<ITelegramFileDownloader, TelegramFileDownloader>();
builder.Services.AddSingleton<IPolicyGenerator, TemplatePolicyGenerator>();
builder.Services.AddSingleton<IMarkdownEscaper, MarkdownEscaper>();

builder.Services.AddSingleton<TextMessageHandler>();
builder.Services.AddSingleton<FileMessageHandler>();
builder.Services.AddSingleton<CallbackHandler>();
builder.Services.AddSingleton<UpdateHandler>();
builder.Services.AddHostedService<BotBackgroundService>();

var app = builder.Build();
await app.RunAsync();