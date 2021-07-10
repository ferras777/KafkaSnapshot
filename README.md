![KafkaSnapshot](logo_s.png)
# KafkaSnapshot
Tool that allows to read current data snapshot from Apache Kafka topic with compacting.

Application supports Apache Kafka topics with string keys (optionally as json) and long keys. Topics with NULL keys will be skipped.
Topics could be filtered by key.

Messages should contains JSON data.

![Details](Case1.PNG)

Config with topics for export:

```yaml
  "BootstrapServersConfiguration": {
    "BootstrapServers": [
    ]
  },
  "TopicWatermarkLoaderConfiguration": {
    "AdminClientTimeout": "00:00:05"
  },
  "LoaderToolConfiguration": {
    "Topics": [
      {
        "Name": "topic1",
        "KeyType": "Json",
        "Compacting": "On",
        "ExportFileName": "topic1.json",
        "FilterType": "Equals",
        "FilterValue": "{\"value\": 1 }"
      },
      {
        "Name": "topic2",
        "KeyType": "String",
        "Compacting": "Off",
        "ExportFileName": "topic2.json"
      },
      {
        "Name": "topic3",
        "KeyType": "Long",
        "Compacting": "Off",
        "ExportFileName": "topic3.json",
        "FilterType": "Equals",
        "FilterValue": 42
      }
    ]
  }
```

| Parameter name | Description   |
| -------------- | ------------- |
| AdminClientTimeout | Cluster metadata loading timeout |
| BootstrapServers | List of kafka cluster servers, like "kafka-test:9092"  |
| Name           | Apache Kafka topic name |
| KeyType        | Apache Kafka topic key representation (Json,String,Long) |
| Compacting     | Use compacting by key or not (On,Off) |
| ExportFileName | File name for exported data  |
| FilterType | Equals or None (optional)  |
| FilterValue | Sample value for filtering (if FilterType sets as 'Equals') |
