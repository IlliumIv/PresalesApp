using PresalesApp.DistanceCalculator;

namespace PresalesApp.Web.Server.Models;

public interface IModuleRepository
{
    IEnumerable<Module> GetModules();
}
