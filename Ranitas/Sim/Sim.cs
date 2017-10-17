using Ranitas.Core;
using Ranitas.Frog.Sim;
using Ranitas.Insects;
using Ranitas.Pond;
using Ranitas.Data;
using System.Collections.Generic;

namespace Ranitas.Sim
{
    public sealed class RanitasSim
    {
        private PondSimState mPondState;
        private List<FrogSimState> mFrogStates;
        public FlySim FlySim;

        private FixedTimeStepDynamics mDynamics;    //TODO: Shareable service!

        public RanitasSim(FlyData flyData, PondSimState pondState, List<FrogSimState> frogStates, float fixedTimeStep)
        {
            mPondState = pondState;
            mFrogStates = frogStates;
            mDynamics = new FixedTimeStepDynamics(fixedTimeStep);
            FlySim = new FlySim(flyData, mPondState, mDynamics);
        }

        public void Update()
        {
            FrogSim.UpdateFrogs(mFrogStates, mPondState, mDynamics);
            FlySim.Update();
        }
    }
}
