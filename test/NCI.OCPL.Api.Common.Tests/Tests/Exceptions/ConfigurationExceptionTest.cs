using System;

using Xunit;

namespace NCI.OCPL.Api.Common.Tests
{
  public partial class ConfigurationExceptionTest
  {
    /// <summary>
    /// Verify the default constructor creates an exception.
    /// </summary>
    [Fact]
    public void DefaultConstructor()
    {
      // Act
      ConfigurationException ex = new ConfigurationException();

      // Assert
      Assert.NotNull(ex);
      Assert.IsType<ConfigurationException>(ex);
    }

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
      catch (Exception ex)
      {
        Assert.Equal(theMessage, ex.Message);
      }
    }

    /// <summary>
    /// Verify the message and inner exception are preserved.
    /// </summary>
    [Fact]
    public void MessageAndInnerExceptionIntact()
    {
      // Arrange
      string theMessage = "the outer message";
      Exception innerException = new InvalidOperationException("inner exception message");

      // Act
      try
      {
        throw new ConfigurationException(theMessage, innerException);
      }
      catch (ConfigurationException ex)
      {
        // Assert
        Assert.Equal(theMessage, ex.Message);
        Assert.NotNull(ex.InnerException);
        Assert.Same(innerException, ex.InnerException);
        Assert.Equal("inner exception message", ex.InnerException.Message);
      }
    }

  }
}