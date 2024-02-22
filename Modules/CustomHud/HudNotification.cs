namespace BaphometPlugin.Modules.CustomHud;

public class HudNotification(string message)
{
    public readonly string Message = message;
    
    public float Duration = 6f;
}