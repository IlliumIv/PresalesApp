﻿using Serilog;
using Serilog.Templates;
using Serilog.Templates.Themes;
using System.Reflection;

namespace PresalesApp.Bridge1C.Startup
{
    public static class SerilogConfiguration
    {
        public static void ConfigureLogger()
        {
            var logTemplate = "[{@l:u3}] [{@t:dd.MM.yyyy HH:mm:ss.fff} ThreadId={ThreadId}, ProcessId={ProcessId}] [{SourceContext}]\n" +
                "      {#if @m <> ''}{@m}\n{#end}{@x}\n";
            var logDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ??
                Directory.GetCurrentDirectory();
            logDirectory += "/Logs/";

            Log.Logger = new LoggerConfiguration()
                // https://github.com/serilog/serilog/wiki/Enrichment
                .Enrich.FromLogContext()
                .Enrich.WithThreadId() // Serilog.Enrichers.Thread
                .Enrich.WithProcessId() // Serilog.Enrichers.Process
                                        // .Enrich.WithMachineName() // Serilog.Enrichers.Environment
                .Destructure.ToMaximumDepth(4)
                .Destructure.ToMaximumStringLength(100)
                .Destructure.ToMaximumCollectionCount(10)
                .MinimumLevel.Debug()
                .WriteTo.Async(a => a.Console(
                    formatter: new ExpressionTemplate(logTemplate, theme: TemplateTheme.Code),
                    restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Verbose))
                .WriteTo.Async(a => a.File(
                    formatter: new ExpressionTemplate(logTemplate),
                    restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information,
                    path: $"{logDirectory}{typeof(Program).Assembly.GetName().Name}.log"))
                .WriteTo.Async(a => a.File(
                    formatter: new ExpressionTemplate(logTemplate),
                    restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error,
                    path: $"{logDirectory}Error.log"))
                .CreateLogger();
        }
    }
}