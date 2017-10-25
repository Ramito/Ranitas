using Microsoft.Xna.Framework;
using System;

namespace Ranitas.Core
{
    public class FixedTimeStepDynamics
    {
        public readonly float FixedTimeStep;
        public readonly float HalfFixedTimeStepSquared;
        public readonly float OneOverFixedTimeStep;

        public FixedTimeStepDynamics(float fixedTimeStep)
        {
            FixedTimeStep = fixedTimeStep;
            HalfFixedTimeStepSquared = (fixedTimeStep * fixedTimeStep) * 0.5f;
            OneOverFixedTimeStep = 1f / FixedTimeStep;
        }

        public Vector2 FrameVelocityDelta(Vector2 frameAcceleration)
        {
            return FixedTimeStep * frameAcceleration;
        }

        public Vector2 FramePositionDelta(Vector2 frameAcceleration, Vector2 frameVelocity)
        {
            Vector2 velocityContribution = FixedTimeStep * frameVelocity;
            Vector2 accelerationContribution = HalfFixedTimeStepSquared * frameAcceleration;
            return velocityContribution + accelerationContribution;
        }

        public float LinearDragTerminalVelocity(float dragCoefficient, float acceleration)
        {
            return acceleration / dragCoefficient;
        }

        public Vector2 FrameLinearDragVelocity(Vector2 frameVelocity, float dragCoefficient, Vector2 acceleration)
        {
            if (dragCoefficient == 0f)  //TODO: "Small" checks!
            {
                return frameVelocity;
            }
            float dragFactor = (float)Math.Exp(-dragCoefficient * FixedTimeStep);
            float accelerationModule = acceleration.Length();
            Vector2 terminalVelocity = (1f / dragCoefficient) * acceleration;
            return (frameVelocity - terminalVelocity) * dragFactor + terminalVelocity;
        }

        public Vector2 FrameLinearDragPositionDelta(Vector2 frameVelocity, float dragCoefficient, Vector2 acceleration)
        {
            if (dragCoefficient == 0f)  //TODO: "Small" checks!
            {
                return frameVelocity * FixedTimeStep;
            }
            float dragFactor = (float)Math.Exp(-dragCoefficient * FixedTimeStep);
            float accelerationModule = acceleration.Length();
            Vector2 terminalVelocity = (1f / dragCoefficient) * acceleration;
            return (terminalVelocity * FixedTimeStep) + ((frameVelocity - terminalVelocity) / dragCoefficient) * (1f - dragFactor);
        }
    }
}
