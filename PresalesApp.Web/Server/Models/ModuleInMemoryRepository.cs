using PresalesApp.DistanceCalculator;

namespace PresalesApp.Web.Server.Models;

public class ModuleInMemoryRepository : IModuleRepository
{
    private static readonly IList<Module> _Modules;

    static ModuleInMemoryRepository() => _Modules =
        [
            new () { Id = 1, Name = "Оставленные предметы" },
            new () { Id = 1, Name = "ГРЗ" }
        ];

    public IEnumerable<Module> GetModules() => _Modules;
}
