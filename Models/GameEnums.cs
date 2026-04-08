namespace Avalon.Models;

public enum GamePhase
{
    Lobby,
    RevealRoles,
    SelectTeam,
    MissionVote,
    MissionResult,
    GameOver
}

public enum PlayerRole
{
    Resistance,
    Merlin,
    Spy,
    Assassin
}

public enum MissionVoteType
{
    Cooperate,
    Sabotage
}

public enum MissionOutcome
{
    Pending,
    Success,
    Failure
}
