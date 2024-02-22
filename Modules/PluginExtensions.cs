using System.Collections.Generic;
using BaphometPlugin.Modules.CustomHud;
using UniverseModule.API.Player;

namespace BaphometPlugin.Modules;

public static class PluginExtensions
{
    private static readonly Dictionary<UniversePlayer, HudRenderer> Hubs = new();

    public static void ClearHubs() => Hubs.Clear();
    
    public static void StartCustomManager(this UniversePlayer player) => Hubs.Add(player, player.gameObject.AddComponent<HudRenderer>());
    
    public static void RemoveCustomManager(this UniversePlayer player) => Hubs.Remove(player);
    
    public static void ClearCustomManagers()
    {
        foreach (var hub in Hubs)
            hub.Key.RemoveCustomManager();
    }
    
    public static void SendHudHint(this UniversePlayer player, ScreenZone zone, string message, float duration = 7)
    {
        if (Hubs.TryGetValue(player, out var hub))
            hub.AddMessage(zone, message, duration);
    }
    
    public static void ClearHintZone(this UniversePlayer player, ScreenZone zone)
    {
        if (Hubs.TryGetValue(player, out var hub))
            hub.ClearZone(zone);
    }
}