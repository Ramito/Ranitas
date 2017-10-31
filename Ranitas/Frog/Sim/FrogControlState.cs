using Microsoft.Xna.Framework;

namespace Ranitas.Frog.Sim
{
    public class FrogControlState
    {
        public enum States
        {
            Grounded,
            Airborne,
            Swimming,
        }
        public States State;
        public Vector2 InputDirection;
        public float RelativeJumpPower;
        public bool ToungueSignalState;
        public int FacingDirection;
    }
}
