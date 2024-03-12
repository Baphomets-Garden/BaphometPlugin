using Neuron.Modules.Commands;
using Neuron.Modules.Commands.Command;
using Ninject;
using UniverseModule.API.Command;
using UniverseModule.API.Player;
using UniverseModule.Generic.Core;

namespace BaphometPlugin.Modules.PointSystem;

// ReSharper disable ClassNeverInstantiated.Global

[AutomaticExecution]
[RemoteAdminCommand(CommandName = "GivePoints", Aliases = [], Description = "Give Mvp Points to a specific player", Parameters = ["Player", "Points Amount"], Permission = "baphomet.remoteadmin.points", Platforms = [CommandPlatform.RemoteAdmin])]
public class GivePointsCommand : UniverseCommand
{
    [Inject]
    public PlayerManager PlayerManager { get; set; }
    
    [Inject]
    public PointSystemManager PointSystem { get; set; }
    
    public override void Execute(UniverseContext context, ref CommandResult result)
    {
        if (context.Arguments.Length < 2)
        {
            result.Response = "You need to specify a player and a points amount!";
            result.StatusCode = CommandStatusCode.BadSyntax;
            return;
        }
        
        var player = PlayerManager.GetPlayer(context.Arguments[0]);
        
        if (player is null)
        {
            result.Response = "The player was not found!";
            result.StatusCode = CommandStatusCode.Error;
            return;
        }

        if (!uint.TryParse(context.Arguments[1], out var points))
        {
            result.Response = "The points amount is not valid!";
            result.StatusCode = CommandStatusCode.Error;
            return;
        }

        PointSystem.HandlePoints(player, true, points);
        
        result.Response = $"You gave {points} points to {player.NickName}!";
        result.StatusCode = CommandStatusCode.Ok;
    }
}