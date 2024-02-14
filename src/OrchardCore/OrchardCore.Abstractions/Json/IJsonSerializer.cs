using System;

namespace OrchardCore.Json;

public interface IJsonSerializer
{
    /// <summary>
    /// Serializes an object into a <see cref="string" />.
    /// </summary>
    /// <param name="item">The object to serialize.</param>
    /// <returns>The serialized object.</returns>
    string Serialize(object item);

    /// <summary>
    /// Serializes an object of a given type into a <see cref="string" />.
    /// </summary>
    /// <param name="item">The object to serialize.</param>
    /// <returns>The serialized object.</returns>
    string Serialize<T>(T item);

    /// <summary>
    /// Deserializes an object from a string.
    /// </summary>
    /// <param name="content">The <see cref="string" /> instance representing the object to deserialize.</param>
    /// <param name="type">The type of the object to deserialize.</param>
    /// <returns>The deserialized object.</returns>
    object Deserialize(string content, Type type);

    /// <summary>
    /// Deserializes an object from a string.
    /// </summary>
    /// <param name="content">The <see cref="string" /> instance representing the object to deserialize.</param>
    /// <returns>The deserialized object.</returns>
    T Deserialize<T>(string content);
}
