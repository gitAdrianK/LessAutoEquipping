using JumpKing.PauseMenu.BT.Actions;

namespace LessAutoEquipping
{
    public class TogglePreventAutoEquip : ITextToggle
    {
        public TogglePreventAutoEquip() : base(ModEntry.Preferences.ShouldPreventAutoEquip)
        {
        }

        protected override string GetName() => "Disable auto-equip";

        protected override void OnToggle()
        {
            ModEntry.Preferences.ShouldPreventAutoEquip = !ModEntry.Preferences.ShouldPreventAutoEquip;
        }
    }
}
