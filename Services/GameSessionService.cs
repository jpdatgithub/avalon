using Avalon.Models;

namespace Avalon.Services;

public class GameSessionService
{
    public GameState State { get; private set; } = new();

    public void SetState(GameState state)
    {
        State = state;
    }

    public void Reset()
    {
        State = new GameState
        {
            UseAssassinMerlin = State.UseAssassinMerlin,
            UsePercivalMorgana = State.UsePercivalMorgana,
            UseMordred = State.UseMordred,
            UseOberon = State.UseOberon
        };
    }
}
