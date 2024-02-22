using Neuron.Modules.Commands;
using Neuron.Modules.Commands.Command;
using Ninject;
using UniverseModule.API.Command;
using UniverseModule.API.Player;
using UniverseModule.Generic.Config;
using UniverseModule.Generic.Core;

namespace BaphometPlugin.Modules.AdminTools.Commands;

// ReSharper disable ClassNeverInstantiated.Global

[AutomaticExecution]
[RemoteAdminCommand(CommandName = "RemoveJail", Aliases = [], Description = "Removes a player from jail", Parameters = ["Players"], Permission = "serpents.remoteadmin.commands", Platforms = [CommandPlatform.RemoteAdmin])]
public class RemoveJailCommand : UniverseCommand
{
    [Inject]
    public PlayerManager PlayerManager { get; set; }
    
    public override void Execute(UniverseContext context, ref CommandResult result)
    {
        if (context.Arguments.Length < 1)
        {
            result.Response = "Missing Params! Usage: RemoveJail Players";
            result.StatusCode = CommandStatusCode.BadSyntax;
            return;
        }

        if (!PlayerManager.TryGetPlayers(context.Arguments[0], out var players, context.Player))
        {
            result.Response = "No players found!";
            result.StatusCode = CommandStatusCode.BadSyntax;
            return;
        }

        foreach (var player in players)
        {
            if (!player.Data.ContainsKey("jail")) continue;
            if (player.Data["jail"] is not SerializedPlayerState state) continue;
            player.State = state;
            player.Data.Remove("jail");
            JailCommand.JailedPlayers.Remove(player);
        }

        result.Response = "Player(s) removed from jail!";
        result.StatusCode = CommandStatusCode.Ok;
    }
}