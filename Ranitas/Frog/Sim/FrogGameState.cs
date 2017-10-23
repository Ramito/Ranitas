using Microsoft.Xna.Framework;

namespace Ranitas.Frog.Sim
{
    public class FrogGameState
    {
        public enum States
        {
            Grounded,
            Airborne,
            Swimming,
        }
        public States State;
        public Vector2 InputDirection;
        public float JumpPercentage;
    }
}
