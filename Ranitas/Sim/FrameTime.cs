namespace Ranitas.Sim
{
    public class FrameTime
    {
        public FrameTime(float deltaTime)
        {
            DeltaTime = deltaTime;
            HalfDeltaSquaredTime = deltaTime * deltaTime * 0.5f;
        }

        public float DeltaTime { get; private set; }
        public float HalfDeltaSquaredTime { get; private set; }
    }
}
