using System.Configuration;

namespace PresalesMonitor
{
    public static class Settings
    {
        public sealed class Application : ConfigurationSection
        {
            [ConfigurationProperty("projectsUpdatedAt",
                DefaultValue = "2022-09-20T00:00:00",
                IsRequired = true)]
            public DateTime ProjectsUpdatedAt
            {
                get => (DateTime)this["projectsUpdatedAt"];
                set => this["projectsUpdatedAt"] = value;
            }

            [ConfigurationProperty("invoicesUpdatedAt",
                DefaultValue = "2022-09-20T00:00:00",
                IsRequired = true)]
            public DateTime InvoicesUpdatedAt
            {
                get => (DateTime)this["invoicesUpdatedAt"];
                set => this["invoicesUpdatedAt"] = value;
            }

            [ConfigurationProperty("debug",
                DefaultValue = "false",
                IsRequired = true)]
            public bool Debug
            {
                get => (bool)this["debug"];
                set => this["debug"] = value;
            }
        }
        public sealed class Database : ConfigurationSection
        {
            [ConfigurationProperty("url",
                DefaultValue = "192.168.200.51",
                IsRequired = true)]
            public string Url
            {
                get => (string)this["url"];
                set => this["url"] = value;
            }

            [ConfigurationProperty("port",
                DefaultValue = (int)5432,
                IsRequired = true)]
            [IntegerValidator(MinValue = 0,
                MaxValue = 65536, ExcludeRange = false)]
            public int Port
            {
                get => (int)this["port"];
                set => this["port"] = value;
            }

            [ConfigurationProperty("databaseName",
                DefaultValue = "presalesDb",
                IsRequired = true)]
            public string DatabaseName
            {
                get => (string)this["databaseName"];
                set => this["databaseName"] = value;
            }

            [ConfigurationProperty("username",
                DefaultValue = "presales",
                IsRequired = true)]
            public string Username
            {
                get => (string)this["username"];
                set => this["username"] = value;
            }

            [ConfigurationProperty("password",
                DefaultValue = "12345",
                IsRequired = true)]
            public string Password
            {
                get => (string)this["password"];
                set => this["password"] = value;
            }
        }
        public sealed class Connection : ConfigurationSection
        {
            [ConfigurationProperty("url",
                DefaultValue = "127.0.0.1",
                IsRequired = true)]
            public string Url
            {
                get => (string)this["url"];
                set => this["url"] = value;
            }

            [ConfigurationProperty("username",
                DefaultValue = "***REMOVED***",
                IsRequired = true)]
            public string Username
            {
                get => (string)this["username"];
                set => this["username"] = value;
            }

            [ConfigurationProperty("password",
                DefaultValue = "***REMOVED***",
                IsRequired = true)]
            public string Password
            {
                get => (string)this["password"];
                set => this["password"] = value;
            }
        }

        private static readonly Configuration _configuration = ConfigurationManager
                .OpenExeConfiguration(ConfigurationUserLevel.None);
        public static void CreateConfigurationFile()
        {
            try
            {
                var sections = typeof(Settings).GetNestedTypes();
                foreach (var section in sections)
                {
                    var s = Activator.CreateInstance(section);
                    if (_configuration.Sections[section.Name] == null
                        && s != null)
                    {
                        var configurationSection = (ConfigurationSection)s;
                        _configuration.Sections.Add(section.Name, configurationSection);
                        configurationSection.SectionInformation.ForceSave = true;
                    }
                }
                _configuration.Save(ConfigurationSaveMode.Full);
            }
            catch (ConfigurationErrorsException e)
            {
                Console.WriteLine("CreateConfigurationFile: {0}", e.ToString());
            }
        }
        public static bool TryGetSection<T>(out ConfigurationSection result)
        {
            result = _configuration.Sections[typeof(T).Name];
            return result != null;
        }
        public static bool ConfigurationFileIsExists()
        {
            return File.Exists(_configuration.FilePath);
        }
    }
}
