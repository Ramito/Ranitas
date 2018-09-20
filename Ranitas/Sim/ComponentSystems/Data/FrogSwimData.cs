namespace Ranitas.Sim
{
    public struct FrogSwimData
    {
        public FrogSwimData(Data.FrogData frogData)
        {
            SwimKickDuration = frogData.SwimKickDuration;
            SwimKickRecharge = frogData.SwimKickRecharge;
            SwimAccelerationModule = frogData.SwimKickVelocity * frogData.WaterDrag;
            WaterDrag = frogData.WaterDrag;
            Density = frogData.FrogDensity;
        }

        public float SwimKickDuration;
        public float SwimKickRecharge;
        public float SwimAccelerationModule;
        public float WaterDrag;
        public float Density;
    }
}
