namespace Ranitas.Data
{
    public sealed class FlyData
    {
        public int MaxActiveFlies = 100;
        public float FliesPerSecond = 0.5f;

        public float Width;
        public float Height;

        public float MinHeight = 25f;
        public float MaxHeight = 400f;

        public float MinSpeed = 10f;
        public float MaxSpeed = 600f;
    }
}
