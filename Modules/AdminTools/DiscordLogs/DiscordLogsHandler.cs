using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BaphometPlugin.Modules.AdminTools.DiscordLogs.BanLogger;
using MEC;
using Neuron.Modules.Commands.Event;
using UnityEngine.Networking;
using UniverseModule.API.Command;
using UniverseModule.API.Server;
using UniverseModule.Enums;
using UniverseModule.Events;
using UniverseModule.Generic.Core;
using UniverseModule.Generic.Logger;
using Utf8Json;

namespace BaphometPlugin.Modules.AdminTools.DiscordLogs;

// ReSharper disable ClassNeverInstantiated.Global

[AutomaticExecution]
public class DiscordLogsHandler
{
    public DiscordLogsHandler(CommandManager commandManager, RoundEvents roundEvents, PlayerEvents playerEvents, MapEvents mapEvents, ScpEvents scpEvents)
    {
        roundEvents.Waiting.Subscribe(OnWaiting);
        roundEvents.Start.Subscribe(OnStart);
        roundEvents.End.Subscribe(OnEnd);
        playerEvents.Death.Subscribe(OnDeath);
        playerEvents.Join.Subscribe(OnJoin);
        playerEvents.Leave.Subscribe(OnLeave);
        playerEvents.Report.Subscribe(OnReport);
        playerEvents.Ban.Subscribe(OnBan);
        playerEvents.Kick.Subscribe(OnKick);
        playerEvents.OfflineBan.Subscribe(OnOfflineBan);
        playerEvents.WarheadPanelInteract.Subscribe(OnWarheadPanelInteract);
        playerEvents.StartWarhead.Subscribe(OnStartWarhead);
        mapEvents.DetonateWarhead.Subscribe(OnDetonateWarhead);
        mapEvents.CancelWarhead.Subscribe(OnCancelWarhead);
        mapEvents.GeneratorEngage.Subscribe(OnGeneratorEngage);
        mapEvents.GeneratorActivate.Subscribe(OnGeneratorActivate);
        scpEvents.Scp049FinishRevive.Subscribe(OnScp049FinishRevive);
        scpEvents.Scp079LevelUp.Subscribe(OnScp079LevelUp);
        commandManager.RemoteAdmin.Subscribe(OnRemoteAdminCommand);
        
        Timing.RunCoroutine(ManageQueue(), "Logs_QueueManager");
    }

    private void AddMessageToQueue(string content, WebhookType type)
    {
        content = $"<t:{((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds()}:T> " + content;
        _msgQueue[type].Add(content);
    }
    
    private IEnumerator<float> SendMessage(LogMessage message, WebhookType type)
    {
        var url = Universe.GetManagedClass<BaphometMain>().Config.Webhooks[type];

        if (string.IsNullOrWhiteSpace(url))
            yield break;

        UnityWebRequest webRequest = new(url, UnityWebRequest.kHttpVerbPOST);
        UploadHandlerRaw uploadHandler = new(JsonSerializer.Serialize(message));
        uploadHandler.contentType = "application/json";
        webRequest.uploadHandler = uploadHandler;

        yield return Timing.WaitUntilDone(webRequest.SendWebRequest());
    }
    
    public IEnumerator<float> ManageQueue()
    {
        // ReSharper disable IteratorNeverReturns
        
        UniverseLogger.Warning("Starting Discord Logs Queue Manager");
        
        while (true)
        {
            foreach (var webhook in _msgQueue)
            {
                StringBuilder builder = new("");

                foreach (var message in webhook.Value.ToList())
                {
                    if (builder.Length + message.Length < 2000)
                    {
                        builder.AppendLine(message);
                        _msgQueue[webhook.Key].Remove(message);
                    }
                    else
                    {
                        break;
                    }
                }

                var content = builder.ToString();

                if (string.IsNullOrWhiteSpace(content))
                    continue;

                yield return Timing.WaitUntilDone(Timing.RunCoroutine(SendMessage(new LogMessage(builder.ToString()), webhook.Key)));
            }

            yield return Timing.WaitForSeconds(10);
        }
    }
    
    private readonly Dictionary<WebhookType, List<string>> _msgQueue = new() {[WebhookType.CommandLogs] = [], [WebhookType.GameLogs] = [], [WebhookType.ReportLogs] = [], [WebhookType.BanLogs] = []};
    
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable InconsistentNaming
    public class LogMessage(string _content)
    {
        public string username { get; set; } = $"{Universe.GetManagedClass<ServerManager>().Port} | Logs";
        public string avatar_url { get; set; } = Universe.GetManagedClass<BaphometMain>().Config.LogAvatarImageUrl;
        public string content { get; set; } = _content;
    }
    
    private void OnWaiting(RoundWaitingEvent ev)
    {
        AddMessageToQueue("`Server 🏁🟡🏁 >> Waiting For Players`", WebhookType.GameLogs);
    }

    private void OnOfflineBan(OfflineBanEvent ev)
    {
        AddMessageToQueue($"`Ban 🚫 >> {ev.IpAddress ?? ev.UserId} was banned by {ev.Issuer.NickName} ({ev.Issuer.UserId}) for {ev.Reason}`", WebhookType.BanLogs);
        Universe.GetManagedClass<BaphometMain>().BanLogController.SendMessage(new BanInfo(ev.Issuer, null, ev.Reason, ev.Duration));
    }

    private void OnStart(RoundStartEvent ev)
    {
        AddMessageToQueue("`Server 🏁🟢🏁 >> Round Started`", WebhookType.GameLogs);
    }

    private void OnEnd(RoundEndEvent ev)
    {
        AddMessageToQueue("`Server 🏁🔴🏁 >> Round Ended`", WebhookType.GameLogs);
    }

    private void OnDeath(DeathEvent ev)
    {
        if (ev.Attacker is null) return;
        
        AddMessageToQueue($"`Kill ☠ >> {ev.Attacker.NickName} ({ev.Attacker.RoleName}, {ev.Attacker.UserId}) killed {ev.Player.NickName} ({ev.Player.RoleName}, {ev.Player.UserId}) with {ev.DamageType} (Last Taken Damage: {ev.LastTakenDamage})`", WebhookType.GameLogs);
    }

    private void OnJoin(JoinEvent ev)
    {
        AddMessageToQueue($"`Server 🙍 >> {ev.NickName} ({ev.Player.UserId}) joined the server`", WebhookType.GameLogs);
    }
    
    private void OnLeave(LeaveEvent ev)
    {
        AddMessageToQueue($"`Server 🙍 >> {ev.Player.NickName} ({ev.Player.UserId}) left the server`", WebhookType.GameLogs);
    }

    private void OnReport(ReportEvent ev)
    {
        if (ev.SendToNorthWood)
            AddMessageToQueue("`Report 📝 >> " + ev.Player.NickName + " (" + ev.Player.UserId + ") reported " + ev.ReportedPlayer.NickName + " (" + ev.ReportedPlayer.UserId + ") for " + ev.Reason + " (Cheater Report)`", WebhookType.ReportLogs);
        else
            AddMessageToQueue("`Report 📝 >> " + ev.Player.NickName + " (" + ev.Player.UserId + ") reported " + ev.ReportedPlayer.NickName + " (" + ev.ReportedPlayer.UserId + ") for " + ev.Reason + " (Server Report)`", WebhookType.ReportLogs);
    }

    private void OnBan(BanEvent ev)
    {
        if (ev.Player.PlayerType is PlayerType.Dummy) return;
        
        AddMessageToQueue($"`Ban 🚫 >> {ev.Player.NickName} ({ev.Player.UserId}) was banned by {ev.Admin.NickName} ({ev.Admin.UserId}) for {ev.Reason}`", WebhookType.BanLogs);
        Universe.GetManagedClass<BaphometMain>().BanLogController.SendMessage(new BanInfo(ev.Admin, ev.Player, ev.Reason, ev.Duration));
    }

    private void OnKick(KickEvent ev)
    {
        if (ev.Player.PlayerType is PlayerType.Dummy) return;
        
        AddMessageToQueue("`Kick 🚫 >> " + ev.Player.NickName + " (" + ev.Player.UserId + ") was kicked by " + ev.Admin.NickName + " (" + ev.Admin.UserId + ") for " + ev.Reason + "`", WebhookType.BanLogs);
        Universe.GetManagedClass<BaphometMain>().BanLogController.SendMessage(new BanInfo(ev.Admin, ev.Player, ev.Reason, 0));
    }
    
    private void OnWarheadPanelInteract(WarheadPanelInteractEvent ev)
    {
        AddMessageToQueue($"`Warhead 🚀 >> {ev.Player.NickName} ({ev.Player.UserId}) interacted with the warhead panel`", WebhookType.GameLogs);
    }
    
    private void OnStartWarhead(StartWarheadEvent ev)
    {
        AddMessageToQueue($"`Warhead 🚀 >> {ev.Player.NickName} ({ev.Player.UserId}) started the warhead`", WebhookType.GameLogs);
    }
    
    private void OnDetonateWarhead(DetonateWarheadEvent ev)
    {
        AddMessageToQueue($"`Warhead 🚀 >> Warhead Detonated`", WebhookType.GameLogs);
    }
    
    private void OnCancelWarhead(CancelWarheadEvent ev)
    {
        AddMessageToQueue($"`Warhead 🚀 >> {ev.Player.NickName} ({ev.Player.UserId}) cancelled the warhead`", WebhookType.GameLogs);
    }
    
    private void OnGeneratorEngage(GeneratorEngageEvent ev)
    {
        AddMessageToQueue($"`Generator ⚡ >> Generator Engaged`", WebhookType.GameLogs);
    }
    
    private void OnGeneratorActivate(GeneratorActivateEvent ev)
    {
        AddMessageToQueue($"`Generator ⚡ >> Generator Activated`", WebhookType.GameLogs);
    }
    
    private void OnScp049FinishRevive(Scp049FinishReviveEvent ev)
    {
        AddMessageToQueue($"`SCP049 🧟 >> {ev.Scp.NickName} ({ev.Scp.UserId}) revived {ev.HumanToRevive.NickName} ({ev.HumanToRevive.UserId})`", WebhookType.GameLogs);
    }
    
    private void OnScp079LevelUp(Scp079LevelUpEvent ev)
    {
        AddMessageToQueue($"`SCP079 🤖 >> {ev.Scp.NickName} ({ev.Scp.UserId}) leveled up to level {ev.NewLevel}`", WebhookType.GameLogs);
    }

    private void OnRemoteAdminCommand(CommandEvent ev)
    {
        if (ev.Context.Command == "$")
            return;

        if (ev.Context.Arguments.Length == 0)
            return;

        var context = (UniverseContext)ev.Context;
        AddMessageToQueue($"`Remote Admin 🤖 >> {context.Player.NickName} ({context.Player.UserId}) executed {ev.Context.Command} {ev.Context.Arguments[0]}`", WebhookType.CommandLogs);
    }
    
    public enum WebhookType
    {
        GameLogs,
        CommandLogs,
        ReportLogs,
        BanLogs,
    }
}