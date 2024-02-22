using System.Collections.Generic;
using System.Linq;
using System.Text;
using BaphometPlugin.Modules.AdminTools.Commands;
using BaphometPlugin.Modules.RespawnTimer;
using NorthwoodLib.Pools;
using UniverseModule.API.Gamemodes;
using UniverseModule.API.Player;
using UniverseModule.Generic.Core;

namespace BaphometPlugin.Modules.CustomHud;

public class HudBuilder(StringBuilder builder)
{
    private const string ServerName = "<size=50%><alpha=#44><b><color=#7086fa>B</color><color=#9877e3>a</color><color=#b069ca>p</color><color=#be5daf>h</color><color=#c35494>o</color><color=#c24f7b>m</color><color=#bc4e64>e</color><color=#b14f51>t</color> <color=#a45140>G</color><color=#955434>a</color><color=#85562b>r</color><color=#745727>d</color><color=#655627>e</color><color=#565529>n</color></b><alpha=#ff></size>";
    private static string _pinnedMessage = string.Empty;

    ~HudBuilder() => StringBuilderPool.Shared.Return(builder);
    
    private readonly Dictionary<ScreenZone, string> _saved = new();
    private List<string> _notifications = [ ];
    private string _color = "#fff";
    private string _name = string.Empty;
    
    public void Clear()
    {
        _saved.Clear();
        _notifications.Clear();
    }
    
    public void WithContent(ScreenZone zone, string content)
    {
        if (_saved.ContainsKey(zone))
        {
            _saved[zone] = content;
            return;
        }
        
        _saved.Add(zone, content);
    }
    
    public void WithNotifications(List<string> notifications) => _notifications = notifications;
    public void WithColor(string color) => _color = color;
    public void WithName(string name) => _name = name;
    
    public string CreateHudForHuman(UniversePlayer player)
    {
        builder.Clear();
        builder.AppendLine(ServerName);
        builder.Append("<size=60%><line-height=100%><voffset=14em>");
        
        builder.Append("\n\n\n");
        
        var i = 0;

        for (; i < _notifications.Count; i++)
            builder.AppendLine(_notifications[i]);
        for (; i < 6; i++)
            builder.AppendLine();
        
        builder.Append(RenderHudZone(ScreenZone.Top));
        
        builder.Append(RenderHudZone(ScreenZone.CenterTop));
        builder.Append(RenderHudZone(ScreenZone.Center));
        builder.Append(RenderHudZone(ScreenZone.CenterBottom));
        
        if (JailCommand.JailedPlayers.Contains(player))
        {
            builder.AppendLine();
            builder.AppendLine("<b><color=red><size=65>JAILED</size></color>");
            builder.AppendLine("follow the moderator instructions</b>");
            builder.AppendLine();
            builder.AppendLine();
        }
        else
            builder.Append(RenderHudZone(ScreenZone.Bottom));
        
        builder.Append(FormatStringForHud(GetHudZone(ScreenZone.InteractionMessage), 1));
        builder.Append(FormatStringForHud(GetHudZone(ScreenZone.CompletelyBottom), 1));

        if (Universe.GetManagedClass<GamemodeManager>().CurrentGamemode != null)
        {
            _pinnedMessage = Universe.GetManagedClass<GamemodeManager>().CurrentGamemode.GamemodeName;
            builder.AppendLine($"<b>Event: {_pinnedMessage}</b>");
        }
        else
        {
            _pinnedMessage = string.Empty;
            builder.AppendLine(_pinnedMessage);
        }
        
        builder.Append($"<color={_color}>");
        builder.Append($"<b><< {_name} >></b>");

        return builder.ToString();
    }
    
    public string CreateHudForSpectators()
    {
        builder.Clear();
        builder.AppendLine(ServerName);
        builder.Append("<size=60%><line-height=100%><voffset=14em>");

        builder.Append("\n\n\n");

        var i = 0;

        for (; i < _notifications.Count; i++)
            builder.AppendLine(_notifications[i]);
        for (; i < 6; i++)
            builder.AppendLine();
        
        builder.Append(RenderHudZone(ScreenZone.Top));

        builder.Append(RenderHudZone(ScreenZone.CenterTop));
        builder.Append(RenderHudZone(ScreenZone.Center));
        builder.Append(RenderHudZone(ScreenZone.CenterBottom));
        builder.Append(FormatStringForHud(Universe.GetManagedClass<RespawnTimerManager>().RenderedZone));
        builder.Append(FormatStringForHud(GetHudZone(ScreenZone.InteractionMessage), 1));
        
        builder.AppendLine(Universe.GetManagedClass<RespawnTimerManager>().Tip);
        
        if (Universe.GetManagedClass<GamemodeManager>().CurrentGamemode != null)
        {
            _pinnedMessage = Universe.GetManagedClass<GamemodeManager>().CurrentGamemode.GamemodeName;
            builder.AppendLine($"<b>Event: {_pinnedMessage}</b>");
        }
        else
        {
            _pinnedMessage = string.Empty;
            builder.AppendLine(_pinnedMessage);
        }
        
        builder.Append($"<color={_color}>");
        builder.Append($"<b><< {_name} >></b>");

        return builder.ToString();
    }
    
    private string GetHudZone(ScreenZone zone) => _saved.ContainsKey(zone) ? _saved[zone] : string.Empty;

    private string RenderHudZone(ScreenZone zone) => FormatStringForHud(GetHudZone(zone));
    
    private static string FormatStringForHud(string text, int linesNeeded = 6)
    {
        var textLines = text.Count(x => x == '\n');

        for (var i = 0; i < linesNeeded - textLines; i++)
            text += '\n';

        return text;
    }
}