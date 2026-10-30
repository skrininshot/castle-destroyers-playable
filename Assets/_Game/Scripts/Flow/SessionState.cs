namespace CastleDestroyers.Flow
{
    /// <summary>Состояния сессии (ТЗ §3.1). Чья очередь — в SessionFlow.ActiveSide.</summary>
    public enum SessionState
    {
        Boot,
        Intro,
        Aim,
        Fire,
        Flight,
        Resolve,
        Victory,
        Defeat,
        EndCard
    }

    /// <summary>Сторона дуэли: игрок (синие, слева) или ИИ (красные, справа).</summary>
    public enum Side
    {
        Player,
        Enemy
    }

    /// <summary>Почему показан EndCard.</summary>
    public enum EndReason
    {
        None,
        Victory,
        Defeat,
        Limit
    }
}
