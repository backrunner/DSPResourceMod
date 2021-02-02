using System;
using UnityEngine;
using BepInEx;
using BepInEx.Configuration;

namespace DSPResourceMod
{
  [BepInPlugin("top.backrunner.DSP.ResourceMod", "DSP Resource Mod", "1.0")]
  public class ResourceMod : BaseUnityPlugin
  {
    ConfigEntry<KeyCode> multiplierModHotKeyConfig, existedResourceModHotKeyConfig;
    ConfigEntry<float> multiplierRateConfig;
    ConfigEntry<bool> autoReloadConfig;
    ConfigEntry<int> singleVeinAmountConfig;

    void modifyMultiplierRate()
    {
      GameSave.SaveCurrentGame("_before_mutiplier_mod_" + DateTime.Now.ToString("s"));
      DSPGame.GameDesc.resourceMultiplier = multiplierRateConfig.Value;
      GameSave.SaveCurrentGame("_after_multiplier_mod_" + DateTime.Now.ToString("s"));
      UIMessageBox.Show("DSPResourceMod", "资源倍率修改成功，当前倍率：" + multiplierRateConfig.Value.ToString() + "。你可以在存档里找到修改前和修改后的存档。", "确定", 0);
    }
    void modifyExistedResource()
    {
      GameSave.SaveCurrentGame("_before_resource_mod_" + DateTime.Now.ToString("s"));
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
        for (int i = 0; i < veins.Length; i++)
        {
          if (veins[i].id != 0 && veins[i].type != EVeinType.Oil)
          {
            veins[i].amount = singleVeinAmountConfig.Value;
          }
        }
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
      if (autoReloadConfig.Value)
      {
        DSPGame.StartGame(saveGameName);
        UIMessageBox.Show("DSPResourceMod", "资源修改成功，你可以在存档里面找到修改前和修改后的存档。" + multiplierRateConfig.Value.ToString(), "确定", 0);
      }
      else
      {
        UIMessageBox.Show("DSPResourceMod", "资源修改成功，你可以在存档里面找到修改前和修改后的存档。为避免游戏出错，我们建议加载一次修改后的存档。" + multiplierRateConfig.Value.ToString(), "确定", 0);
      }
    }
    void Start()
    {
      existedResourceModHotKeyConfig = Config.Bind("HotKey", "ModifyExistedResource", KeyCode.F7, "已存在资源改成无限");
      multiplierModHotKeyConfig = Config.Bind("HotKey", "ModifyResourceMultiplier", KeyCode.F8, "修改资源倍率按键");
      multiplierRateConfig = Config.Bind("Options", "MultipilerRate", 100f, "资源倍率修改");
      autoReloadConfig = Config.Bind("Options", "AutoReload", true, "自动保存存档并读取");
      singleVeinAmountConfig = Config.Bind("Options", "SingleVeinAmount", 100000000, "单个矿的数量");
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
