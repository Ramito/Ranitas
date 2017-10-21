using Ranitas.Frog.Sim;

namespace Ranitas
{
    public sealed class PlayerBinding
    {
        public readonly int PlayerIndex;
        public readonly FrogSimState Frog;

        public PlayerBinding(int index, FrogSimState frog)
        {
            PlayerIndex = index;
            Frog = frog;
        }
    }
}
