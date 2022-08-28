using Blish_HUD;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Lorf.BH.TTBlockersStuff.Settings;
using Lorf.BH.TTBlockersStuff.UI;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using TTBlockersStuff.Language;

namespace Lorf.BH.TTBlockersStuff
{
    /// <summary>
    /// Main class / entrypoint .. too much logik here but hey it works for the moment
    /// </summary>
    [Export(typeof(Blish_HUD.Modules.Module))]
    public class Module : Blish_HUD.Modules.Module
    {
        private const int PROGRESS_BAR_MARGIN = 7;

        private static readonly Logger Logger = Logger.GetLogger<Module>();
        public static Module Instance;

        // UI Elements
        private TTPanel mainPanel;
        private TimerBar husksBar;
        private TimerBar eggsBar;

        // Data
        private GatheringSpot gatheringSpot;
        public IEnumerable<Gw2Sharp.WebApi.V2.Models.Color> Colors;

        // Blockers Friend Service Managers
        private TimerManager husksTimerManager;
        private TimerManager eggsTimerManager;

        // Service Managers
        internal SettingsManager SettingsManager => ModuleParameters.SettingsManager;
        internal ContentsManager ContentsManager => ModuleParameters.ContentsManager;
        internal DirectoriesManager DirectoriesManager => ModuleParameters.DirectoriesManager;
        internal Gw2ApiManager Gw2ApiManager => ModuleParameters.Gw2ApiManager;

        [ImportingConstructor]
        public Module([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters) 
        {
            // UwU
            Instance = this;
        }

        protected override void Initialize()
        {
            Logger.Info("Initializing TT Blockers Stuff");

            mainPanel = new TTPanel()
            {
                Title = "Blockers Stuff",
                Parent = GameService.Graphics.SpriteScreen,
            };

            // Husks stuff
            husksBar = new TimerBar(TimerBar.Husks)
            {
                Location = new Point(PROGRESS_BAR_MARGIN, PROGRESS_BAR_MARGIN),
                Size = new Point(mainPanel.ContentRegion.Width - (PROGRESS_BAR_MARGIN * 2), (mainPanel.ContentRegion.Height / 2) - PROGRESS_BAR_MARGIN - (PROGRESS_BAR_MARGIN / 2)),
                Parent = mainPanel,
                MaxValue = 1f,
                Value = 1f,
                BarText = $"{Translations.TimerBarTextHusks} ({Translations.TimerBarTextReady})",
            };
            mainPanel.Resized += (previousSize, currentSize) =>
            {
                husksBar.Width = mainPanel.ContentRegion.Width - (PROGRESS_BAR_MARGIN * 2);
                husksBar.Height = (mainPanel.ContentRegion.Height / 2) - PROGRESS_BAR_MARGIN - (PROGRESS_BAR_MARGIN / 2);
                husksBar.RecalculateLayout();
            };
            husksTimerManager = new TimerManager()
            {
                Name = Translations.TimerBarTextHusks,
                TimerBar = husksBar,
            };
            husksBar.InternalClick += (e, e1) => 
            {
                GameService.Content.PlaySoundEffectByName(@"button-click");
                husksTimerManager.Activate(gatheringSpot.HuskTime);
            };

            // Eggs stuff
            eggsBar = new TimerBar(TimerBar.Eggs)
            {
                Location = new Point(PROGRESS_BAR_MARGIN, husksBar.Location.Y + husksBar.Size.Y + PROGRESS_BAR_MARGIN),
                Size = new Point(mainPanel.ContentRegion.Width - (PROGRESS_BAR_MARGIN * 2), (mainPanel.ContentRegion.Height / 2) - PROGRESS_BAR_MARGIN - (PROGRESS_BAR_MARGIN / 2)),
                Parent = mainPanel,
                MaxValue = 1f,
                Value = 1f,
                BarText = $"{Translations.TimerBarTextEggs} ({Translations.TimerBarTextReady})",
            };
            mainPanel.Resized += (previousSize, currentSize) =>
            {
                eggsBar.Width = mainPanel.ContentRegion.Width - (PROGRESS_BAR_MARGIN * 2);
                eggsBar.Height = (mainPanel.ContentRegion.Height / 2) - PROGRESS_BAR_MARGIN - (PROGRESS_BAR_MARGIN / 2);
                eggsBar.Location = new Point(PROGRESS_BAR_MARGIN, husksBar.Location.Y + husksBar.Size.Y + PROGRESS_BAR_MARGIN);
                eggsBar.RecalculateLayout();
            };
            eggsTimerManager = new TimerManager()
            {
                Name = Translations.TimerBarTextEggs,
                TimerBar = eggsBar,
            };
            eggsBar.InternalClick += (e, e1) =>
            {
                GameService.Content.PlaySoundEffectByName(@"button-click"); 
                eggsTimerManager.Activate(40);
            };
        }

        protected override async Task LoadAsync()
        {
            // i will get my default color even if its the last thing i do
            Colors = new List<Gw2Sharp.WebApi.V2.Models.Color>() 
            { 
                new Gw2Sharp.WebApi.V2.Models.Color()
                {
                    Name = "Default",
                    Cloth = new Gw2Sharp.WebApi.V2.Models.ColorMaterial()
                    {
                        Rgb = new List<int>() { 237, 121, 38 },
                    }
                } 
            }.Concat(await Instance.Gw2ApiManager.Gw2ApiClient.V2.Colors.AllAsync());

            SettingsManager.ModuleSettings.DefineSetting("colorPickerSettingTimerBar0", Colors?.First(),
                () => Translations.SettingColorSelectionHusksText,
                () => Translations.SettingColorSelectionHusksTooltipText);
            SettingsManager.ModuleSettings.DefineSetting("colorPickerSettingTimerBarRefilling0", Colors?.First(),
                () => Translations.SettingColorSelectionHusksRefillingText,
                () => Translations.SettingColorSelectionHusksRefillingTooltipText);
            SettingsManager.ModuleSettings.DefineSetting("colorPickerSettingTimerBar1", Colors?.First(),
                () => Translations.SettingColorSelectionEggsText,
                () => Translations.SettingColorSelectionEggsTooltipText);
            SettingsManager.ModuleSettings.DefineSetting("colorPickerSettingTimerBarRefilling1", Colors?.First(),
                () => Translations.SettingColorSelectionEggsRefillingText,
                () => Translations.SettingColorSelectionEggsRefillingTooltipText);

            await base.LoadAsync();
        }

        public override IView GetSettingsView()
        {
            return new TTSettingsCollection(SettingsManager.ModuleSettings);
        }

        protected override void Unload()
        {
            Logger.Info("Unloading TT Blockers Stuff");
            mainPanel?.Dispose();
            husksBar?.Dispose();
            eggsBar?.Dispose();

            Instance = null;
        }

        protected override void Update(GameTime gameTime)
        {
            // 73 == Bloodtide coast .. if we are not there just hide our panel
            if (GameService.Gw2Mumble.CurrentMap.Id != 73)
            {
                if (mainPanel.TargetVisibility)
                    mainPanel.Hide();
                return;
            }

            // gathering spot identification
            Vector3 pos = GameService.Gw2Mumble.PlayerCharacter.Position;
            Vector2 vec2Pos = new Vector2(pos.X, pos.Y);
            gatheringSpot = GatheringSpot.FromPosition(vec2Pos);
            if (gatheringSpot == null)
            {
                if (mainPanel.TargetVisibility)
                    mainPanel.Hide();
                return;
            }

            // we are in the correct map and we are at one of the gathering spots but our panel is hidden .. let us fix that
            if (!mainPanel.TargetVisibility)
            {
                mainPanel.BlockerIconVisible = false;
                mainPanel.BlockerIconTint = new Color(Color.White, 0f);

                // set a bunch of one time values that only change once we also change the gathering spot (which will hide the panel inbetween due to how they are placed right now)
                // TODO: maybe do that stuff based on the actual gathering spot changing but *eh* fine for the moment
                mainPanel.Title = "Blockers Stuff - " + gatheringSpot.Name;

                mainPanel.Show();
            }

            // this right here is about the most complex stuff you will find here .. all of this controls that one little icon next to the title
            // - 3 distance checks based on the character being 10, 1 and .35 units away from the gathering spot
            // - fancy pointer rotation based on the camera and rotation
            // - detects if the player is mounted and therefore shows a "you can't block mate" icon
            // - color tint based on the distance indicating if a moving entitie gets closer or further away based on a color change of the compass icon
            // - note: i know this can be improved like many other things but for now it works 
            bool isInMajorBlockRange = Vector2.Distance(gatheringSpot.Position, vec2Pos) < 10f;

            if (mainPanel.BlockerIconVisible != isInMajorBlockRange)
                mainPanel.BlockerIconVisible = isInMajorBlockRange;

            if (isInMajorBlockRange)
            {
                bool isInMiddleBlockRange = Vector2.Distance(gatheringSpot.Position, vec2Pos) < 1f;
                bool isInBlockRange = Vector2.Distance(gatheringSpot.Position, vec2Pos) < .35f;
                bool isMounted = GameService.Gw2Mumble.PlayerCharacter.CurrentMount != Gw2Sharp.Models.MountType.None;

                if (mainPanel.IsMounted != isMounted)
                    mainPanel.IsMounted = isMounted;

                if (mainPanel.IsBlocking != isInBlockRange)
                    mainPanel.IsBlocking = isInBlockRange;

                if (isInBlockRange)
                {
                    if (mainPanel.BlockerIconRotation != 0f)
                        mainPanel.BlockerIconRotation = 0f;

                    mainPanel.BlockerIconTint = Color.LawnGreen;
                }
                else
                {
                    Vector3 rawCharacterPosition = GameService.Gw2Mumble.RawClient.AvatarPosition.ToXnaVector3();

                    // rotation calculation of the angle between the players camera and the spot where the target (gathering spot location) is .. rotates a little compass icon in the title 
                    Vector2 playerChameraDirection = new Vector2(GameService.Gw2Mumble.PlayerCamera.Position.X, GameService.Gw2Mumble.PlayerCamera.Position.Y) - new Vector2(rawCharacterPosition.X, rawCharacterPosition.Y);
                    Vector2 targetDirection = gatheringSpot.Position - new Vector2(rawCharacterPosition.X, rawCharacterPosition.Y);

                    double sin = playerChameraDirection.X * targetDirection.Y - targetDirection.X * playerChameraDirection.Y;
                    double cos = playerChameraDirection.X * targetDirection.X + playerChameraDirection.Y * targetDirection.Y;

                    float angle = MathHelper.ToRadians((float)(Math.Atan2(cos, sin) * (180 / Math.PI)));

                    mainPanel.BlockerIconRotation = angle;

                    if (isInMiddleBlockRange)
                        mainPanel.BlockerIconTint = new Color((Vector2.Distance(gatheringSpot.Position, vec2Pos) - .35f) / .65f + .486f, (Vector2.Distance(gatheringSpot.Position, vec2Pos) - .35f) / .65f + .988f, 0f, 1f);
                    else if (isInMajorBlockRange)
                        mainPanel.BlockerIconTint = new Color(1f, 1f - ((Vector2.Distance(gatheringSpot.Position, vec2Pos) - 1f) / 9f), 0f, 1f);
                }
            }

            // just simply update the timers once we are at a gathering spot
            husksTimerManager.Update();
            eggsTimerManager.Update();
        }
    }
}
