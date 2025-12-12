using Xunit;

namespace NCI.OCPL.Api.Common.Tests
{
  public partial class ConfigurationExceptionTest
  {
    /// <summary>
    /// Verify the exception message is unchanged.
    /// </summary>
    [Fact]
    public void MessageIntact()
    {
      string theMessage = "the message";
      try
      {
        throw new ConfigurationException(theMessage);
      }
      catch (System.Exception ex)
      {
        Assert.Equal(theMessage, ex.Message);
      }
    }

  }
}