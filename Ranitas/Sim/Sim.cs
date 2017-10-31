using Ranitas.Core;
using Ranitas.Core.EventSystem;
using Ranitas.Data;
using Ranitas.Frog.Sim;
using Ranitas.Input;
using Ranitas.Insects;
using Ranitas.Pond;
using System.Collections.Generic;

namespace Ranitas.Sim
{
    public sealed class RanitasSim
    {
        private EventSystem mEventSystem;

        private PondSimState mPondState;
        public FlySim FlySim;
        public FrogSim FrogSim;

        private FixedTimeStepDynamics mDynamics;    //TODO: Shareable service!

        public RanitasSim(FlyData flyData, PondSimState pondState, List<FrogSimState> frogStates, float fixedTimeStep, PlayerBinding[] bindings)
        {
            mEventSystem = new EventSystem();
            mPondState = pondState;
            mDynamics = new FixedTimeStepDynamics(fixedTimeStep);
            FlySim = new FlySim(flyData, mPondState, mDynamics);
            FrogSim = new FrogSim(fixedTimeStep, mEventSystem, bindings, frogStates);
        }

        public void Update(FrogInput[] inputs)
        {
            FrogSim.Update(inputs);
            FrogSim.UpdateFrogs(mPondState, mDynamics);
            FlySim.Update();
            UpdateFlyEating();
        }

        public void UpdateFlyEating()
        {
            foreach (var frog in FrogSim.FrogStates)
            {
                if (!frog.Toungue.ToungueActive)
                {
                    continue;
                }
                Rect toungue = frog.GetToungueRect();
                for (int i = FlySim.ActiveFlies.Count - 1; i >= 0; --i)
                {
                    var fly = FlySim.ActiveFlies[i];
                    Rect flyRect = new Rect(fly.Position, fly.Width, fly.Height);
                    if (flyRect.Intersects(toungue))
                    {
                        FlySim.DespawnFly(fly);
                    }
                }
            }
        }
    }
}
