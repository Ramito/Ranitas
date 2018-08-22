﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ranitas.Core.ECS;
using System;
using System.Collections.Generic;

namespace CoreUnitTests
{
    [TestClass]
    public class EntityRegistryTests
    {
        [TestMethod]
        public void TestEntityCreationAndDestruction()
        {
            const int kEntitiesToTest = 1000;
            int tests = 100;
            EntityRegistry registry = new EntityRegistry(kEntitiesToTest);

            Assert.IsFalse(registry.IsValid(Entity.NullEntity));

            List<Entity> validEntities = new List<Entity>(kEntitiesToTest * 100);
            List<Entity> invalidEntities = new List<Entity>(kEntitiesToTest * 100);
            for (int i = 0; i < kEntitiesToTest; ++i)
            {
                validEntities.Add(registry.Create());
            }

            Random randomizer = new Random(457);
            while (tests > 0)
            {
                --tests;
                for (int i = validEntities.Count - 1; i >= 0; --i)
                {
                    Entity entity = validEntities[i];
                    if (randomizer.NextDouble() < 0.01)
                    {
                        Assert.IsTrue(registry.IsValid(entity));
                        registry.Destroy(entity);
                        validEntities.RemoveAt(i);
                        invalidEntities.Add(entity);
                    }
                }
                for (int i = validEntities.Count; i < kEntitiesToTest; ++i)
                {
                    if (randomizer.NextDouble() < 0.01)
                    {
                        Entity replacement = registry.Create();
                        validEntities.Add(replacement);
                    }
                }
            }

            foreach (Entity entity in validEntities)
            {
                Assert.IsTrue(registry.IsValid(entity));
            }

            foreach (Entity entity in invalidEntities)
            {
                Assert.IsFalse(registry.IsValid(entity));
            }
        }

        #region Test Components
        public struct TagComponent
        {
        }

        public struct PositionComponent
        {
            public PositionComponent(float x, float y)
            {
                X = x;
                Y = y;
            }

            public readonly float X;
            public readonly float Y;
        }

        public struct ParentedComponent
        {
            public ParentedComponent(Entity parent)
            {
                Parent = parent;
            }

            public readonly Entity Parent;
        }
        #endregion

        [TestMethod]
        public void TestComponents()
        {
            EntityRegistry registry = new EntityRegistry(100);

            registry.RegisterComponentType<TagComponent>();
            registry.RegisterComponentType<PositionComponent>();
            registry.RegisterComponentType<ParentedComponent>();

            Entity entity1 = registry.Create();
            Entity entity2 = registry.Create();

            PositionComponent pos1 = new PositionComponent(1, 0);
            PositionComponent pos2 = new PositionComponent(0, 1);

            registry.AddComponent(entity1, new TagComponent());

            registry.AddComponent(entity1, pos1);
            registry.AddComponent(entity2, pos2);

            registry.AddComponent(entity2, new ParentedComponent(entity1));

            Assert.IsTrue(registry.HasComponent<TagComponent>(entity1));
            Assert.IsFalse(registry.HasComponent<TagComponent>(entity2));
            Assert.IsTrue(registry.HasComponent<PositionComponent>(entity1));
            Assert.IsTrue(registry.HasComponent<PositionComponent>(entity2));
            Assert.IsFalse(registry.HasComponent<ParentedComponent>(entity1));
            Assert.IsTrue(registry.HasComponent<ParentedComponent>(entity2));

            Assert.AreEqual(registry.GetComponent<PositionComponent>(entity1), pos1);
            Assert.AreEqual(registry.GetComponent<PositionComponent>(entity2), pos2);

            registry.Destroy(entity1);

            Assert.IsFalse(registry.IsValid(registry.GetComponent<ParentedComponent>(entity2).Parent));
            
            Assert.IsFalse(registry.HasComponent<TagComponent>(entity2));
            Assert.IsTrue(registry.HasComponent<PositionComponent>(entity2));
            Assert.IsTrue(registry.HasComponent<ParentedComponent>(entity2));

            Entity entity3 = registry.Create();
            Entity entity4 = registry.Create();
            registry.AddComponent(entity4, pos1);
            registry.AddComponent(entity3, pos1);
            registry.AddComponent(entity3, new ParentedComponent());
            registry.AddComponent(entity4, new ParentedComponent());
            Assert.IsFalse(registry.HasComponent<TagComponent>(entity3));
            Assert.IsTrue(registry.HasComponent<PositionComponent>(entity3));
            Assert.IsTrue(registry.HasComponent<PositionComponent>(entity4));
            Assert.IsTrue(registry.HasComponent<ParentedComponent>(entity3));
            Assert.IsTrue(registry.HasComponent<ParentedComponent>(entity4));

            Assert.IsFalse(registry.HasComponent<TagComponent>(entity2));
            Assert.IsTrue(registry.HasComponent<PositionComponent>(entity2));
            Assert.IsTrue(registry.HasComponent<ParentedComponent>(entity2));
        }
    }
}