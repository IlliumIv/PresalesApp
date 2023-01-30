using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Newtonsoft.Json;
using PresalesMonitor.Entities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PresalesMonitor
{
    public static class DbMock
    {
        private static readonly DbController.Context writeContext = new();
        private static readonly Queue<Task> _writeQueue = new();

        public static void WriteQueueRun()
        {
            while (true)
                if (_writeQueue.Count > 0)
                    _writeQueue.Dequeue()?.RunSynchronously();
        }

        public static async void Proceed<T>(T item) where T : notnull
            // => DbMock.AddTaskToWriteQueue(DbMock.CreateTaskToWrite(item).Result);
        {
            if (Settings.TryGetSection<Settings.Application>(
                out ConfigurationSection? r) && r != null)
            {
                Settings.Application appSettings = (Settings.Application)r;

                var log = typeof(T).Name switch
                {
                    "Project" => await ProceedProject(item as Project),
                    "Invoice" => await ProceedInvoice(item as Invoice),
                    _ => throw new NotImplementedException()
                };

                if (appSettings.Debug && !string.IsNullOrEmpty(log))
                {
                    File.AppendAllTextAsync(Synchronizer._workLog.FullName, $"[{DateTime.Now:dd.MM.yyyy HH:mm:ss.fff}] {log}\n\n").Start();
                }
            }
            else throw new ConfigurationErrorsException();
        }

        private static async Task<string> ProceedProject(Project project)
        {
            string log = string.Empty;
            var proj = await GetProjectAsync(project);
            if (proj == null) log += $"\tAdd: ";
            else log += $"\tUpdate: ";

            log += $"\"Код\":\"{project.Number}\"," +
                $"\"Наименование\":\"{project.Name}\"," +
                $"\"Потенциал\":\"{project.PotentialAmount}\"," +
                $"\"Статус\":\"{project.Status}\"," +
                $"\"ПричинаПроигрыша\":\"{project.LossReason}\"," +
                $"\"ПлановаяДатаОкончанияТек\":\"{project.PotentialWinAt}\"," +
                $"\"ДатаОкончания\":\"{project.ClosedAt}\"," +
                $"\"ДатаСогласованияРТС\":\"{project.ApprovalByTechDirectorAt}\"," +
                $"\"ДатаСогласованияРОП\":\"{project.ApprovalBySalesDirectorAt}\"," +
                $"\"ДатаНачалаРаботыПресейла\":\"{project.PresaleStartAt}\"," +
                $"\"ДействияПресейла\":\"{project.Actions?.Count}\"," +
                $"\"Пресейл\":\"{project.Presale?.Name}\"," +
                $"\"ОсновнойПроект\":\"{project.MainProject?.Name}\"";

            return log;
        }

        private static async Task<string> ProceedInvoice(Invoice invoice)
        {
            string log = string.Empty;
            var inv = await GetInvoiceAsync(invoice);
            if (inv == null) log += $"\tAdd: ";
            else log += $"\tUpdate: ";

            string profitPeriods = string.Empty;
            if (invoice.ProfitPeriods.Count > 0)
            {
                foreach (var p in invoice.ProfitPeriods)
                    profitPeriods += $"\"{p.StartTime}\":\"{p.Amount}\",";
                profitPeriods = profitPeriods.Remove(profitPeriods.Length - 1);
            }

            log += $"\"Номер\":\"{invoice.Number}\"," +
                $"\"Дата\":\"{invoice.Date}\"," +
                $"\"Контрагент\":\"{invoice.Counterpart}\"," +
                $"\"Проект\":\"{invoice.Project?.Number}\"," +
                $"\"СуммаРуб\":\"{invoice.Amount}\"," +
                $"\"ДатаПоследнейОплаты\":\"{invoice.LastPayAt}\"," +
                $"\"ДатаПоследнейОтгрузки\":\"{invoice.LastShipmentAt}\"," +
                $"\"Пресейл\":\"{invoice.Presale?.Name}\"," +
                $"\"Суммарная прибыль за периоды\":[{profitPeriods}]";

            return log;
        }

        public static void AddTaskToWriteQueue(Task task) => _writeQueue.Enqueue(task);

        /*
        public static async Task<Task> CreateTaskToWrite<T>(T item) where T : notnull
        {
            return typeof(T).Name switch
            {
                "Project" => await Insert(item as Project),
                "Invoice" => await Insert(item as Invoice),
                _ => throw new NotImplementedException()
            };
        }
        //*/


        private static async Task<Task> Insert(Project project)
        {
            var proj = await GetProjectAsync(project);
            if (proj == null) return new(() => Console.WriteLine($"Project {project.Number} added!"));
            else return new(() => Console.WriteLine($"Project {project.Number} alredy exist\tId:\t{proj.ProjectId}"));
        }

        private static async Task<Task> Insert(Invoice invoice)
        {
            var inv = await GetInvoiceAsync(invoice);
            if (inv == null) return new(() => Console.WriteLine($"Invoice {invoice.Number} added!"));
            else return new(() => Console.WriteLine($"Invoice {invoice.Number} alredy exist\tId:\t{inv.InvoiceId}"));
        }

        public static async Task<Project?> GetProjectAsync(Project? project)
        {
            if (project == null) return null;
            using var context = new DbController.Context();
            return await context.Projects.SingleOrDefaultAsync(p => p.Number == project.Number);
        }

        public static async Task<Invoice?> GetInvoiceAsync(Invoice? invoice)
        {
            if (invoice == null) return null;
            using var context = new DbController.Context();
            return await context.Invoices.Include(i => i.ProfitPeriods)
                .SingleOrDefaultAsync(
                    i => i.Number == invoice.Number && i.Date.Date == invoice.Date.Date
                    || i.Number == invoice.Number && i.Counterpart == invoice.Counterpart);
        }
    }
}
