using Microsoft.Xna.Framework;
using Ranitas.Core;
using Ranitas.Data;
using System;
using static Ranitas.Frog.Sim.FrogSim;

namespace Ranitas.Frog.Sim
{
    public sealed class FrogSimState
    {
        public readonly FrogData Prototype;

        public readonly RigidBodyState RigidBodyState;

        public FrogControlState ControlState = new FrogControlState();
        public int ToungueDirection;

        public float SwimKickPhase;

        public float TimePreparingJump;

        public readonly FrogToungue Toungue;
        public Rect GetToungueRect()
        {
            Rect frogRect = RigidBodyState.Rect;
            Vector2 extendDirection;
            Vector2 anchor;
            if (ToungueDirection >= 0f)
            {
                extendDirection = Vector2.UnitX;
                anchor = frogRect.MaxCorner;
            }
            else
            {
                extendDirection = -Vector2.UnitX;
                anchor = frogRect.MinMaxCorner;
            }
            anchor -= (Prototype.ToungueThickness * 0.25f * Vector2.UnitY);
            Vector2 otherCorner = anchor + Prototype.ToungueLength * Toungue.RelativeLength * extendDirection - (Prototype.ToungueThickness * Vector2.UnitY);
            return new Rect(anchor, otherCorner);
        }

        public FrogSimState(FrogData data)
        {
            Prototype = data;
            RigidBodyState = new RigidBodyState(data);
            ControlState.State = FrogControlState.States.Grounded;
            TimePreparingJump = 0f;
            Toungue = new FrogToungue(data);
        }
    }
}
