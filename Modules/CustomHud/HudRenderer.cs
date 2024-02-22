using System.Collections.Generic;
using System.Threading.Tasks;
using Hints;
using NorthwoodLib.Pools;
using PlayerRoles;
using UnityEngine;
using UniverseModule;
using UniverseModule.API.Player;

namespace BaphometPlugin.Modules.CustomHud;

public class HudRenderer : MonoBehaviour
{
    private readonly Dictionary<ScreenZone, float> _timers = new() { [ScreenZone.Top] = -1, [ScreenZone.CenterTop] = -1, [ScreenZone.Center] = -1, [ScreenZone.CenterBottom] = -1, [ScreenZone.Bottom] = -1 , [ScreenZone.InteractionMessage] = -1 , [ScreenZone.CompletelyBottom] = -1 };
    private readonly Dictionary<ScreenZone, string> _messages = new() { [ScreenZone.Top] = string.Empty, [ScreenZone.CenterTop] = string.Empty, [ScreenZone.Center] = string.Empty, [ScreenZone.CenterBottom] = string.Empty, [ScreenZone.Bottom] = string.Empty , [ScreenZone.InteractionMessage] = string.Empty , [ScreenZone.CompletelyBottom] = string.Empty };
    
    private float _counter;
    private UniversePlayer _player;
    
    private HudBuilder _mainDisplay;
    
    private void Start()
    {
        _player = gameObject.GetUniversePlayer();

        _mainDisplay = new HudBuilder(StringBuilderPool.Shared.Rent());
        _mainDisplay.WithName($"{_player.NickName.ToLower()}");
    }
    
    private void OnDestroy()
    {
        _mainDisplay = null;
    }
    
    private void Update()
    {
        _counter += Time.deltaTime;

        if (_counter < .5f)
            return;
        
        DrawHud();

        _counter = 0;
    }
    
    private async void DrawHud()
    {
        var msg = await Task.Run(() =>
        {
            UpdateMessage();
            UpdateNotifications();
                
            return _player.RoleType.GetTeam() == Team.Dead ? _mainDisplay.CreateHudForSpectators() : _mainDisplay.CreateHudForHuman(_player);
        });

        _player.Connection.Send(new HintMessage(new TextHint(msg, [new StringHintParameter(string.Empty)], null, 2)));
    }
    
    public void AddMessage(ScreenZone zone, string message, float time = 10f)
    {
        if (zone == ScreenZone.Notifications)
        {
            var hud = new HudNotification(message)
            {
                Duration = time
            };
            _notifications.Add(hud);
            return;
        }
        
        _messages[zone] = message;
        _timers[zone] = time;
    }
    
    public void ClearZone(ScreenZone zone)
    {
        if (zone == ScreenZone.Notifications)
        {
            _notifications.Clear();
            return;
        }
        
        _messages[zone] = string.Empty;
        _timers[zone] = -1;
    }
    
    private void UpdateMessage()
    {
        var color = _player.RoleManager.CurrentRole.RoleColor.ToHex();
        
        _mainDisplay.Clear();
        _mainDisplay.WithColor(color);

        for (var i = 0; i < _timers.Count; i++)
        {
            var zone = (ScreenZone)i;

            if (_timers[zone] >= 0)
                _timers[zone] -= 0.5f;

            if (_timers[zone] < 0)
                _messages[zone] = string.Empty;

            var message = _messages[zone].TrimEnd('\n');
                
            if (string.IsNullOrEmpty(message))
                message = '\n' + message;
            
            _mainDisplay.WithContent(zone, message);
        }
    }
    
    private readonly List<HudNotification> _notifications = [ ];
    
    private void UpdateNotifications()
    {
        List<string> queue = [ ];

        for (var i = 0; i < (_notifications.Count > 5 ? 6 : _notifications.Count); i++)
        {
            queue.Add(_notifications[i].Message);
            _notifications[i].Duration -= 0.5f;

            if (_notifications[i].Duration <= 0)
                _notifications.Remove(_notifications[i]);
        }

        _mainDisplay.WithNotifications(queue);
    }
}