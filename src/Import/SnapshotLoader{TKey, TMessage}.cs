﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Confluent.Kafka;

using Microsoft.Extensions.Logging;

using KafkaSnapshot.Import.Metadata;
using KafkaSnapshot.Import.Watermarks;
using KafkaSnapshot.Abstractions.Import;

namespace KafkaSnapshot.Import
{
    ///<inheritdoc/>
    public class SnapshotLoader<TKey, TMessage> : ISnapshotLoader<TKey, TMessage> where TKey : notnull
    {
        /// <summary>
        /// Creates <see cref="SnapshotLoader{Key, Message}"/>.
        /// </summary>
        public SnapshotLoader(ILogger<SnapshotLoader<TKey, TMessage>> logger,
                              Func<IConsumer<TKey, TMessage>> consumerFactory,
                              ITopicWatermarkLoader topicWatermarkLoader
                             )
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _topicWatermarkLoader = topicWatermarkLoader ?? throw new ArgumentNullException(nameof(topicWatermarkLoader));
            _consumerFactory = consumerFactory ?? throw new ArgumentNullException(nameof(consumerFactory));

            _logger.LogDebug("Instance created.");
        }

        ///<inheritdoc/>
        public async Task<IEnumerable<KeyValuePair<TKey, TMessage>>> LoadCompactSnapshotAsync(bool withCompacting, CancellationToken ct)
        {
            _logger.LogDebug("Loading topic watermark.");
            var topicWatermark = await _topicWatermarkLoader.LoadWatermarksAsync(_consumerFactory, ct).ConfigureAwait(false);

            _logger.LogDebug("Loading initial state.");
            var initialState = await ConsumeInitialAsync(topicWatermark, ct).ConfigureAwait(false);

            _logger.LogDebug("Creating compacting state.");
            var compactedState = CreateSnapshot(initialState, withCompacting);

            return compactedState;
        }

        private async Task<IEnumerable<KeyValuePair<TKey, TMessage>>> ConsumeInitialAsync
           (TopicWatermark topicWatermark,
           CancellationToken ct)
        {
            var consumedEntities = await Task.WhenAll(topicWatermark.Watermarks
                .Select(watermark =>
                            Task.Run(() => ConsumeFromWatermark(watermark, ct))
                       )
                ).ConfigureAwait(false);

            return consumedEntities.SelectMany(сonsumerResults => сonsumerResults);
        }

        private IEnumerable<KeyValuePair<TKey, TMessage>> ConsumeFromWatermark(PartitionWatermark watermark, CancellationToken ct)
        {
            using var consumer = _consumerFactory();
            try
            {
                watermark.AssingWithConsumer(consumer);
                ConsumeResult<TKey, TMessage> result = default!;
                do
                {
                    result = consumer.Consume(ct);

                    _logger.LogTrace("Loading {Key} - {Value}", result.Message.Key, result.Message.Value);

                    yield return new KeyValuePair<TKey, TMessage>(result.Message.Key, result.Message.Value);

                } while (watermark.IsWatermarkAchievedBy(result));
            }
            finally
            {
                consumer?.Close();
            }
        }

        private IEnumerable<KeyValuePair<TKey, TMessage>> CreateSnapshot(IEnumerable<KeyValuePair<TKey, TMessage>> items, bool withCompacting)
        {
            if (withCompacting)
            {
                _logger.LogDebug("Compacting data.");

                return items.Where(x => x.Key is not null).Aggregate(
                    new Dictionary<TKey, TMessage>(),
                    (d, e) =>
                    {
                        d[e.Key] = e.Value;
                        return d;
                    });
            }
            else
            {
                return items.ToList();
            }
        }

        private readonly ITopicWatermarkLoader _topicWatermarkLoader;
        private readonly Func<IConsumer<TKey, TMessage>> _consumerFactory;
        private readonly ILogger<SnapshotLoader<TKey, TMessage>> _logger;
    }
}

