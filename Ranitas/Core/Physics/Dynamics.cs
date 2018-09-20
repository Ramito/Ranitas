using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;

namespace Ranitas.Core.Physics
{
    public static class Dynamics
    {
        public static Vector2 NewtonianPositionDelta(Vector2 frameVelocity, Vector2 acceleration, FrameTime time)
        {
            Vector2 velocityContribution = time.DeltaTime * frameVelocity;
            Vector2 accelerationContribution = time.HalfDeltaSquaredTime * acceleration;
            return velocityContribution + accelerationContribution;
        }

        public static Vector2 NewtonianVelocityDelta(Vector2 acceleration, FrameTime time)
        {
            return time.DeltaTime * acceleration;
        }

        public static Vector2 LinearDragVelocityDelta(Vector2 frameVelocity, float dragCoefficient, Vector2 acceleration, FrameTime time)
        {
            Debug.Assert(dragCoefficient > 0f);
            float dragFactor = (float)Math.Exp(-dragCoefficient * time.DeltaTime);
            float accelerationModule = acceleration.Length();
            Vector2 terminalVelocity = (1f / dragCoefficient) * acceleration;
            return (dragFactor - 1f) * (frameVelocity - terminalVelocity);
        }

        public static Vector2 LinearDragPositionDelta(Vector2 frameVelocity, float dragCoefficient, Vector2 acceleration, FrameTime time)
        {
            Debug.Assert(dragCoefficient > 0f); //TODO: Enforce even if data is bad!
            float dragFactor = (float)Math.Exp(-dragCoefficient * time.DeltaTime);
            float accelerationModule = acceleration.Length();
            Vector2 terminalVelocity = (1f / dragCoefficient) * acceleration;
            return (terminalVelocity * time.DeltaTime) + ((frameVelocity - terminalVelocity) / dragCoefficient) * (1f - dragFactor);
        }
    }
}
