using Neuron.Modules.Commands;
using Neuron.Modules.Commands.Command;
using Ninject;
using UnityEngine;
using UniverseModule.API.Command;
using UniverseModule.API.Player;
using UniverseModule.Generic.Core;

namespace BaphometPlugin.Modules.AdminTools.Commands;

// ReSharper disable ClassNeverInstantiated.Global

[AutomaticExecution]
[RemoteAdminCommand(
    CommandName = "Size",
    Aliases = ["Scale"],
    Description = "Changes the Size of the selected players.",
    Permission = "baphomet.remoteadmin.commands",
    Platforms = [CommandPlatform.RemoteAdmin, CommandPlatform.ServerConsole],
    Parameters = ["Players", "X Size", "Y Size", "Z Size"]
)]
public class SizeCommand : UniverseCommand
{
    [Inject]
    public PlayerManager PlayerManager { get; set; }
    
    public override void Execute(UniverseContext context, ref CommandResult result)
    {
        if (context.Arguments.Length < 4)
        {
            result.Response = "Missing Parameters! size <player(s)> <x> <y> <z>";
            result.StatusCode = CommandStatusCode.BadSyntax;
            return;
        }

        if (!PlayerManager.TryGetPlayers(context.Arguments[0], out var players, context.Player))
        {
            result.Response = "Player Not Found!";
            result.StatusCode = CommandStatusCode.NotFound;
            return;
        }

        if (!float.TryParse(context.Arguments[1], out var x))
        {
            result.Response = "Invalid Argument For X";
            result.StatusCode = CommandStatusCode.BadSyntax;
            return;
        }

        if (!float.TryParse(context.Arguments[2], out var y))
        {
            result.Response = "Invalid Argument For Y";
            result.StatusCode = CommandStatusCode.BadSyntax;
            return;
        }

        if (!float.TryParse(context.Arguments[3], out var z))
        {
            result.Response = "Invalid Argument For Z";
            result.StatusCode = CommandStatusCode.BadSyntax;
            return;
        }

        foreach (var player in players)
            player.Scale = new Vector3(x, y, z);

        result.Response = "Size changed successfully!";
        result.StatusCode = CommandStatusCode.Ok;
    }
}