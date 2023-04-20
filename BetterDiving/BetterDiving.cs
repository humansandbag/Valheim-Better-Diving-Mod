using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Jotunn;
using Jotunn.Configs;
using Jotunn.Managers;
using Jotunn.Utils;
using System.Reflection;
using UnityEngine;

namespace BetterDiving
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Main.ModGuid)]
    [BepInIncompatibility("ch.easy.develope.vh.diving.mod")]
    [BepInIncompatibility("blacks7ar.VikingsDoSwim")]
    [BepInIncompatibility("projjm.improvedswimming")]
    //[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]

    /*

    ===============================================================================


    BetterDiving - create the Unity plugin


    ===============================================================================

    */
    internal class BetterDiving : BaseUnityPlugin
    {
        public const string PluginGUID = "MainStreetGaming.BetterDiving";
        public const string PluginName = "BetterDiving";
        public const string PluginVersion = "1.0.6";

        // Use this class to add your own localization to the game
        // https://valheim-modding.github.io/Jotunn/tutorials/localization.html
        //public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();


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

        //player stamina values
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

        //Env values
        public static string EnvName = "";

        //Diving skill
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

            AssetBundle watermatAssetBundle = AssetUtils.LoadAssetBundleFromResources("watermat.assets");

            if (watermatAssetBundle == null)
            {
                Debug.LogError("load_watermat_assets_failed");
                return null;
            }

            AssetBundle betterDivingAssetBundle = AssetUtils.LoadAssetBundleFromResources("betterdiving.assets");

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

        /*

        ====================

        Awake


        Mod setup

        ====================

        */
        void Awake()
        {
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);

            CreateConfigValues();

            if (configDisplayGreeting.Value)
            {
                Debug.Log(PluginName + ": " + configGreeting.Value);
            }
        }

        /*

        ====================

        Update


        Runs every frame

        ====================

        */
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

        /*

        ====================

        CreateConfigValues


        Defines the mod config values

        ====================

        */
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
            ow_staminaRestoreValue = Config.Bind("Server config", "ow_staminaRestoreValue", false, new ConfigDescription("Overwrite stamina restore value per tick when take rest in water", null, isAdminOnly));
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

        /*

        ====================

        IsEnvAllowed


        Checks if the player is allows to dive in the current environment

        ====================

        */
        public static bool IsEnvAllowed()
        {
            if (BetterDiving.EnvName == "SunkenCrypt") return false;

            return true;
        }

        /*

        ====================

        DebugLog


        Writes debug data to the log/console

        ====================

        */
        public static void DebugLog(string data)
        {
            if (doDebug.Value) Debug.Log(PluginName + ": " + data);
        }
    }
}