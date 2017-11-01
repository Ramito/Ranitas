using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Ranitas.Core;
using Ranitas.Core.Render;

namespace Ranitas.Insects
{
    public sealed class FlyRenderer
    {
        private FlySim mFlySim;

        public FlyRenderer(FlySim flySim)
        {
            mFlySim = flySim;
        }

        public void Render(PrimitiveRenderer renderer)
        {
            foreach (var fly in mFlySim.ActiveFlies)
            {
                Rect flyRect = new Rect(fly.Position, mFlySim.FlyData.Width, mFlySim.FlyData.Height);
                renderer.PushRect(flyRect, Color.Coral);
            }
            renderer.Render();
        }
    }
}
