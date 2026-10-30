using System;
using CastleDestroyers.Core;

namespace CastleDestroyers.Flow
{
    /// <summary>
    /// Стейт-машина сессии (ТЗ §3.1): Boot → Intro → [Aim → Fire → Flight → Resolve] по очереди сторон →
    /// Victory / Defeat → EndCard. Aim игрока заканчивается жестом или таймером простоя, Flight держится, пока
    /// ракеты в воздухе (FlightBusy), остальные переходы идут по таймерам. Потери пушек — заглушка из конфига до этапа 5.
    /// Лимиты ходов и времени, Director и счётчик ходов настоящие.
    /// </summary>
    public sealed class SessionFlow : ITickable
    {
        private const int MaxTransitionsPerTick = 8;
        private const float TerminalDuration = -1f;

        private GameConfig config;
        private MatchState match;

        public SessionState State { get; private set; }
        /// <summary>Чья очередь (для Victory / Defeat — победитель).</summary>
        public Side ActiveSide { get; private set; }
        public EndReason EndReason { get; private set; }
        public bool HasWinner { get; private set; }
        public Side Winner { get; private set; }
        /// <summary>Номер хода игрока с единицы; 0 до первого хода.</summary>
        public int PlayerTurn { get; private set; }
        /// <summary>Время в текущем состоянии, секунды.</summary>
        public float StateTime { get; private set; }
        /// <summary>Время сессии с момента Boot, секунды.</summary>
        public float SessionTime { get; private set; }

        /// <summary>Ракеты ещё летят: Flight не заканчивается, но не дольше flightMaxDuration.</summary>
        public bool FlightBusy { get; set; }

        /// <summary>Срабатывает после входа в новое состояние.</summary>
        public event Action<SessionState, Side> StateChanged;

        public Side Loser { get { return Winner == Side.Player ? Side.Enemy : Side.Player; } }
        public TurnHint CurrentHint { get { return config.GetHint(PlayerTurn - 1); } }
        public bool IsFinished { get { return State == SessionState.EndCard; } }

        public void Init(GameConfig gameConfig, MatchState matchState)
        {
            config = gameConfig;
            match = matchState;
            Restart();
        }

        public void Restart()
        {
            match.Reset();
            PlayerTurn = 0;
            SessionTime = 0f;
            EndReason = EndReason.None;
            HasWinner = false;
            Winner = Side.Player;
            EnterState(SessionState.Boot, Side.Player);
        }

        public void Tick(float dt)
        {
            if (IsFinished)
            {
                return;
            }
            StateTime += dt;
            SessionTime += dt;

            if (SessionTime >= config.maxSessionSeconds && !IsEnding(State))
            {
                FinishWith(EndReason.Limit);
                return;
            }

            for (int i = 0; i < MaxTransitionsPerTick; i++)
            {
                float duration = Duration(State, ActiveSide);
                if (duration < 0f || StateTime < duration)
                {
                    return;
                }
                bool flight = State == SessionState.Flight;
                if (flight && FlightBusy && StateTime < config.flightMaxDuration)
                {
                    return;
                }
                // Ожидание ракет не должно съедать время следующего состояния.
                float carry = flight ? 0f : StateTime - duration;
                Advance();
                StateTime = carry;
            }
        }

        /// <summary>Игрок ведёт жест: таймер простоя начинается заново.</summary>
        public void KeepAiming()
        {
            if (State == SessionState.Aim)
            {
                StateTime = 0f;
            }
        }

        /// <summary>Досрочный переход в следующее состояние (выстрел игрока и чит).</summary>
        public void Skip()
        {
            if (Duration(State, ActiveSide) < 0f)
            {
                return;
            }
            Advance();
        }

        /// <summary>Длительность состояния, секунды; отрицательная — терминальное.</summary>
        public float Duration(SessionState state, Side side)
        {
            switch (state)
            {
                case SessionState.Boot: return config.bootDuration;
                case SessionState.Intro: return config.introDuration;
                case SessionState.Aim: return side == Side.Player ? config.playerAimDuration : config.aiAimDuration;
                case SessionState.Fire: return config.fireDuration;
                case SessionState.Flight: return config.flightDuration;
                case SessionState.Resolve: return config.resolveDuration;
                case SessionState.Victory: return config.victoryDuration;
                case SessionState.Defeat: return config.defeatDuration;
                default: return TerminalDuration;
            }
        }

        private static bool IsEnding(SessionState state)
        {
            return state == SessionState.Victory || state == SessionState.Defeat || state == SessionState.EndCard;
        }

        private void Advance()
        {
            switch (State)
            {
                case SessionState.Boot:
                    EnterState(SessionState.Intro, Side.Player);
                    break;
                case SessionState.Intro:
                    StartPlayerTurn();
                    break;
                case SessionState.Aim:
                    EnterState(SessionState.Fire, ActiveSide);
                    break;
                case SessionState.Fire:
                    EnterState(SessionState.Flight, ActiveSide);
                    break;
                case SessionState.Flight:
                    EnterState(SessionState.Resolve, ActiveSide);
                    break;
                case SessionState.Resolve:
                    AfterResolve();
                    break;
                case SessionState.Victory:
                    FinishWith(EndReason.Victory);
                    break;
                case SessionState.Defeat:
                    FinishWith(EndReason.Defeat);
                    break;
            }
        }

        /// <summary>Проверка победы (ТЗ §3.1) и выбор следующего хода.</summary>
        private void AfterResolve()
        {
            if (match.IsEliminated(Side.Enemy))
            {
                HasWinner = true;
                Winner = Side.Player;
                EnterState(SessionState.Victory, Side.Player);
                return;
            }
            if (match.IsEliminated(Side.Player))
            {
                HasWinner = true;
                Winner = Side.Enemy;
                EnterState(SessionState.Defeat, Side.Enemy);
                return;
            }
            if (ActiveSide == Side.Player)
            {
                EnterState(SessionState.Aim, Side.Enemy);
            }
            else if (PlayerTurn >= config.maxPlayerTurns)
            {
                FinishWith(EndReason.Limit);
            }
            else
            {
                StartPlayerTurn();
            }
        }

        private void StartPlayerTurn()
        {
            PlayerTurn++;
            EnterState(SessionState.Aim, Side.Player);
        }

        private void FinishWith(EndReason reason)
        {
            EndReason = reason;
            EnterState(SessionState.EndCard, HasWinner ? Winner : ActiveSide);
        }

        private void EnterState(SessionState state, Side side)
        {
            State = state;
            ActiveSide = side;
            StateTime = 0f;
            if (state == SessionState.Resolve)
            {
                ApplyStubDamage();
            }
            if (StateChanged != null)
            {
                StateChanged(state, side);
            }
        }

        /// <summary>Заглушка потерь до этапа 5. Director: на последнем ходу игрока добивает врага, ИИ не убивает последнюю пушку игрока.</summary>
        private void ApplyStubDamage()
        {
            if (ActiveSide == Side.Player)
            {
                bool finisher = config.directorEnabled && PlayerTurn >= config.maxPlayerTurns;
                int loss = finisher ? match.EnemyAlive : config.stubEnemyLossPerPlayerTurn;
                match.Destroy(Side.Enemy, loss, 0);
            }
            else
            {
                int floor = config.directorEnabled ? 1 : 0;
                match.Destroy(Side.Player, config.stubPlayerLossPerAiTurn, floor);
            }
        }
    }
}
