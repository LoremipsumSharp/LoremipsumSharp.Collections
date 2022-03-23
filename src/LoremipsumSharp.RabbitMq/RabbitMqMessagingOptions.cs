using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace LoremipsumSharp.RabbitMq
{
    public class RabbitMqMessagingOptions
    {
        public string MySqlStorageConnectionString;
        public IConfiguration ConfigurationSection;
        public IEnumerable<Assembly> AssembliesToScan;
        public IEnumerable<Type> TypeFilters;
    }
}