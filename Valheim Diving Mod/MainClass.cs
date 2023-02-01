using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Jotunn.Configs;
using Jotunn.Managers;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Steamworks;
using static Mono.Security.X509.X520;

namespace Valheim_Diving_Mod
{
    namespace MainClass
    {
        [BepInPlugin("MainStreetGaming.BetterDiving", "Valheim Better Diving", "1.0.0")]
        [BepInProcess("valheim.exe")]
        [BepInDependency(Jotunn.Main.ModGuid)]

        public class MainClass : BaseUnityPlugin
        {
            public static Harmony harmony;

            //config values
            public static ConfigEntry<string> configGreeting;

            public static ConfigEntry<bool> configDisplayGreeting;
            public static ConfigEntry<bool> showYouCanBreatheMsg;
            public static ConfigEntry<bool> showDivingMsg;
            public static ConfigEntry<string> divingMsg;
            public static ConfigEntry<string> divingCancelledMsg;
            public static ConfigEntry<bool> showSurfacingMsg;
            public static ConfigEntry<string> surfacingMsg;
            public static ConfigEntry<bool> allowRestInWater;
            public static ConfigEntry<bool> owBIPos;
            public static ConfigEntry<float> owBIPosX;
            public static ConfigEntry<float> owBIPosY;
            public static ConfigEntry<float> breatheDrain;
            public static ConfigEntry<float> c_swimStaminaDrainMinSkill;
            public static ConfigEntry<float> c_swimStaminaDrainMaxSkill;
            public static ConfigEntry<bool> ow_staminaRestoreValue;
            public static ConfigEntry<float> ow_staminaRestorPerTick;
            public static ConfigEntry<float> ow_color_brightness_factor;
            public static ConfigEntry<float> ow_fogdensity_factor;
            public static ConfigEntry<float> ow_Min_fogdensity;
            public static ConfigEntry<float> ow_Max_fogdensity;
            public static ConfigEntry<bool> doDebug;


            //character values
            public static float loc_m_distance;
            public static float loc_m_minDistance;
            public static float loc_m_m_maxDistance = 0f;
            public static float loc_m_WaterDist = -5000f;
            public static float loc_m_diveAaxis = 0f;
            public static float loc_m_player_dist = 0f;
            public static float loc_cam_pos_y = 0;
            public static float char_swim_depth = 0;
            public static Vector3 character_pos;

            public static bool toggleDive = false;
            public static bool is_diving = false;
            public static bool is_swimming = false;
            public static bool cameraEffect = false;
            public static bool loc_is_walking = false;
            public static bool is_rotated_down = false;
            public static bool is_rotated_up = false;
            public static bool has_set_rest_swim_depth = false;

            //camera values
            public static Color defaultFogColor;

            public static float defaultFogDensity;

            public static bool reder_settings_updated_camera = true;
            public static bool set_force_env = false;

            public static float sky_cam_fogd = 0f;
            public static string env_before = "";
            public static bool has_set_render = false;
            public static float minwaterdist = 0f;

            //water values
            public static bool has_water_flipped = false;
            public static Material mai_water_mat = null;
            public static float water_level_camera = 30f;
            public static float water_level_player = 30f;

            //player stamina
            public static float m_swimStaminaDrainMinSkill = 0f;
            public static float m_swimStaminaDrainMaxSkill = 0f;

            public static float loc_remining_diveTime = 1f;
            public static bool dive_timer_is_running = false;
            public static bool restor_timer_is_running = false;
            public static float breathBarRemoveDelay = 1f;
            public static float breathDelayTimer;
            public static float highestOxygen = 1f;
            public static string last_activity = "";
            public static bool came_from_diving = false;
            public static bool has_ping_sent = true;

            public static bool has_created_breathe_bar = false;
            public static GameObject loc_breath_bar;
            public static GameObject loc_depleted_breath;
            public static GameObject loc_breath_bar_bg;
            public static GameObject loc_breathe_overlay;
            public static Sprite breath_prog_sprite;
            public static Texture2D breath_prog_tex;
            public static Sprite breath_bg_sprite;
            public static Texture2D breath_bg_tex;
            public static Sprite breath_overlay_sprite;
            public static Texture2D breath_overlay_tex;

            public static bool is_take_rest_in_water = false;
            public static float player_stamina_to_update = 100f;
            public static float player_max_stamina = 100f;

            public static Shader water_shader;
            public static Texture2D water_texture;
            public static Material water_mat;

            public static Material[] water_volum_list;
            public static bool apply_mat = false;

            //Env vars
            public static string EnvName = "";

            //EpicLoot vars
            public static bool epicLootLoaded = false;
            public static bool epicLootWaterRunning = false;

            //DivingSkill
            public static Skills.SkillType DivingSkillType = 0;
            public static Texture2D dive_texture;
            public static Sprite DivingSprite;
            public static float m_diveSkillImproveTimer = 0f;
            public static float m_minDiveSkillImprover = 0f;

            public System.Collections.IEnumerator Start()
            {
                DebugLog("Better Diving Mod: Ienumerator start!");

                string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                AssetBundle localAssetBundle = AssetBundle.LoadFromFile(assemblyFolder + "/watermat.assets");

                if (localAssetBundle == null) {
                    Debug.LogError("load_assets_failed");
                    return null;
                }

                water_mat = localAssetBundle.LoadAsset<Material>("WaterMat");
                dive_texture = localAssetBundle.LoadAsset<Texture2D>("vhdm_dive_icon_1");
                //breath_prog_sprite = localAssetBundle.LoadAsset<Sprite>("Square");

                //Texture path relative to "plugins" BepInEx folder
                breath_prog_tex = Jotunn.Utils.AssetUtils.LoadTexture("MainStreetGaming-BetterDiving/Assets/Image/BreathBar.png");
                breath_prog_sprite = Sprite.Create(breath_prog_tex, new Rect(0f, 0f, breath_prog_tex.width, breath_prog_tex.height), Vector2.zero);
                breath_bg_tex = Jotunn.Utils.AssetUtils.LoadTexture("MainStreetGaming-BetterDiving/Assets/Image/BreathBar_BG.png");
                breath_bg_sprite = Sprite.Create(breath_bg_tex, new Rect(0f, 0f, breath_bg_tex.width, breath_bg_tex.height), Vector2.zero);
                breath_overlay_tex = Jotunn.Utils.AssetUtils.LoadTexture("MainStreetGaming-BetterDiving/Assets/Image/BreathBarOverlay.png");
                breath_overlay_sprite = Sprite.Create(breath_overlay_tex, new Rect(0f, 0f, breath_overlay_tex.width, breath_overlay_tex.height), Vector2.zero);

                //Register DivingSkill
                MainClass.DivingSprite = Sprite.Create(dive_texture, new Rect(0f, 0f, dive_texture.width, dive_texture.height), Vector2.zero);
                    MainClass.DivingSkillType = SkillManager.Instance.AddSkill(new SkillConfig
                    {
                        Identifier = "MainStreetGaming.BetterDiving.divingduration.1",
                        Name = "Diving",
                        Description = "Dive duration.",
                        IncreaseStep = 0.25f,
                        Icon = MainClass.DivingSprite
                    });

                localAssetBundle.Unload(false);
                return null;
            }

            public static System.Collections.IEnumerator StartCountdown()
            {
                while (true)
                {
                    yield return new WaitForSeconds(1.0f);

                    //Debug
                    DebugLog("---------------------Section Debug Start------------------------");

                    DebugLog("is_diving" + " -> " + is_diving);
                    DebugLog("is_swimming" + " -> " + is_swimming);
                    DebugLog("m_minDiveSkillImprover" + " -> " + m_minDiveSkillImprover);
                    DebugLog("loc_remining_diveTime" + " -> " + loc_remining_diveTime);
                    DebugLog("loc_m_distance" + " -> " + loc_m_distance);
                    DebugLog("loc_m_minDistance" + " -> " + loc_m_minDistance);
                    DebugLog("loc_m_m_maxDistance" + " -> " + loc_m_m_maxDistance);
                    DebugLog("loc_m_WaterDist" + " -> " + loc_m_WaterDist);
                    DebugLog("loc_m_diveAaxis" + " -> " + loc_m_diveAaxis);
                    DebugLog("loc_m_player_dist" + " -> " + loc_m_player_dist);
                    DebugLog("loc_cam_pos_y" + " -> " + loc_cam_pos_y);
                    DebugLog("char_swim_depth" + " -> " + char_swim_depth);
                    DebugLog("cameraEffect" + " -> " + cameraEffect);
                    DebugLog("loc_is_walking" + " -> " + loc_is_walking);
                    DebugLog("is_rotated_down" + " -> " + is_rotated_down);
                    DebugLog("is_rotated_up" + " -> " + is_rotated_up);
                    DebugLog("has_set_rest_swim_depth" + " -> " + has_set_rest_swim_depth);
                    DebugLog("defaultFogColor" + " -> " + defaultFogColor);
                    DebugLog("defaultFogDensity" + " -> " + defaultFogDensity);
                    DebugLog("reder_settings_updated_camera" + " -> " + reder_settings_updated_camera);
                    DebugLog("set_force_env" + " -> " + set_force_env);
                    DebugLog("sky_cam_fogd" + " -> " + sky_cam_fogd);
                    DebugLog("env_before" + " -> " + env_before);
                    DebugLog("has_set_render" + " -> " + has_set_render);
                    DebugLog("minwaterdist" + " -> " + minwaterdist);
                    DebugLog("has_water_flipped" + " -> " + has_water_flipped);
                    DebugLog("water_level_camera" + " -> " + water_level_camera);
                    DebugLog("water_level_player" + " -> " + water_level_player);
                    DebugLog("m_swimStaminaDrainMinSkill" + " -> " + m_swimStaminaDrainMinSkill);
                    DebugLog("m_swimStaminaDrainMaxSkill" + " -> " + m_swimStaminaDrainMaxSkill);
                    DebugLog("loc_remining_diveTime" + " -> " + loc_remining_diveTime);
                    DebugLog("dive_timer_is_running" + " -> " + dive_timer_is_running);
                    DebugLog("restor_timer_is_running" + " -> " + restor_timer_is_running);
                    DebugLog("last_activity" + " -> " + last_activity);
                    DebugLog("came_from_diving" + " -> " + came_from_diving);
                    DebugLog("has_ping_sent" + " -> " + has_ping_sent);
                    DebugLog("has_created_breathe_bar" + " -> " + has_created_breathe_bar);
                    DebugLog("is_take_rest_in_water" + " -> " + is_take_rest_in_water);
                    DebugLog("player_stamina_to_update" + " -> " + player_stamina_to_update);
                    DebugLog("player_max_stamina" + " -> " + player_max_stamina);
                    DebugLog("apply_mat" + " -> " + apply_mat);
                    DebugLog("EnvName" + " -> " + EnvName);
                    DebugLog("epicLootLoaded" + " -> " + epicLootLoaded);
                    DebugLog("epicLootWaterRunning" + " -> " + epicLootWaterRunning);
                    DebugLog("m_diveSkillImproveTimer" + " -> " + m_diveSkillImproveTimer);
                    DebugLog("m_minDiveSkillImprover" + " -> " + m_minDiveSkillImprover);
                    DebugLog("toggleDive" + " -> " + toggleDive);

                    DebugLog("----------------------Section Debug End-------------------------");

                    DebugLog("---------------------Section Update Some Values Start------------------------");

                    if(Player.m_localPlayer != null)
                    {
                        player_max_stamina = Player.m_localPlayer.GetMaxStamina();
                        player_stamina_to_update = Player.m_localPlayer.GetStamina();
                    }

                    if (is_diving && is_swimming)
                    {
                        m_minDiveSkillImprover++;
                        if (loc_remining_diveTime >= 0f)
                        {
                            if (loc_remining_diveTime >= 1f) loc_remining_diveTime = 1f;

                            var one_percentage = 1f / 120f;

                            float final_dive_drain = breatheDrain.Value;
                            float final_dive_drain_one_percentage = breatheDrain.Value / 120f;

                            DebugLog("one_percentage" + " -> " + one_percentage);
                            DebugLog("final_dive_drain" + " -> " + final_dive_drain);
                            DebugLog("final_dive_drain_one_percentage" + " -> " + final_dive_drain_one_percentage);


                            if (Player.m_localPlayer != null)
                            {
                                float skillFactor = Player.m_localPlayer.GetSkillFactor(DivingSkillType);
                                float num = Mathf.Lerp(1f, 0.5f, skillFactor);
                                final_dive_drain = (final_dive_drain * num) / 120f;
                                if (final_dive_drain > final_dive_drain_one_percentage) final_dive_drain = final_dive_drain_one_percentage;
                                
                                loc_remining_diveTime -= final_dive_drain;

                                DebugLog("skillFactor" + " -> " + skillFactor);
                                DebugLog("converted skillFactor" + " -> " + num);
                                DebugLog("final_dive_drain" + " -> " + final_dive_drain);
                            }
                        }
                        if (loc_remining_diveTime <= 0f)
                        {
                            player_stamina_to_update -= player_max_stamina * 0.2f;
                        }
                    }
                    else
                    {
                        if (loc_remining_diveTime < 1f)
                        {
                            var one_percentage = 1f / 120f;

                            float final_dive_drain = breatheDrain.Value;
                            float final_dive_drain_one_percentage = breatheDrain.Value / 120f;

                            DebugLog("one_percentage" + " -> " + one_percentage);
                            DebugLog("final_dive_drain" + " -> " + final_dive_drain);
                            DebugLog("final_dive_drain_one_percentage" + " -> " + final_dive_drain_one_percentage);

                            if (Player.m_localPlayer != null)
                            {
                                float skillFactor = Player.m_localPlayer.GetSkillFactor(DivingSkillType);
                                float num = Mathf.Lerp(1f, 0.5f, skillFactor);
                                final_dive_drain = (final_dive_drain * num) / 120f;
                                if (final_dive_drain > final_dive_drain_one_percentage) final_dive_drain = final_dive_drain_one_percentage;

                                loc_remining_diveTime += 0.125f;

                                DebugLog("skillFactor" + " -> " + skillFactor);
                                DebugLog("converted skillFactor" + " -> " + num);
                                DebugLog("final_dive_drain" + " -> " + final_dive_drain);
                            }
                        }
        
                    }

                    DebugLog("is_take_rest_in_water" + " -> " + is_take_rest_in_water);

                    if (is_take_rest_in_water == true)
                    {
                        if (player_stamina_to_update < player_max_stamina)
                        {
                            if(ow_staminaRestoreValue.Value == true)
                            {
                                player_stamina_to_update += ow_staminaRestorPerTick.Value;

                                DebugLog("ow_staminaRestorPerTick" + " -> " + ow_staminaRestorPerTick.Value);
                                DebugLog("player_stamina_to_update" + " -> " + player_stamina_to_update);
                            }
                            else
                            {
                                player_stamina_to_update += player_max_stamina * 0.0115f;

                                DebugLog("ow_staminaRestorPerTick" + " -> " + player_max_stamina * 0.0115f);
                                DebugLog("player_stamina_to_update" + " -> " + player_stamina_to_update);
                            }
                        }
                    }

                    DebugLog("----------------------Section Update Some Values End-------------------------");
                }
            }

            //Mod setup
            void Awake()
            {
                harmony = new Harmony("MainStreetGaming.BetterDiving");
                harmony.PatchAll();

                configGreeting = Config.Bind("General",                                // The section under which the option is shown
                                     "GreetingText",                                   // The key of the configuration option in the configuration file
                                     "Hello, thanks for using the Better Diving Mod by Main Street Gaming!", // The default value
                                     "");                                              // The description

                configDisplayGreeting = Config.Bind("General",
                                           "DisplayGreeting",
                                           true,
                                           "Whether or not to show the greeting text");

                showYouCanBreatheMsg = Config.Bind("GUI",
                                            "showYouCanBreatheMsg",
                                            false,
                                            "Whether or not to show the You Can Breathe message. Disable if the surfacing message is enabled.");

                showDivingMsg = Config.Bind("GUI",
                                            "showDivingMsg",
                                            true,
                                            "Whether or not to show the a messages when triggering/cancelling diving");

                divingMsg = Config.Bind("GUI",
                                            "Diving message",
                                            "You prepare to dive");

                divingCancelledMsg = Config.Bind("GUI",
                                            "Diving cancelled message",
                                            "You remain on the surface");

                showSurfacingMsg = Config.Bind("GUI",
                                            "showSurfacingMsg",
                                            true,
                                            "Whether or not to show the a message when surfacing");

                surfacingMsg = Config.Bind("GUI",
                                            "Surfacing message",
                                            "You have surfaced");

                allowRestInWater = Config.Bind("GUI",
                                           "allowRestInWater",
                                           true,
                                           "Whether or not to allow stamina regen in water when able to breath and not moving");

                owBIPos = Config.Bind("GUI",
                                           "owBIPos",
                                           false,
                                           "Override breathe indicator position");

                owBIPosX = Config.Bind("GUI",
                                               "owBIPosX",
                                               30f,
                                           "Override breathe indicator position X");

                owBIPosY = Config.Bind("GUI",
                                               "owBIPosY",
                                               150f,
                                           "Override breathe indicator position Y");

                breatheDrain = Config.Bind("Overrides",
                                               "breatheDrain",
                                               4f,
                                           "Breathe indicator reduction per tick");

                c_swimStaminaDrainMinSkill = Config.Bind("Overrides",
                                               "c_swimStaminaDrainMinSkill",
                                               0.7f,
                                           "Min stamina drain while diving");

                c_swimStaminaDrainMaxSkill = Config.Bind("Overrides",
                                               "c_swimStaminaDrainMaxSkill",
                                               0.8f,
                                           "Max stamina drain while diving");

                ow_staminaRestoreValue = Config.Bind("Overrides",
                                               "ow_staminaRestoreValue",
                                               false,
                                           "Overwrite stamina restore value per tick when take rest in water");

                ow_staminaRestorPerTick = Config.Bind("Overrides",
                                               "ow_staminaRestorPerTick",
                                               0.7f,
                                           "Stamina restore value per tick when take rest in water");

                ow_color_brightness_factor = Config.Bind("Water",
                                               "ow_color_brightness_factor",
                                               -0.0092f,
                                           "Reduce color brightness based on swimdepth (RGB)\n\n" +
                                               "char_swim_depth * ow_color_brightness_factor = correctionFactor.\n\nCorrection:\n" +
                                               "correctionFactor *= -1;\n" +
                                               "red -= red * correctionFactor;\n" +
                                               "green -= green * correctionFactor;\n" +
                                               "blue -= blue * correctionFactor;\n\n" +
                                               "ow_color_brightness_factor must be a negative value");

                ow_fogdensity_factor = Config.Bind("Water",
                                               "ow_fogdensity_factor",
                                               0.00092f,
                                           "Set fog density based on swimdepth\n\nCorrection:\n" +
                                               "RenderSettings.fogDensity = RenderSettings.fogDensity + (char_swim_depth * ow_fogdensity_factor)");

                ow_Min_fogdensity = Config.Bind("Water",
                                               "ow_Min_fogdensity",
                                               0.175f,
                                           "Set min fog density");

                ow_Max_fogdensity = Config.Bind("Water",
                                               "ow_Max_fogdensity",
                                               2f,
                                           "Set max fog density");

                doDebug = Config.Bind("Debug",
                                               "doDebug",
                                               false,
                                               "Debug mode on or off");


                if (configDisplayGreeting.Value)
                {
                    Debug.Log("Better Diving Mod: " + configGreeting.Value);
                }
            }

            void Update()
            {
                if(MainClass.loc_breath_bar != null)
                {
                    if (MainClass.owBIPos.Value == true) MainClass.loc_breath_bar_bg.transform.position = new Vector3(MainClass.owBIPosX.Value, MainClass.owBIPosY.Value, Hud.instance.transform.position.y);
                    if (MainClass.owBIPos.Value == true) MainClass.loc_depleted_breath.transform.position = new Vector3(MainClass.owBIPosX.Value, MainClass.owBIPosY.Value, Hud.instance.transform.position.y);
                    if (MainClass.owBIPos.Value == true) MainClass.loc_breath_bar.transform.position = new Vector3(MainClass.owBIPosX.Value, MainClass.owBIPosY.Value, Hud.instance.transform.position.y);
                    if (MainClass.owBIPos.Value == true) MainClass.loc_breathe_overlay.transform.position = new Vector3(MainClass.owBIPosX.Value, MainClass.owBIPosY.Value, Hud.instance.transform.position.y);
                }
            }

            //Env
            public static bool isEnvAllowed()
            {
                if (MainClass.EnvName == "SunkenCrypt") return false;

                return true;
            }

            //Debug
            public static void DebugLog(string data)
            {
                if(doDebug.Value) Debug.Log("Better Diving Mod: " + data);
            }
        }

        //Character awake -> setup values
        [HarmonyPatch(typeof(Character), "Awake")]
        public class Character_Awake
        {
            [HarmonyPrefix]
            public static void Prefix(Character __instance)
            {
                MainClass.DebugLog("---------------------Section Character Awake Start------------------------");
                MainClass.DebugLog("__instance.IsPlayer()" + " -> " + __instance.IsPlayer());

                if (__instance.IsPlayer())
                {
                    __instance.m_swimDepth = 1.6f;
                    MainClass.loc_m_player_dist = __instance.m_swimDepth;
                    MainClass.DebugLog("__instance.IsPlayer()" + " -> " + __instance.IsPlayer());

                    //EpicLoot
                    if (Harmony.HasAnyPatches("randyknapp.mods.epicloot") && MainClass.epicLootLoaded == false)
                    {
                        MainClass.DebugLog("epic_loot_patched_log");
                        MainClass.epicLootLoaded = true;
                    }
                    else if(MainClass.epicLootLoaded == false)
                    {
                        MainClass.DebugLog("epic_loot_not_patched_log");
                        
                    }
                }

                MainClass.DebugLog("----------------------Section Character Awake End-------------------------");
            }
        }

        //Character awake -> setup values
        [HarmonyPatch(typeof(Humanoid), "FixedUpdate")]
        public class Humanoid_FixedUpdate
        {
            [HarmonyPrefix]
            public static void Prefix(Humanoid __instance, ItemDrop.ItemData ___m_legItem)
            {
                if (Hud.instance != null && ___m_legItem != null && __instance.IsPlayer() && MainClass.epicLootLoaded && ___m_legItem.GetTooltip().Contains("_epicloot_me_waterwalk_"))
                {
                    if(MainClass.epicLootWaterRunning == false)
                    {
                        Debug.Log("Better Diving Mod: " + "water_walking_on_log");
                        MainClass.epicLootWaterRunning = true;
                    }
                } else if (MainClass.epicLootWaterRunning == true) {
                        Debug.Log("Better Diving Mod: " + "water_walking_off_log");
                        MainClass.epicLootWaterRunning = false;
                }
            }
        }

        [HarmonyPatch(typeof(Character), "OnDestroy")]
        public class Character_OnDestroy
        {
            [HarmonyPrefix]
            public static void Prefix(Character __instance)
            {
                if (__instance.IsPlayer())
                {
                    MainClass.has_created_breathe_bar = false;
                    MainClass.dive_timer_is_running = false;

                    //EpicLoot
                    MainClass.epicLootLoaded = false;

                    MainClass.DebugLog("Better Diving Mod: OnDestroy Character...");
                }
                
            }
        }

        [HarmonyPatch(typeof(Player), "OnDeath")]
        public class Player_OnDeath
        {
            [HarmonyPrefix]
            public static void Prefix(Player __instance)
            {
                if (Player.m_localPlayer)
                {
                    MainClass.is_diving = false;
                    MainClass.is_swimming = false;

                    //Bug fix for breath bar getting stuck active after player death until logoff
                    MainClass.loc_breath_bar_bg.SetActive(false);
                    MainClass.loc_depleted_breath.SetActive(false);
                    MainClass.loc_breath_bar.SetActive(false);
                    MainClass.loc_breathe_overlay.SetActive(false);

                    MainClass.DebugLog("Better Diving Mod: OnDeath...");
                }

            }
        }

        [HarmonyPatch(typeof(Character), "UpdateMotion")]
        public class Character_UpdateWalking
        {
            [HarmonyPrefix]
            public static void Prefix(Character __instance, ref float ___m_lastGroundTouch, ref float ___m_swimTimer)
            {
                if (__instance.IsPlayer() && MainClass.isEnvAllowed())
                {
                    // Bug fix for swimming on land glitch - originally __instance.m_swimDepth > 2.5f
                    if (__instance.m_swimDepth > 2.5f && (Mathf.Max(0f, __instance.GetLiquidLevel() - __instance.transform.position.y) > 2.5f))
                    {
                        MainClass.is_diving = true;
                        MainClass.has_ping_sent = false;
                        ___m_lastGroundTouch = 0.3f;
                        ___m_swimTimer = 0f;
                    }
                    else
                    {
                        MainClass.is_diving = false;

                        // Fix for oxygen bar bug. Remove the bar if full.
                        if (MainClass.loc_remining_diveTime >= 1f)
                        {
                            MainClass.breathDelayTimer += Time.deltaTime;

                            // Delay removal of the breathe bar for a number of seconds
                            if (MainClass.breathDelayTimer >= MainClass.breathBarRemoveDelay)
                            {
                                MainClass.loc_breath_bar_bg.SetActive(false);
                                MainClass.loc_depleted_breath.SetActive(false);
                                MainClass.loc_breath_bar.SetActive(false);
                                MainClass.loc_breathe_overlay.SetActive(false);
                                MainClass.breathDelayTimer = 0;
                            }
                        }
                        else 
                        {
                            // Amount of delay before removing the breath bar
                            MainClass.breathDelayTimer = 0;
                        }

                        if (MainClass.has_ping_sent == false && __instance.GetStandingOnShip() == null)
                        {
                            if ( !__instance.IsDead() && MainClass.showYouCanBreatheMsg.Value == true)
                            {
                                    __instance.Message(MessageHud.MessageType.Center, "You can breath now.");
                            }

                            MainClass.toggleDive = false;
                            
                            if (!__instance.IsDead() && MainClass.showSurfacingMsg.Value == true)
                            {
                                __instance.Message(MessageHud.MessageType.Center, MainClass.surfacingMsg.Value);
                            }

                            MainClass.has_ping_sent = true;
                        }
                    }
                    MainClass.loc_m_player_dist = __instance.m_swimDepth;
                }
            }
        }

        //character update swiming -> update character vals for diving
        [HarmonyPatch(typeof(Character), "FixedUpdate")]
        public class Character_FixedUpdate
        {
            [HarmonyPrefix]
            public static void Prefix(Character __instance, ref Vector3 ___m_moveDir, ref Vector3 ___m_lookDir, ref float ___m_lastGroundTouch, ref bool ___m_walking, ref bool ___m_wallRunning, ref bool ___m_sliding, ref bool ___m_running, ref float ___m_swimTimer)
            {
                if (__instance.IsPlayer() && MainClass.isEnvAllowed())
                {

                    if (!__instance.InWater())
                    {
                        __instance.m_swimDepth = 1.6f;
                    }

                    bool crouchButtonDown = false;

                    // Toggle diving when "Crouch" button is pressed
                    if (ZInput.GetButtonDown("Crouch") && !crouchButtonDown && __instance.InWater() && !__instance.IsOnGround() && __instance.IsSwiming() && MainClass.epicLootWaterRunning == false)
                    {
                        crouchButtonDown = true;

                        if (MainClass.toggleDive == false)
                        {
                            MainClass.toggleDive = true;

                            if (MainClass.showDivingMsg.Value == true)
                            {
                                __instance.Message(MessageHud.MessageType.Center, MainClass.divingMsg.Value);
                            }
                        }
                        //Cancel diving if button is pressed again and still near the surface
                        else if (MainClass.toggleDive == true && __instance.m_swimDepth <= 2.5f)
                        {
                            MainClass.toggleDive = false;
                            if (MainClass.showDivingMsg.Value == true)
                            {
                                __instance.Message(MessageHud.MessageType.Center, MainClass.divingCancelledMsg.Value);
                            }
                        }
                    }
                    else if (ZInput.GetButtonUp("Crouch"))
                    {
                        crouchButtonDown = false;
                    }

                    //Cancel diving if player is on land
                    if ( __instance.IsOnGround() || !__instance.IsSwiming() || !__instance.InWater())
                    {
                        MainClass.toggleDive = false;
                    }

                    // If player can dive and has pressed the dive toggle key
                    if (MainClass.toggleDive == true && __instance.InWater() && !__instance.IsOnGround() && __instance.IsSwiming() && MainClass.epicLootWaterRunning == false)
                    {

                        //Diving Skill
                        if (Player.m_localPlayer != null && __instance.m_swimDepth > 2.5f)
                        {
                            MainClass.m_diveSkillImproveTimer += Time.deltaTime;

                            if (MainClass.m_diveSkillImproveTimer > 1f)
                            {
                                MainClass.m_diveSkillImproveTimer = 0f;
                                Player.m_localPlayer.RaiseSkill(MainClass.DivingSkillType, 0.25f);
                            }
                        }

                        MainClass.character_pos = __instance.transform.position;
                        MainClass.char_swim_depth = __instance.m_swimDepth;

                        float multiplier = 0f;
                        if (___m_lookDir.y < -0.25)
                        {
                            MainClass.loc_m_diveAaxis = 1;
                        }
                        if (___m_lookDir.y > 0.15)
                        {
                            MainClass.loc_m_diveAaxis = 0;
                        }

                        multiplier = (___m_lookDir.y * ___m_lookDir.y) * 0.25f;

                        if (multiplier > 0.025f) multiplier = 0.025f;

                        if (ZInput.GetButton("Forward"))
                        {

                            if (___m_lookDir.y > -0.25f && ___m_lookDir.y < 0.15f)
                            {
                                multiplier = 0f;
                            }

                            if (MainClass.loc_m_diveAaxis == 1)
                            {
                                __instance.m_swimDepth += multiplier;
                            }
                            if (MainClass.loc_m_diveAaxis == 0)
                            {
                                if (__instance.m_swimDepth > 1.6f)
                                {
                                    __instance.m_swimDepth -= multiplier;
                                }

                                if (__instance.m_swimDepth < 1.6f)
                                {
                                    __instance.m_swimDepth = 1.6f;
                                }
                            }

                            if (__instance.m_swimDepth > 2.5f) __instance.SetMoveDir(___m_lookDir);
                        }

                        //Swim up
                        // Currently doesn't work right as the player is rotated backward and still underwater when he surfaces
                        /*if (ZInput.GetButton("Jump"))
                        {
                            if (__instance.m_swimDepth > 1.6f)
                            {
                                multiplier = 0.025f;
                                __instance.m_swimDepth -= multiplier;
                                if (__instance.m_swimDepth > 1.6f) __instance.SetMoveDir(Vector3.up);
                            }
                        }*/

                        //Swim down
                        // Currently bugs the swim up to where the player faces down while swimming upward
                        /*if (ZInput.GetButton("Crouch"))
                        {
                            //___m_lookDir = Vector3.down;
                            if (__instance.m_swimDepth > 1.0f)
                            {
                                multiplier = 0.025f;
                                __instance.m_swimDepth += multiplier;
                                if (__instance.m_swimDepth > 1.0f) __instance.SetMoveDir(Vector3.down);
                            }
                        }*/

                    } 
                    else
                    {
                        if((__instance.IsOnGround()  || MainClass.is_diving == false) && !MainClass.is_take_rest_in_water)
                        {
                            __instance.m_swimDepth = 1.6f;
                        }
                    }
                }
            }
        }

        //Update Breathing indicator
        [HarmonyPatch(typeof(Player), "Update")]
        public class Player_Update
        {
            [HarmonyPrefix]
            public static void Prefix(Player __instance, ref float ___m_stamina, ref float ___m_maxStamina) 
            {
                if (Player.m_localPlayer)
                {
                    //Remove the breath bar if the player is dead
                    if (Player.m_localPlayer.IsDead())
                    {
                        MainClass.loc_breath_bar_bg.SetActive(false);
                        MainClass.loc_depleted_breath.SetActive(false);
                        MainClass.loc_breath_bar.SetActive(false);
                        MainClass.loc_breathe_overlay.SetActive(false);
                    }

                    MainClass.player_max_stamina = ___m_maxStamina;

                    if (ZInput.GetButtonDown("Forward") || ZInput.GetButtonDown("Backward") || ZInput.GetButtonDown("Left") || ZInput.GetButtonDown("Right") || MainClass.toggleDive == true)
                    {
                        MainClass.is_take_rest_in_water = false;
                    }

                    if (MainClass.is_take_rest_in_water == false)
                    {
                        if (MainClass.is_diving && MainClass.loc_remining_diveTime <= 0f)
                        {
                            if (___m_stamina > MainClass.player_stamina_to_update && ___m_stamina != 0)
                            {
                                ___m_stamina = MainClass.player_stamina_to_update;
                            }
                        }
                    }

                    if (MainClass.is_take_rest_in_water) ___m_stamina = MainClass.player_stamina_to_update;

                    //Bug fix for negative stamina bug
                    if (___m_stamina < 0f) ___m_stamina = 0f;


                    if (MainClass.is_swimming != Player.m_localPlayer.IsSwiming()) MainClass.is_swimming = Player.m_localPlayer.IsSwiming();

                    if (MainClass.m_swimStaminaDrainMaxSkill == 0f)
                    {
                        MainClass.m_swimStaminaDrainMaxSkill = Player.m_localPlayer.m_swimStaminaDrainMaxSkill;
                    }
                    if (MainClass.m_swimStaminaDrainMinSkill == 0f)
                    {
                        MainClass.m_swimStaminaDrainMinSkill = Player.m_localPlayer.m_swimStaminaDrainMinSkill;
                    }
                    if (MainClass.is_diving && MainClass.is_swimming)
                    {
                        if (MainClass.m_swimStaminaDrainMaxSkill != MainClass.c_swimStaminaDrainMaxSkill.Value)
                        {
                            Player.m_localPlayer.m_swimStaminaDrainMaxSkill = MainClass.c_swimStaminaDrainMaxSkill.Value;
                        }
                        if (MainClass.m_swimStaminaDrainMinSkill != MainClass.c_swimStaminaDrainMinSkill.Value)
                        {
                            Player.m_localPlayer.m_swimStaminaDrainMinSkill = MainClass.c_swimStaminaDrainMinSkill.Value;
                        }

                        MainClass.last_activity = "diving";
                        MainClass.came_from_diving = true;

                        bool minimapOpen = false;
                        if(Minimap.instance != null && Minimap.IsOpen())
                        {
                            minimapOpen = true;
                        }

                        bool inventoryOpen = false;
                        if (InventoryGui.instance != null && InventoryGui.IsVisible())
                        {
                            inventoryOpen = true;
                        }

                        bool menuOpen = false;
                        if (Menu.instance != null && Menu.IsVisible())
                        {
                            menuOpen = true;
                        }

                        if (MainClass.loc_breath_bar != null && Hud.instance != null && MainClass.has_created_breathe_bar == true && !MainClass.loc_breath_bar.activeSelf && !minimapOpen && !inventoryOpen && !menuOpen)
                        {
                            MainClass.loc_breath_bar_bg.SetActive(true);
                            MainClass.loc_depleted_breath.SetActive(true);
                            MainClass.loc_breath_bar.SetActive(true);
                            MainClass.loc_breathe_overlay.SetActive(true);
                        } else
                        {
                            if(MainClass.loc_breath_bar != null && Hud.instance != null && MainClass.has_created_breathe_bar == true && MainClass.loc_breath_bar.activeSelf && (minimapOpen || inventoryOpen || menuOpen))
                            {
                                
                                MainClass.loc_breath_bar_bg.SetActive(false);
                                MainClass.loc_depleted_breath.SetActive(false);
                                MainClass.loc_breath_bar.SetActive(false);
                                MainClass.loc_breathe_overlay.SetActive(false);
                            }
                        }
                    }
                    else
                    {
                        if (Player.m_localPlayer.IsSwiming())
                        {
                            if (!ZInput.GetButton("Forward") && !ZInput.GetButton("Backward") && !ZInput.GetButton("Left") && !ZInput.GetButton("Right") && !MainClass.toggleDive)
                            {
                                //MainClass.loc_remining_diveTime = 1f;

                                if (MainClass.allowRestInWater.Value == true)
                                {
                                    //Begin resting if player is not moving or diving
                                    MainClass.is_take_rest_in_water = true;
                                    //__instance.Message(MessageHud.MessageType.Center, "Resting enabled.");
                                }
                            }
                            else if (ZInput.GetButtonDown("Forward") || ZInput.GetButtonDown("Backward") || ZInput.GetButtonDown("Left") || ZInput.GetButtonDown("Right") && MainClass.toggleDive == true)
                            {
                                    MainClass.came_from_diving = false;
                                    MainClass.last_activity = "swimming";
                                    //Stop resting if player is moving or diving
                                    MainClass.is_take_rest_in_water = false;
                                    //__instance.Message(MessageHud.MessageType.Center, "Resting disabled.");
                            }
                            if (Player.m_localPlayer.m_swimStaminaDrainMaxSkill == MainClass.c_swimStaminaDrainMaxSkill.Value)
                            {
                                Player.m_localPlayer.m_swimStaminaDrainMaxSkill = MainClass.m_swimStaminaDrainMaxSkill;
                                Player.m_localPlayer.m_swimStaminaDrainMinSkill = MainClass.m_swimStaminaDrainMinSkill;
                            }
                        }
                        //Hides the healthbar immediately when on land
                        /*else if (MainClass.loc_breath_bar != null && Hud.instance != null && MainClass.has_created_breathe_bar == true && MainClass.loc_breath_bar.activeSelf)
                        {
                            MainClass.loc_breath_bar_bg.SetActive(false);
                            MainClass.loc_depleted_breath.SetActive(false);
                            MainClass.loc_breath_bar.SetActive(false);
                            MainClass.loc_breathe_overlay.SetActive(false);
                        }*/
                    }
                    if (MainClass.loc_breath_bar != null && Hud.instance != null && MainClass.has_created_breathe_bar == true && MainClass.loc_breath_bar.activeSelf)
                    {
                        //Set the bar fill amount based on divetime remaining

                        //Smoothly fill/deplete the breath bar
                        MainClass.loc_breath_bar.GetComponent<Image>().fillAmount = Mathf.Lerp(MainClass.loc_breath_bar.GetComponent<Image>().fillAmount, MainClass.loc_remining_diveTime, Time.deltaTime);

                        float barMultiplier = 1.5f;

                        if (!MainClass.is_diving)
                        {
                            // Smoothly deplete gray bar
                            if (MainClass.loc_depleted_breath.GetComponent<Image>().fillAmount >= MainClass.loc_remining_diveTime) {
                                MainClass.loc_depleted_breath.GetComponent<Image>().fillAmount = Mathf.Lerp(MainClass.loc_depleted_breath.GetComponent<Image>().fillAmount, 0f, Time.deltaTime * barMultiplier);
                            }
                            else if (MainClass.loc_depleted_breath.GetComponent<Image>().fillAmount < MainClass.loc_remining_diveTime)
                            {
                                MainClass.loc_depleted_breath.SetActive(false);
                                MainClass.highestOxygen = 0f;
                            }
                        }
                        else
                        {
                            MainClass.loc_depleted_breath.SetActive(true);
                            if (MainClass.loc_breath_bar.GetComponent<Image>().fillAmount > MainClass.highestOxygen)
                            {
                                MainClass.highestOxygen = MainClass.loc_breath_bar.GetComponent<Image>().fillAmount;
                            }
                            MainClass.loc_depleted_breath.GetComponent<Image>().fillAmount = MainClass.highestOxygen;
                        }

                        //Change the color of the bar depending on how much divetime is left
                        if (MainClass.loc_remining_diveTime <= 0.25f)
                        {
                            //Set to Red
                            MainClass.loc_breath_bar.GetComponent<Image>().color = new Color32(255, 68, 68, 255);
                        } else
                        {
                            //Set to blue
                            MainClass.loc_breath_bar.GetComponent<Image>().color = new Color32(0, 255, 239, 255);
                        }
                    }
                }
            }
        }

        //Hud -> build breathing indicator
        [HarmonyPatch(typeof(Hud), "Update")]
        public class Hud_Update
        {
            [HarmonyPrefix]
            public static void Prefix(Hud __instance)
            {
                if (MainClass.has_created_breathe_bar == false)
                {
                    if(Player.m_localPlayer != null && Hud.instance != null && Hud.instance.m_pieceSelectionWindow != null)
                    {
                        Vector3 position = new Vector3(0, 0, 0);
                        Transform transform = Hud.instance.transform;

                        var panel = GUIManager.Instance.CreateWoodpanel(
                            parent: Hud.instance.transform,
                            anchorMin: new Vector2(0.5f, 0.5f),
                            anchorMax: new Vector2(0.5f, 0.5f),
                            position: new Vector2(60f, -60f),
                            width: 20f,
                            height: 124f,
                            draggable: false);
                        
                        MainClass.loc_breath_bar_bg = panel;

                        MainClass.loc_breath_bar_bg.name = "BreathBarBG";
                        MainClass.loc_breath_bar_bg.GetComponent<Image>().sprite = MainClass.breath_bg_sprite;

                        MainClass.loc_breath_bar_bg = panel;

                        // Create depleted breath bar
                        var depleted_breath_progress = GUIManager.Instance.CreateWoodpanel(
                            parent: Hud.instance.transform,
                            anchorMin: new Vector2(0.5f, 0.5f),
                            anchorMax: new Vector2(0.5f, 0.5f),
                            position: new Vector2(60f, -60f),
                            width: 15f,
                            height: 120f,
                            draggable: false);

                        MainClass.loc_depleted_breath = depleted_breath_progress;

                        MainClass.loc_depleted_breath.name = "depleted_breath_progress";
                        MainClass.loc_depleted_breath.GetComponent<Image>().sprite = MainClass.breath_prog_sprite;
                        MainClass.loc_depleted_breath.GetComponent<Image>().type = Image.Type.Filled;
                        MainClass.loc_depleted_breath.GetComponent<Image>().fillMethod = Image.FillMethod.Vertical;
                        MainClass.loc_depleted_breath.GetComponent<Image>().fillAmount = 1f;

                        MainClass.loc_depleted_breath.GetComponent<Image>().color = new Color32(204, 204, 204, 255);

                        // Create the breath progress bar

                        var breath_progress = GUIManager.Instance.CreateWoodpanel(
                            parent: Hud.instance.transform,
                            anchorMin: new Vector2(0.5f, 0.5f),
                            anchorMax: new Vector2(0.5f, 0.5f),
                            position: new Vector2(60f, -60f),
                            width: 15f,
                            height: 120f,
                            draggable:false);

                        MainClass.loc_breath_bar = breath_progress;

                        MainClass.loc_breath_bar.name = "breathbar_progress";
                        MainClass.loc_breath_bar.GetComponent<Image>().sprite = MainClass.breath_prog_sprite;
                        MainClass.loc_breath_bar.GetComponent<Image>().type = Image.Type.Filled;
                        MainClass.loc_breath_bar.GetComponent<Image>().fillMethod = Image.FillMethod.Vertical;
                        MainClass.loc_breath_bar.GetComponent<Image>().fillAmount = 1f;

                        // Create the breath progress overlay to apply a Valheim style

                        var breath_overlay = GUIManager.Instance.CreateWoodpanel(
                            parent: Hud.instance.transform,
                            anchorMin: new Vector2(0.5f, 0.5f),
                            anchorMax: new Vector2(0.5f, 0.5f),
                            position: new Vector2(60f, -60f),
                            width: 15f,
                            height: 120f,
                            draggable: false);

                        MainClass.loc_breathe_overlay = breath_overlay;
                        MainClass.loc_breathe_overlay.name = "BreathBarOverlay";
                        MainClass.loc_breathe_overlay.GetComponent<Image>().sprite = MainClass.breath_overlay_sprite;

                        if (MainClass.owBIPos.Value == true)
                        {
                          
                            MainClass.loc_breath_bar_bg.transform.position = new Vector3(MainClass.owBIPosX.Value, MainClass.owBIPosY.Value, Hud.instance.transform.position.y);
                            MainClass.loc_depleted_breath.transform.position = new Vector3(MainClass.owBIPosX.Value, MainClass.owBIPosY.Value, Hud.instance.transform.position.y);
                            MainClass.loc_breath_bar.transform.position = new Vector3(MainClass.owBIPosX.Value, MainClass.owBIPosY.Value, Hud.instance.transform.position.y);
                            MainClass.loc_breathe_overlay.transform.position = new Vector3(MainClass.owBIPosX.Value, MainClass.owBIPosY.Value, Hud.instance.transform.position.y);
                        }

                        MainClass.loc_breath_bar_bg.SetActive(false);
                        MainClass.loc_depleted_breath.SetActive(false);
                        MainClass.loc_breath_bar.SetActive(false);
                        MainClass.loc_breathe_overlay.SetActive(false);



                        MainClass.has_created_breathe_bar = true;

                        if (MainClass.dive_timer_is_running == false)
                        {
                            __instance.StartCoroutine(MainClass.StartCountdown());
                            MainClass.dive_timer_is_running = true;
                        }
                    }
                }
            }

            [HarmonyPostfix]
            public static void Postfix(Hud __instance)
            {

            }
        }

        //Watervolume update -> update watervolume values for diving or walking
        [HarmonyPatch(typeof(WaterVolume), "Update")]
        public class WaterVolume_Update
        {
            public static int RandomDigits(int length)
            {
                var random = new System.Random();
                string s = string.Empty;
                for (int i = 0; i < length; i++)
                    s = String.Concat(s, random.Next(10).ToString());
                return int.Parse(s);
            }

            [HarmonyPrefix]
            public static void Prefix(WaterVolume __instance, ref float ___m_waterTime, ref float[] ___m_normalizedDepth, ref Collider ___m_collider)
            {
                if (GameCamera.instance)
                {
                    MainClass.water_level_camera = __instance.GetWaterSurface(new Vector3(GameCamera.instance.transform.position.x, GameCamera.instance.transform.position.y, GameCamera.instance.transform.position.z));
                }
                if (Player.m_localPlayer)
                {
                    MainClass.water_level_player = __instance.GetWaterSurface(new Vector3(Player.m_localPlayer.transform.position.x, Player.m_localPlayer.transform.position.y, Player.m_localPlayer.transform.position.z));
                }
                if (MainClass.loc_cam_pos_y < MainClass.water_level_camera && MainClass.is_swimming)
                {
                    if (__instance.m_waterSurface.GetComponent<MeshRenderer>().transform.rotation.eulerAngles.y != 180f && MainClass.isEnvAllowed())
                    {
                        __instance.m_waterSurface.transform.Rotate(180f, 0f, 0f);
                        __instance.m_waterSurface.material = MainClass.water_mat;
                        __instance.m_waterSurface.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;

                        if (__instance.m_forceDepth >= 0f)
                        {
                            __instance.m_waterSurface.material.SetFloatArray(Shader.PropertyToID("_depth"), new float[]
                            {
                                __instance.m_forceDepth,
                                __instance.m_forceDepth,
                                __instance.m_forceDepth,
                                __instance.m_forceDepth
                            });
                        }
                        else
                        {
                            __instance.m_waterSurface.material.SetFloatArray(Shader.PropertyToID("_depth"), ___m_normalizedDepth);
                        }
                        __instance.m_waterSurface.material.SetFloat(Shader.PropertyToID("_UseGlobalWind"), __instance.m_useGlobalWind ? 1f : 0f);
                    }

                    __instance.m_waterSurface.transform.position = new Vector3(__instance.m_waterSurface.transform.position.x, MainClass.water_level_camera, __instance.m_waterSurface.transform.position.z);
                }
                else
                {
                    if (__instance.m_waterSurface.GetComponent<MeshRenderer>().transform.rotation.eulerAngles.y == 180f && MainClass.isEnvAllowed())
                    {

                        __instance.m_waterSurface.transform.Rotate(-180f, 0f, 0f);
                        __instance.m_waterSurface.material = MainClass.mai_water_mat;

                        if (__instance.m_forceDepth >= 0f)
                        {
                            __instance.m_waterSurface.material.SetFloatArray(Shader.PropertyToID("_depth"), new float[]
                            {
                            __instance.m_forceDepth,
                            __instance.m_forceDepth,
                            __instance.m_forceDepth,
                            __instance.m_forceDepth
                            });
                        }
                        else
                        {
                            __instance.m_waterSurface.material.SetFloatArray(Shader.PropertyToID("_depth"), ___m_normalizedDepth);
                        }
                        __instance.m_waterSurface.transform.position = new Vector3(__instance.m_waterSurface.transform.position.x, 30f, __instance.m_waterSurface.transform.position.z);
                        __instance.m_waterSurface.material.SetFloat(Shader.PropertyToID("_UseGlobalWind"), __instance.m_useGlobalWind ? 1f : 0f);
                    }
                }
            }

            [HarmonyPostfix]
            public static void Postfix(WaterVolume __instance)
            {
            
            }
        }

        //Water volume awake -> detect water volumes
        [HarmonyPatch(typeof(WaterVolume), "Awake")]
        public class WaterVolume_Awake
        {
            [HarmonyPrefix]
            public static void Prefix(WaterVolume __instance)
            {
                MainClass.DebugLog("---------------------Section WaterVolume Awake Prefix Start------------------------");
                MainClass.DebugLog("water_volume_awake" + " -> " + "true");
                MainClass.DebugLog("----------------------Section WaterVolume Awake Prefix End-------------------------");
            }

            [HarmonyPostfix]
            public static void Postfix(WaterVolume __instance)
            {
                MainClass.DebugLog("---------------------Section WaterVolume Awake Postfix Start------------------------");
                MainClass.DebugLog("mai_water_mat" + " -> " + (MainClass.mai_water_mat == null ? "null" : "not null"));

                if (MainClass.mai_water_mat == null)
                {
                    MainClass.mai_water_mat = __instance.m_waterSurface.material;
                    MainClass.DebugLog("Info" + " -> " + "set water mat");
                }

                MainClass.DebugLog("----------------------Section WaterVolume Awake Postfix End-------------------------");
            }
        }

        //GameCamera update camera -> update camera effects for diving or reset them for walking
        [HarmonyPatch(typeof(GameCamera), "UpdateCamera")]
        public class GameCamera_UpdateCamera
        {
            public static Color ChangeColorBrightness(Color color, float correctionFactor)
            {
                float red = (float)color.r;
                float green = (float)color.g;
                float blue = (float)color.b;

                if (correctionFactor < 0)
                {
                    correctionFactor *= -1;
                    red -= red * correctionFactor;
                    if (red < 0f) red = 0f;
                    green -= green * correctionFactor;
                    if (green < 0f) green = 0f;
                    blue -= blue * correctionFactor;
                    if (blue < 0f) blue = 0f;
                }

                return new Color(red, green, blue, color.a);
            }

            [HarmonyPrefix]
            public static void Prefix(GameCamera __instance, Camera ___m_camera, ref bool ___m_waterClipping)
            {
                //Env
                if(MainClass.EnvName != EnvMan.instance.GetCurrentEnvironment().m_name)
                {
                    MainClass.EnvName = EnvMan.instance.GetCurrentEnvironment().m_name;
                }

                //float water_level = 30f;
                if ((MainClass.is_diving || MainClass.is_swimming) && MainClass.is_take_rest_in_water == false && MainClass.isEnvAllowed())
                {
                    //MainClass.DebugLog("m_minWaterDistance: " + __instance.m_minWaterDistance);
                    __instance.m_minWaterDistance = -5000f;
                } else
                {
                    //MainClass.DebugLog("m_minWaterDistance: " + __instance.m_minWaterDistance);
                    __instance.m_minWaterDistance = 0.3f;
                }

                if (__instance.m_maxDistance != 3f && MainClass.loc_m_m_maxDistance == 0) MainClass.loc_m_m_maxDistance = __instance.m_maxDistance;

                MainClass.loc_cam_pos_y = ___m_camera.gameObject.transform.position.y;

                if (___m_camera.gameObject.transform.position.y < MainClass.water_level_camera && (MainClass.is_swimming || MainClass.is_diving) && MainClass.isEnvAllowed())
                {
                    if (__instance.m_minWaterDistance != -5000f) __instance.m_minWaterDistance = -5000f;
                    if (MainClass.is_diving) __instance.m_maxDistance = 3f;

                    EnvSetup curr_env = EnvMan.instance.GetCurrentEnvironment();
                    Color water_color;
                    if (EnvMan.instance.IsNight())
                    {
                        water_color = curr_env.m_fogColorNight;
                    } else
                    {
                        water_color = curr_env.m_fogColorDay;
                        //water_color = ChangeColorBrightness(water_color, -0.01f);
                    }

                    water_color.a = 1f;
                    water_color = ChangeColorBrightness(water_color, MainClass.char_swim_depth * (MainClass.ow_color_brightness_factor.Value));
                    RenderSettings.fogColor = water_color;

                    float fog_dens = RenderSettings.fogDensity + (MainClass.char_swim_depth * MainClass.ow_fogdensity_factor.Value);
                    if (fog_dens < MainClass.ow_Min_fogdensity.Value) fog_dens = MainClass.ow_Min_fogdensity.Value;
                    if (fog_dens > MainClass.ow_Max_fogdensity.Value) fog_dens = MainClass.ow_Max_fogdensity.Value;

                    RenderSettings.fogDensity = fog_dens;

                    MainClass.reder_settings_updated_camera = false;    
                }
                else
                {
                    if (___m_camera.gameObject.transform.position.y > MainClass.water_level_camera)
                    {
                        if (MainClass.reder_settings_updated_camera == false)
                        {
                            if (MainClass.loc_m_m_maxDistance != 0) __instance.m_maxDistance = MainClass.loc_m_m_maxDistance;
                            EnvMan.instance.SetForceEnvironment(EnvMan.instance.GetCurrentEnvironment().m_name);
                            MainClass.reder_settings_updated_camera = true;
                            MainClass.set_force_env = true;
                        }
                        if (MainClass.set_force_env == true)
                        {
                            EnvMan.instance.SetForceEnvironment("");
                            MainClass.set_force_env = false;
                        }
                    }
                    if (!MainClass.is_diving && MainClass.loc_m_m_maxDistance != 0) __instance.m_maxDistance = MainClass.loc_m_m_maxDistance;
                    if (!MainClass.is_diving && MainClass.minwaterdist != 0f) __instance.m_minWaterDistance = MainClass.minwaterdist;
                }
            }
        }
    }
}
