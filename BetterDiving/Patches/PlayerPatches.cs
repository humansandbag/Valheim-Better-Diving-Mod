using HarmonyLib;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace BetterDiving.Patches
{
    /*

    ===============================================================================


    Patches the Player Awake method and extends the player with a custom component


    ===============================================================================

    */
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

    /*

    ===============================================================================


    Custom component for extending the player


    ===============================================================================

    */
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

    /*

    ===============================================================================


    Patches the Player OnDeath method


    ===============================================================================

    */
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

    /*

    ===============================================================================


    Patches the Player Update method to update oxygen bar and starts the StartCountdown coroutine


    ===============================================================================

    */
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
}