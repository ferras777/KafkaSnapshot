﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Confluent.Kafka;

using KafkaSnapshot.Import.Configuration;
using KafkaSnapshot.Import.Watermarks;
using KafkaSnapshot.Models.Import;
using Microsoft.Extensions.Options;

namespace KafkaSnapshot.Import.Metadata
{
    /// <summary>
    /// Service that loads <see cref="TopicWatermark"/>.
    /// </summary>
    public class TopicWatermarkLoader : ITopicWatermarkLoader
    {
        /// <summary>
        /// Creates <see cref="TopicWatermarkLoader"/>.
        /// </summary>
        /// <param name="adminClient">Kafla admin client.</param>
        /// <param name="intTimeoutSeconds">Timeout in seconds for loading watermarks.</param>
        public TopicWatermarkLoader(IAdminClient adminClient,
                                    IOptions<TopicWatermarkLoaderConfiguration> options)
        {
            if (adminClient is null)
            {
                throw new ArgumentNullException(nameof(adminClient));
            }

            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (options.Value is null)
            {
                throw new ArgumentException("Options value is not set", nameof(options));
            }

            _metaTimeout = options.Value.AdminClientTimeout;
            _adminClient = adminClient;
        }

        private IEnumerable<TopicPartition> SplitTopicOnPartitions(LoadTopicParams topicName)
        {
            var topicMeta = _adminClient.GetMetadata(topicName.Value, _metaTimeout);

            var partitions = topicMeta.Topics.Single().Partitions;

            return partitions.Select(partition => new TopicPartition(topicName.Value, new Partition(partition.PartitionId)));
        }

        private PartitionWatermark CreatePartitionWatermark<Key, Value>
            (IConsumer<Key, Value> consumer,
            LoadTopicParams topicName,
            TopicPartition topicPartition)
        {
            var watermarkOffsets = consumer.QueryWatermarkOffsets(
                                    topicPartition,
                                    _metaTimeout);

            return new PartitionWatermark(topicName, watermarkOffsets, topicPartition.Partition);
        }

        /// <inheritdoc/>>
        public async Task<TopicWatermark> LoadWatermarksAsync<Key, Value>(
                            Func<IConsumer<Key, Value>> consumerFactory,
                            LoadTopicParams topicName,
                            CancellationToken ct
                            )
        {
            if (consumerFactory is null)
            {
                throw new ArgumentNullException(nameof(consumerFactory));
            }

            if (topicName is null)
            {
                throw new ArgumentNullException(nameof(topicName));
            }

            using var consumer = consumerFactory();

            try
            {
                var partitions = SplitTopicOnPartitions(topicName);

                var partitionWatermarks = await Task.WhenAll(partitions.Select(
                            topicPartition => Task.Run(() =>
                            CreatePartitionWatermark(consumer, topicName, topicPartition), ct)
                                                       )).ConfigureAwait(false);

                return new TopicWatermark(partitionWatermarks.Where(item => item.IsReadyToRead()));
            }
            finally
            {
                consumer.Close();
            }
        }

        private readonly IAdminClient _adminClient;
        private readonly TimeSpan _metaTimeout;
    }
}
