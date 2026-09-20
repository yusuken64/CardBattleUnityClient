using System;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace MonsterGirl.Tests
{
    public class CardArtNetworkTests
    {
        private static byte[] Encode(Sprite sprite) => (byte[])typeof(CardArtNetworkService)
            .GetMethod("EncodeSpriteToPng", BindingFlags.Static | BindingFlags.NonPublic)
            .Invoke(null, new object[] { sprite });

        [Test]
        public void WhirlwindArtFitsServerMessageLimit()
        {
            var definition = AssetDatabase.LoadAssetAtPath<SpellCardDefinition>("Assets/Prefab/SpellDefinitions/Whirlwind.asset");
            var bytes = Encode(definition.Sprite);
            var frame = JsonConvert.SerializeObject(new
            {
                type = 1, invocationId = "123", target = "SubmitCardArt",
                arguments = new object[] { "PKY9-WM", definition.ID, bytes }
            }) + '\u001e';
            Assert.LessOrEqual(bytes.Length, 512 * 1024);
            Assert.LessOrEqual(Encoding.UTF8.GetByteCount(frame), 1024 * 1024);
            Debug.Log($"ART SIZE VERIFIED: Whirlwind PNG={bytes.Length}, complete message={Encoding.UTF8.GetByteCount(frame)} bytes");
        }

        [Test]
        public void PoorlyCompressibleArtIsReducedUntilItFits()
        {
            var texture = new Texture2D(512, 512, TextureFormat.RGBA32, false);
            var random = new System.Random(7);
            var pixels = new byte[512 * 512 * 4];
            random.NextBytes(pixels);
            texture.LoadRawTextureData(pixels);
            texture.Apply();
            var sprite = Sprite.Create(texture, new Rect(0, 0, 512, 512), Vector2.one * 0.5f);
            var decoded = new Texture2D(2, 2);
            try
            {
                var bytes = Encode(sprite);
                Assert.LessOrEqual(bytes.Length, 512 * 1024);
                Assert.IsTrue(decoded.LoadImage(bytes));
                Assert.Less(decoded.width, 512);
                Assert.AreEqual(decoded.width, decoded.height);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(decoded);
                UnityEngine.Object.DestroyImmediate(sprite);
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        [Test]
        public void OversizedInvocationFailsBeforeSendingAndReleasesPendingCall()
        {
            var client = new MiniSignalRClient("http://localhost:5299/hubs/match");
            // UTF-8 byte count matters: this is below the character limit but above the byte limit.
            var exception = Assert.Throws<InvalidOperationException>(() => client.InvokeAsync<object>(
                "SubmitCardArt", new string('\u3042', 400000)).GetAwaiter().GetResult());
            StringAssert.Contains("Message was not sent", exception.Message);
            var pending = typeof(MiniSignalRClient).GetField("_pendingInvocations", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(client);
            Assert.AreEqual(0, pending.GetType().GetProperty("Count").GetValue(pending));
        }

        [MenuItem("Tools/Network/Validate Art Message Limits %#F11")]
        public static void Run()
        {
            var tests = new CardArtNetworkTests();
            tests.WhirlwindArtFitsServerMessageLimit();
            tests.PoorlyCompressibleArtIsReducedUntilItFits();
            tests.OversizedInvocationFailsBeforeSendingAndReleasesPendingCall();
            Debug.Log("ART MESSAGE LIMIT TESTS PASSED (3)");
        }
    }
}
