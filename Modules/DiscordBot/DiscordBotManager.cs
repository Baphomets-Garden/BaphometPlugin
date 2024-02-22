using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading.Tasks;
using UniverseModule.API.Map;
using UniverseModule.API.Player;
using UniverseModule.API.Server;
using UniverseModule.Generic.Core;
using UniverseModule.Generic.Logger;

namespace BaphometPlugin.Modules.DiscordBot;

public static class DiscordBotManager
{
    public static Task BotTask;
    
    public static void StartBot()
    {
        if (!string.IsNullOrEmpty("/home/container/Universe/Plugins/ServerBot/ServerBot"))
        {
            foreach (var process in Process.GetProcesses())
            {
                if (process.ProcessName is "ServerBot")
                {
                    process.Kill();
                }
            }

            ProcessStartInfo p = new("/home/container/Universe/Plugins/ServerBot/ServerBot", (Universe.GetManagedClass<ServerManager>().Port + 2000).ToString() + ' ' + Universe.GetManagedClass<BaphometMain>().Config.BotToken);
            Process.Start(p);
            UniverseLogger.Warning("Status Bot Started!");
        }
        BotTask = Task.Run(() => BotRunner(Universe.GetManagedClass<ServerManager>().Port + 2000, "127.0.0.1"));
    }
    
    private static async Task BotRunner(int port, string ip)
    {
        await Task.Delay(35000);

        try
        {
            TcpClient c = new();
            await c.ConnectAsync(ip, port);

            while (true)
            {
                await c.GetStream().WriteAsync([(byte)Universe.GetManagedClass<PlayerManager>().PlayersAmount, GetStatus()], 0, 2);
                await Task.Delay(20000);
            }
        }
        catch (Exception e)
        {
            UniverseLogger.Error("Error while trying to connect to the bot! " + e.StackTrace);
        }
    }

    private static byte GetStatus()
    {
        return !Universe.GetManagedClass<RoundManager>().RoundIsActive ? (byte)3 : (byte)1;
    }
}