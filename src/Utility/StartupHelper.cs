﻿using System;
using System.Collections.Generic;
using System.Diagnostics;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Confluent.Kafka;

using Serilog;

using KafkaSnapshot.Import;
using KafkaSnapshot.Import.Metadata;
using KafkaSnapshot.Processing;
using KafkaSnapshot.Processing.Configuration;
using KafkaSnapshot.Abstractions.Export;
using KafkaSnapshot.Abstractions.Processing;
using KafkaSnapshot.Models.Processing;
using KafkaSnapshot.Export.File.Json;
using KafkaSnapshot.Processing.Configuration.Validation;
using KafkaSnapshot.Models.Export;
using KafkaSnapshot.Export.File.Common;

namespace KafkaSnapshot.Utility
{
    /// <summary>
    /// Helper utility class with services registration methods
    /// </summary>
    public static class StartupHelper
    {
        /// <summary>
        /// Registers appsettings.
        /// </summary>
        public static void RegisterApplicationSettings(this IConfigurationBuilder builder)
        {
            builder.AddJsonFile("appsettings.json", optional: true);
        }

        /// <summary>
        /// Registers configuration and entry point of application.
        /// </summary>
        public static void AddTools(this IServiceCollection services, HostBuilderContext hostContext)
        {
            services.AddScoped(typeof(LoaderTool));
            services.Configure<LoaderToolConfiguration>(hostContext.Configuration.GetSection(nameof(LoaderToolConfiguration)));
            services.AddSingleton<IValidateOptions<LoaderToolConfiguration>, LoaderToolConfigurationValidator>();

        }

        /// <summary>
        /// Adds export providers.
        /// </summary>
        public static void AddExport(this IServiceCollection services)
        {
            services.AddSingleton<IDataExporter<long, string, ExportedTopic>, JsonLongKeyStringValueDataExporter>();
            services.AddSingleton<IDataExporter<string, string, ExportedTopic>, JsonStringKeyStringValueDataExporter>();
            services.AddSingleton<IFileSaver, FileSaver>();
        }

        /// <summary>
        /// Adds Serilog.
        /// </summary>
        public static void AddLogging(this IServiceCollection services, HostBuilderContext hostContext)
        {
            var logger = new LoggerConfiguration()
                             .ReadFrom.Configuration(hostContext.Configuration)
                             .CreateLogger();

            services.AddLogging(x =>
            {
                x.SetMinimumLevel(LogLevel.Information);
                x.AddSerilog(logger: logger, dispose: true);
            });
        }

        /// <summary>
        /// Adds topic loaders.
        /// </summary>
        public static void AddTopicLoaders(this IServiceCollection services, HostBuilderContext hostContext)
        {
            services.AddSingleton(sp => CreateTopicLoaders(sp, hostContext.Configuration));
        }

        /// <summary>
        /// Creates topic loaders from config
        /// </summary>
        private static ICollection<IProcessingUnit> CreateTopicLoaders(IServiceProvider sp, IConfiguration configuration)
        {
            var list = new List<IProcessingUnit>();

            var section = configuration.GetSection(nameof(LoaderToolConfiguration));

            var config = section.Get<LoaderToolConfiguration>();

            Debug.Assert(config is not null);

            var validator = sp.GetRequiredService<IValidateOptions<LoaderToolConfiguration>>();

            // Crutch to use IValidateOptions in manual generation logic.
            var validationResult = validator.Validate(string.Empty, config);
            if (validationResult.Failed)
            {
                throw new OptionsValidationException
                    (string.Empty, config.GetType(), new[] { validationResult.FailureMessage });
            }

            var servers = string.Join(",", config.BootstrapServers);

            IConsumer<Key, string> createConsumer<Key>()
            {
                var conf = new ConsumerConfig
                {
                    BootstrapServers = servers,
                    AutoOffsetReset = AutoOffsetReset.Earliest,
                    GroupId = Guid.NewGuid().ToString(),
                };

                return new ConsumerBuilder<Key, string>(conf).Build();
            }

            void InitUnit<Key>(LoadedTopic topic) where Key : notnull
            {
                var adminConfig = new AdminClientConfig()
                {
                    BootstrapServers = servers
                };

                var adminClient = new AdminClientBuilder(adminConfig).Build();

                var wLoader = new TopicWatermarkLoader(new TopicName(topic.Name), adminClient, config.MetadataTimeout);

                list.Add(new ProcessingUnit<Key, string>(sp.GetRequiredService<ILogger<ProcessingUnit<Key, string>>>(),
                                            new ProcessingTopic(topic.Name, topic.ExportFileName),
                                            new SnapshotLoader<Key, string>(sp.GetRequiredService<ILogger<SnapshotLoader<Key, string>>>(), createConsumer<Key>, wLoader),
                                            sp.GetRequiredService<IDataExporter<Key, string, ExportedTopic>>()
                                            )
                        );
            }

            foreach (var topic in config.Topics)
            {
                switch (topic.KeyType)
                {
                    case KeyType.Json: InitUnit<string>(topic); break;
                    case KeyType.Long: InitUnit<long>(topic); break;
                    default: throw new NotSupportedException($"Topic key type {topic.KeyType} not supported.");
                }
            }

            return list;
        }
    }
}
