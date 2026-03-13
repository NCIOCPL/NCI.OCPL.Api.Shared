using System.Text.Json.Serialization;

namespace integration_test_harness
{
  /// <summary>
  /// Demonstration of Elasticsearch serialization.
  /// </summary>
  [JsonConverter(typeof(CustomJsonConverter))]
  public class CustomSerializationModel
  {
    /// <summary>
    /// Property which uses default serialization.
    /// </summary>
    public string Default { get; set; }

    /// <summary>
    /// Property which uses custom serialization to convert an array
    /// to a single value.
    /// </summary>
    public string Custom { get; set; }
  }
}