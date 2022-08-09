using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace TTBlockersFriend
{
    /// <summary>
    /// Mostly a copy & paste of the "default" blish hud panel since it doesnt allow you to really change the header.
    /// Not 100% with what this thing does at the moment but for the overall complexity of the plugin i dont mind it too much (feel free to rant about it)
    /// </summary>
    class TTPanel : Container
    {
        // Used when border is enabled
        public const int TOP_PADDING = 7;
        public const int RIGHT_PADDING = 4;
        public const int BOTTOM_PADDING = 7;
        public const int LEFT_PADDING = 4;

        public const int HEADER_HEIGHT = 36;
        private const int MAX_ACCENT_WIDTH = 256;

        // some weird internal textures from blish hud .. getting that stuff by name or via a constant class would be too easy now wouldn't it be? 
        private static readonly Texture2D _textureCornerAccent = Content.GetTexture("controls/panel/1002144");
        private static readonly Texture2D _textureLeftSideAccent = Content.GetTexture("605025");
        private static readonly Texture2D _texturePanelHeader = Content.GetTexture("controls/panel/1032325");
        private static readonly Texture2D _texturePanelHeaderActive = Content.GetTexture("controls/panel/1032324");

        // small icons that get displayed next to the title once specific flags are set
        private static readonly Texture2D armorIcon = Module.Instance.ContentsManager.GetTexture("Armor_(attribute).png");
        private static readonly Texture2D markerIcon = Module.Instance.ContentsManager.GetTexture("marker_icon.png");
        private static readonly Texture2D impossibleIcon = Module.Instance.ContentsManager.GetTexture("Closed.png");

        // shamelessly stolen from the WindowBase2 stuff
        private static readonly SettingCollection _windowSettings = Module.Instance.SettingsManager.ModuleSettings.AddSubCollection("TTPanel");

        // all the different layout bounds
        private Rectangle _layoutHeaderBounds;
        private Rectangle _layoutHeaderTextBounds;
        private Rectangle _layoutTopLeftAccentBounds;
        private Rectangle _layoutBottomRightAccentBounds;
        private Rectangle _layoutCornerAccentSrc;
        private Rectangle _layoutLeftAccentBounds;
        private Rectangle _layoutLeftAccentSrc;

        private Point _dragStart = Point.Zero;

        private bool _dragging;
        public bool Dragging
        {
            get => _dragging;
            private set => SetProperty(ref _dragging, value);
        }

        /// <summary>
        /// Indicates if the character is in the perfect spot for blocking (mostly controls the swap over to the armorIcon instead of the markerIcon)
        /// </summary>
        private bool isBlocking;
        public bool IsBlocking
        {
            get => isBlocking;
            set => SetProperty(ref isBlocking, value, true);
        }

        /// <summary>
        /// If the character is mounted then he can't block (mostly controls the swap over to the impossibleIcon instead of the other 2)
        /// </summary>
        private bool isMounted;
        public bool IsMounted
        {
            get => isMounted;
            set => SetProperty(ref isMounted, value, true);
        }

        /// <summary>
        /// Fancy rotation of the markerIcon
        /// </summary>
        private float blockerIconRotation;
        public float BlockerIconRotation
        {
            get => blockerIconRotation;
            set => SetProperty(ref blockerIconRotation, value);
        }

        /// <summary>
        /// If any of the icons should even be visible (usually set when the character is inside the "major" blocking range)
        /// </summary>
        private bool blockerIconVisible;
        public bool BlockerIconVisible
        {
            get => blockerIconVisible;
            set => SetProperty(ref blockerIconVisible, value, true);
        }

        /// <summary>
        /// Tints the color of the markerIcon to have a visual representation of the distance towards the "perfect" spot
        /// </summary>
        private Color blockerIconTint;
        public Color BlockerIconTint
        {
            get => blockerIconTint;
            set => SetProperty(ref blockerIconTint, value);
        }

        /// <summary>
        /// Title that is shown at the top
        /// </summary>
        protected string _title;
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value, true);
        }

        /// <summary>
        /// Yes there is a "Visibility" property .. but you know what? i want fancy fading and i use this for fancy fading! .. fight me >:C
        /// </summary>
        protected bool targetVisibility;
        public bool TargetVisibility
        {
            get => targetVisibility;
            set => SetProperty(ref targetVisibility, value, true);
        }

        public TTPanel()
        {
            // necessary for drag and drop since the usual mouse release bugs out 
            GameService.Input.Mouse.LeftMouseButtonReleased += OnGlobalMouseRelease;
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            if (Dragging)
            {
                Location += Input.Mouse.Position - _dragStart;
                _dragStart = Input.Mouse.Position;
            }

            // for now this is the fancy fading effect of the entire panel .. should be a window mask but this was a 5 minute implementation and works without a flaw
            if(Visible != TargetVisibility || _opacity != 0 || _opacity != 1)
            {
                if (TargetVisibility)
                    Opacity = MathHelper.Clamp(Opacity + (float)(gameTime.ElapsedGameTime.TotalMilliseconds / 200), 0, 1);
                else
                    Opacity = MathHelper.Clamp(Opacity - (float)(gameTime.ElapsedGameTime.TotalMilliseconds / 200), 0, 1);

                Visible = _opacity > 0;
            }

            base.UpdateContainer(gameTime);
        }

        /// <inheritdoc />
        public override void RecalculateLayout()
        {
            bool showsHeader = !string.IsNullOrEmpty(_title);

            int topOffset = Math.Max(TOP_PADDING, showsHeader ? HEADER_HEIGHT : 0);
            int rightOffset = RIGHT_PADDING;
            int bottomOffset = BOTTOM_PADDING;
            int leftOffset = LEFT_PADDING;

            // Corner accents
            int cornerAccentWidth = Math.Min(_size.X, MAX_ACCENT_WIDTH);
            _layoutTopLeftAccentBounds = new Rectangle(-2, topOffset - 12, cornerAccentWidth, _textureCornerAccent.Height);

            _layoutBottomRightAccentBounds = new Rectangle(_size.X - cornerAccentWidth + 2, _size.Y - 59, cornerAccentWidth, _textureCornerAccent.Height);

            _layoutCornerAccentSrc = new Rectangle(MAX_ACCENT_WIDTH - cornerAccentWidth, 0, cornerAccentWidth, _textureCornerAccent.Height);

            // Left side accent
            _layoutLeftAccentBounds = new Rectangle(leftOffset - 7, topOffset, _textureLeftSideAccent.Width, Math.Min(_size.Y - topOffset - bottomOffset, _textureLeftSideAccent.Height));
            _layoutLeftAccentSrc = new Rectangle(0, 0, _textureLeftSideAccent.Width, _layoutLeftAccentBounds.Height);

            ContentRegion = new Rectangle(leftOffset, topOffset, _size.X - leftOffset - rightOffset, _size.Y - topOffset - bottomOffset);

            _layoutHeaderBounds = new Rectangle(ContentRegion.Left, 0, ContentRegion.Width, HEADER_HEIGHT);
            _layoutHeaderTextBounds = new Rectangle(_layoutHeaderBounds.Left + 10 + (BlockerIconVisible ? 26 : 0), 0, _layoutHeaderBounds.Width - 10, HEADER_HEIGHT);
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            spriteBatch.DrawOnCtrl(this, _texturePanelHeader, _layoutHeaderBounds);

            // Panel header
            if (_mouseOver && RelativeMousePosition.Y <= HEADER_HEIGHT)
                spriteBatch.DrawOnCtrl(this, _texturePanelHeaderActive, _layoutHeaderBounds);
            else
                spriteBatch.DrawOnCtrl(this, _texturePanelHeader, _layoutHeaderBounds);

            // Panel header text
            spriteBatch.DrawStringOnCtrl(this, _title, Content.DefaultFont16, _layoutHeaderTextBounds, Color.White);
            
            // Lightly tint the background of the panel
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, ContentRegion, Color.Black * 0.1f);

            PaintAccents(spriteBatch);

            if (blockerIconVisible)
            {
                Texture2D icon = isMounted ? impossibleIcon : (isBlocking ? armorIcon : markerIcon);
                spriteBatch.DrawOnCtrl(this, icon, new Rectangle(LEFT_PADDING + 18, 18, 26, 26), icon.Bounds, isMounted ? Color.White : blockerIconTint, isMounted ? 0 : blockerIconRotation, new Vector2(icon.Width / 2, icon.Height / 2));
            }
        }

        private void PaintAccents(SpriteBatch spriteBatch)
        {
            // Top left accent
            spriteBatch.DrawOnCtrl(this, _textureCornerAccent, _layoutTopLeftAccentBounds, _layoutCornerAccentSrc, Color.White, 0, Vector2.Zero, SpriteEffects.FlipHorizontally);

            // Bottom right accent
            spriteBatch.DrawOnCtrl(this, _textureCornerAccent, _layoutBottomRightAccentBounds, _layoutCornerAccentSrc, Color.White, 0, Vector2.Zero, SpriteEffects.FlipVertically);

            // Left side accent
            spriteBatch.DrawOnCtrl(this, _textureLeftSideAccent, _layoutLeftAccentBounds, _layoutLeftAccentSrc, Color.Black, 0, Vector2.Zero, SpriteEffects.FlipVertically);
        }

        protected override void OnLeftMouseButtonPressed(MouseEventArgs e)
        {
            if (_mouseOver && RelativeMousePosition.Y <= HEADER_HEIGHT)
            {
                Dragging = true;
                _dragStart = Input.Mouse.Position;
            }

            base.OnLeftMouseButtonPressed(e);
        }

        protected void OnGlobalMouseRelease(object sender, MouseEventArgs e)
        {
            if (Visible && Dragging)
            {
                (_windowSettings["main"] as SettingEntry<Point> ?? _windowSettings.DefineSetting("main", _dragStart)).Value = Location;
                Dragging = false;
            }
        }

        public override void Hide()
        {
            if (!TargetVisibility) 
                return;

            TargetVisibility = false;
            Dragging = false;
        }

        public override void Show()
        {
            if (TargetVisibility)
                return;
            TargetVisibility = true;

            if (_windowSettings.TryGetSetting("main", out var windowPosition))
                Location = (windowPosition as SettingEntry<Point> ?? new SettingEntry<Point>()).Value;

            base.Show();
        }

        protected override void DisposeControl()
        {
            GameService.Input.Mouse.LeftMouseButtonReleased -= OnGlobalMouseRelease;
            base.DisposeControl();
        }
    }
}
