using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace MonsterGirl.Tests
{
    public class GameCanvasViewportTests
    {
        [Test]
        public void HierarchyNotificationDefersAnchorWritesAndIgnoresPopupCanvases()
        {
            var root = new GameObject("Viewport", typeof(RectTransform), typeof(Canvas));
            try
            {
                var panel = new GameObject("Panel", typeof(RectTransform));
                panel.transform.SetParent(root.transform, false);
                var viewport = root.AddComponent<GameCanvasViewport>();
                var rect = (RectTransform)panel.transform;
                var sentinel = new Vector2(.17f, .23f);
                rect.anchorMin = sentinel;

                var popup = new GameObject("Dropdown List", typeof(RectTransform), typeof(Canvas));
                popup.transform.SetParent(root.transform, false);
                var popupRect = (RectTransform)popup.transform;
                popupRect.anchorMin = new Vector2(.2f, .3f);
                popupRect.anchorMax = new Vector2(.7f, .8f);
                // Adding/removing dropdown objects must never mutate existing anchors
                // synchronously inside Unity's hierarchy notification.
                typeof(GameCanvasViewport).GetMethod("OnTransformChildrenChanged", BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(viewport, null);
                Assert.That(rect.anchorMin, Is.EqualTo(sentinel));
                typeof(GameCanvasViewport).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(viewport, null);
                Assert.That(popupRect.anchorMin, Is.EqualTo(new Vector2(.2f, .3f)));
                Assert.That(popupRect.anchorMax, Is.EqualTo(new Vector2(.7f, .8f)));

                rect.anchorMin = sentinel;
                Object.DestroyImmediate(popup);
                typeof(GameCanvasViewport).GetMethod("OnTransformChildrenChanged", BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(viewport, null);
                Assert.That(rect.anchorMin, Is.EqualTo(sentinel));
                typeof(GameCanvasViewport).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(viewport, null);
                Assert.That(rect.anchorMin.x, Is.InRange(0f, 1f));
            }
            finally { Object.DestroyImmediate(root); }
        }
    }
}
