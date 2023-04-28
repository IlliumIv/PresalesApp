using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PresalesApp.Database.DbController;

namespace PresalesApp.Database.Helpers
{
    internal static class EntityListExtensions
    {
        internal static List<T>? Update<T>(this List<T>? itemsA, List<T>? itemsB, ControllerContext dbContext)
            where T : Entity
        {
            if (itemsB == null) return null;
            if (itemsA != null)
                foreach (var item in itemsA)
                {
                    var equal_item = itemsB.FirstOrDefault(i => i.Equals(item));

                    if (equal_item == null)
                    {
                        dbContext.Remove(item);
                        continue;
                    }
                    itemsB.Remove(equal_item);
                    itemsB.Add(item);
                }

            return itemsB;
        }
    }
}
