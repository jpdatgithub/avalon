namespace Avalon.Models;

public class Player
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public PlayerRole Role { get; set; } = PlayerRole.Resistance;
}
