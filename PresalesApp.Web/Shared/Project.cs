using Google.Protobuf.WellKnownTypes;

namespace PresalesApp.Web.Shared
{
    public partial class Project
    {
        public decimal dPotential => (decimal)Potential;

        public DateTime dtApprovalByTechDirectorAt => ApprovalByTechDirectorAt.ToDateTime();

        public DateTime dtLastActionTime => Actions?.LastOrDefault()?.dtDate ?? DateTime.MinValue;
    }
}
