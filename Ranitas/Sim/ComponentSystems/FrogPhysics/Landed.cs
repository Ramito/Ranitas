namespace Ranitas.Sim
{
    public struct Landed
    {
        public Landed(float relativeJumpPower, int facingDirection)
        {
            RelativeJumpPower = relativeJumpPower;
            FacingDirection = facingDirection;  //TODO: Should be on a different component?
        }

        public float RelativeJumpPower;
        public int FacingDirection;
    }
}
