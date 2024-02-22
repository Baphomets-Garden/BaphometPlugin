using System.Collections.Generic;
using System.Linq;
using System.Text;
using BaphometPlugin.Modules.CustomHud;
using UniverseModule.API.Player;
using UniverseModule.Enums;
using UniverseModule.Events;
using UniverseModule.Generic.Core;

namespace BaphometPlugin.Modules.PointSystem;

// ReSharper disable ClassNeverInstantiated.Global

[AutomaticExecution]
public class PointSystemManager
{
    private readonly PlayerManager _playerManager;
    
    private readonly Dictionary<UniversePlayer, uint> _pointsPerPlayer = new();

    public PointSystemManager(PlayerManager playerManager, PlayerEvents playerEvents, RoundEvents roundEvents)
    {
        _playerManager = playerManager;
        
        playerEvents.Join.Subscribe(OnJoin);
        playerEvents.Leave.Subscribe(OnLeave);
        playerEvents.Death.Subscribe(OnDeath);
        playerEvents.Escape.Subscribe(OnEscape);
        roundEvents.Restart.Subscribe(OnRestart);
        roundEvents.End.Subscribe(OnRoundEnd);
    }

    private void OnJoin(JoinEvent ev) => _pointsPerPlayer.Add(ev.Player, 0);
    
    private void OnLeave(LeaveEvent ev) => _pointsPerPlayer.Remove(ev.Player);
    
    private void OnRestart(RoundRestartEvent _) => _pointsPerPlayer.Clear();

    private void OnDeath(DeathEvent ev)
    {
        if (ev.Attacker is null || ev.Attacker == ev.Player)
        {
            HandlePoints(ev.Player, false, 5);
            return;
        }
        
        HandlePoints(ev.Attacker, true, 5);
        HandlePoints(ev.Player, false, 5);
    }

    private void OnRoundEnd(RoundEndEvent ev)
    {
        foreach (var player in _playerManager.Players)
            player.SendBroadcast(BuildEndMessage(), 10);
    }

    private void OnEscape(EscapeEvent ev)
    {
        if (ev.EscapeType is EscapeType.TooFarAway) return;
        
        HandlePoints(ev.Player, true, 10);
    }

    internal void HandlePoints(UniversePlayer player, bool addPoints, uint points)
    {
        if (!_pointsPerPlayer.ContainsKey(player)) return;

        if (addPoints)
        {
            _pointsPerPlayer[player] += points;
            player.SendHudHint(ScreenZone.Top, $"<b><color=green>[ +{points} P<lowercase>oints</lowercase> ]</color></b>", 4f);
        }
        else
        {
            if (_pointsPerPlayer[player] < 1) return;
            
            _pointsPerPlayer[player] -= points;
            player.SendHudHint(ScreenZone.Top, $"<b><color=red>[ -{points} P<lowercase>oints</lowercase> ]</color></b>", 4f);
        }
    }

    private string BuildEndMessage()
    {
        var builder = new StringBuilder();
        var sorted = _pointsPerPlayer.OrderByDescending(x => x.Value).ToList();
        
        builder.AppendLine("<size=50%><b><color=yellow>P<lowercase>oint</lowercase> MVP</color></b></size>");
        builder.AppendLine();
        
        for (var i = 0; i < 3; i++)
        {
            if (i >= sorted.Count) break;
            
            var player = sorted[i].Key;
            var points = sorted[i].Value;
            
            if (points < 1) continue;
            
            if (i == 0) return string.Empty;
            
            builder.AppendLine($"<size=50%><b><color=green><b>{player.NickName}</b> - {points} P<lowercase>oints</lowercase></color> <color=yellow> 🏆</color></b></size>");
        }
        
        return builder.ToString();
    }
}