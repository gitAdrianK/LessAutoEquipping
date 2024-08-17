using BehaviorTree;
using EntityComponent;
using HarmonyLib;
using JumpKing;
using JumpKing.GameManager.MultiEnding;
using JumpKing.MiscEntities.WorldItems;
using JumpKing.MiscEntities.WorldItems.Inventory;
using JumpKing.Mods;
using JumpKing.PauseMenu;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace LessAutoEquipping
{
    [JumpKingMod("Zebra.LessAutoEquipping")]
    public static class ModEntry
    {
        const string IDENTIFIER = "Zebra.LessAutoEquipping";
        const string HARMONY_IDENTIFIER = "Zebra.LessAutoEquipping.Harmony";
        const string SETTINGS_FILE = "Zebra.LessAutoEquipping.Settings.xml";

        private static string AssemblyPath { get; set; }
        public static Preferences Preferences { get; private set; }

        private static Type worldItemComp;

        private static Type saveLube;
        private static MethodInfo removeWorldItem;

        [MainMenuItemSetting]
        [PauseMenuItemSetting]
        public static TogglePreventAutoEquip ToggleDiscover(object factory, GuiFormat format)
        {
            return new TogglePreventAutoEquip();
        }

        /// <summary>
        /// Called by Jump King before the level loads
        /// </summary>
        [BeforeLevelLoad]
        public static void BeforeLevelLoad()
        {
            //Debugger.Launch();

            AssemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            try
            {
                Preferences = XmlSerializerHelper.Deserialize<Preferences>($@"{AssemblyPath}\{SETTINGS_FILE}");
            }
            catch (Exception e)
            {
                Debug.WriteLine($"[ERROR] [{IDENTIFIER}] {e.Message}");
                Preferences = new Preferences();
            }
            Preferences.PropertyChanged += SaveSettingsOnFile;

            Harmony harmony = new Harmony(HARMONY_IDENTIFIER);
            MethodInfo giveWINMyRun = typeof(GiveWearableItemNode).GetMethod("MyRun", BindingFlags.NonPublic | BindingFlags.Instance);
            HarmonyMethod preventEquip = new HarmonyMethod(typeof(ModEntry).GetMethod(nameof(PreventEquip)));
            harmony.Patch(
                giveWINMyRun,
                prefix: preventEquip);

            worldItemComp = AccessTools.TypeByName("JumpKing.MiscEntities.WorldItems.WorldItemComp");
            MethodInfo onPickup = worldItemComp.GetMethod("OnPickup", BindingFlags.NonPublic | BindingFlags.Instance);
            HarmonyMethod preventPickup = new HarmonyMethod(typeof(ModEntry).GetMethod(nameof(PreventPickup)));
            harmony.Patch(
                onPickup,
                prefix: preventPickup);

            saveLube = AccessTools.TypeByName("JumpKing.SaveThread.SaveLube");
            removeWorldItem = saveLube.GetMethod("RemoveWorldItem");
        }

        public static bool PreventEquip(GiveWearableItemNode __instance, ref BTresult __result)
        {
            if (!Preferences.ShouldPreventAutoEquip)
            {
                return true;
            }
            Traverse instance = Traverse.Create(__instance);
            Items item = instance.Field("m_item").GetValue<Items>();
            InventoryManager.AddItemOnce(item);
            __result = BTresult.Success;
            return false;
        }

        public static bool PreventPickup(object __instance)
        {
            if (!Preferences.ShouldPreventAutoEquip)
            {
                return true;
            }
            Traverse instance = Traverse.Create(__instance);
            Traverse gaveItem = instance.Field("gave_item");
            if (!gaveItem.GetValue<bool>())
            {
                instance.Field("m_owner").GetValue<Entity>().Destroy();

                gaveItem.SetValue(true);

                WorldItemState state = instance.Field("m_state").GetValue<WorldItemState>();
                InventoryManager.AddItem(state.item);

                removeWorldItem.Invoke(null, new object[] { state });
                if (instance.Field("m_sfx").GetValue<bool>())
                {
                    Game1.instance.contentManager.audio.menu.CursorMove.Play();
                }
            }
            return false;
        }

        private static void SaveSettingsOnFile(object sender, System.ComponentModel.PropertyChangedEventArgs args)
        {
            try
            {
                XmlSerializerHelper.Serialize($@"{AssemblyPath}\{SETTINGS_FILE}", Preferences);
            }
            catch (Exception e)
            {
                Debug.WriteLine($"[ERROR] [{IDENTIFIER}] {e.Message}");
            }
        }
    }
}