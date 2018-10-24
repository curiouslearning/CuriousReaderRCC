﻿// Spritedow Animation Plugin by Elendow
// http://elendow.com

using UnityEngine;

namespace Elendow.SpritedowAnimator
{
    /// <summary>
    /// Animator for Sprite Renderers.
    /// </summary>
    [AddComponentMenu("Spritedow/Sprite Animator")]
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteAnimator : BaseAnimator
    {
        private SpriteRenderer spriteRenderer;

        protected override void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            base.Awake();
        }

        /// <summary>
        /// Changes the sprite to the given sprite
        /// </summary>
        protected override void ChangeFrame(Sprite frame)
        {
            spriteRenderer.sprite = frame;
        }

        /// <summary>
        /// Enable or disable the renderer
        /// </summary>
        public override void SetActiveRenderer(bool active)
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.enabled = active;
        }

        /// <summary>
        /// Flip the sprite on the X axis
        /// </summary>
        public override void FlipSpriteX(bool flip)
        {
            spriteRenderer.flipX = flip;
        }

        /// <summary>
        /// Flip the sprite on the Y axis
        /// </summary>
        public override void FlipSpriteY(bool flip)
        {
            spriteRenderer.flipY = flip;
        }

        /// <summary>
        /// The bounds of the Sprite Renderer
        /// </summary>
        public Bounds SpriteBounds
        {
            get { return spriteRenderer.bounds; }
        }

        /// <summary>
        /// The Sprite Renderer used to render
        /// </summary>
        public SpriteRenderer SpriteRenderer
        {
            get { return spriteRenderer; }
        }
    }
}