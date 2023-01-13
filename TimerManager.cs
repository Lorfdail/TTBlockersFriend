using Blish_HUD;
using Blish_HUD.Controls;
using Lorf.BH.TTBlockersStuff.UI;
using Microsoft.Xna.Framework;
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

        private TimeSpan textBlinkDuration = new TimeSpan(200 * TimeSpan.TicksPerMillisecond);
        private TimeSpan lastBlinkRequestTime = TimeSpan.Zero;
        private int lastBlinkRequestSecond = 0;

        public string Name { get; set; }
        public bool Active { get; private set; }
        public TimerBar TimerBar { get; set; }

        private DateTime targetTime;

        public TimerManager()
        {
            targetTime = DateTime.MinValue;
        }

        public void Activate(int time)
        {
            if(Active)
            {
                Active = false;
                TimerBar.BarText = $"{Name} ({Translations.TimerBarTextReady})";
                TimerBar.Value = TimerBar.MaxValue;
                targetTime = DateTime.MinValue;
            }
            else
            {
                var pos = GameService.Gw2Mumble.PlayerCharacter.Position;
                Logger.Debug($"Timer {Name} activated (x: {pos.X}, y: {pos.Y}, z: {pos.Z}, time: {time})");

                targetTime = DateTime.UtcNow.AddSeconds(time);
                TimerBar.MaxValue = time;
                Active = true;
            }
        }

        public void Update(GameTime gameTime)
        {
            if (!Active)
                return;

            var now = DateTime.UtcNow;
            var secondsRemaining = (targetTime - now).TotalSeconds;

            if (targetTime > now && secondsRemaining > 1)
            {
                TimerBar.BarText = $"{Name} ({(int)secondsRemaining}s)";
                TimerBar.Value = TimerBar.MaxValue - ((float)((targetTime - now).TotalMilliseconds / 1000));
                if (TimerBar.MaxValue != (int)secondsRemaining && (((int)secondsRemaining) % 10 == 0 || secondsRemaining < 10) && lastBlinkRequestTime == TimeSpan.Zero && lastBlinkRequestSecond != (int)secondsRemaining)
                {
                    lastBlinkRequestTime = gameTime.TotalGameTime;
                    lastBlinkRequestSecond = (int)secondsRemaining;
                }
                    

                if(lastBlinkRequestTime.Add(textBlinkDuration) > gameTime.TotalGameTime)
                    TimerBar.TextColor = Color.Red;
                else
                {
                    TimerBar.TextColor = Color.White;
                    lastBlinkRequestTime = TimeSpan.Zero;
                }
            }
            else
            {
                Active = false;
                ScreenNotification.ShowNotification(Name + " " + Translations.TimerBarTextReady + "!", ScreenNotification.NotificationType.Info);
                TimerBar.BarText = $"{Name} ({Translations.TimerBarTextReady})";
                TimerBar.Value = TimerBar.MaxValue;
                TimerBar.TextColor = Color.White;
            }
        }
    }
}
