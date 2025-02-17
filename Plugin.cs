﻿using System;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace BetterSkeld
{
    [BepInPlugin(Id, Name, Version)]
    public class BetterSkeldPlugin : BasePlugin
    {
        public const string Id = "com.dadoum.betterskeld";
        public const string Name = "BetterSkeld";
        public const string Version = "0.1.2";
        public static BetterSkeldPlugin Instance { get; private set; }

        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Start))]
        public static class ShipStatusAwakePatch
        {
            public static void Prefix(ShipStatus __instance)
            {
                var patcher = new GameObject("BetterSkeld Patcher");
                switch (__instance.Type)
                {
                    case ShipStatus.MapType.Ship:
                        patcher.AddComponent<SkeldPatcher>();
                        break;
                }
            }
        }

        public class SkeldPatcher : MonoBehaviour
        {
            private void FixedUpdate()
            {
                var client = AmongUsClient.Instance;
                
                // On charge Polus 
                var res = Addressables.LoadAssetAsync<GameObject>(client.ShipPrefabs[(Index) (int) ShipStatus.MapType.Pb]).Result;
                if (!res)
                    return; // On réessaiera de charger à la prochaine update
                
                Instance.Log.LogDebug("Patching TheSkeld...");
                
                // Récup les ressources (avec une méthode moche mais rapide).
                GameObject admin = null;
                GameObject animation = null;
                Vent adminVent = null;
                Vent cafeteriaVent = null;
                Vent navNordVent = null;
                Vent navSudVent = null;
                Vent weaponsVent = null;
                Vent shieldVent = null;
                Vent couloirVent = null;
                Vent elecVent = null;
                Vent reactorNordVent = null;
                Vent reactorSudVent = null;
                Vent engineNordVent = null;
                Vent engineSudVent = null;
                Vent securityVent = null;
                Vent medVent = null;
                var gameObjects = GameObject.FindObjectsOfType<GameObject>();
                foreach (var gameObject in gameObjects)
                {
                    var name = gameObject.name;
                    if (name == "MapRoomConsole")
                    {
                        admin = gameObject;
                        continue;
                    }
                    else if (name == "MapAnimation")
                    {
                        animation = gameObject;
                        continue;
                    }

                    var vent = gameObject.GetComponent<Vent>();
                    if (vent != null)
                    {
                        if (name == "AdminVent")
                        {
                            adminVent = vent;
                        }
                        else if (name == "CafeVent")
                        {
                            cafeteriaVent = vent;
                        }
                        else if (name == "NavVentNorth")
                        {
                            navNordVent = vent;
                        }
                        else if (name == "NavVentSouth")
                        {
                            navSudVent = vent;
                        }
                        else if (name == "WeaponsVent")
                        {
                            weaponsVent = vent;
                        }
                        else if (name == "ShieldsVent")
                        {
                            shieldVent = vent;
                        }
                        else if (name == "BigYVent")
                        {
                            couloirVent = vent;
                        }
                        else if (name == "ElecVent")
                        {
                            elecVent = vent;
                        }
                        else if (name == "UpperReactorVent")
                        {
                            reactorNordVent = vent;
                        }
                        else if (name == "ReactorVent")
                        {
                            reactorSudVent = vent;
                        }
                        else if (name == "LEngineVent")
                        {
                            engineNordVent = vent;
                        }
                        else if (name == "REngineVent")
                        {
                            engineSudVent = vent;
                        }
                        else if (name == "SecurityVent")
                        {
                            securityVent = vent;
                        }
                        else if (name == "MedVent")
                        {
                            medVent = vent;
                        }
                    }
                }

                if (admin == null ||
                    animation == null ||
                    adminVent == null ||
                    cafeteriaVent == null ||
                    navNordVent == null ||
                    navSudVent == null ||
                    weaponsVent == null ||
                    shieldVent == null ||
                    couloirVent == null ||
                    elecVent == null ||
                    reactorNordVent == null ||
                    reactorSudVent == null ||
                    engineNordVent == null ||
                    engineSudVent == null ||
                    securityVent == null ||
                    medVent == null)
                    return;
                
                var vitalsObj = res.transform.Find("Office/panel_vitals").gameObject;
                
                // Ajouter les signaux vitaux
                Instance.Log.LogDebug("Placing vitals...");
                var vitals = GameObject.Instantiate(vitalsObj);
                vitals.transform.position = new Vector3(1.9162f, -16.1985f, -2.4142f);
                vitals.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
                vitals.transform.localScale = new Vector3(0.6636f, 0.7418f, 1f);
                
                // Désactiver l'admin
                Instance.Log.LogDebug("Disabling admin...");
                admin.GetComponent<CircleCollider2D>().enabled = false;
                
                // Désactiver l'animation car il n'y a plus de carte, donc pour le RP faut plus qu'on la voie
                animation.active = false;
                
                // Relier toutes les vents de droite entre elles...
                Instance.Log.LogDebug("Rerouting vents...");
                weaponsVent.Center = cafeteriaVent;
                cafeteriaVent.Center = weaponsVent;

                navNordVent.Right = navSudVent;
                navSudVent.Right = navNordVent;

                navNordVent.Center = couloirVent;
                navSudVent.Center = couloirVent;
                couloirVent.Center = navNordVent;
                
                weaponsVent.Left = couloirVent;

                adminVent.Center = shieldVent;
                shieldVent.Center = adminVent;

                // ... et toutes celles de gauche entre elles
                engineNordVent.Center = medVent;
                medVent.Center = engineNordVent;

                reactorNordVent.Center = securityVent;
                securityVent.Center = reactorNordVent;

                reactorNordVent.Left = reactorSudVent;
                reactorSudVent.Left = reactorNordVent;
                
                reactorSudVent.Center = securityVent;

                elecVent.Center = engineSudVent;
                engineSudVent.Center = elecVent;
                
                Instance.Log.LogInfo("Successfully patched TheSkeld !");
                Destroy(this.gameObject);
            }
        }

        [HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
        public static class VersionShowerPatch
        {
            public static void Postfix(VersionShower __instance)
            {
                __instance.text.text += $"<size=70%> + {Name} v{Version} (Dadoum)</size>";
            }
        }
        
        public override void Load()
        {
            Instance = this;
            
            ClassInjector.RegisterTypeInIl2Cpp<SkeldPatcher>();
            new Harmony(Id).PatchAll();
            
            SceneManager.sceneLoaded += (Action<Scene, LoadSceneMode>) ((scene, loadSceneMode) =>
            {
                ModManager.Instance.ShowModStamp();
            });
        }
    }
}
