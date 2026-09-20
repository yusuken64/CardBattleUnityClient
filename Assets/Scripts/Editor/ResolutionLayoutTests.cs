using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace MonsterGirl.Tests
{
    public class ResolutionLayoutTests
    {
        [TestCase(1280, 720)]
        [TestCase(1920, 1080)]
        [TestCase(2560, 1440)]
        [TestCase(3840, 2160)]
        [TestCase(1920, 1200)]
        [TestCase(3440, 1440)]
        [TestCase(1080, 1920)]
        public void GameAreaIsCenteredAndKeepsBoardAspect(int width, int height)
        {
            var rect = GameViewport.Fit(width, height);
            Assert.That(rect.width / rect.height, Is.EqualTo(16f / 9f).Within(.00001f));
            Assert.That(rect.center.x, Is.EqualTo(width / 2f).Within(.001f));
            Assert.That(rect.center.y, Is.EqualTo(height / 2f).Within(.001f));
            Assert.That(rect.xMin, Is.GreaterThanOrEqualTo(-.001f));
            Assert.That(rect.yMin, Is.GreaterThanOrEqualTo(-.001f));
            Assert.That(rect.xMax, Is.LessThanOrEqualTo(width + .001f));
            Assert.That(rect.yMax, Is.LessThanOrEqualTo(height + .001f));
        }

        [Test]
        public void ResolutionChoicesDeduplicateBeforeIndicesAreAssigned()
        {
            var choices = VideoSettingsManager.BuildResolutionOptions(new[] {
                new Vector2Int(1920,1080), new Vector2Int(1920,1080), new Vector2Int(2560,1440) },
                new Vector2Int(2560,1440), new Vector2Int(1500,900));
            Assert.That(choices.Distinct().Count(), Is.EqualTo(choices.Length));
            Assert.That(choices, Does.Contain(new Vector2Int(1280,720)));
            Assert.That(choices, Does.Contain(new Vector2Int(1500,900)));
            var requested = new Vector2Int(1920,1080);
            Assert.That(choices[VideoSettingsManager.FindResolutionIndex(choices, requested)], Is.EqualTo(requested));
            Array.Reverse(choices);
            Assert.That(choices[VideoSettingsManager.FindResolutionIndex(choices, requested)], Is.EqualTo(requested));
        }

        [Test]
        public void MissingMonitorModesStillProvideAUsableFallback()
        {
            var choices = VideoSettingsManager.BuildResolutionOptions(Array.Empty<Vector2Int>(), Vector2Int.zero, Vector2Int.zero);
            Assert.That(choices, Does.Contain(new Vector2Int(1920,1080)));
            Assert.That(choices.All(r => r.x > 0 && r.y > 0), Is.True);
            Assert.That(VideoSettingsManager.FindResolutionIndex(choices, new Vector2Int(9999,9999)),
                Is.InRange(0, choices.Length - 1));
        }

        [TestCase(20, 80, 0)]
        [TestCase(-20, 30, 20)]
        [TestCase(80, 130, -30)]
        [TestCase(0, 200, -50)]
        public void PopupBoundsIncludeScaledSizeAndCenterOversizedContent(float min, float max, float expected)
        {
            Assert.That(KeepInScreen.Correction(min, max, 0, 100), Is.EqualTo(expected).Within(.001f));
        }
    }
}
