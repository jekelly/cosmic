namespace Cosmic.Model
{
    public interface IDestinyCard : ICard
    {
        IPlayer SelectPlayer(GameState game);
    }
}
