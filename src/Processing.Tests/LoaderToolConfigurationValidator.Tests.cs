﻿using Xunit;

using Microsoft.Extensions.Options;


using FluentAssertions;

using KafkaSnapshot.Processing.Configuration;
using KafkaSnapshot.Processing.Configuration.Validation;

namespace KafkaSnapshot.Processing.Tests
{
    public class LoaderToolConfigurationValidatorTests
    {
        [Fact(DisplayName = "LoaderToolConfigurationValidator can be created.")]
        [Trait("Category", "Unit")]
        public void LoaderToolConfigurationValidatorCanBeCreated()
        {

            // Arrange

            // Act
            var exception = Record.Exception(() => new LoaderToolConfigurationValidator());

            // Assert
            exception.Should().BeNull();
        }

        [Fact(DisplayName = "LoaderToolConfigurationValidator can be validated.")]
        [Trait("Category", "Unit")]
        public void LoaderToolConfigurationValidatorCanBeValidated()
        {

            // Arrange
            var validator = new LoaderToolConfigurationValidator();
            var name = "Test";
            ValidateOptionsResult result = null!;
            var options = new LoaderToolConfiguration
            {
                UseConcurrentLoad = true,
                Topics = new List<TopicConfiguration>
                 {
                      new TopicConfiguration
                      {
                          Name = "test",
                          Compacting = CompactingMode.On,
                          ExportFileName = "export",
                          ExportRawMessage = true,
                          FilterType = Models.Filters.FilterType.None,
                          KeyType = Models.Filters.KeyType.Json,
                      }
                 }
            };

            // Act
            var exception = Record.Exception(() => result = validator.Validate(name, options));

            // Assert
            exception.Should().BeNull();
            result.Succeeded.Should().BeTrue();
        }

        [Fact(DisplayName = "LoaderToolConfigurationValidator cant be validated with null options.")]
        [Trait("Category", "Unit")]
        public void LoaderToolConfigurationValidatorCantBeValidatedWithNullOptions()
        {

            // Arrange
            var validator = new LoaderToolConfigurationValidator();
            var name = "Test";
            ValidateOptionsResult result = null!;
            var options = (LoaderToolConfiguration)null!;

            // Act
            var exception = Record.Exception(() => result = validator.Validate(name, options));

            // Assert
            exception.Should().BeNull();
            result.Succeeded.Should().BeFalse();
        }

        [Fact(DisplayName = "LoaderToolConfigurationValidator cant be validated with null topics.")]
        [Trait("Category", "Unit")]
        public void LoaderToolConfigurationValidatorCantBeValidatedWithNullTopics()
        {

            // Arrange
            var validator = new LoaderToolConfigurationValidator();
            var name = "Test";
            ValidateOptionsResult result = null!;
            var options = new LoaderToolConfiguration
            {
                UseConcurrentLoad = true,
                Topics = null!
            };

            // Act
            var exception = Record.Exception(() => result = validator.Validate(name, options));

            // Assert
            exception.Should().BeNull();
            result.Succeeded.Should().BeFalse();
        }

        [Fact(DisplayName = "LoaderToolConfigurationValidator cant be validated with empty topics.")]
        [Trait("Category", "Unit")]
        public void LoaderToolConfigurationValidatorCantBeValidatedWithEmptyTopics()
        {

            // Arrange
            var validator = new LoaderToolConfigurationValidator();
            var name = "Test";
            ValidateOptionsResult result = null!;
            var options = new LoaderToolConfiguration
            {
                UseConcurrentLoad = true,
                Topics = new List<TopicConfiguration>()
            };

            // Act
            var exception = Record.Exception(() => result = validator.Validate(name, options));

            // Assert
            exception.Should().BeNull();
            result.Succeeded.Should().BeFalse();
        }

        [Fact(DisplayName = "LoaderToolConfigurationValidator cant be validated with null topic name.")]
        [Trait("Category", "Unit")]
        public void LoaderToolConfigurationValidatorCantBeValidatedWithNullTopicName()
        {

            // Arrange
            var validator = new LoaderToolConfigurationValidator();
            var name = "Test";
            ValidateOptionsResult result = null!;
            var options = new LoaderToolConfiguration
            {
                UseConcurrentLoad = true,
                Topics = new List<TopicConfiguration>()
                {
                      new TopicConfiguration
                      {
                          Name = "test",
                          Compacting = CompactingMode.On,
                          ExportFileName = "export",
                          ExportRawMessage = true,
                          FilterType = Models.Filters.FilterType.None,
                          KeyType = Models.Filters.KeyType.Json,
                      },
                      new TopicConfiguration
                      {
                          Name = null!,
                          Compacting = CompactingMode.On,
                          ExportFileName = "export",
                          ExportRawMessage = true,
                          FilterType = Models.Filters.FilterType.None,
                          KeyType = Models.Filters.KeyType.Json,
                      }
                }
            };

            // Act
            var exception = Record.Exception(() => result = validator.Validate(name, options));

            // Assert
            exception.Should().BeNull();
            result.Succeeded.Should().BeFalse();
        }

        [Fact(DisplayName = "LoaderToolConfigurationValidator cant be validated with empty topic name.")]
        [Trait("Category", "Unit")]
        public void LoaderToolConfigurationValidatorCantBeValidatedWithEmptyTopicName()
        {

            // Arrange
            var validator = new LoaderToolConfigurationValidator();
            var name = "Test";
            ValidateOptionsResult result = null!;
            var options = new LoaderToolConfiguration
            {
                UseConcurrentLoad = true,
                Topics = new List<TopicConfiguration>()
                {
                      new TopicConfiguration
                      {
                          Name = "test",
                          Compacting = CompactingMode.On,
                          ExportFileName = "export",
                          ExportRawMessage = true,
                          FilterType = Models.Filters.FilterType.None,
                          KeyType = Models.Filters.KeyType.Json,
                      },
                      new TopicConfiguration
                      {
                          Name = string.Empty,
                          Compacting = CompactingMode.On,
                          ExportFileName = "export",
                          ExportRawMessage = true,
                          FilterType = Models.Filters.FilterType.None,
                          KeyType = Models.Filters.KeyType.Json,
                      }
                }
            };

            // Act
            var exception = Record.Exception(() => result = validator.Validate(name, options));

            // Assert
            exception.Should().BeNull();
            result.Succeeded.Should().BeFalse();
        }

        [Fact(DisplayName = "LoaderToolConfigurationValidator cant be validated with space topic name.")]
        [Trait("Category", "Unit")]
        public void LoaderToolConfigurationValidatorCantBeValidatedWithSpaceTopicName()
        {

            // Arrange
            var validator = new LoaderToolConfigurationValidator();
            var name = "Test";
            ValidateOptionsResult result = null!;
            var options = new LoaderToolConfiguration
            {
                UseConcurrentLoad = true,
                Topics = new List<TopicConfiguration>()
                {
                      new TopicConfiguration
                      {
                          Name = "test",
                          Compacting = CompactingMode.On,
                          ExportFileName = "export",
                          ExportRawMessage = true,
                          FilterType = Models.Filters.FilterType.None,
                          KeyType = Models.Filters.KeyType.Json,
                      },
                      new TopicConfiguration
                      {
                          Name = "   ",
                          Compacting = CompactingMode.On,
                          ExportFileName = "export",
                          ExportRawMessage = true,
                          FilterType = Models.Filters.FilterType.None,
                          KeyType = Models.Filters.KeyType.Json,
                      }
                }
            };

            // Act
            var exception = Record.Exception(() => result = validator.Validate(name, options));

            // Assert
            exception.Should().BeNull();
            result.Succeeded.Should().BeFalse();
        }

        [Theory(DisplayName = "LoaderToolConfigurationValidator cant be validated with any space topic name.")]
        [Trait("Category", "Unit")]
        [InlineData(" test")]
        [InlineData("test ")]
        [InlineData("te st")]
        public void LoaderToolConfigurationValidatorCantBeValidatedWithAnySpaceTopicName(string topicName)
        {

            // Arrange
            var validator = new LoaderToolConfigurationValidator();
            var name = "Test";
            ValidateOptionsResult result = null!;
            var options = new LoaderToolConfiguration
            {
                UseConcurrentLoad = true,
                Topics = new List<TopicConfiguration>()
                {
                      new TopicConfiguration
                      {
                          Name = "test",
                          Compacting = CompactingMode.On,
                          ExportFileName = "export",
                          ExportRawMessage = true,
                          FilterType = Models.Filters.FilterType.None,
                          KeyType = Models.Filters.KeyType.Json,
                      },
                      new TopicConfiguration
                      {
                          Name = topicName,
                          Compacting = CompactingMode.On,
                          ExportFileName = "export",
                          ExportRawMessage = true,
                          FilterType = Models.Filters.FilterType.None,
                          KeyType = Models.Filters.KeyType.Json,
                      }
                }
            };

            // Act
            var exception = Record.Exception(() => result = validator.Validate(name, options));

            // Assert
            exception.Should().BeNull();
            result.Succeeded.Should().BeFalse();
        }

        [Fact(DisplayName = "LoaderToolConfigurationValidator cant be validated with long topic name.")]
        [Trait("Category", "Unit")]
        public void LoaderToolConfigurationValidatorCantBeValidatedWithLongTopicName()
        {

            // Arrange
            var validator = new LoaderToolConfigurationValidator();
            var name = "Test";
            ValidateOptionsResult result = null!;
            var options = new LoaderToolConfiguration
            {
                UseConcurrentLoad = true,
                Topics = new List<TopicConfiguration>()
                {
                      new TopicConfiguration
                      {
                          Name = "test",
                          Compacting = CompactingMode.On,
                          ExportFileName = "export",
                          ExportRawMessage = true,
                          FilterType = Models.Filters.FilterType.None,
                          KeyType = Models.Filters.KeyType.Json,
                      },
                      new TopicConfiguration
                      {
                          Name = new string('a',250),
                          Compacting = CompactingMode.On,
                          ExportFileName = "export",
                          ExportRawMessage = true,
                          FilterType = Models.Filters.FilterType.None,
                          KeyType = Models.Filters.KeyType.Json,
                      }
                }
            };

            // Act
            var exception = Record.Exception(() => result = validator.Validate(name, options));

            // Assert
            exception.Should().BeNull();
            result.Succeeded.Should().BeFalse();
        }


        [Fact(DisplayName = "LoaderToolConfigurationValidator cant be validated with bad topic name.")]
        [Trait("Category", "Unit")]
        public void LoaderToolConfigurationValidatorCantBeValidatedWithBadTopicName()
        {

            // Arrange
            var validator = new LoaderToolConfigurationValidator();
            var name = "Test";
            ValidateOptionsResult result = null!;
            var options = new LoaderToolConfiguration
            {
                UseConcurrentLoad = true,
                Topics = new List<TopicConfiguration>()
                {
                      new TopicConfiguration
                      {
                          Name = "test",
                          Compacting = CompactingMode.On,
                          ExportFileName = "export",
                          ExportRawMessage = true,
                          FilterType = Models.Filters.FilterType.None,
                          KeyType = Models.Filters.KeyType.Json,
                      },
                      new TopicConfiguration
                      {
                          Name = "*()=%$#@!!",
                          Compacting = CompactingMode.On,
                          ExportFileName = "export",
                          ExportRawMessage = true,
                          FilterType = Models.Filters.FilterType.None,
                          KeyType = Models.Filters.KeyType.Json,
                      }
                }
            };

            // Act
            var exception = Record.Exception(() => result = validator.Validate(name, options));

            // Assert
            exception.Should().BeNull();
            result.Succeeded.Should().BeFalse();
        }
    }
}
