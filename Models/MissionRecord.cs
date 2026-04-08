namespace Avalon.Models;

public class MissionRecord
{
    public int RoundNumber { get; set; }

    public int RequiredTeamSize { get; set; }

    public int RequiredSabotagesToFail { get; set; } = 1;

    public List<Guid> TeamPlayerIds { get; set; } = new();

    public List<string> TeamPlayerNames { get; set; } = new();

    public int SabotageCount { get; set; }

    public MissionOutcome Outcome { get; set; } = MissionOutcome.Pending;
}
