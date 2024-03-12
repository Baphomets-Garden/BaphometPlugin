using System.Collections.Generic;
using BaphometPlugin.Modules.CustomHud;
using BaphometPlugin.Modules.DisconnectReplacer;
using PlayerRoles;
using UnityEngine;
using UniverseModule.API.Map;
using UniverseModule.API.Player;
using UniverseModule.Generic.Core;

namespace BaphometPlugin.Modules.AfkChecker;

public class AfkCheckerComponent : MonoBehaviour
{
    private const int AfkTime = 90;
    
    private float _counter;
    private int _afkTime;
    private Vector3 _lastPos;
    private Vector2 _lastRot;

    private UniversePlayer _player;
    
    public static void AddController(UniversePlayer player)
    {
        player.gameObject.AddComponent<AfkCheckerComponent>().Init(player);
    }
    
    public void Init(UniversePlayer player)
    {
        _player = player;
    }
    
    private void Update()
    {
        _counter += Time.deltaTime;

        if (_counter < 1)
            return;
        
        _counter = 0;
        
        var pos = _player.Position; 
        var rot = _player.RotationVector2;
            
        if (_player.CurrentRole.Team != Team.Dead && _player.RoleType != RoleTypeId.Scp079 && _player.RoleType != RoleTypeId.Filmmaker && Universe.GetManagedClass<RoundManager>().RoundIsActive && _lastPos == pos && _lastRot == rot)
        {
            if (WhitelistedRanks.Contains(_player.Group.Badge))
            {
                _afkTime = 0;
                return;
            }
            
            _afkTime++;

            if (_afkTime < AfkTime - 10) 
                return;
            
            _player.SendBroadcast($"<size=150%><b><u><color=#fc0345>AFK CHECKER</color></u></b></size>\n<b>Move in less than <color=#fc0345>{AfkTime - _afkTime}</color> seconds or you will be kicked", 1);

            if (_afkTime < AfkTime)
                return;

            if (_player.RoleType != RoleTypeId.Tutorial && PlayerReplacer.TryGetRandomSpectator(out var target))
            {
                PlayerReplacer.ReplacePlayer(target, _player);
                target.SendHudHint(ScreenZone.CompletelyBottom, "R<lowercase>eplaced a player that was afk for too long</lowercase>", 5f);
            }

            if (Universe.GetManagedClass<PlayerManager>().Players.Count > 25)
            {
                Destroy(this);
                _player.Kick("Kicked for being AFK.\n[KICKED BY A SERVER MODIFICATION]");
                return;
            }

            _player.SetRoleFlags(RoleTypeId.Spectator, RoleSpawnFlags.All, RoleChangeReason.RemoteAdmin);
            _lastPos = pos;
            _lastRot = rot;
            _afkTime = -5;
        }
        else
        {
            _lastPos = pos;
            _lastRot = rot;
            _afkTime = 0;
        }
    }

    private static readonly List<string> WhitelistedRanks = 
    [
        "Overlord",
        "Garden Developer",
        "Overseer",
        "Council",
        "Administrator",
        "Moderator"
    ];
}