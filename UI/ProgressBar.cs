using Blish_HUD;
using Blish_HUD._Extensions;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Blish_HUD.Settings;
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
        private static readonly Texture2D _textureTrackTop = Module.Instance.ContentsManager.GetTexture("middle_top.png");
        private static readonly Texture2D _textureTrackBottom = Module.Instance.ContentsManager.GetTexture("middle_bottom.png");
        private static readonly Texture2D _leftSide = Module.Instance.ContentsManager.GetTexture("side_left.png");
        private static readonly Texture2D _rightSide = Module.Instance.ContentsManager.GetTexture("right_side.png");
        private static readonly Texture2D textureBarGradient = Module.Instance.ContentsManager.GetTexture("bar_gradient.png");

        private Rectangle _layoutLeftBumper;
        private Rectangle _layoutRightBumper;
        private Rectangle _layoutBack;
        private Rectangle layoutMiddleTopBounds;
        private Rectangle layoutMiddleBottomBounds;
        private Rectangle layoutBarGradientBounds;

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

        /// <summary>
        /// Title that is shown at the top
        /// </summary>
        protected string barText;
        public string BarText
        {
            get => barText;
            set => SetProperty(ref barText, value, true);
        }

        protected int barIndex;


        public ProgressBar(int barIndex)
        {
            Size = new Point(256, 16);
            this.barIndex = barIndex;
        }

        public override void RecalculateLayout()
        {
            _layoutLeftBumper = new Rectangle(0, 0, _leftSide.Width, Height);
            _layoutRightBumper = new Rectangle(Width - _leftSide.Width, 0, _rightSide.Width, Height);

            float valueOffset = ((Value - MinValue) / (MaxValue - MinValue) * (Width - _leftSide.Width));

            _layoutBack = new Rectangle(_leftSide.Width, 2, (int)valueOffset - _leftSide.Width, Height - 4);

            layoutMiddleTopBounds = new Rectangle(0, 0, Width, _textureTrackTop.Height);
            layoutMiddleBottomBounds = new Rectangle(0, Height - _textureTrackBottom.Height, Width, _textureTrackTop.Height);

            layoutBarGradientBounds = new Rectangle((int)valueOffset - textureBarGradient.Width, 2, textureBarGradient.Width, Height - 4);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            Color color = new Color(237, 121, 38);
            if (Module.Instance.SettingsManager.ModuleSettings.TryGetSetting("colorPickerSettingHusksBar", out var newColor) && Value >= MaxValue)
            {
                var tmp = (newColor as SettingEntry<Gw2Sharp.WebApi.V2.Models.Color[]>).Value[barIndex];
                color = tmp.Cloth?.ToXnaColor() ?? color;
            }

            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, bounds, Color.Black * .3f);

            spriteBatch.DrawOnCtrl(this, _textureTrackTop, layoutMiddleTopBounds);
            spriteBatch.DrawOnCtrl(this, _textureTrackBottom, layoutMiddleBottomBounds);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, _layoutBack, color);

            if (Value < MaxValue)
                spriteBatch.DrawOnCtrl(this, textureBarGradient, layoutBarGradientBounds);

            spriteBatch.DrawStringOnCtrl(this, barText, GameService.Content.DefaultFont16, bounds, Color.White, horizontalAlignment: HorizontalAlignment.Center);

            spriteBatch.DrawOnCtrl(this, _leftSide, _layoutLeftBumper);
            spriteBatch.DrawOnCtrl(this, _rightSide, _layoutRightBumper);
        }
    }
}
