using Blish_HUD;
using Blish_HUD._Extensions;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Lorf.BH.TTBlockersStuff.UI
{
    /// <summary>
    /// Definetly not mostly a copy pasta of blish huds internal track bar implementation
    /// Simple Healthbar / Progress bar thing. Mostly based on an ingame screenshot and *definetly* not a perfect match towards ingame graphics
    /// There are also visual glitches with the nub on values close and at 0
    /// (Why does blish hud not have something like this again?)
    /// </summary>
    class TimerBar : Control
    {
        // Little constants for the bar indexes .. i use this for color selection (not fancy, pretty or nice yes but it's a 0.1.0 so idc)
        public const int Husks = 0;
        public const int Eggs = 1;

        // Textures
        private readonly Texture2D textureBarTop = Module.Instance.ContentsManager.GetTexture("middle_top.png");
        private readonly Texture2D textureBarBottom = Module.Instance.ContentsManager.GetTexture("middle_bottom.png");
        private readonly Texture2D textureBarLeftSide = Module.Instance.ContentsManager.GetTexture("side_left.png");
        private readonly Texture2D textureBarRightSide = Module.Instance.ContentsManager.GetTexture("right_side.png");
        private readonly Texture2D textureBarGradient = Module.Instance.ContentsManager.GetTexture("bar_gradient.png");

        // The boundaries of all the areas / textures
        private Rectangle layoutBarLeftSide;
        private Rectangle layourBarRightSide;
        private Rectangle layoutBarBackground;
        private Rectangle layoutBarTop;
        private Rectangle layoutBarBottom;
        private Rectangle layoutBarGradient;

        // since i have some personal issues with the event handling at the moment
        public event EventHandler<MouseEventArgs> InternalClick;

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

        // usually doesn't make sense to start with the maxValue here
        protected float value = 100f;
        public float Value
        {
            get => value;
            set => SetProperty(ref this.value, MathHelper.Clamp(value, minValue, maxValue), true);
        }

        /// <summary>
        /// Text that should be shown ontop
        /// </summary>
        protected string barText;
        public string BarText
        {
            get => barText;
            set => SetProperty(ref barText, value, false);
        }

        protected Color textColor = Color.White;
        public Color TextColor
        {
            get => textColor;
            set => SetProperty(ref textColor, value, false);
        }

        protected int barIndex;

        public TimerBar(int barIndex)
        {
            Size = new Point(256, 16);
            this.barIndex = barIndex;
        }

        /// <summary>
        /// The usual recalculation of all major layout components .. nothing fancy
        /// </summary>
        public override void RecalculateLayout()
        {
            layoutBarLeftSide = new Rectangle(0, 0, textureBarLeftSide.Width, Height);
            layourBarRightSide = new Rectangle(Width - textureBarRightSide.Width, 0, textureBarRightSide.Width, Height);

            layoutBarTop = new Rectangle(0, 0, Width, textureBarTop.Height);
            layoutBarBottom = new Rectangle(0, Height - textureBarBottom.Height, Width, textureBarTop.Height);

            float valueOffset = ((Value - MinValue) / (MaxValue - MinValue) * (Width - textureBarLeftSide.Width));
            layoutBarBackground = new Rectangle(textureBarLeftSide.Width, 2, (int)valueOffset - textureBarLeftSide.Width, Height - 4);
            layoutBarGradient = new Rectangle((int)valueOffset - textureBarGradient.Width, 2, textureBarGradient.Width, Height - 4);
        }

        /// <summary>
        /// Since event handling is a pain otherwise we just roll our own for the moment
        /// </summary>
        public void OnInternalClick(object sender, MouseEventArgs e)
        {
            InternalClick?.Invoke(sender, e);
        }

        /// <summary>
        /// Paint everything
        /// </summary>
        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            // this is far from being nice yes  .. and you know what? i will *leave* it like *this* until i get enough feedback for 0.3.0! fight me >:3
            Color color = new Color(237, 121, 38);
            if (Value >= MaxValue)
            {
                if (Module.Instance.SettingsManager.ModuleSettings.TryGetSetting("colorPickerSettingTimerBar" + barIndex, out var newColor))
                {
                    var tmp = (newColor as SettingEntry<Gw2Sharp.WebApi.V2.Models.Color>).Value;
                    color = tmp.Cloth?.ToXnaColor() ?? color;
                }
            }
            else
            {
                if (Module.Instance.SettingsManager.ModuleSettings.TryGetSetting("colorPickerSettingTimerBarRefilling" + barIndex, out var newColor))
                {
                    var tmp = (newColor as SettingEntry<Gw2Sharp.WebApi.V2.Models.Color>).Value;
                    color = tmp.Cloth?.ToXnaColor() ?? color;
                }
            }

            // Slightly tint the background
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, bounds, Color.Black * .3f);

            // content
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, layoutBarBackground, color);
            if (Value < MaxValue)
                spriteBatch.DrawOnCtrl(this, textureBarGradient, layoutBarGradient);
            spriteBatch.DrawStringOnCtrl(this, barText, GameService.Content.DefaultFont16, bounds, textColor, horizontalAlignment: HorizontalAlignment.Center);

            // borders
            spriteBatch.DrawOnCtrl(this, textureBarTop, layoutBarTop);
            spriteBatch.DrawOnCtrl(this, textureBarBottom, layoutBarBottom);
            spriteBatch.DrawOnCtrl(this, textureBarLeftSide, layoutBarLeftSide);
            spriteBatch.DrawOnCtrl(this, textureBarRightSide, layourBarRightSide);
        }

        protected override void DisposeControl()
        {
            textureBarTop?.Dispose();
            textureBarBottom?.Dispose();
            textureBarLeftSide?.Dispose();
            textureBarRightSide?.Dispose();
            textureBarGradient?.Dispose();

            base.DisposeControl();
        }
    }
}
