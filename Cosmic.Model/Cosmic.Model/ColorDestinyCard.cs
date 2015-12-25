namespace Cosmic.Model
{
    class ColorDestinyCard : IDestinyCard
    {
        private readonly IPlayer player;

        public ColorDestinyCard(IPlayer player)
        {
            this.player = player;
        }

        public IPlayer SelectPlayer(GameState game)
        {
            return this.player;
        }
    }
}
