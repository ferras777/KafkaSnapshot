using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using KafkaSnapshot.Abstractions.Import;
using KafkaSnapshot.Abstractions.Export;
using KafkaSnapshot.Abstractions.Processing;
using KafkaSnapshot.Models.Processing;
using KafkaSnapshot.Models.Export;
using KafkaSnapshot.Models.Import;
using KafkaSnapshot.Abstractions.Filters;

namespace KafkaSnapshot.Processing
{
    /// <summary>
    /// Single topic processor that loads data from Apache Kafka and exports to file.
    /// </summary>
    /// <typeparam name="TKey">Message Key.</typeparam>
    /// <typeparam name="TKeyMarker">Message Key marker.</typeparam>
    /// <typeparam name="TValue">Message Value.</typeparam>
    public class ProcessingUnit<TKey, TKeyMarker, TValue> : IProcessingUnit where TKey : notnull
                                                                            where TKeyMarker : IKeyRepresentationMarker
                                                                            where TValue : notnull
    {
        /// <summary>
        /// Creates <see cref="ProcessingUnit{TKey,TKeyMarker, TValue}"/>.
        /// </summary>
        /// <param name="logger">Creates logger for <see cref="ProcessingUnit{TKey, TKeyMarker, TValue}"/>.</param>
        /// <param name="topic">Apahe Kafka topic.</param>
        /// <param name="kafkaLoader">Kafka topic loader.</param>
        /// <param name="exporter">Data exporter.</param>
        public ProcessingUnit(ILogger<ProcessingUnit<TKey, TKeyMarker, TValue>> logger,
                              ProcessingTopic<TKey> topic,
                              ISnapshotLoader<TKey, TValue> kafkaLoader,
                              IDataExporter<TKey, TKeyMarker, TValue, ExportedTopic> exporter,
                              IKeyFiltersFactory<TKey> filterFactory
                              )
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _kafkaLoader = kafkaLoader ?? throw new ArgumentNullException(nameof(kafkaLoader));
            _exporter = exporter ?? throw new ArgumentNullException(nameof(exporter));

            if (topic is null)
            {
                throw new ArgumentNullException(nameof(topic));
            }

            if (filterFactory is null)
            {
                throw new ArgumentNullException(nameof(filterFactory));
            }

            _filter = filterFactory.Create(topic.FilterType, topic.KeyType, topic.FilterValue);

            _logger.LogDebug("Instance created for topic {topic}.", topic);

            _topicParams = topic.StartingDate.HasValue
                ? new LoadingTopic(topic.Name, topic.LoadWithCompacting, topic.StartingDate.Value)
                : new LoadingTopic(topic.Name, topic.LoadWithCompacting);

            _exportedTopic = new ExportedTopic(topic.Name, topic.ExportName, topic.ExportRawMessage);

            _logger.LogDebug("Instance created for topic {topic}.", topic);
        }

        /// <inheritdoc/>
        public async Task ProcessAsync(CancellationToken ct)
        {
            _logger.LogDebug("Start loading data from Kafka.");
            var items = await _kafkaLoader.LoadCompactSnapshotAsync(
                _topicParams,
                _filter,
                ct).ConfigureAwait(false);

            _logger.LogDebug("Start exporting data to file.");
            await _exporter.ExportAsync(items, _exportedTopic, ct).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public string TopicName => _topicParams.Value;

        private readonly ISnapshotLoader<TKey, TValue> _kafkaLoader;
        private readonly IDataExporter<TKey, TKeyMarker, TValue, ExportedTopic> _exporter;
        private readonly ILogger<ProcessingUnit<TKey, TKeyMarker, TValue>> _logger;
        private readonly IKeyFilter<TKey> _filter;
        private readonly LoadingTopic _topicParams;
        private readonly ExportedTopic _exportedTopic;
    }
}
