using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using TTBlockersFriend.Settings;

namespace TTBlockersFriend
{
    /// <summary>
    /// Main class / entrypoint .. too much logik here but hey it works for the moment
    /// </summary>
    [Export(typeof(Blish_HUD.Modules.Module))]
    public class Module : Blish_HUD.Modules.Module
    {
        private static readonly Logger Logger = Logger.GetLogger<Module>();
        public static Module Instance;

        // UI Elements
        private TTPanel mainPanel;
        private ProgressBar husksBar;
        private ProgressBar eggsBar;
        private StandardButton panelButtonHusks;
        private StandardButton panelButtonEggs;

        // Data
        private GatheringSpot gatheringSpot;
        private IEnumerable<Gw2Sharp.WebApi.V2.Models.Color> colors;

        private SettingEntry<Gw2Sharp.WebApi.V2.Models.Color[]> colorPickerSettingHusksBar;

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
            Instance = this;
        }

        protected override void Initialize()
        {
            Logger.Info("Initializing TT Blockers Friend");

            mainPanel = new TTPanel()
            {
                Visible = false,
                Title = "Blockers stuff",
                Location = new Point((GameService.Graphics.WindowHeight / 2) - 200, (GameService.Graphics.WindowHeight / 2) - 57), // TODO: fix the initial position .. for whatever reason this returns a windowsize of 800 * 480 (during debugging atleast)
                Size = new Point(400, 134), // TODO: make size dynamic
                Parent = GameService.Graphics.SpriteScreen,
            };

            // Husks stuff
            husksBar = new ProgressBar(0)
            {
                Location = new Point(7, 9),
                Size = new Point(378, 32),
                Parent = mainPanel,
                MaxValue = 80,
                Value = 0,
                BarText = "Husks",
            };
            panelButtonHusks = new StandardButton()
            {
                Text = "Husks",
                Location = new Point(261, 7),
                Size = new Point(128, 36),
                Parent = mainPanel,
                Visible = false,
            };
            husksTimerManager = new TimerManager()
            {
                Name = "Husks",
                PanelButton = panelButtonHusks,
                TimerBar = husksBar,
            };
            panelButtonHusks.Click += (e, e1) => husksTimerManager.Activate(gatheringSpot, gatheringSpot.HuskTime);
            husksBar.Click += (e, e1) => husksTimerManager.Activate(gatheringSpot, gatheringSpot.HuskTime);

            // Eggs stuff
            eggsBar = new ProgressBar(1)
            {
                Location = new Point(7, 52),
                Size = new Point(378, 32),
                Parent = mainPanel,
                MaxValue = 40,
                Value = 0,
                BarText = "Eggs",
            };
            panelButtonEggs = new StandardButton()
            {
                Text = "Eggs",
                Location = new Point(261, 49),
                Size = new Point(128, 36),
                Parent = mainPanel,
                Visible = false,
            };
            eggsTimerManager = new TimerManager()
            {
                Name = "Eggs",
                PanelButton = panelButtonEggs,
                TimerBar = eggsBar,
            };
            panelButtonEggs.Click += (e, e1) => eggsTimerManager.Activate(gatheringSpot, 40);
            eggsBar.Click += (e, e1) => eggsTimerManager.Activate(gatheringSpot, 40);
        }

        protected override async Task LoadAsync()
        {
            colors = await Instance.Gw2ApiManager.Gw2ApiClient.V2.Colors.AllAsync();

            colorPickerSettingHusksBar = SettingsManager.ModuleSettings.DefineSetting("colorPickerSettingHusksBar", new Gw2Sharp.WebApi.V2.Models.Color[] { colors?.First(), colors?.First() },
                () => null,
                () => "Toggles the display of data.");

            await base.LoadAsync();
        }

        public override IView GetSettingsView()
        {
            var test = new ColorPickerSettingView(colorPickerSettingHusksBar, colors);
            return test;
        }

        protected override void Unload()
        {
            Logger.Info("Unloading TT Blockers Friend");
            mainPanel?.Dispose();
            husksBar?.Dispose();
            panelButtonHusks?.Dispose();
            eggsBar?.Dispose();
            panelButtonEggs?.Dispose();
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
                eggsTimerManager.Reset();
                husksTimerManager.Reset();

                mainPanel.BlockerIconVisible = false;
                mainPanel.BlockerIconTint = new Color(Color.White, 0f);

                // set a bunch of one time values that only change once we also change the gathering spot (which will hide the panel inbetween due to how they are placed right now)
                // TODO: maybe do that stuff based on the actual gathering spot changing but *eh* fine for the moment
                mainPanel.Title = "Blockers Stuff - " + gatheringSpot.Name;

                mainPanel.Show();
            }

            // this right here is about the most complex stuff you will find here .. all of this controls that one little icon next to the title
            // - 3 distance checks based on the character being 5, 1 and .35 units away from the gathering spot
            // - fancy pointer rotation based on the camera and rotation
            // - detects if the player is mounted and therefore shows a "you can't block mate" icon
            // - color tint based on the distance indicating if a moving entitie gets closer or further away based on a color change of the compass icon
            // - note: i know this can be improved like many other things but for now it works 
            bool isInMajorBlockRange = Vector2.Distance(gatheringSpot.Position, vec2Pos) < 5f;
            if (isInMajorBlockRange)
            {
                bool isMounted = GameService.Gw2Mumble.PlayerCharacter.CurrentMount != Gw2Sharp.Models.MountType.None;
                if (mainPanel.IsMounted != isMounted)
                    mainPanel.IsMounted = isMounted;

                if (mainPanel.BlockerIconVisible != isInMajorBlockRange)
                    mainPanel.BlockerIconVisible = isInMajorBlockRange;

                if (!isMounted)
                {
                    Color blockerIconTint;

                    bool isInMiddleBlockRange = Vector2.Distance(gatheringSpot.Position, vec2Pos) < 1f;
                    bool isInBlockRange = false;
                    if (isInMiddleBlockRange)
                    {
                        isInBlockRange = Vector2.Distance(gatheringSpot.Position, vec2Pos) < .35f;
                        if (isInBlockRange)
                        {
                            if (mainPanel.IsBlocking != isInBlockRange)
                                mainPanel.IsBlocking = isInBlockRange;

                            if (mainPanel.BlockerIconRotation != 0f)
                                mainPanel.BlockerIconRotation = 0f;

                            blockerIconTint = Color.LawnGreen;
                        }
                        else
                        {
                            if (mainPanel.IsBlocking)
                                mainPanel.IsBlocking = false;

                            blockerIconTint = new Color((Vector2.Distance(gatheringSpot.Position, vec2Pos) - .35f) / .65f + .486f, (Vector2.Distance(gatheringSpot.Position, vec2Pos) - .35f) / .65f + .988f, 0f, 1f);
                        }
                    }
                    else
                        blockerIconTint = new Color(1f, 1f - ((Vector2.Distance(gatheringSpot.Position, vec2Pos) - 1f) / 4f), 0f, 1f);

                    if (!isInBlockRange)
                    {
                        // rotation calculation of the angle between the players camera and the spot where the target (gathering spot location) is .. rotates a little compass icon in the title 
                        Vector2 playerChameraDirection = new Vector2(GameService.Gw2Mumble.PlayerCamera.Position.X, GameService.Gw2Mumble.PlayerCamera.Position.Y) - new Vector2(GameService.Gw2Mumble.PlayerCharacter.Position.X, GameService.Gw2Mumble.PlayerCharacter.Position.Y);
                        Vector2 targetDirection = gatheringSpot.Position - new Vector2(GameService.Gw2Mumble.PlayerCharacter.Position.X, GameService.Gw2Mumble.PlayerCharacter.Position.Y);

                        double sin = playerChameraDirection.X * targetDirection.Y - targetDirection.X * playerChameraDirection.Y;
                        double cos = playerChameraDirection.X * targetDirection.X + playerChameraDirection.Y * targetDirection.Y;

                        float angle = MathHelper.ToRadians((float)(Math.Atan2(cos, sin) * (180 / Math.PI)));

                        mainPanel.BlockerIconRotation = angle;
                    }

                    if (mainPanel.BlockerIconTint != blockerIconTint)
                        mainPanel.BlockerIconTint = blockerIconTint;
                }
                else
                    mainPanel.IsBlocking = false; // hard reset since otherwise we may nt get the update it seems .. TODO: do that in a smarter way
            }
            else if(mainPanel.BlockerIconVisible)
                mainPanel.BlockerIconVisible = false;

            if (!gatheringSpot.IsWurm)
                return;

            // just simply update the timers once we are at a wurm spot
            husksTimerManager.Update();
            eggsTimerManager.Update();
        }
    }
}
