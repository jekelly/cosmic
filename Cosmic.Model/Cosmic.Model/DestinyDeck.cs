using System.Linq;

namespace Cosmic.Model
{
    public class DestinyDeck : Deck<IDestinyCard>
    {
        public DestinyDeck(params IPlayer[] players)
            : base(players.SelectMany(player => new IDestinyCard[]
            {
            new ColorDestinyCard(player),
            new ColorDestinyCard(player),
            new ColorDestinyCard(player)
            }).ToArray())
        {
        }
    }
}
