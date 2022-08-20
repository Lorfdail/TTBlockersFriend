using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Blish_HUD.Settings;
using Blish_HUD.Settings.UI.Views;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using TTBlockersStuff.Language;

namespace Lorf.BH.TTBlockersStuff.Settings
{
    /// <summary>
    /// Quick and dirty implementation for a ColorPicker settings entry .. why does blish hud not have something like this again?
    /// </summary>
    class ColorPickerSettingView : SettingView<Gw2Sharp.WebApi.V2.Models.Color>
    {
        private IEnumerable<Gw2Sharp.WebApi.V2.Models.Color> colors;
        private ColorBox box;
        private Label nameLabel;
        private ColorPickerWindow window;

        public ColorPickerSettingView(SettingEntry<Gw2Sharp.WebApi.V2.Models.Color> setting, int definedWidth = -1) : base(setting, definedWidth) 
        {
            colors = Module.Instance.Colors;
        }

        public override bool HandleComplianceRequisite(IComplianceRequisite complianceRequisite)
        {
            switch (complianceRequisite)
            {
                case SettingDisabledComplianceRequisite disabledRequisite:
                    box.Enabled = !disabledRequisite.Disabled;
                    break;
                default:
                    return false;
            }

            return true;
        }

        protected override void BuildSetting(Container buildPanel)
        {
            box = new ColorBox()
            {
                Visible = true,
                Color = colors.First(),
                Parent = buildPanel,
            };

            box.Click += UpdateActiveColorBox;

            nameLabel = new Label()
            {
                Text = "",
                Location = new Point(box.Right + 10, box.Top + 5),
                ShowShadow = true,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
                Parent = buildPanel,
                Font = GameService.Content.DefaultFont16
            };
        }

        private void UpdateActiveColorBox(object sender, MouseEventArgs e)
        {
            window = new ColorPickerWindow(Translations.ColorSelectionTitle, box)
            {
                Parent = GameService.Graphics.SpriteScreen
            };
            window.Show();
            window.AssignmentAccepted += OnPickedColorChange;
        }

        private void OnPickedColorChange(object sender, EventArgs e)
        {
            OnValueChanged(new ValueEventArgs<Gw2Sharp.WebApi.V2.Models.Color>(box.Color));
        }

        protected override void RefreshDisplayName(string displayName)
        {
            if(nameLabel != null)
                nameLabel.Text = displayName;
        }

        protected override void RefreshDescription(string description)
        {
            if (nameLabel != null)
                nameLabel.BasicTooltipText = description;
        }

        protected override void Unload()
        {
            box.Click -= UpdateActiveColorBox;
            box.Dispose();
            nameLabel.Dispose();
            window.AssignmentAccepted -= OnPickedColorChange;
            window.Dispose();

            base.Unload();
        }

        protected override void RefreshValue(Gw2Sharp.WebApi.V2.Models.Color value)
        {
            box.Color = value;
        }
    }
}
