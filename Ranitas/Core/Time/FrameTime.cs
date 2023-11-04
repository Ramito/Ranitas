namespace Ranitas.Core
{
    public class FrameTime
    {
        public FrameTime(float deltaTime)
        {
            DeltaTime = deltaTime;
            HalfDeltaSquaredTime = deltaTime * deltaTime * 0.5f;
            CurrentGameTime = 0f;
        }

        public float DeltaTime;
        public float HalfDeltaSquaredTime;
        public float CurrentGameTime;
    }
}
