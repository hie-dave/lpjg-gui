using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using LpjGuess.Core.Serialisation.Json;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Engine.ClientProtocol;
using Xunit;

namespace LpjGuess.Tests.Serialisation.Json;

public class PolymorphicConverterTests
{
    #region Test Interfaces and Classes

    // Simple interface for testing
    public interface ITestInterface
    {
        string Value { get; set; }
    }

    // Concrete implementation of the interface
    public class TestImplementation : ITestInterface
    {
        public string Value { get; set; } = string.Empty;
    }

    // Generic interface for testing
    public interface IGenericInterface<T>
    {
        T Value { get; set; }
    }

    // Concrete implementation of the generic interface
    public class GenericImplementation<T> : IGenericInterface<T>
    {
        public T Value { get; set; } = default!;
    }

    // Nested generic interface for testing
    public interface INestedGenericInterface<T>
    {
        IGenericInterface<T> NestedValue { get; set; }
    }

    // Concrete implementation of the nested generic interface
    public class NestedGenericImplementation<T> : INestedGenericInterface<T>
    {
        public IGenericInterface<T> NestedValue { get; set; } = default!;
    }

    // Double nested generic interface for testing
    public interface IDoubleNestedGenericInterface<T, U>
    {
        IGenericInterface<IGenericInterface<T>> OuterValue { get; set; }
        U InnerValue { get; set; }
    }

    // Concrete implementation of the double nested generic interface
    public class DoubleNestedGenericImplementation<T, U> : IDoubleNestedGenericInterface<T, U>
    {
        public IGenericInterface<IGenericInterface<T>> OuterValue { get; set; } = default!;
        public U InnerValue { get; set; } = default!;
    }

    #endregion

    #region Test Methods

    [Fact]
    public void SerializeDeserialize_SimpleType_WorksCorrectly()
    {
        // Arrange
        var options = new JsonSerializerOptions
        {
            Converters = { new PolymorphicConverter<ITestInterface>() }
        };

        var original = new TestImplementation { Value = "Test Value" };

        // Act
        string json = JsonSerializer.Serialize<ITestInterface>(original, options);
        var deserialized = JsonSerializer.Deserialize<ITestInterface>(json, options);

        // Assert
        Assert.NotNull(deserialized);
        Assert.IsType<TestImplementation>(deserialized);
        Assert.Equal(original.Value, deserialized.Value);
        Assert.Contains("$type", json);
        Assert.Contains("TestImplementation", json);
    }

    [Fact]
    public void SerializeDeserialize_GenericType_WorksCorrectly()
    {
        // Arrange
        var options = new JsonSerializerOptions
        {
            Converters = { new PolymorphicConverter<IGenericInterface<string>>() }
        };

        var original = new GenericImplementation<string> { Value = "Test Value" };

        // Act
        string json = JsonSerializer.Serialize<IGenericInterface<string>>(original, options);
        var deserialized = JsonSerializer.Deserialize<IGenericInterface<string>>(json, options);

        // Assert
        Assert.NotNull(deserialized);
        Assert.IsType<GenericImplementation<string>>(deserialized);
        Assert.Equal(original.Value, deserialized.Value);
        Assert.Contains("$type", json);
        Assert.Contains("GenericImplementation\\u003CString\\u003E", json);
    }

    [Fact]
    public void SerializeDeserialize_NestedGenericType_WorksCorrectly()
    {
        // Arrange
        var options = new JsonSerializerOptions
        {
            Converters = 
            { 
                new PolymorphicConverter<INestedGenericInterface<int>>(),
                new PolymorphicConverter<IGenericInterface<int>>()
            }
        };

        var innerValue = new GenericImplementation<int> { Value = 42 };
        var original = new NestedGenericImplementation<int> { NestedValue = innerValue };

        // Act
        string json = JsonSerializer.Serialize<INestedGenericInterface<int>>(original, options);
        var deserialized = JsonSerializer.Deserialize<INestedGenericInterface<int>>(json, options);

        // Assert
        Assert.NotNull(deserialized);
        Assert.IsType<NestedGenericImplementation<int>>(deserialized);
        Assert.NotNull(deserialized.NestedValue);
        Assert.IsType<GenericImplementation<int>>(deserialized.NestedValue);
        Assert.Equal(original.NestedValue.Value, deserialized.NestedValue.Value);
        Assert.Contains("$type", json);
        Assert.Contains("NestedGenericImplementation\\u003CInt32\\u003E", json);
    }

    [Fact]
    public void SerializeDeserialize_ComplexNestedGenericType_WorksCorrectly()
    {
        // Arrange
        var options = new JsonSerializerOptions
        {
            Converters = 
            { 
                new PolymorphicConverter<IDoubleNestedGenericInterface<string, int>>(),
                new PolymorphicConverter<IGenericInterface<IGenericInterface<string>>>(),
                new PolymorphicConverter<IGenericInterface<string>>()
            }
        };

        var innerValue = new GenericImplementation<string> { Value = "Inner" };
        var middleValue = new GenericImplementation<IGenericInterface<string>> { Value = innerValue };
        var original = new DoubleNestedGenericImplementation<string, int> 
        { 
            OuterValue = middleValue,
            InnerValue = 42
        };

        // Act
        string json = JsonSerializer.Serialize<IDoubleNestedGenericInterface<string, int>>(original, options);
        var deserialized = JsonSerializer.Deserialize<IDoubleNestedGenericInterface<string, int>>(json, options);

        // Assert
        Assert.NotNull(deserialized);
        Assert.IsType<DoubleNestedGenericImplementation<string, int>>(deserialized);
        Assert.NotNull(deserialized.OuterValue);
        Assert.IsType<GenericImplementation<IGenericInterface<string>>>(deserialized.OuterValue);
        Assert.NotNull(deserialized.OuterValue.Value);
        Assert.IsType<GenericImplementation<string>>(deserialized.OuterValue.Value);
        Assert.Equal(original.OuterValue.Value.Value, deserialized.OuterValue.Value.Value);
        Assert.Equal(original.InnerValue, deserialized.InnerValue);
        Assert.Contains("$type", json);
        Assert.Contains("DoubleNestedGenericImplementation\\u003CString,Int32\\u003E", json);
    }

    [Fact]
    public void Deserialize_InvalidTypeName_ThrowsJsonException()
    {
        // Arrange
        var options = new JsonSerializerOptions
        {
            Converters = { new PolymorphicConverter<ITestInterface>() }
        };

        string json = @"{""$type"":""NonExistentType"",""Value"":""Test""}";

        // Act & Assert
        var exception = Assert.Throws<JsonException>(() => 
            JsonSerializer.Deserialize<ITestInterface>(json, options));
        Assert.Contains("Could not find type", exception.Message);
    }

    [Fact]
    public void Deserialize_MissingTypeDiscriminator_ThrowsJsonException()
    {
        // Arrange
        var options = new JsonSerializerOptions
        {
            Converters = { new PolymorphicConverter<ITestInterface>() }
        };

        string json = @"{""Value"":""Test""}";

        // Act & Assert
        var exception = Assert.Throws<JsonException>(() => 
            JsonSerializer.Deserialize<ITestInterface>(json, options));
        Assert.Contains("Missing type discriminator", exception.Message);
    }

    [Fact]
    public void Deserialize_InvalidGenericTypeName_ThrowsJsonException()
    {
        // Arrange
        var options = new JsonSerializerOptions
        {
            Converters = { new PolymorphicConverter<IGenericInterface<string>>() }
        };

        string json = @"{""$type"":""NonExistentGeneric<String>"",""Value"":""Test""}";

        // Act & Assert
        var exception = Assert.Throws<JsonException>(() => 
            JsonSerializer.Deserialize<IGenericInterface<string>>(json, options));
        Assert.Contains("Could not find generic type definition", exception.Message);
    }

    [Fact]
    public void SanitiseTypeName_GenericTypeWithMultipleParameters_FormatsCorrectly()
    {
        // Arrange
        var options = new JsonSerializerOptions
        {
            Converters = { new PolymorphicConverter<IDoubleNestedGenericInterface<string, int>>() }
        };

        var original = new DoubleNestedGenericImplementation<string, int>
        {
            OuterValue = new GenericImplementation<IGenericInterface<string>>(),
            InnerValue = 42
        };

        // Act
        string json = JsonSerializer.Serialize<IDoubleNestedGenericInterface<string, int>>(original, options);

        // Assert
        Assert.Contains("DoubleNestedGenericImplementation\\u003CString,Int32\\u003E", json);
    }

    #endregion
}
