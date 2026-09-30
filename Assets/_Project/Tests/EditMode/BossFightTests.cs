using AlmaDino.Core.Interfaces;
using AlmaDino.Core.Progression;
using AlmaDino.Features.Boss.Controllers;
using AlmaDino.Features.Boss.Models;
using AlmaDino.Features.Boss.Projectiles;
using NUnit.Framework;
using UnityEngine;

namespace AlmaDino.Tests.EditMode
{
    [TestFixture]
    public class BossFightTests
    {
        [SetUp]
        public void SetUp()
        {
            GameProgression.ResetProgression();
        }

        [TearDown]
        public void TearDown()
        {
            GameProgression.ResetProgression();
        }

        [Test]
        public void InitialBossState_IsIntro_AndHasZeroHits()
        {
            var go = new GameObject("TestBoss");
            var boss = go.AddComponent<GiantMonkeyBoss2D>();

            Assert.AreEqual(BossStateEnum.Intro, boss.CurrentState);
            Assert.AreEqual(0, boss.CurrentHits);
            Assert.AreEqual(3, boss.MaxHits);
            Assert.IsFalse(boss.IsDefeated);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void ExhaustionDuration_DecreasesAsBossTakesDamage()
        {
            var go = new GameObject("TestBoss");
            var boss = go.AddComponent<GiantMonkeyBoss2D>();
            boss.Configure(3, new float[] { 3.2f, 2.8f, 2.4f }, null, null);

            // Phase 1 (0 hits)
            Assert.AreEqual(3.2f, boss.CurrentExhaustionDuration, 0.01f);

            // Phase 2 (1 hit)
            boss.TransitionToState(BossStateEnum.TiredDescent);
            boss.OnHeadStomped();
            Assert.AreEqual(2.8f, boss.CurrentExhaustionDuration, 0.01f);

            // Phase 3 (2 hits)
            boss.TransitionToState(BossStateEnum.TiredDescent);
            boss.OnHeadStomped();
            Assert.AreEqual(2.4f, boss.CurrentExhaustionDuration, 0.01f);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void OnHeadStomped_InTiredDescent_IncrementsHits()
        {
            var go = new GameObject("TestBoss");
            var boss = go.AddComponent<GiantMonkeyBoss2D>();

            boss.TransitionToState(BossStateEnum.TiredDescent);
            boss.OnHeadStomped();

            Assert.AreEqual(1, boss.CurrentHits);
            Assert.AreEqual(BossStateEnum.HurtEnrage, boss.CurrentState);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void ThreeHeadStomps_TriggersDefeated_AndCompletesWorld1()
        {
            var go = new GameObject("TestBoss");
            var boss = go.AddComponent<GiantMonkeyBoss2D>();

            // Hit 1
            boss.TransitionToState(BossStateEnum.TiredDescent);
            boss.OnHeadStomped();
            Assert.AreEqual(1, boss.CurrentHits);

            // Hit 2
            boss.TransitionToState(BossStateEnum.TiredDescent);
            boss.OnHeadStomped();
            Assert.AreEqual(2, boss.CurrentHits);

            // Hit 3
            boss.TransitionToState(BossStateEnum.TiredDescent);
            boss.OnHeadStomped();
            Assert.AreEqual(3, boss.CurrentHits);
            Assert.AreEqual(BossStateEnum.Defeated, boss.CurrentState);
            Assert.IsTrue(boss.IsDefeated);
            Assert.IsTrue(GameProgression.IsWorldCompleted(1));

            Object.DestroyImmediate(go);
        }

        [Test]
        public void RollingFruitProjectile_ImplementsIHazard2D_AndInitializesCorrectly()
        {
            var go = new GameObject("TestFruit");
            go.AddComponent<CircleCollider2D>();
            var proj = go.AddComponent<RollingFruitProjectile2D>();

            Assert.IsInstanceOf<IHazard2D>(proj);

            proj.InitializeRolling(1f, 6.5f);
            Assert.AreEqual(FruitTrajectoryMode.Rolling, proj.TrajectoryMode);
            Assert.AreEqual(1f, proj.Direction);
            Assert.AreEqual(6.5f, proj.Speed);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void RollingFruitProjectile_LobbedMode_InitializesWithBounces()
        {
            var go = new GameObject("TestLobbedFruit");
            go.AddComponent<CircleCollider2D>();
            var proj = go.AddComponent<RollingFruitProjectile2D>();

            proj.InitializeLobbed(-1f, 7.5f, 6.0f, 3, 5.5f);
            Assert.AreEqual(FruitTrajectoryMode.LobbedBouncing, proj.TrajectoryMode);
            Assert.AreEqual(-1f, proj.Direction);
            Assert.AreEqual(7.5f, proj.Speed);
            Assert.AreEqual(0, proj.CurrentBounces);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void BossBodyHazard_ImplementsIHazard2D_AndTogglesActive()
        {
            var go = new GameObject("TestBossBody");
            go.AddComponent<BoxCollider2D>();
            var hazard = go.AddComponent<BossBodyHazard2D>();

            Assert.IsInstanceOf<IHazard2D>(hazard);
            Assert.IsTrue(hazard.IsActive);

            hazard.SetHazardActive(false);
            Assert.IsFalse(hazard.IsActive);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void GameProgression_CompleteWorld_PersistsAndClears()
        {
            Assert.IsFalse(GameProgression.IsWorldCompleted(1));

            GameProgression.CompleteWorld(1);
            Assert.IsTrue(GameProgression.IsWorldCompleted(1));
            Assert.IsFalse(GameProgression.IsWorldCompleted(2));

            GameProgression.ResetProgression();
            Assert.IsFalse(GameProgression.IsWorldCompleted(1));
        }
    }
}
