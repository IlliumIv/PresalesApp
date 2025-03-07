﻿using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Globalization;

namespace PresalesApp.Web.Client.Startup
{
    public static class ApplicationConfiguration
    {
        public static async Task ConfigureApplication(this WebAssemblyHost host)
        {
            var storage = host.Services.GetService<ILocalStorageService>() ?? throw new Exception("Local storage is not configured.");
            var culture = await storage.GetItemAsStringAsync("i18nextLng");
            var default_culture = new CultureInfo(string.IsNullOrEmpty(culture) ? "ru-RU" : culture);
            await storage.SetItemAsStringAsync("i18nextLng", default_culture.Name);

            CultureInfo.DefaultThreadCurrentCulture = default_culture;
            CultureInfo.DefaultThreadCurrentUICulture = default_culture;
        }
    }
}
