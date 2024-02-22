using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NorthwoodLib.Pools;
using PlayerRoles;
using Respawning;
using UnityEngine;
using UniverseModule.API.Map;
using UniverseModule.API.Player;
using UniverseModule.Events;
using UniverseModule.Generic.Core;

namespace BaphometPlugin.Modules.RespawnTimer;

// ReSharper disable ClassNeverInstantiated.Global

[AutomaticExecution]
public class RespawnTimerManager
{
    private CancellationTokenSource _respawnTimerCancellation;
    
    // Tips Here
    private readonly List<string> _tips = 
    [
        "Test Tip 1",
    ];
    
    public string Tip = string.Empty;
    
    public string RenderedZone = string.Empty;
    
    private string _renderedKill = string.Empty;
    
    private readonly RoundManager _roundManager;
    
    private readonly PlayerManager _playerManager;

    public RespawnTimerManager(RoundManager roundManager, PlayerManager playerManager, RoundEvents roundEvents, PlayerEvents playerEvents)
    {
        _roundManager = roundManager;
        _playerManager = playerManager;
        
        roundEvents.Start.Subscribe(OnStart);
        roundEvents.End.Subscribe(OnEnd);
        roundEvents.Restart.Subscribe(OnRestart);
        playerEvents.Death.Subscribe(OnDeath);
    }

    private void OnStart(RoundStartEvent ev)
    {
        if (!_respawnTimerCancellation?.IsCancellationRequested ?? false)
            return;
        
        _respawnTimerCancellation?.Dispose();
        _respawnTimerCancellation = new CancellationTokenSource();
        Task.Run(RespawnTimer, _respawnTimerCancellation.Token);
    }
    
    private void OnEnd(RoundEndEvent ev)
    {
        _respawnTimerCancellation?.Cancel();
    }
    
    private void OnRestart(RoundRestartEvent ev)
    {
        if (!_respawnTimerCancellation?.IsCancellationRequested ?? false)
            return;
        
        _respawnTimerCancellation?.Cancel();
    }
    
    private void OnDeath(DeathEvent ev)
    {
        _renderedKill = $"<b><color=#ff0000>{ev.Attacker?.NickName ?? "Unknown"} ({ev.Attacker?.RoleName ?? "None"})</color> killed <color=#ff0000>{ev.Player.NickName} ({ev.Player.RoleName})</color></b>";
        
        Task.Run(async () =>
        {
            await Task.Delay(5000);
            _renderedKill = string.Empty;
        });
    }
    
    private async Task RespawnTimer()
    {
        var i = 0;
        var builder = StringBuilderPool.Shared.Rent();
        while (_roundManager.RoundIsActive)
        {
            if (_respawnTimerCancellation.IsCancellationRequested)
            {
                StringBuilderPool.Shared.Return(builder);
                return;
            }

            builder.Clear();

            builder.AppendLine(_renderedKill);

            builder.AppendLine();
            
            var team = _roundManager.NextSpawnTeam switch
            {
                SpawnableTeamType.ChaosInsurgency => "<color=#008000>Chaos</color>",
                SpawnableTeamType.NineTailedFox => "<color=#84b6f4>MTF</color>",
                SpawnableTeamType.None => "",
                _ => ""
            };
            
            builder.AppendLine($"<size=40><b><color=#F08080>{_roundManager.TimeUntilSpawnWave.Minutes.ToString().PadLeft(2,'0')}:{_roundManager.TimeUntilSpawnWave.Seconds.ToString().PadLeft(2,'0')}</color></b></size>");
            builder.AppendLine(_roundManager.WaveSpawning ? $"<b><color=#ffa333>T<lowercase>eam spawn imminent: {team}</lowercase></color></b>" : "<b>T<lowercase>he next spawn is coming</lowercase></b>");
            builder.AppendLine();
            
            if (i == 25)
            {
                i = 0;
                Tip = "<color=#9342f5>❓</color>" + _tips[Random.Range(0, _tips.Count)];
            }
            
            await Task.Delay(1000);

            builder.Append("\n" + $"<size=65%><b><color=#9effe0>👻 S<lowercase>pectators</lowercase>:</color> {_playerManager.Players.Count(x => x.RoleType == RoleTypeId.Spectator)}</b></size>");

            RenderedZone = builder.ToString();

            i++;
        }
    }
}