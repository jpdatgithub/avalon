using Avalon.Models;

namespace Avalon.Services;

public class GameService
{
    private readonly Random _random = new();

    public GameState StartNewGame(IEnumerable<string> playerNames)
    {
        var names = playerNames
            .Select(name => name.Trim())
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (names.Count < 5 || names.Count > 10)
        {
            throw new InvalidOperationException("Adicione entre 5 e 10 jogadores para começar.");
        }

        var players = names
            .Select(name => new Player { Name = name })
            // .OrderBy(_ => _random.Next())
            .ToList();

        AssignRoles(players);

        return new GameState
        {
            Phase = GamePhase.RevealRoles,
            Players = players,
            RevealPlayerIndex = 0,
            LeaderIndex = 0,
            RoundNumber = 1,
            SuccessCount = 0,
            FailureCount = 0,
            CurrentMissionVoteIndex = 0,
            WinnerMessage = string.Empty
        };
    }

    public void AdvanceRoleReveal(GameState state)
    {
        if (state.Phase != GamePhase.RevealRoles)
        {
            return;
        }

        if (state.RevealPlayerIndex < state.Players.Count - 1)
        {
            state.RevealPlayerIndex++;
            return;
        }

        state.Phase = GamePhase.SelectTeam;
    }

    public int GetRequiredTeamSize(GameState state)
    {
        var roundIndex = Math.Clamp(state.RoundNumber, 1, 5) - 1;

        return state.Players.Count switch
        {
            5 => new[] { 2, 3, 2, 3, 3 }[roundIndex],
            6 => new[] { 2, 3, 4, 3, 4 }[roundIndex],
            7 => new[] { 2, 3, 3, 4, 4 }[roundIndex],
            _ => new[] { 3, 4, 4, 5, 5 }[roundIndex]
        };
    }

    public int GetRequiredSabotagesToFail(GameState state)
    {
        return state.Players.Count >= 7 && state.RoundNumber == 4 ? 2 : 1;
    }

    public void StartMission(GameState state, IEnumerable<Player> selectedPlayers)
    {
        if (state.Phase != GamePhase.SelectTeam)
        {
            throw new InvalidOperationException("Agora não é hora de selecionar a equipe.");
        }

        var team = selectedPlayers
            .DistinctBy(player => player.Id)
            .ToList();

        var requiredTeamSize = GetRequiredTeamSize(state);

        if (team.Count != requiredTeamSize)
        {
            throw new InvalidOperationException($"Selecione exatamente {requiredTeamSize} jogador(es).");
        }

        state.CurrentMission = new MissionRecord
        {
            RoundNumber = state.RoundNumber,
            RequiredTeamSize = requiredTeamSize,
            RequiredSabotagesToFail = GetRequiredSabotagesToFail(state),
            TeamPlayerIds = team.Select(player => player.Id).ToList(),
            TeamPlayerNames = team.Select(player => player.Name).ToList(),
            SabotageCount = 0,
            Outcome = MissionOutcome.Pending
        };

        state.CurrentMissionVoteIndex = 0;
        state.Phase = GamePhase.MissionVote;
    }

    public Player GetCurrentMissionVoter(GameState state)
    {
        if (state.CurrentMission is null)
        {
            throw new InvalidOperationException("Nenhuma missão em andamento.");
        }

        var currentPlayerId = state.CurrentMission.TeamPlayerIds[state.CurrentMissionVoteIndex];
        return state.Players.First(player => player.Id == currentPlayerId);
    }

    public bool CanSabotage(Player player)
    {
        return player.Role == PlayerRole.Spy;
    }

    public void SubmitMissionVote(GameState state, MissionVoteType vote)
    {
        if (state.Phase != GamePhase.MissionVote || state.CurrentMission is null)
        {
            throw new InvalidOperationException("Nenhuma votação de missão está ativa.");
        }

        var currentPlayer = GetCurrentMissionVoter(state);

        if (currentPlayer.Role == PlayerRole.Resistance && vote == MissionVoteType.Sabotage)
        {
            throw new InvalidOperationException("Jogadores da Resistência só podem cooperar.");
        }

        if (vote == MissionVoteType.Sabotage)
        {
            state.CurrentMission.SabotageCount++;
        }

        if (state.CurrentMissionVoteIndex < state.CurrentMission.TeamPlayerIds.Count - 1)
        {
            state.CurrentMissionVoteIndex++;
            return;
        }

        FinalizeMission(state);
    }

    public void AdvanceAfterMissionResult(GameState state)
    {
        if (state.SuccessCount >= 3 || state.FailureCount >= 3)
        {
            state.Phase = GamePhase.GameOver;
            return;
        }

        state.RoundNumber++;
        state.LeaderIndex = (state.LeaderIndex + 1) % state.Players.Count;
        state.CurrentMission = null;
        state.CurrentMissionVoteIndex = 0;
        state.Phase = GamePhase.SelectTeam;
    }

    private void FinalizeMission(GameState state)
    {
        var mission = state.CurrentMission!;
        mission.Outcome = mission.SabotageCount >= mission.RequiredSabotagesToFail
            ? MissionOutcome.Failure
            : MissionOutcome.Success;

        state.MissionHistory.Add(mission);

        if (mission.Outcome == MissionOutcome.Success)
        {
            state.SuccessCount++;
        }
        else
        {
            state.FailureCount++;
        }

        state.WinnerMessage = state.SuccessCount >= 3
            ? "A Resistência venceu com 3 missões aprovadas."
            : state.FailureCount >= 3
                ? "Os Sabotadores venceram com 3 missões fracassadas."
                : string.Empty;

        state.Phase = state.SuccessCount >= 3 || state.FailureCount >= 3
            ? GamePhase.GameOver
            : GamePhase.MissionResult;
    }

    private void AssignRoles(List<Player> players)
    {
        var spyCount = players.Count switch
        {
            <= 6 => 2,
            <= 9 => 3,
            _ => 4
        };

        var spyIndexes = players
            .Select((player, index) => index)
            .OrderBy(_ => _random.Next())
            .Take(spyCount)
            .ToHashSet();

        for (var index = 0; index < players.Count; index++)
        {
            players[index].Role = spyIndexes.Contains(index) ? PlayerRole.Spy : PlayerRole.Resistance;
        }
    }

    public void GoBack(GameState state)
    {
        switch (state.Phase)
        {
            case GamePhase.RevealRoles:
                if (state.RevealPlayerIndex > 0)
                {
                    state.RevealPlayerIndex--;
                }
                else
                {
                    state.Phase = GamePhase.Lobby;
                }
                break;

            case GamePhase.SelectTeam:
                if (state.MissionHistory.Count > 0)
                {
                    RevertLastCompletedMission(state, fromAdvancedSelectTeam: true);
                }
                else
                {
                    state.Phase = GamePhase.RevealRoles;
                }
                break;

            case GamePhase.MissionVote:
                state.CurrentMission = null;
                state.CurrentMissionVoteIndex = 0;
                state.Phase = GamePhase.SelectTeam;
                break;

            case GamePhase.MissionResult:
                RevertLastCompletedMission(state, fromAdvancedSelectTeam: false);
                break;

            case GamePhase.GameOver:
                if (state.MissionHistory.Count > 0)
                {
                    RevertLastCompletedMission(state, fromAdvancedSelectTeam: false);
                }
                else
                {
                    state.Phase = GamePhase.Lobby;
                }
                break;
        }
    }

    private static void RevertLastCompletedMission(GameState state, bool fromAdvancedSelectTeam)
    {
        if (state.MissionHistory.Count == 0)
        {
            state.CurrentMission = null;
            state.CurrentMissionVoteIndex = 0;
            state.Phase = GamePhase.SelectTeam;
            return;
        }

        var lastMission = state.MissionHistory[^1];
        state.MissionHistory.RemoveAt(state.MissionHistory.Count - 1);

        if (lastMission.Outcome == MissionOutcome.Success && state.SuccessCount > 0)
        {
            state.SuccessCount--;
        }
        else if (lastMission.Outcome == MissionOutcome.Failure && state.FailureCount > 0)
        {
            state.FailureCount--;
        }

        state.RoundNumber = Math.Max(1, lastMission.RoundNumber);

        if (fromAdvancedSelectTeam && state.Players.Count > 0)
        {
            state.LeaderIndex = (state.LeaderIndex - 1 + state.Players.Count) % state.Players.Count;
        }

        state.CurrentMission = null;
        state.CurrentMissionVoteIndex = 0;
        state.WinnerMessage = string.Empty;
        state.Phase = GamePhase.SelectTeam;
    }
}
