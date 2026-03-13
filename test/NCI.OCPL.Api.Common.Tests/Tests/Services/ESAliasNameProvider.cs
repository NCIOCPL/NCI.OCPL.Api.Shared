using System;

using Xunit;

namespace NCI.OCPL.Api.Common.Tests
{
  /// <summary>
  /// Unit tests for the <see cref="ESAliasNameProvider"/> class.
  /// </summary>
  public class ESAliasNameProviderTest
  {
    /// <summary>
    /// Verify that the Name property returns the value that was set.
    /// </summary>
    [Fact]
    public void Name_SetValidValue_ReturnsValue()
    {
      // Arrange
      string expectedName = "test-alias";
      ESAliasNameProvider provider = new ESAliasNameProvider();

      // Act
      provider.Name = expectedName;

      // Assert
      Assert.Equal(expectedName, provider.Name);
    }

    /// <summary>
    /// Verify that setting Name to null throws ArgumentException.
    /// </summary>
    [Fact]
    public void Name_SetNull_ThrowsArgumentException()
    {
      // Arrange
      ESAliasNameProvider provider = new ESAliasNameProvider();

      // Act & Assert
      Assert.Throws<ArgumentException>(() => provider.Name = null);
    }

    /// <summary>
    /// Verify that setting Name to an empty string throws ArgumentException.
    /// </summary>
    [Fact]
    public void Name_SetEmptyString_ThrowsArgumentException()
    {
      // Arrange
      ESAliasNameProvider provider = new ESAliasNameProvider();

      // Act & Assert
      Assert.Throws<ArgumentException>(() => provider.Name = string.Empty);
    }

    /// <summary>
    /// Verify that setting Name to a whitespace string throws ArgumentException.
    /// </summary>
    [Theory]
    [InlineData(" ")]
    [InlineData("  ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("   \t\n")]
    public void Name_SetWhitespace_ThrowsArgumentException(string whitespace)
    {
      // Arrange
      ESAliasNameProvider provider = new ESAliasNameProvider();

      // Act & Assert
      Assert.Throws<ArgumentException>(() => provider.Name = whitespace);
    }

    /// <summary>
    /// Verify that the Name property can be set multiple times with different valid values.
    /// </summary>
    [Fact]
    public void Name_SetMultipleTimes_ReturnsLatestValue()
    {
      // Arrange
      ESAliasNameProvider provider = new ESAliasNameProvider();

      // Act
      provider.Name = "first-alias";
      provider.Name = "second-alias";
      provider.Name = "final-alias";

      // Assert
      Assert.Equal("final-alias", provider.Name);
    }

    /// <summary>
    /// Verify that the Name property correctly handles values with special characters.
    /// </summary>
    [Theory]
    [InlineData("alias-with-dashes")]
    [InlineData("alias_with_underscores")]
    [InlineData("alias.with.dots")]
    [InlineData("alias123")]
    [InlineData("UPPERCASE-ALIAS")]
    [InlineData("MixedCase-Alias")]
    public void Name_SetValueWithSpecialCharacters_ReturnsValue(string aliasName)
    {
      // Arrange
      ESAliasNameProvider provider = new ESAliasNameProvider();

      // Act
      provider.Name = aliasName;

      // Assert
      Assert.Equal(aliasName, provider.Name);
    }

    /// <summary>
    /// Verify that accessing an uninitialized Name property throws InvalidOperationException.
    /// </summary>
    [Fact]
    public void Name_NotInitialized_ThrowsInvalidOperationException()
    {
      // Arrange
      ESAliasNameProvider provider = new ESAliasNameProvider();

      // Act and Assert
      Assert.Throws<InvalidOperationException>(() => { var name = provider.Name; });
    }
  }
}
