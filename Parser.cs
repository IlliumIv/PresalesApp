using Newtonsoft.Json;
using PresalesStatistic.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PresalesStatistic
{
    public class Parser
    {
        private DateTime _lastUpdated = DateTime.MinValue;
        public void GetUpdate()
        {
            _lastUpdated = DateTime.Now;

            var files = new string[2]
            {
                @"C:\Projects_TestData.txt",
                @"C:\Invoices_TestData.txt"
            };

            for (int i = 0; i <= 1; i++)
            {
                var rawData = File.ReadAllText(files[i]);
                var jsonObjects = JsonConvert.DeserializeObject<dynamic>(rawData);
                if (jsonObjects == null) return;

                foreach (var obj in jsonObjects)
                {
                    if (i == 0)
                    {
                        Project _project = obj.ToObject<Project>();
                        Console.WriteLine(JsonConvert.SerializeObject(_project, Formatting.Indented));
                    }
                    if (i == 1)
                    {
                        Invoice _invoice = obj.ToObject<Invoice>();
                        Console.WriteLine(JsonConvert.SerializeObject(_invoice, Formatting.Indented));
                    }
                }
            }
        }
    }
}
