using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace integration_test_harness
{

  /// <summary>
  /// Converts a JSON element containing either a single string, or an array of strings, into
  /// a single string.
  /// </summary>
  public class CustomJsonConverter : JsonConverter<string>
  {
    /// <summary>
    /// Responsible for reading a JSON element containing either a single string or an array of strings
    /// and converting it into a single string by discarding all but the first one.
    /// </summary>
    /// <param name="reader">The Utf8JsonReader to read from.</param>
    /// <param name="typeToConvert">Type of the destination object (always System.String).</param>
    /// <param name="options">The serializer options.</param>
    /// <returns></returns>
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
      // If it's just a string, return the value.
      if (reader.TokenType == JsonTokenType.String)
      {
        reader.GetString(); // consume the value
        return "Took the string path";
      }
      else
      {
        // Otherwise, save the first value and skip past the rest.
        reader.Read(); // Move to first element
        string value = reader.GetString();
        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
          // consume remaining array elements
        }
        return "Took the not string path";
      }
    }

    /// <summary>
    /// Writes the JSON representation of the object.
    /// </summary>
    /// <param name="writer">The Utf8JsonWriter to write to.</param>
    /// <param name="value">The value.</param>
    /// <param name="options">The serializer options.</param>
    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
      throw new NotImplementedException("This converter not intended for writing.");
    }
  }
}