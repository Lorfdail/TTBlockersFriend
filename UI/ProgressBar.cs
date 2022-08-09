using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TTBlockersFriend
{
    /// <summary>
    /// Definetly not mostly a copy pasta of blish huds internal track bar implementation
    /// Simple Healthbar / Progress bar thing. Mostly based on an ingame screenshot and *definetly* not a perfect match towards ingame graphics
    /// There are also visual glitches with the nub on values close and at 0
    /// (Why does blish hud not have something like this again?)
    /// </summary>
    class ProgressBar : Control
    {
        private static readonly Texture2D _textureTrack = Module.Instance.ContentsManager.GetTexture("middle.png");
        private static readonly Texture2D _endNub = Module.Instance.ContentsManager.GetTexture("end_nub.png");
        private static readonly Texture2D _back = Module.Instance.ContentsManager.GetTexture("back.png");
        private static readonly Texture2D _leftSide = Module.Instance.ContentsManager.GetTexture("side_left.png");
        private static readonly Texture2D _rightSide = Module.Instance.ContentsManager.GetTexture("right_side.png");

        private Rectangle _layoutNubBounds;
        private Rectangle _layoutLeftBumper;
        private Rectangle _layoutRightBumper;
        private Rectangle _layoutBack;

        protected float maxValue = 100f;
        public float MaxValue
        {
            get => maxValue;
            set
            {
                if (SetProperty(ref maxValue, value, false))
                    Value = this.value;
            }
        }

        protected float minValue = 0f;
        public float MinValue
        {
            get => minValue;
            set
            {
                if (SetProperty(ref minValue, value, false))
                    Value = this.value;
            }
        }

        protected float value = 0;
        public float Value
        {
            get => value;
            set => SetProperty(ref this.value, MathHelper.Clamp(value, minValue, maxValue), true);
        }

        public ProgressBar()
        {
            Size = new Point(256, 16);
        }

        public override void RecalculateLayout()
        {
            _layoutLeftBumper = new Rectangle(0, 0, _leftSide.Width, Height);
            _layoutRightBumper = new Rectangle(Width - _leftSide.Width, 0, _rightSide.Width, Height);

            float valueOffset = ((Value - MinValue) / (MaxValue - MinValue) * (Width - _leftSide.Width)) - _endNub.Width;
            _layoutNubBounds = new Rectangle((int)valueOffset + _leftSide.Width / 2, 3, _endNub.Width, _endNub.Height - 2);

            _layoutBack = new Rectangle(_leftSide.Width, 3, (int)valueOffset, _back.Height - 2);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            spriteBatch.DrawOnCtrl(this, _textureTrack, bounds);
            spriteBatch.DrawOnCtrl(this, _back, _layoutBack);
            spriteBatch.DrawOnCtrl(this, _endNub, _layoutNubBounds);

            spriteBatch.DrawOnCtrl(this, _leftSide, _layoutLeftBumper);
            spriteBatch.DrawOnCtrl(this, _rightSide, _layoutRightBumper);
        }
    }
}
