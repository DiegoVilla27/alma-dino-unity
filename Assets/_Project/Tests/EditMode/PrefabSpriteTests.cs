using AlmaDino.Shared.Visuals;
using NUnit.Framework;
using UnityEngine;

namespace AlmaDino.Tests.EditMode
{
    public class PrefabSpriteTests
    {
        [Test]
        public void ReplacingArtworkPreservesColliderTransformAndVisibleSize()
        {
            var go = new GameObject("Platform");
            var texture = new Texture2D(64, 32);
            var placeholder = Sprite.Create(texture, new Rect(0, 0, 32, 32), Vector2.one * .5f, 32, 0, SpriteMeshType.FullRect);
            var artwork = Sprite.Create(texture, new Rect(0, 0, 64, 32), Vector2.one * .5f, 16, 0, SpriteMeshType.FullRect);
            try
            {
                var renderer = go.AddComponent<SpriteRenderer>();
                renderer.sprite = placeholder;
                var collider = go.AddComponent<BoxCollider2D>();
                go.transform.localScale = new Vector3(7, 2, 1);
                var colliderSize = collider.size;
                var bounds = renderer.bounds.size;
                var slot = go.AddComponent<PrefabSprite2D>();
                slot.Artwork = artwork;
                Assert.AreSame(artwork, renderer.sprite);
                Assert.AreEqual(colliderSize, collider.size);
                Assert.AreEqual(new Vector3(7, 2, 1), go.transform.localScale);
                Assert.That(Vector3.Distance(bounds, renderer.bounds.size), Is.LessThan(.001f));
                slot.Artwork = null;
                Assert.AreSame(placeholder, renderer.sprite);
                Assert.AreEqual(SpriteDrawMode.Simple, renderer.drawMode);
                Assert.AreEqual(new Vector3(7, 2, 1), go.transform.localScale);
            }
            finally
            {
                Object.DestroyImmediate(go);
                Object.DestroyImmediate(placeholder);
                Object.DestroyImmediate(artwork);
                Object.DestroyImmediate(texture);
            }
        }
    }
}
