using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Ranitas.Core;
using Ranitas.Core.Render;
using Ranitas.Frog.Sim;

namespace Ranitas.Frog
{
    public sealed class FrogRenderer
    {
        public void RenderFrog(FrogSimState frog, PrimitiveRenderer renderer)
        {
            renderer.PushRect(frog.RigidBodyState.Rect, Color.GreenYellow);
            float relativeToungueLength = frog.Toungue.RelativeLength;
            if (relativeToungueLength > 0f)
            {
                renderer.PushRect(frog.GetToungueRect(), Color.Red);
            }
        }
    }
}
