namespace Ranitas.Sim
{
    public struct FlyNoiseState
    {
        public FlyNoiseState(float xModule, float yModule, float xOscilation, float yOscilation)
        {
            XModulePhase = xModule;
            YModulePhase = yModule;
            XOscilationPhase = xOscilation;
            YOscilationPhase = yOscilation;
        }

        public float XModulePhase;
        public float YModulePhase;
        public float XOscilationPhase;
        public float YOscilationPhase;
    }
}
