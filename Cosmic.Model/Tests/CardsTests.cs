using System.Linq;
using Cosmic.Model;
using FluentAssertions;
using Xunit;

namespace Tests
{
    public class CardsTests
    {
        [Fact]
        public void ShouldBe39AttackCards()
        {
            Cards.AttackCards.Count().Should().Be(39);
        }
    }
}
