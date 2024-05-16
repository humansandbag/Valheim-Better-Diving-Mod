using HarmonyLib;
using UnityEngine;

namespace BetterDiving.Patches
{
    /*

    ===============================================================================


    Patches the GameCamera UpdateCamera method to update camera effects for diving or reset them for walking


    ===============================================================================

    */
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
        public static void Prefix(GameCamera __instance, Camera ___m_camera)
        {
            if (!Player.m_localPlayer) return;

            //Env
            if (BetterDiving.EnvName != EnvMan.instance.GetCurrentEnvironment().m_name)
            {
                BetterDiving.EnvName = EnvMan.instance.GetCurrentEnvironment().m_name;
            }
            if ((Player.m_localPlayer.GetComponent<BetterDivingExtension>().is_diving || Player.m_localPlayer.IsSwimming()) && Player.m_localPlayer.GetComponent<BetterDivingExtension>().isTakeRestInWater == false && BetterDiving.IsEnvAllowed())
            {
                __instance.m_minWaterDistance = -5000f;
            }
            else
            {
                __instance.m_minWaterDistance = 0.3f;
            }

            if (__instance.m_maxDistance != 3f && BetterDiving.loc_m_m_maxDistance == 0) BetterDiving.loc_m_m_maxDistance = __instance.m_maxDistance;

            BetterDiving.loc_cam_pos_y = ___m_camera.gameObject.transform.position.y;

            if (___m_camera.gameObject.transform.position.y < BetterDiving.water_level_camera && (Player.m_localPlayer.IsSwimming() || Player.m_localPlayer.GetComponent<BetterDivingExtension>().is_diving) && BetterDiving.IsEnvAllowed())
            {
                if (__instance.m_minWaterDistance != -5000f) __instance.m_minWaterDistance = -5000f;

                if (Player.m_localPlayer.GetComponent<BetterDivingExtension>().is_diving) __instance.m_maxDistance = 3f;

                EnvSetup curr_env = EnvMan.instance.GetCurrentEnvironment();
                Color water_color;
                if (EnvMan.IsNight())
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