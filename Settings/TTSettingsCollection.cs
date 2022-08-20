using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Settings;
using Blish_HUD.Settings.UI.Views;
using Microsoft.Xna.Framework;
using System.Linq;

namespace TTBlockersStuff.Settings
{
    /// <summary>
    /// This needs to exist since you apparently cant add your own setting display type :)
    /// </summary>
    class TTSettingsCollection : SettingView<SettingCollection>
    {

        private FlowPanel _settingFlowPanel;

        private readonly SettingCollection _settings;

        private bool _lockBounds = true;

        public bool LockBounds
        {
            get => _lockBounds;
            set
            {
                if (_lockBounds == value) return;

                _lockBounds = value;

                UpdateBoundsLocking(_lockBounds);
            }
        }

        private ViewContainer _lastSettingContainer;

        public TTSettingsCollection(SettingEntry<SettingCollection> setting, int definedWidth = -1) : base(setting, definedWidth)
        {
            _settings = setting.Value;
        }

        public TTSettingsCollection(SettingCollection settings, int definedWidth = -1) : this(new SettingEntry<SettingCollection>() { Value = settings }, definedWidth) { /* NOOP */ }

        private void UpdateBoundsLocking(bool locked)
        {
            if (_settingFlowPanel == null) return;

            _settingFlowPanel.ShowBorder = !locked;
            _settingFlowPanel.CanCollapse = !locked;
        }

        protected override void BuildSetting(Container buildPanel)
        {
            _settingFlowPanel = new FlowPanel()
            {
                Size = buildPanel.Size,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new Vector2(5, 2),
                OuterControlPadding = new Vector2(10, 15),
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                AutoSizePadding = new Point(0, 15),
                Parent = buildPanel
            };

            foreach (var setting in _settings.Where(s => s.SessionDefined))
            {
                IView settingView;

                if ((settingView = SettingView.FromType(setting, _settingFlowPanel.Width)) != null)
                {
                    _lastSettingContainer = new ViewContainer()
                    {
                        WidthSizingMode = SizingMode.Fill,
                        HeightSizingMode = SizingMode.AutoSize,
                        Parent = _settingFlowPanel
                    };

                    _lastSettingContainer.Show(settingView);

                    if (settingView is SettingsView subSettingsView)
                        subSettingsView.LockBounds = false;
                }
                else if (setting.SettingType == typeof(Gw2Sharp.WebApi.V2.Models.Color))
                {
                    settingView = new ColorPickerSettingView(setting as SettingEntry<Gw2Sharp.WebApi.V2.Models.Color>, _settingFlowPanel.Width);
                    _lastSettingContainer = new ViewContainer()
                    {
                        WidthSizingMode = SizingMode.Fill,
                        HeightSizingMode = SizingMode.AutoSize,
                        Parent = _settingFlowPanel
                    };
                    _lastSettingContainer.Show(settingView);

                    if (settingView is SettingsView subSettingsView)
                        subSettingsView.LockBounds = false;
                }
            }

            UpdateBoundsLocking(_lockBounds);
        }

        protected override void RefreshDisplayName(string displayName)
        {
            _settingFlowPanel.Title = displayName;
        }

        protected override void RefreshDescription(string description)
        {
            _settingFlowPanel.BasicTooltipText = description;
        }

        protected override void RefreshValue(SettingCollection value) { /* NOOP */ }

    }
}
