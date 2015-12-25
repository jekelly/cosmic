using Cosmic.Model;
using FluentAssertions;
using Xunit;

namespace Tests
{
    public class FlareDeckTests
    {
        private readonly IRandom r = new TestRandom();

        [Fact]
        public void Test()
        {
            FlareDeck fd = new FlareDeck();
            fd.Shuffle(r);
            fd.Draw().Should().NotBeNull();
        }
    }
}
