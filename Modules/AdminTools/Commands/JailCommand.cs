using System.Collections.Generic;
using Neuron.Modules.Commands;
using Neuron.Modules.Commands.Command;
using Ninject;
using PlayerRoles;
using UnityEngine;
using UniverseModule.API.Command;
using UniverseModule.API.Map.Rooms;
using UniverseModule.API.Player;
using UniverseModule.Generic.Config;
using UniverseModule.Generic.Core;

namespace BaphometPlugin.Modules.AdminTools.Commands;

// ReSharper disable ClassNeverInstantiated.Global

[AutomaticExecution]
[RemoteAdminCommand(CommandName = "Jail", Aliases = [], Description = "Jails a player", Parameters = ["Players"], Permission = "serpents.remoteadmin.commands", Platforms = [CommandPlatform.RemoteAdmin])]
public class JailCommand : UniverseCommand
{
    [Inject]
    public PlayerManager PlayerManager { get; set; }
    
    internal static readonly List<UniversePlayer> JailedPlayers = [];

    public override void Execute(UniverseContext context, ref CommandResult result)
    {
        if (context.Arguments.Length < 1)
        {
            result.Response = "Missing Parameters! Usage: Jail <player(s)>";
            result.StatusCode = CommandStatusCode.BadSyntax;
            return;
        }
        
        if (!PlayerManager.TryGetPlayers(context.Arguments[0], out var players, context.Player))
        {
            result.Response = "Player Not Found!";
            result.StatusCode = CommandStatusCode.BadSyntax;
            return;
        }
        
        var amount = 1;
        
        if (context.Arguments.Length > 1 && int.TryParse(context.Arguments[1], out var newAmount))
        {
            if (newAmount <= 0)
                newAmount = 1;

            amount = newAmount > JailStates.Count ? JailStates.Count : newAmount;
        }
        
        foreach (var player in players)
        {
            if (!player.Data.ContainsKey("jail"))
                player.Data["jail"] = player.State;
            
            player.State = JailStates[amount - 1];
            JailedPlayers.Add(player);
        }

        result.Response = "Player(s) Jailed!";
        result.StatusCode = CommandStatusCode.Ok;
    }
    
    private static List<SerializedPlayerState> JailStates { get; set; } =
    [
        new SerializedPlayerState
        {
            Position = new RoomPoint("Surface", new Vector3(38.66787f, 14.80164f, -32.61194f), Vector3.zero),
            RoleType = RoleTypeId.Tutorial,
            RoleID = (uint) RoleTypeId.Tutorial,
        }
    ];
}