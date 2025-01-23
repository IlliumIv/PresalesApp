namespace PresalesApp.Shared;

public partial class Project
{
    // public decimal DPotential => (decimal)Potential;

    // public DateTime DtApprovalByTechDirectorAt => ApprovalByTechDirectorAt.ToDateTime();

    public DateTime LastActionTime => Actions?.LastOrDefault()?.Date.ToDateTime() ?? DateTime.MinValue;
}