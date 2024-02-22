using Neuron.Modules.Commands;
using Neuron.Modules.Commands.Command;
using Ninject;
using UniverseModule.API.Command;
using UniverseModule.API.Item;
using UniverseModule.API.Player;
using UniverseModule.Generic.Core;

namespace BaphometPlugin.Modules.AdminTools.Commands;

// ReSharper disable ClassNeverInstantiated.Global

[AutomaticExecution]
[RemoteAdminCommand(
    CommandName = "Ball",
    Aliases = [],
    Description = "Spawns SCP-018 in the selected players position.",
    Permission = "serpents.remoteadmin.commands",
    Platforms = [CommandPlatform.RemoteAdmin, CommandPlatform.ServerConsole],
    Parameters = ["Players", "Amount"]
)]
public class BallCommand : UniverseCommand
{
    [Inject]
    public PlayerManager PlayerManager { get; set; }
    
    public override void Execute(UniverseContext context, ref CommandResult result)
    {
        switch (context.Arguments.Length)
        {
            case < 1:
                result.Response = "Missing Parameters! Usage: ball <players> (<quantity>)!";
                result.StatusCode = CommandStatusCode.Error;
                return;
            case < 2:
                context.Arguments = [context.Arguments[0], "1"];
                break;
        }

        if (!PlayerManager.TryGetPlayers(context.Arguments[0], out var players, context.Player))
        {
            result.Response = "Player not found!";
            result.StatusCode = CommandStatusCode.NotFound;
            return;
        }
        
        if (!int.TryParse(context.Arguments[1], out var amount))
        {
            result.Response = "Invalid amount!";
            result.StatusCode = CommandStatusCode.BadSyntax;
            return;
        }

        foreach (var player in players)
        {
            for (var i = 0; i < amount; i++)
            {
                var item = new UniverseItem(ItemType.SCP018, player.Position);
                item.Throwable.Fuse(player);
            }
        }

        result.Response = "SCP-018 spawned for " + players.Count + " players!";
        result.StatusCode = CommandStatusCode.Ok;
    }
}