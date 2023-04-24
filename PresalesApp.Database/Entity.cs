using Serilog;
using static PresalesApp.Database.DbController;

namespace PresalesApp.Database
{
    public abstract class Entity
    {
        public int Id { get; set; }

        public virtual void Save()
        {
            var query = new Task(() =>
            {
                using var _dbContext = new ControllerContext();
                if (!this.TryUpdateIfExist(_dbContext)) this.GetOrAddIfNotExist(_dbContext);
                _dbContext.SaveChanges();
                _dbContext.Dispose();
            });

            Queries.Enqueue(query);
            query.Wait();
        }

        internal abstract bool TryUpdateIfExist(ControllerContext dbContext);

        internal abstract Entity GetOrAddIfNotExist(ControllerContext dbContext);

        public abstract override string ToString();

        public virtual void ToLog(bool isNew) => new Task(() =>
        {
            Log.Debug("{0}: {1}", isNew switch { true => "Add", false => "Update" }, this);
        }).Start();
    }
}
