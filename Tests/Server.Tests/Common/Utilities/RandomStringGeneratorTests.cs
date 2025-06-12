using System;
using Server.Common.Utilities;
using Xunit;

namespace Server.Tests.Common.Utilities
{
    public class RandomStringGeneratorTests
    {
        [Theory]
        [InlineData(8, RandomStringGenerator.CharSet.Uppercase)]
        [InlineData(12, RandomStringGenerator.CharSet.Lowercase)]
        [InlineData(10, RandomStringGenerator.CharSet.Numbers)]
        [InlineData(16, RandomStringGenerator.CharSet.Uppercase | RandomStringGenerator.CharSet.Lowercase)]
        [InlineData(20, RandomStringGenerator.CharSet.Uppercase | RandomStringGenerator.CharSet.Lowercase | RandomStringGenerator.CharSet.Numbers)]
        public void Generate_ReturnsStringOfCorrectLength(int length, RandomStringGenerator.CharSet charSet)
        {
            var result = RandomStringGenerator.Generate(length, charSet);
            Assert.Equal(length, result.Length);
        }

        [Fact]
        public void Generate_ThrowsException_WhenLengthIsZeroOrNegative()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => RandomStringGenerator.Generate(0, RandomStringGenerator.CharSet.Uppercase));
            Assert.Throws<ArgumentOutOfRangeException>(() => RandomStringGenerator.Generate(-1, RandomStringGenerator.CharSet.Uppercase));
        }

        [Fact]
        public void Generate_ThrowsException_WhenNoCharSetSpecified()
        {
            Assert.Throws<ArgumentException>(() => RandomStringGenerator.Generate(8, 0));
        }

        [Fact]
        public void Generate_DoesNotContainConfusingCharacters()
        {
            var charSet = RandomStringGenerator.CharSet.Uppercase | RandomStringGenerator.CharSet.Lowercase | RandomStringGenerator.CharSet.Numbers;
            var result = RandomStringGenerator.Generate(1000, charSet);
            Assert.DoesNotContain('l', result);
            Assert.DoesNotContain('1', result);
            Assert.DoesNotContain('0', result);
            Assert.DoesNotContain('O', result);
            Assert.DoesNotContain('I', result);
            Assert.DoesNotContain('o', result);
            Assert.DoesNotContain('i', result);
        }
    }
}
