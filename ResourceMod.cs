using System;
using UnityEngine;
using BepInEx;
using BepInEx.Configuration;

namespace DSPResourceMod
{
  [BepInPlugin("top.backrunner.DSP.ResourceMod", "DSP Resource Mod", "1.1")]
  public class ResourceMod : BaseUnityPlugin
  {
    ConfigEntry<KeyCode> multiplierModHotKeyConfig, existedResourceModHotKeyConfig;
    ConfigEntry<float> multiplierRateConfig;
    ConfigEntry<bool> autoReloadConfig;
    ConfigEntry<bool> modExistedResourceByMultiplier;
    ConfigEntry<int> singleVeinAmountConfig;
    ConfigEntry<bool> isDebug;

    float modRate = 0;

    void modifyMultiplierRate()
    {
      if (DSPGame.GameDesc == null)
      {
        return;
      }
      GameSave.SaveCurrentGame("_before_mutiplier_mod_" + DateTime.Now.ToString("s"));
      modRate = multiplierRateConfig.Value / DSPGame.GameDesc.resourceMultiplier;
      DSPGame.GameDesc.resourceMultiplier = multiplierRateConfig.Value;
      GameSave.SaveCurrentGame("_after_multiplier_mod_" + DateTime.Now.ToString("s"));
      Language language = Localization.language;
      if (language == Language.zhCN)
      {
        UIMessageBox.Show("DSPResourceMod", "资源倍率为修改成功，当前资源倍率为" + multiplierRateConfig.Value.ToString() + "。你可以在存档列表里找到修改前和修改后的存档。", "确定", 0);
      }
      else
      {
        UIMessageBox.Show("DSPResourceMod", "Multipler modification completed, current rate: " + multiplierRateConfig.Value.ToString() + ". You can find the pre-modified and post-modified savegame in the savegames.", "Confirm", 0);
      }
    }
    void modifyExistedResource()
    {
      if (DSPGame.GameDesc == null)
      {
        return;
      }
      if (modRate == 0)
      {
        modRate = multiplierRateConfig.Value / DSPGame.GameDesc.resourceMultiplier;
      }
      if (modRate == 0 || modRate == 1)
      {
        if (Localization.language == Language.zhCN)
        {
          UIRealtimeTip.Popup("存档资源倍率未改动，资源量无需修改.");
        } else
        {
          UIRealtimeTip.Popup("There is no need to modify the resource because the resource multiplier rate is not changed");
        }
        return;
      }
      GameSave.SaveCurrentGame("_before_resource_mod_" + DateTime.Now.ToString("s"));
      UIRealtimeTip.Popup("Game saved before modifying.");
      foreach (PlanetFactory factory in GameMain.data.factories)
      {
        if (factory == null)
        {
          continue;
        }
        // update factory data
        VeinData[] veins = factory.veinPool;
        if (veins == null)
        {
          continue;
        }
        if (isDebug.Value)
        {
          UIRealtimeTip.Popup("Current mod rate: " + modRate.ToString());
        }
          for (int i = 0; i < veins.Length; i++)
        {
          if (veins[i].id != 0 && veins[i].type != EVeinType.Oil)
          {
            if (modExistedResourceByMultiplier.Value)
            {
              veins[i].amount = (int)(veins[i].amount * modRate);
            }
            else
            {
              veins[i].amount = singleVeinAmountConfig.Value;
            }
          }
        }
        // reset mod rate
        modRate = 0;
        // update planet data
        PlanetData.VeinGroup[] veinGroups = factory.planet.veinGroups;
        long[] veinAmounts = new long[factory.planet.veinAmounts.Length];
        for (int i = 0; i < veinGroups.Length; i++)
        {
          if (veinGroups[i].type != EVeinType.Oil)
          {
            veinGroups[i].amount = singleVeinAmountConfig.Value * veinGroups[i].count;
            veinAmounts[(int)veinGroups[i].type] += veinGroups[i].amount;
          }
        }
        // modify planet total data
        for (int i = 0; i < veinAmounts.Length; i++)
        {
          factory.planet.veinAmounts[i] = veinAmounts[i];
        }
      }
      string saveGameName = "_after_resource_mod_" + DateTime.Now.ToString("s");
      GameSave.SaveCurrentGame(saveGameName);
      UIRealtimeTip.Popup("Game saved after modifying.");
      if (autoReloadConfig.Value)
      {
        UIRealtimeTip.Popup("Auto reload game.");
        DSPGame.StartGame(saveGameName);
        Language language = Localization.language;
        if (language == Language.zhCN)
        {
          UIMessageBox.Show("DSPResourceMod", "资源修改成功，你可以在存档里面找到修改前和修改后的存档。", "确定", 0);
        }
        else
        {
          UIMessageBox.Show("DSPResourceMod", "The resources are modified successfully, you can find the savegame before and after the modifications in the savegame list.", "确定", 0);
        }

      }
      else
      {
        UIMessageBox.Show("DSPResourceMod", "The resources are modified successfully, you can find the savegame before and after the modifications in the savegame list.\nTo avoid errors in the game, we recommend loading the modified savegame once.", "确定", 0);
      }
    }
    void Start()
    {
      existedResourceModHotKeyConfig = Config.Bind("HotKey", "Modify existed resource", KeyCode.F7, "已存在资源改成无限");
      multiplierModHotKeyConfig = Config.Bind("HotKey", "Modify resource multiplier", KeyCode.F8, "修改资源倍率按键");
      multiplierRateConfig = Config.Bind("Options", "Multipiler rate", 100f, "资源倍率修改");
      autoReloadConfig = Config.Bind("Options", "Auto reload", true, "自动保存存档并读取");
      singleVeinAmountConfig = Config.Bind("Options", "Single vein amount", 100000000, "单个矿的数量");
      modExistedResourceByMultiplier = Config.Bind("Options", "Use modified mulitplier value to modify vein amount", false, "是否使用修改的资源倍率修改现有矿物");
      isDebug = Config.Bind("Debug", "Open debug messages", false, "打开调试信息");
    }
    void Update()
    {
      if (Input.GetKeyDown(multiplierModHotKeyConfig.Value))
      {
        modifyMultiplierRate();
      }
      if (Input.GetKeyDown(existedResourceModHotKeyConfig.Value))
      {
        modifyExistedResource();
      }
    }
  }
}
