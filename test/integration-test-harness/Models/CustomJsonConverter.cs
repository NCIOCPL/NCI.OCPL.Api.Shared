using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;

namespace integration_test_harness
{

  /// <summary>
  /// Converts a JSON element containing either a single string, or an array of strings, into
  /// a single string.
  /// </summary>
  public class CustomJsonConverter : JsonConverter<CustomSerializationModel>
  {
    /// <summary>
    /// Responsible for deserialinng CustomSerializationModel, handling the tricky caso of
    /// a custom JSON element containing either a single string or an array of strings
    /// and (either way) converting it to a single string.
    /// </summary>
    /// <param name="reader">The Utf8JsonReader to read from.</param>
    /// <param name="typeToConvert">Type of the destination object (always System.String).</param>
    /// <param name="options">The serializer options.</param>
    /// <returns></returns>
    public override CustomSerializationModel Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
      var model = new CustomSerializationModel();

      while(reader.Read() && reader.TokenType != JsonTokenType.EndObject)
      {
        if (reader.TokenType == JsonTokenType.PropertyName)
        {
          if(reader.ValueTextEquals("default"))
          {
            reader.Read(); // Move to property value
            model.Default = "Took the string path";
            continue;
          }
          else if (reader.ValueTextEquals("custom"))
          {
            reader.Read(); // Move to property value
            if(reader.TokenType == JsonTokenType.String)
            {
              // This is the simple string path, so we'll report that fact,
              // and deliberately ignore the actual value for demonstration purposes.
              model.Custom = "Took the string path";
            }
            else
            {
              // This is the array path, so we'll report that fact, advance to the
              // end of the array, and again, ignore the actual values for demonstration purposes.
              model.Custom = "Took the not string path";
              while(reader.Read() && reader.TokenType != JsonTokenType.EndArray)
              {
                // consume array elements
              }
            }
          }
        }
      }

      return model;
    }

    /// <summary>
    /// Writes the JSON representation of the object using default serialization.
    /// </summary>
    /// <param name="writer">The Utf8JsonWriter to write to.</param>
    /// <param name="value">The value.</param>
    /// <param name="options">The serializer options.</param>
    public override void Write(Utf8JsonWriter writer, CustomSerializationModel value, JsonSerializerOptions options)
    {
      // Perform default serialization by writing each property manually.
      writer.WriteStartObject();

      writer.WriteString("default", value.Default);
      writer.WriteString("custom", value.Custom);

      writer.WriteEndObject();
    }
  }
}