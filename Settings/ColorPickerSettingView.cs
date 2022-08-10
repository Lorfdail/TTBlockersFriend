using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Blish_HUD.Settings.UI.Views;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTBlockersFriend.Settings
{
    class ColorPickerSettingView : SettingView<Gw2Sharp.WebApi.V2.Models.Color[]>
    {
        private IEnumerable<Gw2Sharp.WebApi.V2.Models.Color> colors;
        private ColorPicker picker;
        private ColorBox box;
        private ColorBox box2;

        public ColorPickerSettingView(SettingEntry<Gw2Sharp.WebApi.V2.Models.Color[]> setting, IEnumerable<Gw2Sharp.WebApi.V2.Models.Color> colors, int definedWidth = -1) : base(setting, definedWidth) 
        {
            this.colors = colors;
        }

        public override bool HandleComplianceRequisite(IComplianceRequisite complianceRequisite)
        {
            switch (complianceRequisite)
            {
                case SettingDisabledComplianceRequisite disabledRequisite:
                    picker.Enabled = !disabledRequisite.Disabled;
                    box.Enabled = !disabledRequisite.Disabled;
                    break;
                default:
                    return false;
            }

            return true;
        }

        protected override void BuildSetting(Container buildPanel)
        {
            picker = new ColorPicker();
            picker.Visible = false;
            picker.Parent = buildPanel;
            picker.Size = new Point(buildPanel.Width - 10, 200);
            picker.CanScroll = true;
            picker.Location = new Point(0, 32);

            box = new ColorBox();
            box.Visible = false;
            box.Color = colors.First();
            box.Parent = buildPanel;

            box2 = new ColorBox();
            box2.Visible = false;
            box2.Color = colors.First();
            box2.Parent = buildPanel;
            box2.Location = new Point(32, 0);


            box.Click += UpdateActiveColorBox;
            box2.Click += UpdateActiveColorBox;

            foreach (var color in colors)
                picker.Colors.Add(color);

            picker.AssociatedColorBox = box;
            picker.Visible = true;
            box.Visible = true;
            box2.Visible = true;

            picker.SelectedColorChanged += OnPickedColorChange;
        }

        private void UpdateActiveColorBox(object sender, MouseEventArgs e)
        {
            picker.AssociatedColorBox = (ColorBox)sender;
        }

        private void OnPickedColorChange(object sender, EventArgs e)
        {
            if(picker.AssociatedColorBox == box)
                OnValueChanged(new ValueEventArgs<Gw2Sharp.WebApi.V2.Models.Color[]>(new Gw2Sharp.WebApi.V2.Models.Color[] { picker.SelectedColor, box2.Color }));
            else
                OnValueChanged(new ValueEventArgs<Gw2Sharp.WebApi.V2.Models.Color[]>(new Gw2Sharp.WebApi.V2.Models.Color[] { box.Color, picker.SelectedColor }));
        }

        protected override void RefreshDisplayName(string displayName)
        {
            
        }

        protected override void RefreshDescription(string description)
        {
            picker.BasicTooltipText = description;
        }

        protected override void Unload()
        {
            if (picker != null)
                picker.SelectedColorChanged -= OnPickedColorChange;
        }

        protected override void RefreshValue(Gw2Sharp.WebApi.V2.Models.Color[] value)
        {
            box.Color = value[0];
            box2.Color = value[1];
        }
    }
}
