using Ranitas.Core;
using Ranitas.Frog.Sim;
using Ranitas.Insects;
using Ranitas.Pond;
using Ranitas.Data;
using System.Collections.Generic;
using Ranitas.Input;

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

        public void Update(PlayerBinding[] playerBindings, FrogInput[] inputs)
        {
            foreach (var player in playerBindings)
            {
                if (player != null)
                {
                    FrogSim.UpdateFrogInputs(player.Frog, inputs[player.PlayerIndex], mDynamics.FixedTimeStep);
                }
            }
            FrogSim.UpdateFrogs(mFrogStates, mPondState, mDynamics);
            FlySim.Update();
            UpdateFlyEating();
        }

        public void UpdateFlyEating()
        {
            foreach (var frog in mFrogStates)
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
