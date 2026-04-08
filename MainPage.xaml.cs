using System.Collections.ObjectModel;
using Avalon.Models;
using Avalon.Services;

namespace Avalon;

public partial class MainPage : ContentPage
{
	private readonly GameService _gameService = new();
	private readonly GameSessionService _session = new();
	private readonly ObservableCollection<string> _lobbyPlayers = new();
	private bool _isRoleVisible;

	public MainPage()
	{
		InitializeComponent();

		LobbyPlayersCollection.ItemsSource = _lobbyPlayers;
		TeamPlayersCollection.ItemsSource = _session.State.Players;

		RenderState();
	}

	private async void OnAddPlayerClicked(object? sender, EventArgs e)
	{
		var name = PlayerNameEntry.Text?.Trim() ?? string.Empty;

		if (string.IsNullOrWhiteSpace(name))
		{
			await DisplayAlertAsync("Nome obrigatório", "Digite o nome de um jogador.", "OK");
			return;
		}

		if (_lobbyPlayers.Count >= 10)
		{
			await DisplayAlertAsync("Limite atingido", "Este MVP aceita até 10 jogadores.", "OK");
			return;
		}

		if (_lobbyPlayers.Any(existing => string.Equals(existing, name, StringComparison.OrdinalIgnoreCase)))
		{
			await DisplayAlertAsync("Nome duplicado", "Cada jogador precisa de um nome diferente.", "OK");
			return;
		}

		_lobbyPlayers.Add(name);
		PlayerNameEntry.Text = string.Empty;
		RenderState();
	}

	private void OnClearPlayersClicked(object? sender, EventArgs e)
	{
		_lobbyPlayers.Clear();
		PlayerNameEntry.Text = string.Empty;
		RenderState();
	}

	private async void OnStartGameClicked(object? sender, EventArgs e)
	{
		try
		{
			_session.SetState(_gameService.StartNewGame(_lobbyPlayers));
			_isRoleVisible = false;
			TeamPlayersCollection.ItemsSource = _session.State.Players;
			TeamPlayersCollection.SelectedItems = new List<object>();
			ResetVoteUi();
			RenderState();
		}
		catch (Exception ex)
		{
			await DisplayAlertAsync("Não foi possível iniciar", ex.Message, "OK");
		}
	}

	private void OnShowRoleClicked(object? sender, EventArgs e)
	{
		_isRoleVisible = true;
		RenderState();
	}

	private void OnNextRevealClicked(object? sender, EventArgs e)
	{
		_gameService.AdvanceRoleReveal(_session.State);
		_isRoleVisible = false;
		TeamPlayersCollection.SelectedItems = new List<object>();
		RenderState();
	}

	private void OnTeamSelectionChanged(object? sender, SelectionChangedEventArgs e)
	{
		if (_session.State.Phase != GamePhase.SelectTeam)
		{
			return;
		}

		var required = _gameService.GetRequiredTeamSize(_session.State);
		var selectedCount = GetSelectedPlayers().Count;
		TeamSelectionCountLabel.Text = $"Selecionados: {selectedCount}/{required}";
		ConfirmTeamButton.IsEnabled = selectedCount == required;
	}

	private async void OnConfirmTeamClicked(object? sender, EventArgs e)
	{
		try
		{
			_gameService.StartMission(_session.State, GetSelectedPlayers());
			ResetVoteUi();
			RenderState();
		}
		catch (Exception ex)
		{
			await DisplayAlertAsync("Equipe inválida", ex.Message, "OK");
		}
	}

	private void OnRevealVoteOptionsClicked(object? sender, EventArgs e)
	{
		var currentPlayer = _gameService.GetCurrentMissionVoter(_session.State);
		RevealVoteOptionsButton.IsVisible = false;
		VoteButtonsPanel.IsVisible = true;
		SabotageButton.IsVisible = _gameService.CanSabotage(currentPlayer);
		VotingHintLabel.Text = _gameService.CanSabotage(currentPlayer)
			? "Você é um Sabotador: pode cooperar para blefar ou sabotar a missão."
			: "Você é da Resistência: sua única opção é cooperar.";
	}

	private async void OnCooperateClicked(object? sender, EventArgs e)
	{
		await SubmitMissionVoteAsync(MissionVoteType.Cooperate);
	}

	private async void OnSabotageClicked(object? sender, EventArgs e)
	{
		await SubmitMissionVoteAsync(MissionVoteType.Sabotage);
	}

	private void OnContinueAfterMissionClicked(object? sender, EventArgs e)
	{
		_gameService.AdvanceAfterMissionResult(_session.State);
		TeamPlayersCollection.SelectedItems = new List<object>();
		ResetVoteUi();
		RenderState();
	}

	private void OnRestartGameClicked(object? sender, EventArgs e)
	{
		_session.Reset();
		_isRoleVisible = false;
		TeamPlayersCollection.ItemsSource = _session.State.Players;
		TeamPlayersCollection.SelectedItems = new List<object>();
		ResetVoteUi();
		RenderState();
	}

	private async Task SubmitMissionVoteAsync(MissionVoteType vote)
	{
		try
		{
			_gameService.SubmitMissionVote(_session.State, vote);
			ResetVoteUi();
			RenderState();
		}
		catch (Exception ex)
		{
			await DisplayAlertAsync("Voto inválido", ex.Message, "OK");
		}
	}

	private List<Player> GetSelectedPlayers()
	{
		return TeamPlayersCollection.SelectedItems?.Cast<Player>().ToList() ?? new List<Player>();
	}

	private void ResetVoteUi()
	{
		RevealVoteOptionsButton.IsVisible = true;
		VoteButtonsPanel.IsVisible = false;
		SabotageButton.IsVisible = true;
	}

	private void RenderState()
	{
		var state = _session.State;

		LobbyInfoLabel.Text = $"Jogadores cadastrados: {_lobbyPlayers.Count}/10 (mínimo 5).";
		StartGameButton.IsEnabled = _lobbyPlayers.Count >= 5;
		ScoreLabel.Text = $"Missões aprovadas: {state.SuccessCount} | Missões falhas: {state.FailureCount}";
		StatusLabel.Text = BuildStatusText(state);

		LobbySection.IsVisible = state.Phase == GamePhase.Lobby;
		RevealSection.IsVisible = state.Phase == GamePhase.RevealRoles;
		TeamSelectionSection.IsVisible = state.Phase == GamePhase.SelectTeam;
		MissionVoteSection.IsVisible = state.Phase == GamePhase.MissionVote;
		MissionResultSection.IsVisible = state.Phase == GamePhase.MissionResult;
		GameOverSection.IsVisible = state.Phase == GamePhase.GameOver;

		switch (state.Phase)
		{
			case GamePhase.RevealRoles:
				RenderRevealPhase(state);
				break;
			case GamePhase.SelectTeam:
				RenderTeamSelectionPhase(state);
				break;
			case GamePhase.MissionVote:
				RenderMissionVotePhase(state);
				break;
			case GamePhase.MissionResult:
				RenderMissionResultPhase(state);
				break;
			case GamePhase.GameOver:
				RenderGameOverPhase(state);
				break;
		}
	}

	private string BuildStatusText(GameState state)
	{
		return state.Phase switch
		{
			GamePhase.Lobby => "Adicione entre 5 e 10 jogadores para começar a partida.",
			GamePhase.RevealRoles => $"Revelando identidades ({state.RevealPlayerIndex + 1}/{state.Players.Count}).",
			GamePhase.SelectTeam => $"Rodada {state.RoundNumber}: {state.Players[state.LeaderIndex].Name} escolhe a equipe.",
			GamePhase.MissionVote => "A missão está recebendo votos secretos.",
			GamePhase.MissionResult => "Missão concluída. Veja apenas o resultado agregado.",
			GamePhase.GameOver => "A partida terminou. Você pode começar outra com os mesmos jogadores.",
			_ => string.Empty
		};
	}

	private void RenderRevealPhase(GameState state)
	{
		var currentPlayer = state.Players[state.RevealPlayerIndex];

		RevealPromptLabel.Text = $"Passe o celular para {currentPlayer.Name} e toque somente quando a pessoa estiver pronta.";
		ShowRoleButton.IsVisible = !_isRoleVisible;
		RoleCardFrame.IsVisible = _isRoleVisible;
		NextRevealButton.IsVisible = _isRoleVisible;
		NextRevealButton.Text = state.RevealPlayerIndex == state.Players.Count - 1
			? "Começar a missão 1"
			: "Passar para o próximo jogador";

		RoleNameLabel.Text = currentPlayer.Role == PlayerRole.Resistance ? "Resistência" : "Sabotador";
		RoleNameLabel.TextColor = Color.FromArgb(currentPlayer.Role == PlayerRole.Resistance ? "#1D4ED8" : "#B42318");
		RoleDescriptionLabel.Text = currentPlayer.Role == PlayerRole.Resistance
			? "Seu objetivo é aprovar 3 missões. Durante uma missão você sempre coopera."
			: "Seu objetivo é falhar 3 missões. Durante uma missão você pode cooperar para disfarçar ou sabotar.";
	}

	private void RenderTeamSelectionPhase(GameState state)
	{
		var leader = state.Players[state.LeaderIndex];
		var requiredPlayers = _gameService.GetRequiredTeamSize(state);
		var requiredSabotages = _gameService.GetRequiredSabotagesToFail(state);
		var selectedCount = GetSelectedPlayers().Count;

		LeaderLabel.Text = $"Líder da rodada: {leader.Name}";
		RoundInfoLabel.Text = $"Rodada {state.RoundNumber}: selecione {requiredPlayers} jogador(es) para a missão.";
		SpecialRuleLabel.Text = requiredSabotages > 1
			? "Regra especial: esta missão só falha com 2 sabotagens."
			: "Regra padrão: 1 sabotagem já faz a missão falhar.";
		TeamSelectionCountLabel.Text = $"Selecionados: {selectedCount}/{requiredPlayers}";
		ConfirmTeamButton.IsEnabled = selectedCount == requiredPlayers;
	}

	private void RenderMissionVotePhase(GameState state)
	{
		var currentPlayer = _gameService.GetCurrentMissionVoter(state);
		var teamSize = state.CurrentMission?.TeamPlayerIds.Count ?? 0;

		VotingPlayerLabel.Text = $"Passe o celular para {currentPlayer.Name} ({state.CurrentMissionVoteIndex + 1}/{teamSize} da equipe).";

		if (!VoteButtonsPanel.IsVisible)
		{
			VotingHintLabel.Text = "Toque em 'Estou pronto para votar' quando o jogador estiver sozinho com o aparelho.";
		}
	}

	private void RenderMissionResultPhase(GameState state)
	{
		var mission = state.MissionHistory.Last();

		MissionOutcomeLabel.Text = mission.Outcome == MissionOutcome.Success
			? "✅ Missão aprovada"
			: "❌ Missão falhou";
		MissionOutcomeLabel.TextColor = Color.FromArgb(mission.Outcome == MissionOutcome.Success ? "#0F766E" : "#B42318");
		MissionSummaryLabel.Text = $"Rodada {mission.RoundNumber} • Equipe: {string.Join(", ", mission.TeamPlayerNames)}";
		MissionRuleSummaryLabel.Text = mission.Outcome == MissionOutcome.Success
			? "Nenhuma sabotagem necessária foi detectada para derrubar a missão."
			: mission.RequiredSabotagesToFail > 1
				? $"Foram necessárias {mission.RequiredSabotagesToFail} sabotagens para falhar. Sabotagens detectadas: {mission.SabotageCount}."
				: "Pelo menos uma sabotagem foi detectada.";
		MissionHistoryLabel.Text = BuildMissionHistory(state);
		ContinueAfterMissionButton.Text = "Passar para o próximo líder";
	}

	private void RenderGameOverPhase(GameState state)
	{
		GameOverLabel.Text = state.WinnerMessage;
		FinalHistoryLabel.Text = BuildMissionHistory(state);
	}

	private static string BuildMissionHistory(GameState state)
	{
		if (state.MissionHistory.Count == 0)
		{
			return "Histórico: nenhuma missão concluída ainda.";
		}

		var summary = state.MissionHistory.Select(mission =>
			$"R{mission.RoundNumber}: {(mission.Outcome == MissionOutcome.Success ? "✅" : "❌")} [{string.Join(", ", mission.TeamPlayerNames)}]");

		return $"Histórico: {string.Join(" | ", summary)}";
	}

	private void OnBackClicked(object? sender, EventArgs e)
	{
		_gameService.GoBack(_session.State);
		_isRoleVisible = false;
		ResetVoteUi();
		RenderState();
	}
}
