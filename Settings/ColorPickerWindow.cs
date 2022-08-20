using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using TTBlockersStuff.Language;

namespace Lorf.BH.TTBlockersStuff.Settings
{
    /// <summary>
    /// Quick and dirty implementation of a color picker window like the keybinding selector .. same question as for the setting view (although that can maybe be a bit more a special case .. but this?)
    /// </summary>
    class ColorPickerWindow : Container, IWindow
    {
        private int textureMarginLeft;
        private int textureMarginTop;
        private int textureMarginRight;
        private int textureMarginBottom;

        private Gw2Sharp.WebApi.V2.Models.Color originalColor;

        #region Load Static

        private static readonly Texture2D _textureWindowTexture = GameService.Content.GetTexture("controls/window/502049");

        #endregion

        #region Events

        /// <summary>
        /// Fires when the "Accept" button is pressed within the assignment window.
        /// Indicates that the assignment was accepted and the resulting primary and
        /// modifier keys should update their target.
        /// </summary>
        public event EventHandler<EventArgs> AssignmentAccepted;

        /// <summary>
        /// Fires when the "Cancel" button or escape is pressed within the assignment window.
        /// Indicates that the assignment was canceled and the resulting primary and modifier
        /// keys should be ignored.
        /// </summary>
        public event EventHandler<EventArgs> AssignmentCanceled;

        private void OnAssignmentAccepted(EventArgs e)
        {
            AssignmentAccepted?.Invoke(this, e);
            Dispose();
        }

        private void OnAssignmentCanceled(EventArgs e)
        {
            colorBox.Color = originalColor;
            AssignmentCanceled?.Invoke(this, e);
            Dispose();
        }

        #endregion

        private readonly Rectangle _normalizedWindowRegion = new Rectangle(0, 0, 400, 400);

        private readonly string pickName;

        private readonly ColorBox colorBox;
        private ColorPicker picker;

        public ColorPickerWindow(string name, ColorBox box)
        {
            pickName = name;
            colorBox = box;

            originalColor = colorBox.Color;

            BackgroundColor = Color.Black * 0.3f;
            Size = new Point(_normalizedWindowRegion.Width, _normalizedWindowRegion.Height);
            ZIndex = Screen.TOOLTIP_BASEZINDEX - 1;
            Visible = false;

            BuildChildElements();
        }

        protected override void OnShown(EventArgs e)
        {
            Invalidate();

            base.OnShown(e);
        }

        private StandardButton _acceptBttn;
        private StandardButton _cancelBttn;

        private int doABitOfMath(float size, bool height)
        {
            float originalSize = height ? _textureWindowTexture.Height : _textureWindowTexture.Width;
            float newSize = height ? Height : Width;

            return (int)(size * (newSize / originalSize));
        }

        private void BuildChildElements()
        {
            textureMarginLeft = doABitOfMath(36f, false);
            textureMarginTop = doABitOfMath(26f, true);
            textureMarginRight = doABitOfMath(72f, false);
            textureMarginBottom = doABitOfMath(35f, true);

            var assignInputsLbl = new Label()
            {
                Text = pickName,
                Location = new Point(textureMarginLeft + 18, textureMarginTop + 18),
                ShowShadow = true,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
                Parent = this,
                Font = GameService.Content.DefaultFont16
            };

            _cancelBttn = new StandardButton()
            {
                Text = Translations.ColorSelectionButtonTextCancel,
                Location = new Point(Width - textureMarginRight - 100 - 18, Height - textureMarginBottom - 18 - 25),
                Width = 100,
                Height = 25,
                Parent = this
            };

            _acceptBttn = new StandardButton()
            {
                Text = Translations.ColorSelectionButtonTextOk,
                Width = 100,
                Height = 25,
                Parent = this
            };
            _acceptBttn.Location = new Point(_cancelBttn.Left - 8 - _acceptBttn.Width, _cancelBttn.Top);

            _cancelBttn.Click += delegate {
                OnAssignmentCanceled(EventArgs.Empty);
            };

            _acceptBttn.Click += delegate {
                OnAssignmentAccepted(EventArgs.Empty);
            };

            picker = new ColorPicker();
            picker.Visible = true;
            picker.Parent = this;
            picker.Size = new Point(Width - assignInputsLbl.Left - 36, _cancelBttn.Top - assignInputsLbl.Bottom - 36);
            picker.CanScroll = true;
            picker.Location = new Point(assignInputsLbl.Left, assignInputsLbl.Bottom + 18);
            picker.AssociatedColorBox = colorBox;

            // this takes ages .. why? its an IObservableCollection and there is no update suppression 
            // makes sense to recalculate everything ~600 times since that is kinda the normal color amount if you want to have them all
            foreach (var color in Module.Instance.Colors)
                picker.Colors.Add(color);
        }

        private Rectangle _windowRegion;

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            var parent = Parent;

            if (parent != null)
            {
                _size = parent.Size;

                var distanceInwards = new Point(_size.X / 2 - _normalizedWindowRegion.Width / 2,
                                                _size.Y / 2 - _normalizedWindowRegion.Height / 2);

                _windowRegion = _normalizedWindowRegion.OffsetBy(distanceInwards);

                ContentRegion = _windowRegion;
            }
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            spriteBatch.DrawOnCtrl(this, _textureWindowTexture, _windowRegion);
        }

        // We implement IWindow to avoid other windows from reacting to our ESC input

        public bool TopMost => false;
        public double LastInteraction => double.MaxValue;
        public bool CanClose => false;
        public bool CanCloseWithEscape => true;
        public void BringWindowToFront() { /* NOOP */ }

    }
}
