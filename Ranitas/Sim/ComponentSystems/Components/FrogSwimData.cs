namespace Ranitas.Sim
{
    public struct FrogSwimData
    {
        public FrogSwimData(float kickDuration, float kickRecharge, float kickVelocity, float waterDrag, float density)
        {
            SwimKickDuration = kickDuration;
            SwimKickRecharge = kickRecharge;
            SwimKickVelocity = kickVelocity;
            WaterDrag = waterDrag;
            Density = density;
        }

        public float SwimKickDuration;
        public float SwimKickRecharge;
        public float SwimKickVelocity;
        public float WaterDrag;
        public float Density;
    }
}
