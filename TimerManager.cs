using Blish_HUD;
using Blish_HUD.Controls;
using System;

namespace TTBlockersFriend
{
    /// <summary>
    /// The timer data and button / progress bar control
    /// </summary>
    class TimerManager
    {
        private static readonly Logger Logger = Logger.GetLogger<TimerManager>();

        public string Name { get; set; }
        public bool Active { get; private set; }
        public StandardButton PanelButton { get; set; }
        public ProgressBar TimerBar { get; set; }

        private DateTime targetTime;

        public void Reset()
        {
            targetTime = DateTime.MinValue;
            PanelButton.Text = Name;
            TimerBar.MaxValue = 1f;
            TimerBar.Value = 0f;
        }

        public void Activate(GatheringSpot gatheringSpot, int time)
        {
            var pos = GameService.Gw2Mumble.PlayerCharacter.Position;
            Logger.Debug($"{Name} pressed (x: {pos.X}, y: {pos.Y}, z: {pos.Z})");

            if (!gatheringSpot.IsWurm)
                return;

            var now = DateTime.UtcNow;
            targetTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
            targetTime = targetTime.AddSeconds(time);
            TimerBar.MaxValue = time;
            Active = true;
        }

        public void Update()
        {
            if (!Active)
                return;

            var now = DateTime.UtcNow;
            var secondsNow = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);

            if (targetTime >= secondsNow)
            {
                PanelButton.Text = $"{Name} ({(int)(targetTime - secondsNow).TotalSeconds}s)";
                TimerBar.BarText = PanelButton.Text;
                TimerBar.Value = TimerBar.MaxValue - ((float)((targetTime - now).TotalMilliseconds / 1000));
                if ((targetTime - secondsNow).TotalSeconds == 0)
                {
                    Active = false;
                    ScreenNotification.ShowNotification(Name + " ready!", ScreenNotification.NotificationType.Info);
                    PanelButton.Text = Name + " (ready)";
                    TimerBar.BarText = Name + " (ready)";
                }
            }
            else
            {
                PanelButton.Text = Name + " (ready)";
                TimerBar.BarText = Name + " (ready)";
            }
        }
    }
}
