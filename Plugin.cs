using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using System.Collections.Generic;
using Character;
using BepInEx.Configuration;
using HarmonyLib;
using SV.H;
using SaveData;

namespace SVS_HStartClothingState;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BasePlugin
{
    internal static new ManualLogSource Log;

    public static readonly string[] _KindNames =
    {
        "Top",
        "Bottom",
        "Bra",
        "Underwear",
        "Gloves",
        "Pantyhose",
        "Legwear",
        "Shoes"
    };

    public enum _StateNamesClothes
    {
        On,
        Shift,
        Off
    }

    public enum _StateNamesShoes
    {
        On,
        Off
    }

    public static ConfigEntry<bool> EnabledPlayer { get; set; }
    public static ConfigEntry<_StateNamesClothes> ConfigPlayerTop { get; set; }
    public static ConfigEntry<_StateNamesClothes> ConfigPlayerBottom { get; set; }
    public static ConfigEntry<_StateNamesClothes> ConfigPlayerBra { get; set; }
    public static ConfigEntry<_StateNamesClothes> ConfigPlayerUnderwear { get; set; }
    public static ConfigEntry<_StateNamesClothes> ConfigPlayerGloves { get; set; }
    public static ConfigEntry<_StateNamesClothes> ConfigPlayerPantyhose { get; set; }
    public static ConfigEntry<_StateNamesClothes> ConfigPlayerLegwear { get; set; }
    public static ConfigEntry<_StateNamesShoes> ConfigPlayerShoes { get; set; }
    public static ConfigEntry<bool> EnabledNPCs { get; set; }
    public static ConfigEntry<_StateNamesClothes> ConfigNPCsTop { get; set; }
    public static ConfigEntry<_StateNamesClothes> ConfigNPCsBottom { get; set; }
    public static ConfigEntry<_StateNamesClothes> ConfigNPCsBra { get; set; }
    public static ConfigEntry<_StateNamesClothes> ConfigNPCsUnderwear { get; set; }
    public static ConfigEntry<_StateNamesClothes> ConfigNPCsGloves { get; set; }
    public static ConfigEntry<_StateNamesClothes> ConfigNPCsPantyhose { get; set; }
    public static ConfigEntry<_StateNamesClothes> ConfigNPCsLegwear { get; set; }
    public static ConfigEntry<_StateNamesShoes> ConfigNPCsShoes { get; set; }

    public static HScene HSceneInstance;
    public static List<Actor> Act;
    public static int FramesTillClothesChange = 0;

    public override void Load()
    {
        // Plugin startup logic
        Log = base.Log;
        Act = new List<Actor>();

        EnabledPlayer = Config.Bind("Player", "Enable for Player", false, new ConfigDescription("Enable H Start Clothing State for Player.", null, new ConfigurationManagerAttributes { Order = 9 }));
        ConfigPlayerTop = Config.Bind("Player", "Player " + _KindNames[0], new _StateNamesClothes(), new ConfigDescription(_KindNames[0], null, new ConfigurationManagerAttributes { Order = 8 }));
        ConfigPlayerBottom = Config.Bind("Player", "Player " + _KindNames[1], new _StateNamesClothes(), new ConfigDescription(_KindNames[0], null, new ConfigurationManagerAttributes { Order = 7 }));
        ConfigPlayerBra = Config.Bind("Player", "Player " + _KindNames[2], new _StateNamesClothes(), new ConfigDescription(_KindNames[0], null, new ConfigurationManagerAttributes { Order = 6 }));
        ConfigPlayerUnderwear = Config.Bind("Player", "Player " + _KindNames[3], new _StateNamesClothes(), new ConfigDescription(_KindNames[0], null, new ConfigurationManagerAttributes { Order = 5 }));
        ConfigPlayerGloves = Config.Bind("Player", "Player " + _KindNames[4], new _StateNamesClothes(), new ConfigDescription(_KindNames[0], null, new ConfigurationManagerAttributes { Order = 4 }));
        ConfigPlayerPantyhose = Config.Bind("Player", "Player " + _KindNames[5], new _StateNamesClothes(), new ConfigDescription(_KindNames[0], null, new ConfigurationManagerAttributes { Order = 3 }));
        ConfigPlayerLegwear = Config.Bind("Player", "Player " + _KindNames[6], new _StateNamesClothes(), new ConfigDescription(_KindNames[0], null, new ConfigurationManagerAttributes { Order = 2 }));
        ConfigPlayerShoes = Config.Bind("Player", "Player " + _KindNames[7], new _StateNamesShoes(), new ConfigDescription(_KindNames[0], null, new ConfigurationManagerAttributes { Order = 1 }));

        EnabledNPCs = Config.Bind("NPCs", "Enable for NPCs", false, new ConfigDescription("Enable H Start Clothing State for NPCs.", null, new ConfigurationManagerAttributes { Order = 9 }));
        ConfigNPCsTop = Config.Bind("NPCs", "NPCs " + _KindNames[0], new _StateNamesClothes(), new ConfigDescription(_KindNames[0], null, new ConfigurationManagerAttributes { Order = 8 }));
        ConfigNPCsBottom = Config.Bind("NPCs", "NPCs " + _KindNames[1], new _StateNamesClothes(), new ConfigDescription(_KindNames[0], null, new ConfigurationManagerAttributes { Order = 7 }));
        ConfigNPCsBra = Config.Bind("NPCs", "NPCs " + _KindNames[2], new _StateNamesClothes(), new ConfigDescription(_KindNames[0], null, new ConfigurationManagerAttributes { Order = 6 }));
        ConfigNPCsUnderwear = Config.Bind("NPCs", "NPCs " + _KindNames[3], new _StateNamesClothes(), new ConfigDescription(_KindNames[0], null, new ConfigurationManagerAttributes { Order = 5 }));
        ConfigNPCsGloves = Config.Bind("NPCs", "NPCs " + _KindNames[4], new _StateNamesClothes(), new ConfigDescription(_KindNames[0], null, new ConfigurationManagerAttributes { Order = 4 }));
        ConfigNPCsPantyhose = Config.Bind("NPCs", "NPCs " + _KindNames[5], new _StateNamesClothes(), new ConfigDescription(_KindNames[0], null, new ConfigurationManagerAttributes { Order = 3 }));
        ConfigNPCsLegwear = Config.Bind("NPCs", "NPCs " + _KindNames[6], new _StateNamesClothes(), new ConfigDescription(_KindNames[0], null, new ConfigurationManagerAttributes { Order = 2 }));
        ConfigNPCsShoes = Config.Bind("NPCs", "NPCs " + _KindNames[7], new _StateNamesShoes(), new ConfigDescription(_KindNames[0], null, new ConfigurationManagerAttributes { Order = 1 }));

        Harmony.CreateAndPatchAll(typeof(Hooks));
    }
}

public static class Hooks
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Human), nameof(Human.LateUpdate))]
    public unsafe static void Postfix_Human_LateUpdate(ref Human __instance)
    {
        Human h = __instance;
        if (Plugin.Act.Count == 0) return;

        if (h == Plugin.Act[0].chaCtrl)
        {
            if (Plugin.FramesTillClothesChange > 0)
            {
                Plugin.FramesTillClothesChange--;
                return;
            }

            for (int i = 0; i < Plugin.Act.Count; i++)
            {
                //0 is always Player
                if (i == 0)
                {
                    if (Plugin.EnabledPlayer.Value)
                    {
                        Plugin.Act[i].chaCtrl.cloth.SetClothesState(0, ((ChaFileDefine.ClothesState)Plugin.ConfigPlayerTop.Value), false);
                        Plugin.Act[i].chaCtrl.cloth.SetClothesState(1, ((ChaFileDefine.ClothesState)Plugin.ConfigPlayerBottom.Value), false);
                        Plugin.Act[i].chaCtrl.cloth.SetClothesState(2, ((ChaFileDefine.ClothesState)Plugin.ConfigPlayerBra.Value), false);
                        Plugin.Act[i].chaCtrl.cloth.SetClothesState(3, ((ChaFileDefine.ClothesState)Plugin.ConfigPlayerUnderwear.Value), false);
                        Plugin.Act[i].chaCtrl.cloth.SetClothesState(4, ((ChaFileDefine.ClothesState)Plugin.ConfigPlayerGloves.Value), false);
                        Plugin.Act[i].chaCtrl.cloth.SetClothesState(5, ((ChaFileDefine.ClothesState)Plugin.ConfigPlayerPantyhose.Value), false);
                        Plugin.Act[i].chaCtrl.cloth.SetClothesState(6, ((ChaFileDefine.ClothesState)Plugin.ConfigPlayerLegwear.Value), false);
                        var shoes = ((int)Plugin.ConfigPlayerShoes.Value);
                        if (shoes == 1) shoes = 2;
                        Plugin.Act[i].chaCtrl.cloth.SetClothesState(7, (ChaFileDefine.ClothesState)shoes, false);
                        Plugin.Act[i].chaCtrl.cloth.UpdateClothesStateAll();
                    }
                }
                else
                {
                    if (Plugin.EnabledNPCs.Value)
                    {
                        Plugin.Act[i].chaCtrl.cloth.SetClothesState(0, ((ChaFileDefine.ClothesState)Plugin.ConfigNPCsTop.Value), false);
                        Plugin.Act[i].chaCtrl.cloth.SetClothesState(1, ((ChaFileDefine.ClothesState)Plugin.ConfigNPCsBottom.Value), false);
                        Plugin.Act[i].chaCtrl.cloth.SetClothesState(2, ((ChaFileDefine.ClothesState)Plugin.ConfigNPCsBra.Value), false);
                        Plugin.Act[i].chaCtrl.cloth.SetClothesState(3, ((ChaFileDefine.ClothesState)Plugin.ConfigNPCsUnderwear.Value), false);
                        Plugin.Act[i].chaCtrl.cloth.SetClothesState(4, ((ChaFileDefine.ClothesState)Plugin.ConfigNPCsGloves.Value), false);
                        Plugin.Act[i].chaCtrl.cloth.SetClothesState(5, ((ChaFileDefine.ClothesState)Plugin.ConfigNPCsPantyhose.Value), false);
                        Plugin.Act[i].chaCtrl.cloth.SetClothesState(6, ((ChaFileDefine.ClothesState)Plugin.ConfigNPCsLegwear.Value), false);
                        var shoes = ((int)Plugin.ConfigNPCsShoes.Value);
                        if (shoes == 1) shoes = 2;
                        Plugin.Act[i].chaCtrl.cloth.SetClothesState(7, (ChaFileDefine.ClothesState)shoes, false);
                    }
                }
            }

            Plugin.Act.Clear();
        }
    }


    [HarmonyPostfix]
    [HarmonyPatch(typeof(HScene), nameof(HScene.Start))]
    private static void HSceneInitialize(HScene __instance)
    {
        if ((!Plugin.EnabledPlayer.Value) && (!Plugin.EnabledNPCs.Value)) return;

        if (Plugin.HSceneInstance == __instance) return;
        Plugin.HSceneInstance = __instance;

        Plugin.FramesTillClothesChange = 5;
        Plugin.Act.Clear();
        foreach (HActor ha in __instance.Actors) Plugin.Act.Add(ha.Actor);
    }
}

#pragma warning disable 0169, 0414, 0649
internal sealed class ConfigurationManagerAttributes
{
    public bool? ShowRangeAsPercent;
    public System.Action<BepInEx.Configuration.ConfigEntryBase> CustomDrawer;
    public CustomHotkeyDrawerFunc CustomHotkeyDrawer;
    public delegate void CustomHotkeyDrawerFunc(BepInEx.Configuration.ConfigEntryBase setting, ref bool isCurrentlyAcceptingInput);
    public bool? Browsable;
    public string Category;
    public object DefaultValue;
    public bool? HideDefaultButton;
    public bool? HideSettingName;
    public string Description;
    public string DispName;
    public int? Order;
    public bool? ReadOnly;
    public bool? IsAdvanced;
    public System.Func<object, string> ObjToStr;
    public System.Func<string, object> StrToObj;
}