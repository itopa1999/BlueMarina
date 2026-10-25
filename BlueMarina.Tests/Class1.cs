using Xunit;

namespace BlueMarina.Tests
{
    // 1. A simple class to test
    public class Calculator
    {
        public int Add(int a, int b)
        {
            return a + b;
        }
    }

    // 2. The Test Class
    public class SimpleTests
    {
        [Fact] // This attribute tells the test runner this is a test
        public void Add_SimpleValues_ReturnsSum()
        {
            // Arrange (Setup)
            var calculator = new Calculator();
            int a = 2;
            int b = 3;

            // Act (Do the work)
            var result = calculator.Add(a, b);

            // Assert (Check the result)
            Assert.Equal(5, result);
        }
    }
}
