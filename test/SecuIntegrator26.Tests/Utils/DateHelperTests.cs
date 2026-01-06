using SecuIntegrator26.Shared.Utils;
using Xunit;

namespace SecuIntegrator26.Tests.Utils
{
    public class DateHelperTests
    {
        [Theory]
        [InlineData("112/01/01", "2023-01-01")]
        [InlineData("1120101", "2023-01-01")]
        [InlineData("113/02/29", "2024-02-29")] // Leap year
        public void ParseTwseDate_ValidDates_ReturnsCorrectDateTime(string input, string expected)
        {
            // Act
            var result = DateHelper.ParseTwseDate(input);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(DateTime.Parse(expected), result.Value);
        }

        [Theory]
        [InlineData("invalid")]
        [InlineData("112/13/01")]
        [InlineData("")]
        [InlineData(null)]
        public void ParseTwseDate_InvalidDates_ReturnsNull(string? input)
        {
            // Act
            var result = DateHelper.ParseTwseDate(input!);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void ToTwseDateString_ReturnsCorrectFormat()
        {
            // Arrange
            var date = new DateTime(2023, 1, 1);

            // Act
            var result = date.ToTwseDateString();

            // Assert
            Assert.Equal("112/01/01", result);
        }
    }
}
