using Microsoft.Xna.Framework;

namespace Ranitas.Core
{
    public class FixedTimeStepDynamics
    {
        private readonly float mFixedTimeStep;
        private readonly float mHalfFixedTimeStepSquared;
        private readonly float mOneOverFixedTimeStep;

        public FixedTimeStepDynamics(float fixedTimeStep)
        {
            mFixedTimeStep = fixedTimeStep;
            mHalfFixedTimeStepSquared = (fixedTimeStep * fixedTimeStep) * 0.5f;
            mOneOverFixedTimeStep = 1f / mFixedTimeStep;
        }

        public Vector2 ComputeFrameVelocityChange(Vector2 frameAcceleration)
        {
            return mFixedTimeStep * frameAcceleration;
        }

        public Vector2 ComputeFramePositionChange(Vector2 frameAcceleration, Vector2 frameVelocity)
        {
            Vector2 velocityContribution = mFixedTimeStep * frameVelocity;
            Vector2 accelerationContribution = mHalfFixedTimeStepSquared * frameAcceleration;
            return velocityContribution + accelerationContribution;
        }
    }
}
