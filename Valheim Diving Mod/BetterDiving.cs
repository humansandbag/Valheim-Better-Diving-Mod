using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Jotunn;
using Jotunn.Configs;
using Jotunn.Managers;
using Jotunn.Utils;
using System;
using System.CodeDom;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace BetterDiving
{
    [BepInPlugin("MainStreetGaming.BetterDiving", "Valheim Better Diving", "1.0.5")]
    [BepInProcess("valheim.exe")]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [BepInIncompatibility("ch.easy.develope.vh.diving.mod")]
    [BepInIncompatibility("blacks7ar.VikingsDoSwim")]
    [BepInIncompatibility("projjm.improvedswimming")]
    //[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Patch)]

    public class BetterDiving : BaseUnityPlugin
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
        public static ConfigEntry<bool> allowFastSwimming;
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
        public static float loc_m_m_maxDistance = 0f;
        public static float loc_m_diveAaxis = 0f;
        public static float loc_cam_pos_y = 0;
        public static float char_swim_depth = 0;

        //camera values
        public static bool render_settings_updated_camera = true;
        public static bool set_force_env = false;
        public static float minwaterdist = 0f;

        //water values
        public static Material mai_water_mat = null;
        public static float water_level_camera = 30f;
        public static float water_level_player = 30f;

        //player stamina
        public static float m_swimStaminaDrainMinSkill = 0f;
        public static float m_swimStaminaDrainMaxSkill = 0f;

        //Oxygen bar
        public static bool dive_timer_is_running = false;
        public static float breathBarRemoveDelay = 2f;
        public static float breathDelayTimer;
        public static float highestOxygen = 1f;
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

        //Water
        public static Shader water_shader;
        public static Texture2D water_texture;
        public static Material water_mat;
        public static Material[] water_volum_list;

        //Env vars
        public static string EnvName = "";

        //DivingSkill
        public static Skills.SkillType DivingSkillType = 0;
        public static Texture2D dive_texture;
        public static Sprite DivingSprite;
        public static float m_diveSkillImproveTimer = 0f;
        public static float m_minDiveSkillImprover = 0f;

        //Swim speed
        public static float baseSwimSpeed = 2f;
        public static float fastSwimSpeedMultiplier = 0.01f;
        public static float swimStaminaDrainRate = 10f;
        public static float fastSwimSpeed;
        public static float fastSwimStamDrain;

        public System.Collections.IEnumerator Start()
        {
            DebugLog("Better Diving Mod: Ienumerator start!");

            AssetBundle watermatAssetBundle = Jotunn.Utils.AssetUtils.LoadAssetBundleFromResources("watermat.assets");

            if (watermatAssetBundle == null)
            {
                Debug.LogError("load_watermat_assets_failed");
                return null;
            }

            AssetBundle betterDivingAssetBundle = Jotunn.Utils.AssetUtils.LoadAssetBundleFromResources("betterdiving.assets");

            if (betterDivingAssetBundle == null)
            {
                Debug.LogError("load_betterdiving_assets_failed");
                return null;
            }

            // Load assets
            water_mat = watermatAssetBundle.LoadAsset<Material>("WaterMat");
            dive_texture = watermatAssetBundle.LoadAsset<Texture2D>("vhdm_dive_icon_1");
            breath_prog_tex = betterDivingAssetBundle.LoadAsset<Texture2D>("BreathBar.png");
            breath_prog_sprite = Sprite.Create(breath_prog_tex, new Rect(0f, 0f, breath_prog_tex.width, breath_prog_tex.height), Vector2.zero);
            breath_bg_tex = betterDivingAssetBundle.LoadAsset<Texture2D>("BreathBar_BG.png");
            breath_bg_sprite = Sprite.Create(breath_bg_tex, new Rect(0f, 0f, breath_bg_tex.width, breath_bg_tex.height), Vector2.zero);
            breath_overlay_tex = betterDivingAssetBundle.LoadAsset<Texture2D>("BreathBarOverlay.png");
            breath_overlay_sprite = Sprite.Create(breath_overlay_tex, new Rect(0f, 0f, breath_overlay_tex.width, breath_overlay_tex.height), Vector2.zero);

            //Register DivingSkill
            BetterDiving.DivingSprite = Sprite.Create(dive_texture, new Rect(0f, 0f, dive_texture.width, dive_texture.height), Vector2.zero);
            BetterDiving.DivingSkillType = SkillManager.Instance.AddSkill(new SkillConfig
            {
                Identifier = "MainStreetGaming.BetterDiving.divingduration.1",
                Name = "Diving",
                Description = "Dive duration.",
                IncreaseStep = 0.25f,
                Icon = BetterDiving.DivingSprite
            });

            watermatAssetBundle.Unload(false);
            return null;
        }

        //Mod setup
        void Awake()
        {
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);

            CreateConfigValues();

            if (configDisplayGreeting.Value)
            {
                Debug.Log("Better Diving Mod: " + configGreeting.Value);
            }
        }

        void Update()
        {
            if (BetterDiving.loc_breath_bar != null)
            {
                if (BetterDiving.owBIPos.Value == true) BetterDiving.loc_breath_bar_bg.transform.position = new Vector3(BetterDiving.owBIPosX.Value, BetterDiving.owBIPosY.Value, Hud.instance.transform.position.y);
                if (BetterDiving.owBIPos.Value == true) BetterDiving.loc_depleted_breath.transform.position = new Vector3(BetterDiving.owBIPosX.Value, BetterDiving.owBIPosY.Value, Hud.instance.transform.position.y);
                if (BetterDiving.owBIPos.Value == true) BetterDiving.loc_breath_bar.transform.position = new Vector3(BetterDiving.owBIPosX.Value, BetterDiving.owBIPosY.Value, Hud.instance.transform.position.y);
                if (BetterDiving.owBIPos.Value == true) BetterDiving.loc_breathe_overlay.transform.position = new Vector3(BetterDiving.owBIPosX.Value, BetterDiving.owBIPosY.Value, Hud.instance.transform.position.y);
            }
        }

        private void CreateConfigValues()
        {
            ConfigurationManagerAttributes isAdminOnly = new ConfigurationManagerAttributes { IsAdminOnly = true };

            // Local config values
            doDebug = Config.Bind("Debug", "doDebug", false, "Debug mode on or off");
            configGreeting = Config.Bind("Local config", "GreetingText", "Hello, thanks for using the Better Diving Mod by Main Street Gaming!", "");
            configDisplayGreeting = Config.Bind("Local config", "DisplayGreeting", true, "Whether or not to show the greeting text");
            showYouCanBreatheMsg = Config.Bind("Local config", "showYouCanBreatheMsg", false, "Whether or not to show the You Can Breathe message. Disable if the surfacing message is enabled.");
            showDivingMsg = Config.Bind("Local config", "showDivingMsg", true, "Whether or not to show messages when triggering/cancelling diving");
            divingMsg = Config.Bind("Local config", "Diving message", "You prepare to dive");
            divingCancelledMsg = Config.Bind("Local config", "Diving cancelled message", "You remain on the surface");
            showSurfacingMsg = Config.Bind("Local config", "showSurfacingMsg", true, "Whether or not to show a message when surfacing");
            surfacingMsg = Config.Bind("Local config", "Surfacing message", "You have surfaced");
            owBIPos = Config.Bind("Local config", "owBIPos", false, "Override breathe indicator position");
            owBIPosX = Config.Bind("Local config", "owBIPosX", 30f, "Override breathe indicator position X");
            owBIPosY = Config.Bind("Local config", "owBIPosY", 150f, "Override breathe indicator position Y");


            // Server synced config values
            allowRestInWater = Config.Bind("Server config", "allowRestInWater", true, new ConfigDescription("Whether or not to allow stamina regen in water when able to breath and not moving", null, isAdminOnly));
            allowFastSwimming = allowRestInWater = Config.Bind("Server config", "allowFastSwimming", true, new ConfigDescription("Allow fast swimming when holding the Run button", null, isAdminOnly));
            breatheDrain = Config.Bind("Server config", "breatheDrain", 4f, new ConfigDescription("Breathe indicator reduction per tick", null, isAdminOnly));
            c_swimStaminaDrainMinSkill = Config.Bind("Server config", "c_swimStaminaDrainMinSkill", 0.7f, new ConfigDescription("Min stamina drain while diving", null, isAdminOnly));
            c_swimStaminaDrainMaxSkill = Config.Bind("Server config", "c_swimStaminaDrainMaxSkill", 0.8f, new ConfigDescription("Max stamina drain while diving", null, isAdminOnly));
            ow_staminaRestoreValue = Config.Bind("Server config", "ow_staminaRestoreValue",false, new ConfigDescription("Overwrite stamina restore value per tick when take rest in water", null, isAdminOnly));
            ow_staminaRestorPerTick = Config.Bind("Server config", "ow_staminaRestorPerTick", 0.7f, new ConfigDescription("Stamina restore value per tick when take rest in water", null, isAdminOnly));

            // Water - Server synced config values
            ow_color_brightness_factor = Config.Bind("Server config - Water", "ow_color_brightness_factor", -0.0092f, new ConfigDescription(
                                            "Reduce color brightness based on swimdepth (RGB)\n\n" +
                                            "char_swim_depth * ow_color_brightness_factor = correctionFactor.\n\nCorrection:\n" +
                                            "correctionFactor *= -1;\n" +
                                            "red -= red * correctionFactor;\n" +
                                            "green -= green * correctionFactor;\n" +
                                            "blue -= blue * correctionFactor;\n\n" +
                                            "ow_color_brightness_factor must be a negative value", null, isAdminOnly));
            ow_fogdensity_factor = Config.Bind("Server config - Water", "ow_fogdensity_factor", 0.00092f, new ConfigDescription(
                                            "Set fog density based on swimdepth\n\nCorrection:\n" +
                                            "RenderSettings.fogDensity = RenderSettings.fogDensity + (char_swim_depth * ow_fogdensity_factor)", null, isAdminOnly));
            ow_Min_fogdensity = Config.Bind("Server config - Water", "ow_Min_fogdensity", 0.175f, new ConfigDescription("Set min fog density", null, isAdminOnly));
            ow_Max_fogdensity = Config.Bind("Server config - Water", "ow_Max_fogdensity", 2f, new ConfigDescription("Set max fog density", null, isAdminOnly));
        }

        //Env
        public static bool isEnvAllowed()
        {
            if (BetterDiving.EnvName == "SunkenCrypt") return false;

            return true;
        }

        //Debug
        public static void DebugLog(string data)
        {
            if (doDebug.Value) Debug.Log("Better Diving Mod: " + data);
        }
    }

    [HarmonyPatch(typeof(Player))]
    public static class Player_Patch
    {
        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        public static void Postfix(Player __instance)
        {
            __instance.gameObject.AddComponent<BetterDivingExtension>();
        }
    }

    public class BetterDivingExtension : MonoBehaviour
    {
        public bool toggleDive = false;
        public bool is_diving = false;
        public float remainingDiveTime = 1f;
        public string lastActivity = "";
        public string lastDiveCancel = "";
        public bool cameFromDiving = false;
        public bool isUnderwater = false;
        public bool isTakeRestInWater = false;
        public float maxStamina = 100f;
        public float staminaToUpdate = 100f;
    }

    //Character awake -> setup values
    [HarmonyPatch(typeof(Character), "Awake")]
    public class Character_Awake
    {
        [HarmonyPrefix]
        public static void Prefix(Character __instance)
        {
            BetterDiving.DebugLog("---------------------Section Character Awake Start------------------------");
            BetterDiving.DebugLog("__instance.IsPlayer()" + " -> " + __instance.IsPlayer());

            if (__instance.IsPlayer() && Player.m_localPlayer && object.ReferenceEquals(__instance, Player.m_localPlayer))
            {
                __instance.m_swimDepth = 1.6f;
                BetterDiving.DebugLog("__instance.IsPlayer()" + " -> " + __instance.IsPlayer());
            }

            BetterDiving.DebugLog("----------------------Section Character Awake End-------------------------");
        }
    }

    [HarmonyPatch(typeof(Character), "OnDestroy")]
    public class Character_OnDestroy
    {
        [HarmonyPrefix]
        public static void Prefix(Character __instance)
        {

            // check if the character instance is the local player
            if (!object.ReferenceEquals(__instance, Player.m_localPlayer))
                return;

            if (!Player.m_localPlayer || !__instance.IsPlayer())
                return;

            BetterDiving.has_created_breathe_bar = false;
            BetterDiving.dive_timer_is_running = false;
            BetterDiving.DebugLog("Better Diving Mod: OnDestroy Character...");
        }
    }

    [HarmonyPatch(typeof(Player), "OnDeath")]
    public class Player_OnDeath
    {
        [HarmonyPrefix]
        public static void Prefix(Player __instance)
        {

            if (__instance != Player.m_localPlayer)
                return;

            __instance.GetComponent<BetterDivingExtension>().is_diving = false;

            //Bug fix for breath bar getting stuck active after player death until logoff
            BetterDiving.loc_breath_bar_bg.SetActive(false);
            BetterDiving.loc_depleted_breath.SetActive(false);
            BetterDiving.loc_breath_bar.SetActive(false);
            BetterDiving.loc_breathe_overlay.SetActive(false);

            BetterDiving.DebugLog("Better Diving Mod: OnDeath...");
        }
    }

    [HarmonyPatch(typeof(Character), "UpdateMotion")]
    public class Character_UpdateWalking
    {
        [HarmonyPrefix]
        public static void Prefix(Character __instance, ref float ___m_lastGroundTouch, ref float ___m_swimTimer)
        {

            // check if the character instance is the local player
            if (!object.ReferenceEquals(__instance, Player.m_localPlayer))
                return;

            if (!__instance.IsPlayer() || !BetterDiving.isEnvAllowed() || !Player.m_localPlayer)
                return;

            // Bug fix for swimming on land glitch - originally __instance.m_swimDepth > 2.5f
            if (__instance.m_swimDepth > 2.5f && (Mathf.Max(0f, __instance.GetLiquidLevel() - __instance.transform.position.y) > 2.5f))
            {
                __instance.GetComponent<BetterDivingExtension>().is_diving = true;
                __instance.GetComponent<BetterDivingExtension>().isUnderwater = true;
                ___m_lastGroundTouch = 0.3f;
                ___m_swimTimer = 0f;
            }
            else
            {
                __instance.GetComponent<BetterDivingExtension>().is_diving = false;

                // Fix for oxygen bar bug. Remove the bar if full.
                if (__instance.GetComponent<BetterDivingExtension>().remainingDiveTime >= 1f)
                {
                    BetterDiving.breathDelayTimer += Time.deltaTime;

                    // Delay removal of the breathe bar for a number of seconds
                    if (BetterDiving.breathDelayTimer >= BetterDiving.breathBarRemoveDelay)
                    {
                        BetterDiving.loc_breath_bar_bg.SetActive(false);
                        BetterDiving.loc_depleted_breath.SetActive(false);
                        BetterDiving.loc_breath_bar.SetActive(false);
                        BetterDiving.loc_breathe_overlay.SetActive(false);
                        BetterDiving.breathDelayTimer = 0;
                    }
                }
                else
                {
                    // Amount of delay before removing the breath bar
                    BetterDiving.breathDelayTimer = 0;
                }

                if (__instance.GetComponent<BetterDivingExtension>().isUnderwater == true && __instance.GetStandingOnShip() == null)
                {
                    if (!__instance.IsDead() && BetterDiving.showYouCanBreatheMsg.Value == true)
                    {
                        __instance.Message(MessageHud.MessageType.Center, "You can breath now.");
                    }

                    __instance.GetComponent<BetterDivingExtension>().toggleDive = false;
                    __instance.GetComponent<BetterDivingExtension>().lastDiveCancel = "PlayerSurfaced";

                    if (!__instance.IsDead() && BetterDiving.showSurfacingMsg.Value == true)
                    {
                        __instance.Message(MessageHud.MessageType.Center, BetterDiving.surfacingMsg.Value);
                    }

                    __instance.GetComponent<BetterDivingExtension>().isUnderwater = false;
                }
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

            // If the character is not the local player
            if (!object.ReferenceEquals(__instance, Player.m_localPlayer))
                return;

            if (!__instance.IsPlayer() || !BetterDiving.isEnvAllowed() || !Player.m_localPlayer)
                return;

            if (!__instance.InWater())
            {
                __instance.m_swimDepth = 1.6f;
            }

            bool crouchButtonDown = false;

            // Toggle diving when "Crouch" button is pressed
            if ((ZInput.GetButtonDown("Crouch") || ZInput.GetButtonDown("JoyCrouch")) && !crouchButtonDown && __instance.InWater() && !__instance.IsOnGround() && __instance.IsSwiming())
            {
                crouchButtonDown = true;

                if (__instance.GetComponent<BetterDivingExtension>().toggleDive == false)
                {
                    __instance.GetComponent<BetterDivingExtension>().toggleDive = true;
                    __instance.GetComponent<BetterDivingExtension>().lastDiveCancel = "None";

                    if (BetterDiving.showDivingMsg.Value == true)
                    {
                        __instance.Message(MessageHud.MessageType.Center, BetterDiving.divingMsg.Value);
                    }
                }
                //Cancel diving if button is pressed again and still near the surface
                else if (__instance.GetComponent<BetterDivingExtension>().toggleDive == true && __instance.m_swimDepth <= 2.5f)
                {
                    __instance.GetComponent<BetterDivingExtension>().toggleDive = false;
                    __instance.GetComponent<BetterDivingExtension>().lastDiveCancel = "PlayerCancelled";

                    if (BetterDiving.showDivingMsg.Value == true)
                    {
                        __instance.Message(MessageHud.MessageType.Center, BetterDiving.divingCancelledMsg.Value);
                    }
                }
            }
            else if ((ZInput.GetButtonUp("Crouch") || ZInput.GetButtonUp("JoyCrouch")))
            {
                crouchButtonDown = false;
            }

            //Cancel diving if player is on land
            if (__instance.IsOnGround() || !__instance.IsSwiming() || !__instance.InWater())
            {
                __instance.GetComponent<BetterDivingExtension>().toggleDive = false;
                __instance.GetComponent<BetterDivingExtension>().lastDiveCancel = "PlayerOnLand";
            }

            // If player can dive and has pressed the dive toggle key
            if (__instance.GetComponent<BetterDivingExtension>().toggleDive == true && __instance.InWater() && !__instance.IsOnGround() && __instance.IsSwiming())
            {

                //Diving Skill
                if (__instance.IsPlayer() && __instance.m_swimDepth > 2.5f)
                {
                    BetterDiving.m_diveSkillImproveTimer += Time.deltaTime;

                    if (BetterDiving.m_diveSkillImproveTimer > 1f)
                    {
                        BetterDiving.m_diveSkillImproveTimer = 0f;
                        __instance.RaiseSkill(BetterDiving.DivingSkillType, 0.25f);
                    }
                }

                BetterDiving.char_swim_depth = __instance.m_swimDepth;

                float multiplier = 0f;
                if (___m_lookDir.y < -0.25)
                {
                    BetterDiving.loc_m_diveAaxis = 1;
                }
                if (___m_lookDir.y > 0.15)
                {
                    BetterDiving.loc_m_diveAaxis = 0;
                }

                multiplier = (___m_lookDir.y * ___m_lookDir.y) * 0.25f;

                if (multiplier > 0.025f) multiplier = 0.025f;

                if ((ZInput.GetButton("Forward") || ZInput.GetButton("JoyLStickUp")))
                {

                    if (___m_lookDir.y > -0.25f && ___m_lookDir.y < 0.15f)
                    {
                        multiplier = 0f;
                    }

                    if (BetterDiving.loc_m_diveAaxis == 1)
                    {
                        __instance.m_swimDepth += multiplier;
                    }
                    if (BetterDiving.loc_m_diveAaxis == 0)
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
            }
            else
            {
                if ((__instance.IsOnGround() || __instance.GetComponent<BetterDivingExtension>().is_diving == false) && !__instance.GetComponent<BetterDivingExtension>().isTakeRestInWater)
                {
                    __instance.m_swimDepth = 1.6f;
                }
            }
        }
    }

    //Swim speed and stamina drain
    [HarmonyPatch(typeof(Character), "UpdateSwiming")]
    class Character_UpdateSwiming_Patch
    {
        static void Prefix(Character __instance)
        {

            // check if the character instance is the local player
            if (!object.ReferenceEquals(__instance, Player.m_localPlayer))
                return;

            if (!__instance.IsPlayer() || !BetterDiving.allowFastSwimming.Value || !Player.m_localPlayer)
                return;

            //Swim speed
            float swimSkill = __instance.GetSkillFactor(Skills.SkillType.Swim);
            float swimSkillFactor = swimSkill * 100f;
            float sprintSwimEnhancement = (BetterDiving.fastSwimSpeedMultiplier * swimSkillFactor) + 1;
            BetterDiving.fastSwimSpeed = BetterDiving.baseSwimSpeed + sprintSwimEnhancement;

            //Stamina
            float swimStaminaFactor = swimSkill / 100f;
            float maxStaminaDrainReduction = 0.5f;
            float staminaDrainReduction = maxStaminaDrainReduction * swimStaminaFactor;
            float skillDrainMultiplier = 0.1f;
            float skillDrainPenalty = skillDrainMultiplier * swimSkill;
            float totalDrainReduction = staminaDrainReduction + skillDrainPenalty;
            float baseDrainFactor = 0.5f;
            float staminaDrainFactor = baseDrainFactor - totalDrainReduction;
            float staminaDrainRate = (BetterDiving.swimStaminaDrainRate * staminaDrainFactor * 2f) + 5f;
            BetterDiving.fastSwimStamDrain = staminaDrainRate;

            if ((ZInput.GetButton("Run") || ZInput.GetButton("JoyRun")))
            {
                if (!__instance.GetComponent<BetterDivingExtension>().isUnderwater)
                {
                    float staminaCost = Time.deltaTime * staminaDrainRate;
                    __instance.UseStamina(staminaCost);
                }
                __instance.m_swimSpeed = BetterDiving.fastSwimSpeed;
            }
            else
            {
                __instance.m_swimSpeed = BetterDiving.baseSwimSpeed;
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
            if (!Player.m_localPlayer || __instance != Player.m_localPlayer)
                return;

            // Only run the coroutine for the local player
            if (__instance == Player.m_localPlayer)
            {
                if (BetterDiving.dive_timer_is_running == false)
                {
                    __instance.StartCoroutine(Player_Update.StartCountdown());
                    BetterDiving.dive_timer_is_running = true;
                }
            }

            //Remove the breath bar if the player is dead
            if (__instance.IsDead())
            {
                BetterDiving.loc_breath_bar_bg.SetActive(false);
                BetterDiving.loc_depleted_breath.SetActive(false);
                BetterDiving.loc_breath_bar.SetActive(false);
                BetterDiving.loc_breathe_overlay.SetActive(false);
            }

            __instance.GetComponent<BetterDivingExtension>().maxStamina = ___m_maxStamina;

            if (__instance.GetVelocity().magnitude >= 1.0f || __instance.GetComponent<BetterDivingExtension>().toggleDive == true || !__instance.InWater() || !__instance.IsSwiming())
            {
                __instance.GetComponent<BetterDivingExtension>().isTakeRestInWater = false;
            }

            if (__instance.GetComponent<BetterDivingExtension>().isTakeRestInWater == false)
            {
                if (__instance.GetComponent<BetterDivingExtension>().is_diving && __instance.GetComponent<BetterDivingExtension>().remainingDiveTime <= 0f)
                {
                    if (___m_stamina > __instance.GetComponent<BetterDivingExtension>().staminaToUpdate && ___m_stamina != 0)
                    {
                        ___m_stamina = __instance.GetComponent<BetterDivingExtension>().staminaToUpdate;
                    }
                }
            }

            if (__instance.GetComponent<BetterDivingExtension>().isTakeRestInWater) ___m_stamina = __instance.GetComponent<BetterDivingExtension>().staminaToUpdate;

            //Bug fix for negative stamina bug
            if (___m_stamina < 0f) ___m_stamina = 0f;

            if (BetterDiving.m_swimStaminaDrainMaxSkill == 0f)
            {
                BetterDiving.m_swimStaminaDrainMaxSkill = __instance.m_swimStaminaDrainMaxSkill;
            }
            if (BetterDiving.m_swimStaminaDrainMinSkill == 0f)
            {
                BetterDiving.m_swimStaminaDrainMinSkill = __instance.m_swimStaminaDrainMinSkill;
            }

            if (__instance.GetComponent<BetterDivingExtension>().is_diving && __instance.IsSwiming())
            {
                if (BetterDiving.m_swimStaminaDrainMaxSkill != BetterDiving.c_swimStaminaDrainMaxSkill.Value)
                {
                    __instance.m_swimStaminaDrainMaxSkill = BetterDiving.c_swimStaminaDrainMaxSkill.Value;
                }
                if (BetterDiving.m_swimStaminaDrainMinSkill != BetterDiving.c_swimStaminaDrainMinSkill.Value)
                {
                    __instance.m_swimStaminaDrainMinSkill = BetterDiving.c_swimStaminaDrainMinSkill.Value;
                }

                __instance.GetComponent<BetterDivingExtension>().lastActivity = "diving";
                __instance.GetComponent<BetterDivingExtension>().cameFromDiving = true;

                bool minimapOpen = false;
                if (Minimap.instance != null && Minimap.IsOpen())
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

                if (BetterDiving.loc_breath_bar != null && Hud.instance != null && BetterDiving.has_created_breathe_bar == true && !BetterDiving.loc_breath_bar.activeSelf && !minimapOpen && !inventoryOpen && !menuOpen)
                {
                    BetterDiving.loc_breath_bar_bg.SetActive(true);
                    BetterDiving.loc_depleted_breath.SetActive(true);
                    BetterDiving.loc_breath_bar.SetActive(true);
                    BetterDiving.loc_breathe_overlay.SetActive(true);
                }
                else
                {
                    if (BetterDiving.loc_breath_bar != null && Hud.instance != null && BetterDiving.has_created_breathe_bar == true && BetterDiving.loc_breath_bar.activeSelf && (minimapOpen || inventoryOpen || menuOpen))
                    {

                        BetterDiving.loc_breath_bar_bg.SetActive(false);
                        BetterDiving.loc_depleted_breath.SetActive(false);
                        BetterDiving.loc_breath_bar.SetActive(false);
                        BetterDiving.loc_breathe_overlay.SetActive(false);
                    }
                }
            }
            else
            {
                if (__instance.IsSwiming())
                {
                    if (__instance.GetVelocity().magnitude < 1.0f && !__instance.GetComponent<BetterDivingExtension>().toggleDive)
                    {
                        if (BetterDiving.allowRestInWater.Value == true)
                        {
                            //Begin resting if player is not moving or diving
                            __instance.GetComponent<BetterDivingExtension>().isTakeRestInWater = true;
                        }
                    }
                    else if (__instance.GetVelocity().magnitude >= 1.0f || __instance.GetComponent<BetterDivingExtension>().toggleDive == true)
                    {
                        __instance.GetComponent<BetterDivingExtension>().cameFromDiving = false;
                        __instance.GetComponent<BetterDivingExtension>().lastActivity = "swimming";

                        //Stop resting if player is moving or diving
                        __instance.GetComponent<BetterDivingExtension>().isTakeRestInWater = false;
                    }
                    if (__instance.m_swimStaminaDrainMaxSkill == BetterDiving.c_swimStaminaDrainMaxSkill.Value)
                    {
                        __instance.m_swimStaminaDrainMaxSkill = BetterDiving.m_swimStaminaDrainMaxSkill;
                        __instance.m_swimStaminaDrainMinSkill = BetterDiving.m_swimStaminaDrainMinSkill;
                    }
                }
            }
            if (BetterDiving.loc_breath_bar != null && Hud.instance != null && BetterDiving.has_created_breathe_bar == true && BetterDiving.loc_breath_bar.activeSelf)
            {
                //Set the bar fill amount based on divetime remaining

                //Smoothly fill/deplete the breath bar
                BetterDiving.loc_breath_bar.GetComponent<Image>().fillAmount = Mathf.Lerp(BetterDiving.loc_breath_bar.GetComponent<Image>().fillAmount, __instance.GetComponent<BetterDivingExtension>().remainingDiveTime, Time.deltaTime);

                float barMultiplier = 1.5f;

                if (!__instance.GetComponent<BetterDivingExtension>().is_diving)
                {
                    // Smoothly deplete gray bar
                    if (BetterDiving.loc_depleted_breath.GetComponent<Image>().fillAmount >= __instance.GetComponent<BetterDivingExtension>().remainingDiveTime)
                    {
                        BetterDiving.loc_depleted_breath.GetComponent<Image>().fillAmount = Mathf.Lerp(BetterDiving.loc_depleted_breath.GetComponent<Image>().fillAmount, 0f, Time.deltaTime * barMultiplier);
                    }
                    else if (BetterDiving.loc_depleted_breath.GetComponent<Image>().fillAmount < __instance.GetComponent<BetterDivingExtension>().remainingDiveTime)
                    {
                        BetterDiving.loc_depleted_breath.SetActive(false);
                        BetterDiving.highestOxygen = 0f;
                    }
                }
                else
                {
                    BetterDiving.loc_depleted_breath.SetActive(true);
                    if (BetterDiving.loc_breath_bar.GetComponent<Image>().fillAmount > BetterDiving.highestOxygen)
                    {
                        BetterDiving.highestOxygen = BetterDiving.loc_breath_bar.GetComponent<Image>().fillAmount;
                    }
                    BetterDiving.loc_depleted_breath.GetComponent<Image>().fillAmount = BetterDiving.highestOxygen;
                }

                //Change the color of the bar depending on how much divetime is left
                if (__instance.GetComponent<BetterDivingExtension>().remainingDiveTime <= 0.25f)
                {
                    //Set to Red
                    BetterDiving.loc_breath_bar.GetComponent<Image>().color = new Color32(255, 68, 68, 255);
                }
                else
                {
                    //Set to blue
                    BetterDiving.loc_breath_bar.GetComponent<Image>().color = new Color32(0, 255, 239, 255);
                }
            }
        }

        public static IEnumerator StartCountdown()
        {
            while (true)
            {
                yield return new WaitForSeconds(1.0f);

                //Debug
                BetterDiving.DebugLog("---------------------Section Debug Start------------------------");
                BetterDiving.DebugLog("is_swimming" + " -> " + Player.m_localPlayer.IsSwiming());
                BetterDiving.DebugLog("m_minDiveSkillImprover" + " -> " + BetterDiving.m_minDiveSkillImprover);
                BetterDiving.DebugLog("Player_remainingDiveTime" + " -> " + Player.m_localPlayer.GetComponent<BetterDivingExtension>().remainingDiveTime);
                BetterDiving.DebugLog("loc_m_m_maxDistance" + " -> " + BetterDiving.loc_m_m_maxDistance);
                BetterDiving.DebugLog("loc_m_diveAaxis" + " -> " + BetterDiving.loc_m_diveAaxis);
                BetterDiving.DebugLog("Player_swimDepth" + " -> " + Player.m_localPlayer.m_swimDepth);
                BetterDiving.DebugLog("char_swim_depth" + " -> " + BetterDiving.char_swim_depth);
                BetterDiving.DebugLog("loc_cam_pos_y" + " -> " + BetterDiving.loc_cam_pos_y);
                BetterDiving.DebugLog("render_settings_updated_camera" + " -> " + BetterDiving.render_settings_updated_camera);
                BetterDiving.DebugLog("set_force_env" + " -> " + BetterDiving.set_force_env);
                BetterDiving.DebugLog("minwaterdist" + " -> " + BetterDiving.minwaterdist);
                BetterDiving.DebugLog("water_level_camera" + " -> " + BetterDiving.water_level_camera);
                BetterDiving.DebugLog("water_level_player" + " -> " + BetterDiving.water_level_player);
                BetterDiving.DebugLog("m_swimStaminaDrainMinSkill" + " -> " + BetterDiving.m_swimStaminaDrainMinSkill);
                BetterDiving.DebugLog("m_swimStaminaDrainMaxSkill" + " -> " + BetterDiving.m_swimStaminaDrainMaxSkill);
                BetterDiving.DebugLog("dive_timer_is_running" + " -> " + BetterDiving.dive_timer_is_running);
                BetterDiving.DebugLog("Player_lastActivity" + " -> " + Player.m_localPlayer.GetComponent<BetterDivingExtension>().lastActivity);
                BetterDiving.DebugLog("Player_cameFromDiving" + " -> " + Player.m_localPlayer.GetComponent<BetterDivingExtension>().cameFromDiving);
                BetterDiving.DebugLog("Player_isUnderwater" + " -> " + Player.m_localPlayer.GetComponent<BetterDivingExtension>().isUnderwater);
                BetterDiving.DebugLog("has_created_breathe_bar" + " -> " + BetterDiving.has_created_breathe_bar);
                BetterDiving.DebugLog("Player_isTakeRestInWater" + " -> " + Player.m_localPlayer.GetComponent<BetterDivingExtension>().isTakeRestInWater);
                BetterDiving.DebugLog("player_staminaToUpdate" + " -> " + Player.m_localPlayer.GetComponent<BetterDivingExtension>().staminaToUpdate);
                BetterDiving.DebugLog("Player_maxStamina" + " -> " + Player.m_localPlayer.GetComponent<BetterDivingExtension>().maxStamina);
                BetterDiving.DebugLog("EnvName" + " -> " + BetterDiving.EnvName);
                BetterDiving.DebugLog("m_diveSkillImproveTimer" + " -> " + BetterDiving.m_diveSkillImproveTimer);
                BetterDiving.DebugLog("m_minDiveSkillImprover" + " -> " + BetterDiving.m_minDiveSkillImprover);
                BetterDiving.DebugLog("Player_toggleDive" + " -> " + Player.m_localPlayer.GetComponent<BetterDivingExtension>().toggleDive);
                BetterDiving.DebugLog("Player_lastDiveCancel" + " -> " + Player.m_localPlayer.GetComponent<BetterDivingExtension>().lastDiveCancel);
                BetterDiving.DebugLog("fastSwimSpeed" + " -> " + BetterDiving.fastSwimSpeed);
                BetterDiving.DebugLog("fastSwimStamDrain" + " -> " + BetterDiving.fastSwimStamDrain);
                BetterDiving.DebugLog("----------------------Section Debug End-------------------------");
                BetterDiving.DebugLog("---------------------Section Update Some Values Start------------------------");

                if (Player.m_localPlayer)
                {
                    Player.m_localPlayer.GetComponent<BetterDivingExtension>().maxStamina = Player.m_localPlayer.GetMaxStamina();
                    Player.m_localPlayer.GetComponent<BetterDivingExtension>().staminaToUpdate = Player.m_localPlayer.GetStamina();

                    if (Player.m_localPlayer.GetComponent<BetterDivingExtension>().is_diving && Player.m_localPlayer.IsSwiming())
                    {
                        BetterDiving.m_minDiveSkillImprover++;

                        if (Player.m_localPlayer.GetComponent<BetterDivingExtension>().remainingDiveTime >= 0f)
                        {
                            if (Player.m_localPlayer.GetComponent<BetterDivingExtension>().remainingDiveTime >= 1f) Player.m_localPlayer.GetComponent<BetterDivingExtension>().remainingDiveTime = 1f;

                            var one_percentage = 1f / 120f;

                            float final_dive_drain = BetterDiving.breatheDrain.Value;
                            float final_dive_drain_one_percentage = BetterDiving.breatheDrain.Value / 120f;

                            BetterDiving.DebugLog("one_percentage" + " -> " + one_percentage);
                            BetterDiving.DebugLog("final_dive_drain" + " -> " + final_dive_drain);
                            BetterDiving.DebugLog("final_dive_drain_one_percentage" + " -> " + final_dive_drain_one_percentage);

                            float skillFactor = Player.m_localPlayer.GetSkillFactor(BetterDiving.DivingSkillType);
                            float num = Mathf.Lerp(1f, 0.5f, skillFactor);
                            final_dive_drain = (final_dive_drain * num) / 120f;
                            if (final_dive_drain > final_dive_drain_one_percentage) final_dive_drain = final_dive_drain_one_percentage;

                            if ((ZInput.GetButton("Run") || ZInput.GetButton("JoyRun")) && BetterDiving.allowFastSwimming.Value == true)
                            {
                                final_dive_drain *= 2f;
                            }

                            Player.m_localPlayer.GetComponent<BetterDivingExtension>().remainingDiveTime -= final_dive_drain;

                            BetterDiving.DebugLog("skillFactor" + " -> " + skillFactor);
                            BetterDiving.DebugLog("converted skillFactor" + " -> " + num);
                            BetterDiving.DebugLog("final_dive_drain" + " -> " + final_dive_drain);

                        }
                        if (Player.m_localPlayer.GetComponent<BetterDivingExtension>().remainingDiveTime <= 0f)
                        {
                            Player.m_localPlayer.GetComponent<BetterDivingExtension>().staminaToUpdate -= Player.m_localPlayer.GetComponent<BetterDivingExtension>().maxStamina * 0.2f;
                        }
                    }
                    else
                    {
                        if (Player.m_localPlayer.GetComponent<BetterDivingExtension>().remainingDiveTime < 1f)
                        {
                            var one_percentage = 1f / 120f;

                            float final_dive_drain = BetterDiving.breatheDrain.Value;
                            float final_dive_drain_one_percentage = BetterDiving.breatheDrain.Value / 120f;

                            BetterDiving.DebugLog("one_percentage" + " -> " + one_percentage);
                            BetterDiving.DebugLog("final_dive_drain" + " -> " + final_dive_drain);
                            BetterDiving.DebugLog("final_dive_drain_one_percentage" + " -> " + final_dive_drain_one_percentage);

                            float skillFactor = Player.m_localPlayer.GetSkillFactor(BetterDiving.DivingSkillType);
                            float num = Mathf.Lerp(1f, 0.5f, skillFactor);
                            final_dive_drain = (final_dive_drain * num) / 120f;
                            if (final_dive_drain > final_dive_drain_one_percentage) final_dive_drain = final_dive_drain_one_percentage;

                            Player.m_localPlayer.GetComponent<BetterDivingExtension>().remainingDiveTime += 0.125f;

                            BetterDiving.DebugLog("skillFactor" + " -> " + skillFactor);
                            BetterDiving.DebugLog("converted skillFactor" + " -> " + num);
                            BetterDiving.DebugLog("final_dive_drain" + " -> " + final_dive_drain);
                        }
                    }

                    BetterDiving.DebugLog("Player_isTakeRestInWater" + " -> " + Player.m_localPlayer.GetComponent<BetterDivingExtension>().isTakeRestInWater);

                    if (Player.m_localPlayer.GetComponent<BetterDivingExtension>().isTakeRestInWater == true)
                    {
                        if (Player.m_localPlayer.GetComponent<BetterDivingExtension>().staminaToUpdate < Player.m_localPlayer.GetComponent<BetterDivingExtension>().maxStamina)
                        {
                            if (BetterDiving.ow_staminaRestoreValue.Value == true)
                            {
                                Player.m_localPlayer.GetComponent<BetterDivingExtension>().staminaToUpdate += BetterDiving.ow_staminaRestorPerTick.Value;

                                BetterDiving.DebugLog("ow_staminaRestorPerTick" + " -> " + BetterDiving.ow_staminaRestorPerTick.Value);
                                BetterDiving.DebugLog("Player_staminaToUpdate" + " -> " + Player.m_localPlayer.GetComponent<BetterDivingExtension>().staminaToUpdate);
                            }
                            else
                            {
                                Player.m_localPlayer.GetComponent<BetterDivingExtension>().staminaToUpdate += Player.m_localPlayer.GetComponent<BetterDivingExtension>().maxStamina * 0.0115f;

                                BetterDiving.DebugLog("ow_staminaRestorPerTick" + " -> " + Player.m_localPlayer.GetComponent<BetterDivingExtension>().maxStamina * 0.0115f);
                                BetterDiving.DebugLog("Player_staminaToUpdate" + " -> " + Player.m_localPlayer.GetComponent<BetterDivingExtension>().staminaToUpdate);
                            }
                        }
                    }
                }

                BetterDiving.DebugLog("----------------------Section Update Some Values End-------------------------");
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
            if (BetterDiving.has_created_breathe_bar == false)
            {
                if (Player.m_localPlayer != null && Hud.instance != null && Hud.instance.m_pieceSelectionWindow != null)
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

                    BetterDiving.loc_breath_bar_bg = panel;

                    BetterDiving.loc_breath_bar_bg.name = "BreathBarBG";
                    BetterDiving.loc_breath_bar_bg.GetComponent<Image>().sprite = BetterDiving.breath_bg_sprite;

                    BetterDiving.loc_breath_bar_bg = panel;

                    // Create depleted breath bar
                    var depleted_breath_progress = GUIManager.Instance.CreateWoodpanel(
                        parent: Hud.instance.transform,
                        anchorMin: new Vector2(0.5f, 0.5f),
                        anchorMax: new Vector2(0.5f, 0.5f),
                        position: new Vector2(60f, -60f),
                        width: 15f,
                        height: 120f,
                        draggable: false);

                    BetterDiving.loc_depleted_breath = depleted_breath_progress;

                    BetterDiving.loc_depleted_breath.name = "depleted_breath_progress";
                    BetterDiving.loc_depleted_breath.GetComponent<Image>().sprite = BetterDiving.breath_prog_sprite;
                    BetterDiving.loc_depleted_breath.GetComponent<Image>().type = Image.Type.Filled;
                    BetterDiving.loc_depleted_breath.GetComponent<Image>().fillMethod = Image.FillMethod.Vertical;
                    BetterDiving.loc_depleted_breath.GetComponent<Image>().fillAmount = 1f;

                    BetterDiving.loc_depleted_breath.GetComponent<Image>().color = new Color32(204, 204, 204, 255);

                    // Create the breath progress bar

                    var breath_progress = GUIManager.Instance.CreateWoodpanel(
                        parent: Hud.instance.transform,
                        anchorMin: new Vector2(0.5f, 0.5f),
                        anchorMax: new Vector2(0.5f, 0.5f),
                        position: new Vector2(60f, -60f),
                        width: 15f,
                        height: 120f,
                        draggable: false);

                    BetterDiving.loc_breath_bar = breath_progress;

                    BetterDiving.loc_breath_bar.name = "breathbar_progress";
                    BetterDiving.loc_breath_bar.GetComponent<Image>().sprite = BetterDiving.breath_prog_sprite;
                    BetterDiving.loc_breath_bar.GetComponent<Image>().type = Image.Type.Filled;
                    BetterDiving.loc_breath_bar.GetComponent<Image>().fillMethod = Image.FillMethod.Vertical;
                    BetterDiving.loc_breath_bar.GetComponent<Image>().fillAmount = 1f;

                    // Create the breath progress overlay to apply a Valheim style

                    var breath_overlay = GUIManager.Instance.CreateWoodpanel(
                        parent: Hud.instance.transform,
                        anchorMin: new Vector2(0.5f, 0.5f),
                        anchorMax: new Vector2(0.5f, 0.5f),
                        position: new Vector2(60f, -60f),
                        width: 15f,
                        height: 120f,
                        draggable: false);

                    BetterDiving.loc_breathe_overlay = breath_overlay;
                    BetterDiving.loc_breathe_overlay.name = "BreathBarOverlay";
                    BetterDiving.loc_breathe_overlay.GetComponent<Image>().sprite = BetterDiving.breath_overlay_sprite;

                    if (BetterDiving.owBIPos.Value == true)
                    {

                        BetterDiving.loc_breath_bar_bg.transform.position = new Vector3(BetterDiving.owBIPosX.Value, BetterDiving.owBIPosY.Value, Hud.instance.transform.position.y);
                        BetterDiving.loc_depleted_breath.transform.position = new Vector3(BetterDiving.owBIPosX.Value, BetterDiving.owBIPosY.Value, Hud.instance.transform.position.y);
                        BetterDiving.loc_breath_bar.transform.position = new Vector3(BetterDiving.owBIPosX.Value, BetterDiving.owBIPosY.Value, Hud.instance.transform.position.y);
                        BetterDiving.loc_breathe_overlay.transform.position = new Vector3(BetterDiving.owBIPosX.Value, BetterDiving.owBIPosY.Value, Hud.instance.transform.position.y);
                    }

                    BetterDiving.loc_breath_bar_bg.SetActive(false);
                    BetterDiving.loc_depleted_breath.SetActive(false);
                    BetterDiving.loc_breath_bar.SetActive(false);
                    BetterDiving.loc_breathe_overlay.SetActive(false);



                    BetterDiving.has_created_breathe_bar = true;
                }
            }
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
            if (!Player.m_localPlayer) return;

            if (GameCamera.instance)
            {
                BetterDiving.water_level_camera = __instance.GetWaterSurface(new Vector3(GameCamera.instance.transform.position.x, GameCamera.instance.transform.position.y, GameCamera.instance.transform.position.z));
            }
            if (Player.m_localPlayer)
            {
                BetterDiving.water_level_player = __instance.GetWaterSurface(new Vector3(Player.m_localPlayer.transform.position.x, Player.m_localPlayer.transform.position.y, Player.m_localPlayer.transform.position.z));
            }
            if (BetterDiving.loc_cam_pos_y < BetterDiving.water_level_camera && Player.m_localPlayer.IsSwiming())
            {
                if (__instance.m_waterSurface.GetComponent<MeshRenderer>().transform.rotation.eulerAngles.y != 180f && BetterDiving.isEnvAllowed())
                {
                    __instance.m_waterSurface.transform.Rotate(180f, 0f, 0f);
                    __instance.m_waterSurface.material = BetterDiving.water_mat;
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

                __instance.m_waterSurface.transform.position = new Vector3(__instance.m_waterSurface.transform.position.x, BetterDiving.water_level_camera, __instance.m_waterSurface.transform.position.z);
            }
            else
            {
                if (__instance.m_waterSurface.GetComponent<MeshRenderer>().transform.rotation.eulerAngles.y == 180f && BetterDiving.isEnvAllowed())
                {

                    __instance.m_waterSurface.transform.Rotate(-180f, 0f, 0f);
                    __instance.m_waterSurface.material = BetterDiving.mai_water_mat;

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
    }

    //Water volume awake -> detect water volumes
    [HarmonyPatch(typeof(WaterVolume), "Awake")]
    public class WaterVolume_Awake
    {
        [HarmonyPrefix]
        public static void Prefix(WaterVolume __instance)
        {
            if (__instance == null) return;

            BetterDiving.DebugLog("---------------------Section WaterVolume Awake Prefix Start------------------------");
            BetterDiving.DebugLog("water_volume_awake" + " -> " + "true");
            BetterDiving.DebugLog("----------------------Section WaterVolume Awake Prefix End-------------------------");
        }

        [HarmonyPostfix]
        public static void Postfix(WaterVolume __instance)
        {
            if (__instance == null) return;

            BetterDiving.DebugLog("---------------------Section WaterVolume Awake Postfix Start------------------------");
            BetterDiving.DebugLog("mai_water_mat" + " -> " + (BetterDiving.mai_water_mat == null ? "null" : "not null"));

            if (BetterDiving.mai_water_mat == null)
            {
                BetterDiving.mai_water_mat = __instance.m_waterSurface.material;
                BetterDiving.DebugLog("Info" + " -> " + "set water mat");
            }

            BetterDiving.DebugLog("----------------------Section WaterVolume Awake Postfix End-------------------------");
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
        //public static void Prefix(GameCamera __instance, Camera ___m_camera, ref bool ___m_waterClipping)
        public static void Prefix(GameCamera __instance, Camera ___m_camera)
        {
            if (!Player.m_localPlayer) return;

            //Env
            if (BetterDiving.EnvName != EnvMan.instance.GetCurrentEnvironment().m_name)
            {
                BetterDiving.EnvName = EnvMan.instance.GetCurrentEnvironment().m_name;
            }
            if ((Player.m_localPlayer.GetComponent<BetterDivingExtension>().is_diving || Player.m_localPlayer.IsSwiming()) && Player.m_localPlayer.GetComponent<BetterDivingExtension>().isTakeRestInWater == false && BetterDiving.isEnvAllowed())
            {
                __instance.m_minWaterDistance = -5000f;
            }
            else
            {
                __instance.m_minWaterDistance = 0.3f;
            }

            if (__instance.m_maxDistance != 3f && BetterDiving.loc_m_m_maxDistance == 0) BetterDiving.loc_m_m_maxDistance = __instance.m_maxDistance;

            BetterDiving.loc_cam_pos_y = ___m_camera.gameObject.transform.position.y;

            if (___m_camera.gameObject.transform.position.y < BetterDiving.water_level_camera && (Player.m_localPlayer.IsSwiming() || Player.m_localPlayer.GetComponent<BetterDivingExtension>().is_diving) && BetterDiving.isEnvAllowed())
            {
                if (__instance.m_minWaterDistance != -5000f) __instance.m_minWaterDistance = -5000f;

                if (Player.m_localPlayer.GetComponent<BetterDivingExtension>().is_diving) __instance.m_maxDistance = 3f;

                EnvSetup curr_env = EnvMan.instance.GetCurrentEnvironment();
                Color water_color;
                if (EnvMan.instance.IsNight())
                {
                    water_color = curr_env.m_fogColorNight;
                }
                else
                {
                    water_color = curr_env.m_fogColorDay;
                }

                water_color.a = 1f;
                water_color = ChangeColorBrightness(water_color, BetterDiving.char_swim_depth * (BetterDiving.ow_color_brightness_factor.Value));
                RenderSettings.fogColor = water_color;

                float fog_dens = RenderSettings.fogDensity + (BetterDiving.char_swim_depth * BetterDiving.ow_fogdensity_factor.Value);
                if (fog_dens < BetterDiving.ow_Min_fogdensity.Value) fog_dens = BetterDiving.ow_Min_fogdensity.Value;
                if (fog_dens > BetterDiving.ow_Max_fogdensity.Value) fog_dens = BetterDiving.ow_Max_fogdensity.Value;

                RenderSettings.fogDensity = fog_dens;

                BetterDiving.render_settings_updated_camera = false;
            }
            else
            {
                if (___m_camera.gameObject.transform.position.y > BetterDiving.water_level_camera)
                {
                    if (BetterDiving.render_settings_updated_camera == false)
                    {
                        if (BetterDiving.loc_m_m_maxDistance != 0) __instance.m_maxDistance = BetterDiving.loc_m_m_maxDistance;
                        EnvMan.instance.SetForceEnvironment(EnvMan.instance.GetCurrentEnvironment().m_name);
                        BetterDiving.render_settings_updated_camera = true;
                        BetterDiving.set_force_env = true;
                    }
                    if (BetterDiving.set_force_env == true)
                    {
                        EnvMan.instance.SetForceEnvironment("");
                        BetterDiving.set_force_env = false;
                    }
                }
                if (!Player.m_localPlayer.GetComponent<BetterDivingExtension>().is_diving && BetterDiving.loc_m_m_maxDistance != 0) __instance.m_maxDistance = BetterDiving.loc_m_m_maxDistance;

                if (!Player.m_localPlayer.GetComponent<BetterDivingExtension>().is_diving && BetterDiving.minwaterdist != 0f) __instance.m_minWaterDistance = BetterDiving.minwaterdist;
            }
        }
    }
}