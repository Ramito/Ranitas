using Microsoft.VisualStudio.TestTools.UnitTesting;
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
    }
}
