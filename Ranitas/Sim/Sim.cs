using Microsoft.Xna.Framework;
using Ranitas.Core;
using Ranitas.Frog;
using Ranitas.Pond;
using System.Collections.Generic;
using static Ranitas.Frog.FrogSimState;

namespace Ranitas.Sim
{
    public sealed class RanitasSim
    {
        public readonly static Vector2 kGravity = new Vector2(0f, -980f);    //TODO: Data drive!

        private PondSimState mPondState;
        private List<FrogSimState> mFrogStates;

        private FixedTimeStepDynamics mDynamics;

        private List<Rect> mFrameLilyRects;

        public RanitasSim(PondSimState pondState, List<FrogSimState> frogStates, float fixedTimeStep)
        {
            mPondState = pondState;
            mFrogStates = frogStates;
            mDynamics = new FixedTimeStepDynamics(fixedTimeStep);
            mFrameLilyRects = new List<Rect>(pondState.Lilies.Count);
        }

        public void Update()
        {
            UpdateLilies();
            UpdateFrogs();
        }

        private void UpdateLilies()
        {
            mFrameLilyRects.Clear();
            foreach (var lilyState in mPondState.Lilies)
            {
                Rect lilyRect = new Rect(lilyState.Position, lilyState.Width, lilyState.Height);
                mFrameLilyRects.Add(lilyRect);
            }
        }

        private void UpdateFrogs()
        {
            foreach (var frog in mFrogStates)
            {
                UpdateFrog(frog);
            }
        }

        private void UpdateFrog(FrogSimState frog)
        {
            if (frog.State != FrogState.Swimming)
            {
                UpdateDryFrogDynamics(frog);
            }
            else
            {
                UpdateSwimingFrogDynamics(frog);
            }
        }

        private void UpdateDryFrogDynamics(FrogSimState frog)
        {
            Vector2 prospectDelta = mDynamics.ComputeFramePositionChange(kGravity, frog.Velocity);
            Vector2 prospectPosition = frog.Position + prospectDelta;
            Rect prospectRect = new Rect(prospectPosition, frog.Width, frog.Height);
            for (int i = 0; i < mFrameLilyRects.Count; ++i)
            {
                Rect lilyRect = mFrameLilyRects[i];
                if (prospectRect.Intersects(lilyRect))
                {
                    //Assuming only one collision is possible!
                    float landingY = lilyRect.MaxY;
                    Vector2 inheritedVelocity = mPondState.Lilies[i].Velocity;
                    frog.FeetPosition = new Vector2(prospectPosition.X, landingY);
                    frog.Velocity = inheritedVelocity;
                    frog.State = FrogState.Grounded;
                    return;
                }
            }
            frog.Position = prospectPosition;
            frog.Velocity = frog.Velocity + mDynamics.ComputeFrameVelocityChange(kGravity);
            if (frog.FeetPosition.Y < mPondState.WaterLevel)
            {
                frog.State = FrogState.Swimming;
            }
            else
            {
                frog.State = FrogState.Airborne;
            }
        }

        private void UpdateSwimingFrogDynamics(FrogSimState frog)
        {
            //THIS IS NOT IMPLEMENTED YET!
            frog.Velocity = Vector2.Zero;
        }
    }
}
