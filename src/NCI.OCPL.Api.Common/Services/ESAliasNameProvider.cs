using System;

namespace NCI.OCPL.Api.Common
{

  /// <summary>
  /// Helper class to relay the name of the Elasticsearch alias to use for healthchecks from
  /// the application-specific initialization code to the ESHealthCheckService.
  /// </summary>
  public class ESAliasNameProvider : IESAliasNameProvider
  {
    private string _name;

    /// <summary>
    /// Name of the alias to use for the healthcheck.
    /// </summary>
    public string Name
    {
      get
      {
        if (String.IsNullOrWhiteSpace(_name))
          throw new InvalidOperationException("ES Alias Name has not been set.");
        return _name;
      }

      set
      {
        if (String.IsNullOrWhiteSpace(value))
          throw new ArgumentException($"{nameof(Name)} cannot be null or whitespace.", nameof(Name));

        _name = value;
      }
    }
  }
}