using BrokenLinkChecker.DocumentParsing.ModularLinkExtraction.FastParse;
using Xunit;

namespace BrokenLinkChecker.Tests.DocumentParsing.ModularLinkExtraction.FastParse;

public class QuotePositionTests
{
    [Theory]
    [InlineData(0, 0, true)]
    [InlineData(1, 1, true)]
    [InlineData(8191, 8191, true)]  // Max values (2^13 - 1)
    [InlineData(0, 0, false)]
    public void Constructor_SetsValuesCorrectly(int start, int length, bool isValid)
    {
        // Act
        var position = new QuotePosition(start, length, isValid);

        // Assert
        Assert.Equal(start, position.Start);
        Assert.Equal(length, position.Length);
        Assert.Equal(isValid, position.IsValid);
    }
    

    [Fact]
    public void MultipleInstances_DoNotInterfere()
    {
        // Arrange
        var pos1 = new QuotePosition(100, 200, true);
        var pos2 = new QuotePosition(300, 400, false);

        // Assert
        Assert.Equal(100, pos1.Start);
        Assert.Equal(200, pos1.Length);
        Assert.True(pos1.IsValid);

        Assert.Equal(300, pos2.Start);
        Assert.Equal(400 & 0x1FFF, pos2.Length);
        Assert.False(pos2.IsValid);
    }

    [Theory]
    [InlineData(0b1111_1111_1111, 0)]  // All 1s in start position
    [InlineData(0, 0b1111_1111_1111)]  // All 1s in length position
    [InlineData(0b1111_1111_1111, 0b1111_1111_1111)]  // All 1s in both
    public void BitBoundaries_HandledCorrectly(int start, int length)
    {
        // Act
        var position = new QuotePosition(start, length, true);

        // Assert
        Assert.Equal(start & 0x1FFF, position.Start);
        Assert.Equal(length & 0x1FFF, position.Length);
        Assert.True(position.IsValid);
    }

    [Fact]
    public void DefaultValue_IsInvalid()
    {
        // Act
        var position = default(QuotePosition);

        // Assert
        Assert.Equal(0, position.Start);
        Assert.Equal(0, position.Length);
        Assert.False(position.IsValid);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(8191, 8191)]
    public void ValidFlag_PreservedWithDifferentValues(int start, int length)
    {
        // Act
        var validPosition = new QuotePosition(start, length, true);
        var invalidPosition = new QuotePosition(start, length, false);

        // Assert
        Assert.True(validPosition.IsValid);
        Assert.False(invalidPosition.IsValid);
        Assert.Equal(start, validPosition.Start);
        Assert.Equal(length, validPosition.Length);
        Assert.Equal(start, invalidPosition.Start);
        Assert.Equal(length, invalidPosition.Length);
    }

    [Fact]
    public void BoundaryValues_DoNotAffectValidFlag()
    {
        // Arrange
        const int maxValue = 0x1FFF;

        // Act
        var position = new QuotePosition(maxValue, maxValue, true);

        // Assert
        Assert.True(position.IsValid);
        Assert.Equal(maxValue, position.Start);
        Assert.Equal(maxValue, position.Length);
    }
}