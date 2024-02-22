using System;
using System.Collections.Generic;
using BaphometPlugin.Modules.AdminTools.DiscordLogs;
using Syml;
using UniverseModule.Generic.Core;

namespace BaphometPlugin.Configuration;

[AutomaticExecution]
[Serializable]
[DocumentSection("Baphomet Plugin")]
public class Config : IDocumentSection
{
    public List<string> RespawnTimerTips { get; set; } = [];
    
    public string LogAvatarImageUrl { get; set; } = string.Empty;
    public Dictionary<DiscordLogsHandler.WebhookType, string> Webhooks { get; set; } = new()
    {
        [DiscordLogsHandler.WebhookType.CommandLogs] = "",
        [DiscordLogsHandler.WebhookType.GameLogs] = "",
        [DiscordLogsHandler.WebhookType.ReportLogs] = "",
        [DiscordLogsHandler.WebhookType.BanLogs] = "",
    };
    
    public bool EnableStatusBot { get; set; } = true;
    public string BotToken { get; set; } = string.Empty;
}