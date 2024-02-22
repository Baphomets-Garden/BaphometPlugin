using System;
using DSharp4Webhook.Core;
using DSharp4Webhook.Core.Constructor;
using DSharp4Webhook.Util;
using UniverseModule.API.Server;
using UniverseModule.Generic.Core;
using UniverseModule.Generic.Logger;

namespace BaphometPlugin.Modules.AdminTools.DiscordLogs.BanLogger;

// ReSharper disable ClassNeverInstantiated.Global

[AutomaticExecution]
public class BanLogController : IDisposable
{
    private static readonly EmbedBuilder EmbedBuilder = ConstructorProvider.GetEmbedBuilder();
    private static readonly EmbedFieldBuilder FieldBuilder = ConstructorProvider.GetEmbedFieldBuilder();
    private static readonly MessageBuilder MessageBuilder = ConstructorProvider.GetMessageBuilder();
    private IWebhook _webhook;
    private bool _isDisposed;

    public void Start()
    {
        _webhook = WebhookProvider.CreateStaticWebhook(Universe.GetManagedClass<BaphometMain>().Config.Webhooks[DiscordLogsHandler.WebhookType.BanLogs]);
    }
    
    public void SendMessage(BanInfo banInfo)
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(BanLogController));

        _webhook.SendMessage(PrepareMessage(banInfo).Build()).Queue((result, isSuccessful) =>
        {
            if (!isSuccessful)
                UniverseLogger.Error("Failed to send ban webhook message.");
        });
    }
    
    public void Dispose()
    {
        _isDisposed = true;
        _webhook?.Dispose();
    }
    private static string TimeFormatter(long duration)
    {
        if (duration == 0)
            return "Kick";

        var timespan = new TimeSpan(0, 0, (int)duration);
        var finalFormat = string.Empty;

        switch (timespan.TotalDays)
        {
            case >= 365:
                finalFormat += $" {timespan.TotalDays / 365}y";
                break;
            case >= 30:
                finalFormat += $" {timespan.TotalDays / 30}mon";
                break;
            case >= 1:
                finalFormat += $" {timespan.TotalDays}d";
                break;
            default:
            {
                if (timespan.Hours > 0)
                    finalFormat += $" {timespan.Hours}h";
                break;
            }
        }
        if (timespan.Minutes > 0)
            finalFormat += $" {timespan.Minutes}min";
        if (timespan.Seconds > 0)
            finalFormat += $" {timespan.Seconds}s";

        return finalFormat.Trim();
    }
    
    private static string CodeLine(string message) => $"```{message}```";

    private MessageBuilder PrepareMessage(BanInfo banInfo)
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(BanLogController));

        EmbedBuilder.Reset();
        FieldBuilder.Reset();
        MessageBuilder.Reset();

        FieldBuilder.Inline = false;

        FieldBuilder.Name = "User";
        FieldBuilder.Value = CodeLine(banInfo.BannedName + " " + $"({banInfo.BannedId})");
        EmbedBuilder.AddField(FieldBuilder.Build());

        FieldBuilder.Name = "Staff";
        FieldBuilder.Value = CodeLine(banInfo.IssuerName + " " + $"({banInfo.IssuerId})");
        EmbedBuilder.AddField(FieldBuilder.Build());

        FieldBuilder.Name = "Reason";
        FieldBuilder.Value = CodeLine(banInfo.Reason);
        EmbedBuilder.AddField(FieldBuilder.Build());

        FieldBuilder.Name = "Duration";
        FieldBuilder.Value = CodeLine(TimeFormatter(banInfo.Duration));
        EmbedBuilder.AddField(FieldBuilder.Build());

        EmbedBuilder.Title = $"New Sanction - {Universe.GetManagedClass<ServerManager>().Port}";
        EmbedBuilder.Timestamp = DateTimeOffset.UtcNow;
        EmbedBuilder.Color = (uint)ColorUtil.FromHex("#D10E11");

        MessageBuilder.AddEmbed(EmbedBuilder.Build());

        return MessageBuilder;
    }
}