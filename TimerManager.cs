using Blish_HUD;
using Blish_HUD.Controls;
using Lorf.BH.TTBlockersStuff.UI;
using System;
using TTBlockersStuff.Language;

namespace Lorf.BH.TTBlockersStuff
{
    /// <summary>
    /// The timer data and button / progress bar control
    /// </summary>
    class TimerManager
    {
        private static readonly Logger Logger = Logger.GetLogger<TimerManager>();

        public string Name { get; set; }
        public bool Active { get; private set; }
        public TimerBar TimerBar { get; set; }

        private DateTime targetTime;

        public void Reset()
        {
            targetTime = DateTime.MinValue;
            TimerBar.MaxValue = 1f;
            TimerBar.Value = 1f;
        }

        public void Activate(int time)
        {
            var pos = GameService.Gw2Mumble.PlayerCharacter.Position;
            Logger.Debug($"Timer {Name} activated (x: {pos.X}, y: {pos.Y}, z: {pos.Z}, time: {time})");

            targetTime = DateTime.UtcNow.AddSeconds(time);
            TimerBar.MaxValue = time;
            Active = true;
        }

        public void Update()
        {
            if (!Active)
                return;

            var now = DateTime.UtcNow;
            var secondsRemaining = (targetTime - now).TotalSeconds;

            if (targetTime > now && secondsRemaining > 1)
            {
                TimerBar.BarText = $"{Name} ({(int)secondsRemaining}s)";
                TimerBar.Value = TimerBar.MaxValue - ((float)((targetTime - now).TotalMilliseconds / 1000));
            }
            else
            {
                Active = false;
                ScreenNotification.ShowNotification(Name + " " + Translations.TimerBarTextReady + "!", ScreenNotification.NotificationType.Info);
                TimerBar.BarText = $"{Name} ({Translations.TimerBarTextReady})";
                TimerBar.Value = TimerBar.MaxValue;
            }
        }
    }
}
