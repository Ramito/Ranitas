using Microsoft.Xna.Framework;
using Ranitas.Core;
using Ranitas.Core.ECS;
using Ranitas.Pond;
using System.Collections.Generic;

namespace Ranitas.Sim
{
    public sealed class LilyCollisionSystem : ISystem
    {
        public LilyCollisionSystem(PondSimState pond)
        {
            const int kExpectedFrogCount = 4;   //TODO: Data drive or at least manage at a higher level!
            mPond = pond;
            mLandingFrogs = new List<Entity>(kExpectedFrogCount);
            mLandingPositions = new List<Vector2>(kExpectedFrogCount);
            mLandingRects = new List<Rect>(kExpectedFrogCount);
        }

        private PondSimState mPond;
        private readonly List<Entity> mLandingFrogs;
        private readonly List<Vector2> mLandingPositions;
        private readonly List<Rect> mLandingRects;  //We are using the rect position to compute l;anding, so we need to update the rect it self if we update the position!

        private struct FallingEntities
        {
            public SliceEntityOutput Entities;
            public SliceRequirementOutput<Velocity> Velocities;
            public SliceRequirementOutput<Position> Positions;
            public SliceRequirementOutput<Rect> Rects;
            public SliceRequirement<Gravity> Falling;
        }
        private FallingEntities mFalling = new FallingEntities();

        public void Initialize(EntityRegistry registry, EventSystem eventSystem)
        {
            registry.SetupSlice(ref mFalling);
        }

        public void Update(EntityRegistry registry, EventSystem eventSystem)
        {
            int fallingCount = mFalling.Entities.Count;
            for (int i = 0; i < fallingCount; ++i)
            {
                if (mFalling.Velocities[i].Value.Y <= 0f)
                {
                    Rect frogRect = mFalling.Rects[i];
                    foreach (LilyPadSimState lilypad in mPond.Lilies)
                    {
                        if (frogRect.Intersects(lilypad.Rect))
                        {
                            float landedFeet = mFalling.Rects[i].MinY;
                            float offset = lilypad.Rect.MaxY - landedFeet;
                            Vector2 newPosition = mFalling.Positions[i].Value;
                            newPosition.Y = newPosition.Y + offset;
                            Rect newRect = new Rect(newPosition, frogRect.Width, frogRect.Height);
                            mLandingFrogs.Add(mFalling.Entities[i]);
                            mLandingPositions.Add(newPosition);
                            mLandingRects.Add(newRect);
                            break;
                        }
                    }
                }
            }
            for (int i = 0; i < mLandingFrogs.Count; ++i)
            {
                Entity landedFrog = mLandingFrogs[i];
                if (!registry.HasComponent<Landed>(landedFrog))
                {
                    registry.AddComponent(landedFrog, new Landed());
                }
                registry.SetComponent(landedFrog, new Velocity());
                registry.SetComponent(landedFrog, new Position(mLandingPositions[i]));
                registry.SetComponent(landedFrog, mLandingRects[i]);
            }
            mLandingFrogs.Clear();
            mLandingPositions.Clear();
            mLandingRects.Clear();
            //TODO: CHECK LANDING FROMGS THAT NO LONGER TOUCH THE LILLY!!
        }
    }
}
