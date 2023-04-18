using Serilog;
using static PresalesMonitor.Database.Controller;

namespace PresalesMonitor.Database
{
    public abstract class Entity
    {
        public int Id { get; set; }

        public abstract void Save();

        internal abstract bool TryUpdate(ReadWriteContext dbContext);

        internal abstract Entity Add(ReadWriteContext dbContext);

        public abstract override string ToString();

        public void ToLog(bool isNew) => new Task(() =>
        {
            Log.Debug("{0}: {1}", isNew switch { true => "Add", false => "Update" }, this);
        }).Start();
    }
}
