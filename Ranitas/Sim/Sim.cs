using Ranitas.Core;
using Ranitas.Frog.Sim;
using Ranitas.Pond;
using System.Collections.Generic;

namespace Ranitas.Sim
{
    public sealed class RanitasSim
    {
        private PondSimState mPondState;
        private List<FrogSimState> mFrogStates;

        private FixedTimeStepDynamics mDynamics;

        public RanitasSim(PondSimState pondState, List<FrogSimState> frogStates, float fixedTimeStep)
        {
            mPondState = pondState;
            mFrogStates = frogStates;
            mDynamics = new FixedTimeStepDynamics(fixedTimeStep);
        }

        public void Update()
        {
            FrogSim.UpdateFrogs(mFrogStates, mPondState, mDynamics);
        }
    }
}
