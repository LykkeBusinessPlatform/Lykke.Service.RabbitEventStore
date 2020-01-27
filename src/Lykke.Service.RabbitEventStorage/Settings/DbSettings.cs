﻿using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.RabbitEventStorage.Settings
{
    public class DbSettings
    {
        [AzureTableCheck]
        public string LogsConnString { get; set; }
    }
}