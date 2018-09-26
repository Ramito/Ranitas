namespace Ranitas.Sim
{
    public struct AnimationState
    {
        public AnimationState(float min, float max)
        {
            MinX = min;
            MaxX = max;
        }

        public float MinX;
        public float MaxX;
    }
}
