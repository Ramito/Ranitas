namespace Ranitas.Sim
{
    public struct FrogShapeDeformationData
    {
        public FrogShapeDeformationData(Data.FrogData data)
        {
            JumpSquish = data.MovementData.JumpSquish;
        }

        public float JumpSquish;
    }
}
