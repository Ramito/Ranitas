namespace Ranitas.Sim
{
    public enum ToungueStages
    {
        Refreshing = 0,
        Retracting,
        Extended,
        Extending,
    }

    public struct ToungueState
    {
        public ToungueState(ToungueStages stage, float timeLeft)
        {
            Stage = stage;
            TimeLeft = timeLeft;
        }

        public ToungueStages Stage;
        public float TimeLeft;
    }
}
