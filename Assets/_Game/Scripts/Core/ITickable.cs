namespace CastleDestroyers.Core
{
    /// <summary>Система, которую GameLoop обновляет в заданном порядке.</summary>
    public interface ITickable
    {
        void Tick(float dt);
    }
}
