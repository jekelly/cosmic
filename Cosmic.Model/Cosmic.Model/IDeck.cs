namespace Cosmic.Model
{
    public interface IDeck<T> where T : ICard
    {
        T Draw();
    }
}
