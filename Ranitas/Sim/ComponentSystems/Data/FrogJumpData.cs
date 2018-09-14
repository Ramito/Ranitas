namespace Ranitas.Sim
{
    public struct FrogJumpData
    {
        public FrogJumpData(Data.FrogData data)
        {
            JumpPrepareTime = data.MovementData.JumpPrepareTime;
            JumpSpeed = data.MovementData.JumpVelocity;
        }

        public float JumpPrepareTime;
        public float JumpSpeed;
    }
}
