using CastleDestroyers.Spike;
using NUnit.Framework;
using UnityEngine;

namespace CastleDestroyers.Tests
{
    public class SpikeBatchTests
    {
        private SpikeConfig config;

        [SetUp]
        public void SetUp()
        {
            config = ScriptableObject.CreateInstance<SpikeConfig>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(config);
        }

        [Test]
        public void Rockets_ActiveCountIsClampedToCapacity()
        {
            SpikeRockets rockets = new SpikeRockets();
            rockets.Init(config, new SpikeRng(1u));
            rockets.SetActiveCount(config.rocketCapacity + 500);
            Assert.AreEqual(config.rocketCapacity, rockets.ActiveCount);
            Object.DestroyImmediate(rockets.Mesh);
        }

        [Test]
        public void Rockets_ReductionCollapsesUnusedQuads()
        {
            SpikeRockets rockets = new SpikeRockets();
            rockets.Init(config, new SpikeRng(1u));
            rockets.SetActiveCount(10);
            rockets.Tick(0.016f);
            rockets.SetActiveCount(5);
            rockets.UploadMesh();
            Vector3[] vertices = rockets.Mesh.vertices;
            Assert.AreEqual(0f, vertices[5 * 4].x);
            Assert.AreEqual(0f, vertices[10 * 4 - 1].y);
            Assert.AreNotEqual(0f, vertices[0].x);
            Object.DestroyImmediate(rockets.Mesh);
        }

        [Test]
        public void Smoke_TickKeepsActiveCountAndHidesInactive()
        {
            SpikeSmoke smoke = new SpikeSmoke();
            smoke.Init(config, new SpikeRng(1u));
            smoke.SetActiveCount(20);
            for (int i = 0; i < 300; i++)
            {
                smoke.Tick(0.05f);
            }
            smoke.SetActiveCount(10);
            smoke.UploadMesh();
            Assert.AreEqual(10, smoke.ActiveCount);
            Assert.AreEqual(0, smoke.Mesh.colors32[15 * 4].a);
            Object.DestroyImmediate(smoke.Mesh);
        }
    }
}
