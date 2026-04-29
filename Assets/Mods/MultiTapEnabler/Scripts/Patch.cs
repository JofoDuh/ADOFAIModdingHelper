//using ADOFAI;
//using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MultiTapEnabler
{
    internal static class Patch
    {
        //internal static TrackStyle trackColor;
        //internal static List<scrFloor> Floors
        //{
        //    get
        //    {
        //        return scnEditor.instance.floors;
        //    }
        //}
        //[HarmonyPatch(typeof(ADOStartup), "SetupLevelEventsInfo")]
        ////Core of the mod, allows Multitap
        //internal static class SetupLevelEventsInfoPatch
        //{
        //    public static void Postfix()
        //    {
        //        MultiTapEnabler.LoadMultis(Main.IsEnabled);
        //    }
        //}

        //[HarmonyPatch(typeof(LevelData), "Encode")]
        ////Before encoding, checks for if MultiTapEnabler should be a requirement or not
        //internal static class EncodePatch
        //{
        //    private static void Prefix()
        //    {
        //        MultiTapEnabler.UpdateRequiredMods(Floors);
                
        //    }
        //}

        //[HarmonyPatch(typeof(scnGame))]
        //[HarmonyPatch(nameof(scnGame.ApplyCoreEventsToFloors),
        //new Type[] { typeof(List<scrFloor>), typeof(LevelData), typeof(scrLevelMaker), typeof(List<LevelEvent>), typeof(List<LevelEvent>[]) })]
        ////Reset tapsNeeded value when removing Multitap tiles
        //internal static class ApplyCoreEventsToFloorsPatch
        //{
        //    private static void Postfix(List<scrFloor> floors, List<LevelEvent>[] floorEvents)
        //    {
        //        foreach (scrFloor Floor in floors)
        //        {
        //            List<LevelEvent> list = floorEvents[Floor.seqID];
        //            bool multitapFound = false;

        //            foreach (LevelEvent Event in list)
        //            {
        //                LevelEventType Event2 = Event.eventType;
        //                switch (Event2)
        //                {
        //                    case LevelEventType.Multitap:
        //                        multitapFound = true;
        //                        break;
        //                }
        //            }

        //            if (!multitapFound)
        //            {
        //                Floor.tapsNeeded = 1; // Reset tapsNeeded only if no Multitap was found
        //            }
        //        }
        //    }
        //}

        //[HarmonyPatch(typeof(scrController), "ResetCustomLevel")]
        //internal static class ResetCustomLevelPatch
        //{
        //    private static void Postfix()
        //    {
        //        MultiTapEnabler.ResetMultis(Floors, GCS.checkpointNum);
        //    }
        //}

        //[HarmonyPatch(typeof(RDEditorUtils), "CheckModsDependency")]
        //internal static class CheckModsDependencyPatch
        //{
        //    //Checks for if a level requires MultiTapEnabler or not
        //    public static bool Prefix(object[] mods, ref bool __result)
        //    {
        //        bool flag = mods == null || (!mods.Contains("MultiTapEnabler"));
        //        bool result;
        //        if (flag)
        //        {
        //            result = true;
        //        }
        //        else
        //        {
        //            List<object> list = new List<object>(mods);
        //            list.Remove("MultiTapEnabler");
        //            mods = list.ToArray();
        //            __result = RDEditorUtils.CheckModsDependency(mods);
        //            result = false;
        //        }
        //        return result;
        //    }
        //}

        //I spent 50 hours on this shit and its not even needed zob

        //internal static LevelData levelData
        //{
        //    get
        //    {
        //        return scnGame.instance.levelData;
        //    }
        //}
        //[HarmonyPatch(typeof(PropertyControl_Text), "ValidateInput")]
        //internal static class ValidateInputPatch
        //{
        //    //Update tile when changing Multitap value
        //    private static void Prefix(PropertyControl_Text __instance)
        //    {
        //        bool flag = __instance.propertyInfo.name != "taps";
        //        if (!flag)
        //        {
        //            string text = __instance.inputField.text;
        //            if (int.TryParse(text, out int result))
        //            {
        //                MultiTapEnabler.UpdateMulties(Floors: Floors, Events: levelData.levelEvents, update: 2, 
        //                    seqID: scnEditor.instance.selectedFloors[0].seqID, multivalue: result, LevelData: levelData);
        //            }
        //            //scnEditor.instance.ApplyEventsToFloors();
        //        }
        //    }
        //}

        //[HarmonyPatch(typeof(scnGame))]
        //[HarmonyPatch(nameof(scnGame.ApplyEventsToFloors),
        //    new Type[] { typeof(List<scrFloor>), typeof(LevelData), typeof(scrLevelMaker), typeof(List<LevelEvent>) })]
        //internal static class ApplyEventsToFloorsPatch
        //{
        //    static void Postfix() // Using Postfix since we need to extract data after it's set
        //    {
        //        // Get the private field containing CS$<>8__locals1 (it exists only inside this method)
        //        FieldInfo localsField = typeof(scnGame).GetField("CS$<>8__locals1", BindingFlags.NonPublic | BindingFlags.Instance);

        //        if (localsField != null)
        //        {
        //            object localsInstance = localsField.GetValue(null); // null because ApplyEventsToFloors is static

        //            // Now, access the fields inside CS$<>8__locals1 (track color, texture, etc.)
        //            FieldInfo colorField = localsInstance.GetType().GetField("tempStyle", BindingFlags.Public | BindingFlags.Instance);
        //            if (colorField != null)
        //            {
        //                trackColor = (TrackStyle)colorField.GetValue(localsInstance);
        //            }
        //        }
        //    }
        //}
    }
}