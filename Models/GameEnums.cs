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
    Assassin,
    Percival,
    Morgana,
    Mordred,
    Oberon
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
