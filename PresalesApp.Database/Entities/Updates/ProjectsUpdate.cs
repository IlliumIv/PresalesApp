﻿using static PresalesApp.Database.DbController;

namespace PresalesApp.Database.Entities.Updates
{
    public class ProjectsUpdate : Update
    {
        internal override bool TryUpdateIfExist(ControllerContext dbContext)
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

        internal override ProjectsUpdate GetOrAddIfNotExist(ControllerContext dbContext)
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

        public override ProjectsUpdate GetPrevious()
        {
            using var _dbContext = new ReadOnlyContext();
            var projects_update = _dbContext.ProjectsUpdates.SingleOrDefault() ?? new ProjectsUpdate();
            _dbContext.Dispose();
            return projects_update;
        }
    }
}