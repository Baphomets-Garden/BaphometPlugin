using UniverseModule.API.Player;

namespace BaphometPlugin.Modules.AdminTools.DiscordLogs.BanLogger;

public class BanInfo(UniversePlayer issuer, UniversePlayer target, string reason, long duration)
{
    public string IssuerName { get; } = issuer?.NickName ?? "Console";
    public string IssuerId { get; } = issuer?.UserId ?? "Console";
    public string BannedName { get; } = target.NickName;
    public string BannedId { get; } = target.UserId;
    public string Reason { get; } = reason;
    public long Duration { get; } = duration;
}