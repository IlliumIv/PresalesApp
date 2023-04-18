using static PresalesMonitor.Database.Controller;

namespace PresalesMonitor.Database.Entities.Updates
{
    public class ProjectsUpdate : Update
    {
        public override void Save()
        {
            var query = new Task(() =>
            {
                using var _dbContext = new ReadWriteContext();
                if (!this.TryUpdate(_dbContext)) this.Add(_dbContext);
                _dbContext.SaveChanges();
                _dbContext.Dispose();
            });

            Queries.Enqueue(query);
            query.Wait();
        }

        internal override bool TryUpdate(ReadWriteContext dbContext)
        {
            var update_in_db = dbContext.ProjectsUpdates.SingleOrDefault();
            if (update_in_db != null)
            {
                update_in_db.Timestamp = Timestamp;
                update_in_db.SynchronizedTo = SynchronizedTo;
                update_in_db.ToLog(false);
            }

            return update_in_db != null;
        }

        internal override ProjectsUpdate Add(ReadWriteContext dbContext)
        {
            var update_in_db = dbContext.ProjectsUpdates.SingleOrDefault();
            if (update_in_db == null)
            {
                dbContext.Add(this);
                this.ToLog(true);
                return this;
            }

            return update_in_db;
        }

        public override string ToString() => "{" +
            $"\"ДатаРасчета\":\"{this.Timestamp.ToLocalTime():dd.MM.yyyy HH:mm:ss.fff zzz}\"," +
            $"\"СинхронизированоПо\":\"{this.SynchronizedTo.ToLocalTime():dd.MM.yyyy HH:mm:ss.fff zzz}\"" +
            "}";

        public override ProjectsUpdate GetPrevious()
        {
            using var _dbContext = new ReadOnlyContext();
            var projects_update = _dbContext.ProjectsUpdates.SingleOrDefault() ?? new ProjectsUpdate();
            _dbContext.Dispose();
            return projects_update;
        }
    }
}
