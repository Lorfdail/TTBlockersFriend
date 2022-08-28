using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Lorf.BH.TTBlockersStuff.UI
{
    /// <summary>
    /// Mostly a copy & paste of the "default" blish hud panel since it doesnt allow you to really change the header.
    /// Not 100% with what this thing does at the moment but for the overall complexity of the plugin i dont mind it too much (feel free to rant about it)
    /// </summary>
    class TTPanel : Container
    {
        // internal accent padding
        private const int ACCENT_LEFT_MARGIN = 4;
        private const int ACCENT_BOTTOM_MARGIN = 5;
        private const int ACCENT_RIGHT_MARGIN = 4;

        // some more padding for .. stuff
        private const int RIGHT_PADDING = 15;
        private const int LEFT_PADDING = 7;

        // header element
        private const int HEADER_HEIGHT = 36;

        // some weird internal textures from blish hud .. getting that stuff by name or via a constant class would be too easy now wouldn't it be? 
        private Texture2D _texturePanelHeader = Module.Instance.ContentsManager.GetTexture("1032325.png");
        private Texture2D _texturePanelHeaderActive = Module.Instance.ContentsManager.GetTexture("1032324.png");

        // small icons that get displayed next to the title once specific flags are set
        private Texture2D armorIcon = Module.Instance.ContentsManager.GetTexture("Armor_(attribute).png");
        private Texture2D markerIcon = Module.Instance.ContentsManager.GetTexture("marker_icon.png");
        private Texture2D impossibleIcon = Module.Instance.ContentsManager.GetTexture("Closed.png");
        private Texture2D resizeCornerInactive = Module.Instance.ContentsManager.GetTexture("resize_corner_inactive.png");
        private Texture2D resizeCornerActive = Module.Instance.ContentsManager.GetTexture("resize_corner_active.png");
        private Texture2D cornerAccent = Module.Instance.ContentsManager.GetTexture("corner_accent.png");
        private Texture2D borderAccent = Module.Instance.ContentsManager.GetTexture("border_accent.png");

        // shamelessly stolen from the WindowBase2 stuff
        private static readonly SettingCollection windowSettings = Module.Instance.SettingsManager.ModuleSettings.AddSubCollection("TTPanel");

        // all the different layout bounds
        private Rectangle layoutHeaderBounds;
        private Rectangle layoutHeaderTextBounds;
        private Rectangle layoutTopLeftAccentBounds;
        private Rectangle layoutBottomRightAccentBounds;
        private Rectangle layoutCornerAccentSrc;
        private Rectangle layoutLeftAccentBounds;
        private Rectangle layoutInternalBounds;
        private Rectangle resizeHandleBounds;

        private Point dragStart;
        private Point resizeStart;

        private bool dragging;
        public bool Dragging
        {
            get => dragging;
            private set => SetProperty(ref dragging, value);
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

        private bool resizing;
        /// <summary>
        /// Indicates if the window is actively being resized.
        /// </summary>
        public bool Resizing
        {
            get => resizing;
            private set => SetProperty(ref resizing, value);
        }

        /// <summary>
        /// Title that is shown at the top
        /// </summary>
        protected string title;
        public string Title
        {
            get => title;
            set => SetProperty(ref title, value, false);
        }

        /// <summary>
        /// Yes there is a "Visibility" property .. but you know what? i want fancy fading and i use this for fancy fading! .. fight me >:C
        /// </summary>
        protected bool targetVisibility;
        public bool TargetVisibility
        {
            get => targetVisibility;
            set => SetProperty(ref targetVisibility, value, false);
        }

        // This exists for the main reason hat the accents kinda limit the original space but not enough to be the ContentRegion
        protected int InternalWidth { get => Width - ACCENT_LEFT_MARGIN - ACCENT_RIGHT_MARGIN; }
        protected int InternalHeight { get => Height - ACCENT_BOTTOM_MARGIN; }

        public TTPanel()
        {
            // necessary for drag and drop since the usual mouse release bugs out 
            GameService.Input.Mouse.LeftMouseButtonReleased += OnGlobalMouseRelease;
            resizeHandleBounds = Rectangle.Empty;
            dragStart = Point.Zero;
            resizeStart = Point.Zero;
        }

        /// <summary>
        /// Modifies the window size as it's being resized.
        /// </summary>
        protected virtual Point HandleWindowResize(Point newSize)
        {
            return new Point(
                MathHelper.Clamp(newSize.X, 250, (int) (GameService.Graphics.Resolution.X / GameService.Graphics.UIScaleMultiplier)), // 250 here is a made up value which looked nice for me
                MathHelper.Clamp(newSize.Y, HEADER_HEIGHT + ACCENT_BOTTOM_MARGIN, (int) (GameService.Graphics.Resolution.Y / GameService.Graphics.UIScaleMultiplier))
            );
        }

        /// <summary>
        /// Updates some calculated components of the element (dragging, resizing, fading, ..)
        /// </summary>
        public override void UpdateContainer(GameTime gameTime)
        {
            if (Dragging)
            {
                Location += Input.Mouse.Position - dragStart;
                dragStart = Input.Mouse.Position;
            }
            else if (Resizing)
            {
                var nOffset = Input.Mouse.Position - dragStart;
                Size = HandleWindowResize(resizeStart + nOffset);
            }

            // for now this is the fancy fading effect of the entire panel .. should be a window mask but this was a 5 minute implementation and works without a flaw
            // and yes i dont like this tweener stuff
            if (Visible != TargetVisibility || _opacity != 0 || _opacity != 1)
            {
                if (TargetVisibility)
                    Opacity = MathHelper.Clamp(Opacity + (float)(gameTime.ElapsedGameTime.TotalMilliseconds / 200), 0, 1);
                else
                    Opacity = MathHelper.Clamp(Opacity - (float)(gameTime.ElapsedGameTime.TotalMilliseconds / 200), 0, 1);

                Visible = _opacity > 0;
            }

            base.UpdateContainer(gameTime);
        }

        /// <summary>
        /// The usual recalculation of all major layout components .. nothing fancy
        /// </summary>
        public override void RecalculateLayout()
        {
            layoutInternalBounds = new Rectangle(ACCENT_LEFT_MARGIN, 0, InternalWidth, InternalHeight);

            // header layout
            layoutHeaderBounds = new Rectangle(ACCENT_LEFT_MARGIN, 0, InternalWidth, HEADER_HEIGHT);
            layoutHeaderTextBounds = new Rectangle(
                layoutHeaderBounds.X + LEFT_PADDING + (BlockerIconVisible ? markerIcon.Width + (LEFT_PADDING - ACCENT_LEFT_MARGIN) : 0), 
                layoutHeaderBounds.Y, 
                layoutHeaderBounds.Width - RIGHT_PADDING - LEFT_PADDING, 
                layoutHeaderBounds.Height
            );

            // all the fancy border stuff
            resizeHandleBounds = new Rectangle(
                Width - resizeCornerInactive.Width,
                Height - resizeCornerInactive.Height,
                resizeCornerInactive.Width,
                resizeCornerInactive.Height
            );

            int cornerAccentWidth = Math.Min(Width, cornerAccent.Width);
            layoutTopLeftAccentBounds = new Rectangle(0, HEADER_HEIGHT - 6, cornerAccentWidth, cornerAccent.Height);
            layoutBottomRightAccentBounds = new Rectangle(Width - cornerAccentWidth, Height - cornerAccent.Height, cornerAccentWidth, cornerAccent.Height);
            layoutCornerAccentSrc = new Rectangle(0, 0, cornerAccentWidth, cornerAccent.Height);
            layoutLeftAccentBounds = new Rectangle(ACCENT_LEFT_MARGIN, HEADER_HEIGHT, borderAccent.Width, Math.Min(InternalHeight - HEADER_HEIGHT, borderAccent.Height));

            // content layout
            ContentRegion = new Rectangle(ACCENT_LEFT_MARGIN, HEADER_HEIGHT, InternalWidth, InternalHeight - HEADER_HEIGHT);
        }

        /// <summary>
        /// Initial drawing of the main elements
        /// </summary>
        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            // Slightly tint the background of the panel
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, layoutInternalBounds, Color.Black * 0.1f);     

            // Panel header
            spriteBatch.DrawOnCtrl(this, _mouseOver && RelativeMousePosition.Y <= HEADER_HEIGHT ? _texturePanelHeaderActive : _texturePanelHeader, layoutHeaderBounds);
            spriteBatch.DrawStringOnCtrl(this, title, Content.DefaultFont16, layoutHeaderTextBounds, Color.White);
            
            // Accents!
            spriteBatch.DrawOnCtrl(this, cornerAccent, layoutTopLeftAccentBounds, layoutCornerAccentSrc, Color.White, 0, Vector2.Zero, SpriteEffects.FlipHorizontally);
            spriteBatch.DrawOnCtrl(this, cornerAccent, layoutBottomRightAccentBounds, layoutCornerAccentSrc, Color.White, 0, Vector2.Zero, SpriteEffects.FlipVertically);
            spriteBatch.DrawOnCtrl(this, borderAccent, layoutLeftAccentBounds, layoutLeftAccentBounds, Color.Black, 0, Vector2.Zero, SpriteEffects.None);

            // le fancy icon!
            if (blockerIconVisible)
            {
                // actually still too much "magic" here but for version 0.1.0 .. eh :P
                Texture2D icon = isMounted ? impossibleIcon : (isBlocking ? armorIcon : markerIcon);
                spriteBatch.DrawOnCtrl(this, icon, new Rectangle(ACCENT_LEFT_MARGIN + 18, 18, 26, 26), icon.Bounds, isMounted ? Color.White : blockerIconTint, isMounted ? 0 : blockerIconRotation, new Vector2(icon.Width / 2, icon.Height / 2));
            }
        }

        /// <summary>
        /// Mostly the resize icon .. needs to happen here since otherwise child elements could overlap and "hide" this.
        /// </summary>
        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            Texture2D iconTexture = resizeHandleBounds.Contains(RelativeMousePosition) || Resizing ? resizeCornerActive : resizeCornerInactive;
            spriteBatch.DrawOnCtrl(this, iconTexture, resizeHandleBounds);
                
            base.PaintAfterChildren(spriteBatch, bounds);
        }

        /// <summary>
        /// Start of both resizing and dragging
        /// </summary>
        protected override void OnLeftMouseButtonPressed(MouseEventArgs e)
        {
            if (resizeHandleBounds.Contains(RelativeMousePosition))
            {
                Resizing = true;
                resizeStart = Size;
                dragStart = Input.Mouse.Position;
            }
            else if (_mouseOver && RelativeMousePosition.Y <= HEADER_HEIGHT)
            {
                Dragging = true;
                dragStart = Input.Mouse.Position;
            }

            base.OnLeftMouseButtonPressed(e);
        }

        /// <summary>
        /// End of both resizing and dragging + saving their respective settings
        /// </summary>
        protected void OnGlobalMouseRelease(object sender, MouseEventArgs e)
        {
            if (!Visible)
                return;

            if (Dragging || Resizing)
            {
                (windowSettings["location"] as SettingEntry<Point> ?? windowSettings.DefineSetting("location", dragStart)).Value = Location;
                (windowSettings["size"] as SettingEntry<Point> ?? windowSettings.DefineSetting("size", resizeStart)).Value = Size;

                if (Resizing)
                    OnResized(new ResizedEventArgs(resizeStart, Size));

                Dragging = false;
                Resizing = false;
            }
            else
            {
                // Meh .. resize handle overlaps with lower right child element .. since i dont want a margin to the moon and back let's just handle click events ourself
                // Can't we get some nice fancy proper handling for this somehow? besides the fact that you cant mark click events as handled
                foreach (Control child in Children)
                {
                    if (child is TimerBar pb && pb.LocalBounds.OffsetBy(ContentRegion.Location).Contains(RelativeMousePosition))
                        pb.OnInternalClick(this, e);
                }
            }
        }

        /// <summary>
        /// Makes the Panel invisible with a small fade out animation
        /// </summary>
        public override void Hide()
        {
            if (!TargetVisibility) 
                return;

            TargetVisibility = false;
            Dragging = false;
        }

        /// <summary>
        /// Makes the Panel visible with a small fade in animation
        /// </summary>
        public override void Show()
        {
            if (TargetVisibility)
                return;
            TargetVisibility = true;

            // Load size setting or apply default value
            if (windowSettings.TryGetSetting("size", out var windowSize))
            {
                var setting = windowSize as SettingEntry<Point>;
                if (setting == null)
                {
                    setting = new SettingEntry<Point>();
                    setting.Value = new Point(400, 120);
                }
                Size = setting.Value;
            }
            else
                Size = new Point(400, 120);

            // Load location setting or apply default value
            if (windowSettings.TryGetSetting("location", out var windowPosition))
            {
                var setting = windowPosition as SettingEntry<Point>;
                if (setting == null)
                {
                    setting = new SettingEntry<Point>();
                    setting.Value = GetDefaultLocation();
                }
                Location = setting.Value;
            }
            else
                Location = GetDefaultLocation();

            base.Show();
        }

        /// <summary>
        /// In short .. in the center of the screen
        /// </summary>
        private Point GetDefaultLocation()
        {
            return new Point(
                (int)(GameService.Graphics.Resolution.X / GameService.Graphics.UIScaleMultiplier / 2) - (Width / 2), // half the width
                (int)(GameService.Graphics.Resolution.Y / GameService.Graphics.UIScaleMultiplier / 2) - (Height / 2) // half the height
            );
        }

        /// <summary>
        /// Default disposing stuff .. only thing we really have to worry about here is the OnGlobalMouseRelease
        /// </summary>
        protected override void DisposeControl()
        {
            GameService.Input.Mouse.LeftMouseButtonReleased -= OnGlobalMouseRelease;

            _texturePanelHeader?.Dispose();
            _texturePanelHeaderActive?.Dispose();
            armorIcon?.Dispose();
            markerIcon?.Dispose();
            impossibleIcon?.Dispose();
            resizeCornerInactive?.Dispose();
            resizeCornerActive?.Dispose();
            cornerAccent?.Dispose();
            borderAccent?.Dispose();

            base.DisposeControl();
        }
    }
}
