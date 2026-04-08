namespace Avalon.Models;

public class GameState
{
    public GamePhase Phase { get; set; } = GamePhase.Lobby;

    public List<Player> Players { get; set; } = new();

    public List<MissionRecord> MissionHistory { get; set; } = new();

    public MissionRecord? CurrentMission { get; set; }

    public int RoundNumber { get; set; } = 1;

    public int LeaderIndex { get; set; }

    public int RevealPlayerIndex { get; set; }

    public int CurrentMissionVoteIndex { get; set; }

    public int SuccessCount { get; set; }

    public int FailureCount { get; set; }

    public bool UseMerlin { get; set; } = true;

    public bool UseAssassin { get; set; } = true;

    public string WinnerMessage { get; set; } = string.Empty;

    public bool Merlin { get; set; } = true;

    public bool Assassin { get; set; } = true;
}
