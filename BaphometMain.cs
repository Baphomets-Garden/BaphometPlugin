using BaphometPlugin.Configuration;
using BaphometPlugin.Modules.AdminTools.DiscordLogs;
using BaphometPlugin.Modules.AdminTools.DiscordLogs.BanLogger;
using BaphometPlugin.Modules.AfkChecker;
using BaphometPlugin.Modules.BulletHoleCap;
using BaphometPlugin.Modules.CleanupUtilities;
using BaphometPlugin.Modules.CustomHud;
using BaphometPlugin.Modules.DisconnectReplacer;
using BaphometPlugin.Modules.DiscordBot;
using BaphometPlugin.Modules.Lobby;
using BaphometPlugin.Modules.PointSystem;
using BaphometPlugin.Modules.RespawnTimer;
using BaphometPlugin.Modules.ScpVoiceChat;
using HarmonyLib;
using MEC;
using UniverseModule.API.Plugins;
using UniverseModule.Generic.Core;
using UniverseModule.Generic.Logger;
using Translation = BaphometPlugin.Configuration.Translations;

namespace BaphometPlugin;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global

[UniversePlugin("Baphomet Plugin", "Baphomet Garden Dev Team", "1.0.0.0", "The main plugin for the Baphomet Garden Server")]
public class BaphometMain : UniversePlugin<Config, Translation>
{
    public BanLogController BanLogController { get; private set; }
    
    public ScpVoiceChatModule ScpVoiceChatModule { get; private set; }
    
    public RespawnTimerManager RespawnTimerManager { get; private set; }
    
    public PointSystemManager PointSystemManager { get; private set; }
    
    public LobbyManager LobbyManager { get; private set; }
    
    public DisconnectReplacerManager DisconnectReplacerManager { get; private set; }
    
    public HudHandler HudHandler { get; private set; }
    
    public CleanupUtilitiesModule CleanupUtilitiesModule { get; private set; }
    
    public BulletHoleCapModule BulletHoleCapModule { get; private set; }
    
    public AfkHandler AfkHandler { get; private set; }
    
    public DiscordLogsHandler DiscordLogsHandler { get; private set; }
    
    public override void InitializePlugin()
    {
        ScpVoiceChatModule = Universe.GetManagedClass<ScpVoiceChatModule>();
        RespawnTimerManager = Universe.GetManagedClass<RespawnTimerManager>();
        PointSystemManager = Universe.GetManagedClass<PointSystemManager>();
        LobbyManager = Universe.GetManagedClass<LobbyManager>();
        DisconnectReplacerManager = Universe.GetManagedClass<DisconnectReplacerManager>();
        HudHandler = Universe.GetManagedClass<HudHandler>();
        CleanupUtilitiesModule = Universe.GetManagedClass<CleanupUtilitiesModule>();
        BulletHoleCapModule = Universe.GetManagedClass<BulletHoleCapModule>();
        AfkHandler = Universe.GetManagedClass<AfkHandler>();
        DiscordLogsHandler = Universe.GetManagedClass<DiscordLogsHandler>();

        BanLogController = Universe.GetManagedClass<BanLogController>();
        
        BanLogController.Start();

        if (Config.EnableStatusBot)
            DiscordBotManager.StartBot();

        Harmony harmony = new Harmony("com.xnexusacs.baphomet");
        harmony.PatchAll();
        
        UniverseLogger.Information("Baphomet Plugin has been enabled!");
        
        base.InitializePlugin();
    }

    public override void DisablePlugin()
    {
        ScpVoiceChatModule = null;
        RespawnTimerManager = null;
        PointSystemManager = null;
        LobbyManager = null;
        DisconnectReplacerManager = null;
        HudHandler = null;
        CleanupUtilitiesModule = null;
        BulletHoleCapModule = null;
        AfkHandler = null;
        DiscordLogsHandler = null;
        BanLogController = null;
        DiscordBotManager.BotTask.Dispose();
        Timing.KillCoroutines("Logs_QueueManager");
        
        base.DisablePlugin();
    }
}