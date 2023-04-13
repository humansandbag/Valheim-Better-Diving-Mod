using HarmonyLib;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace BetterDiving.Patches
{
    /*

    ===============================================================================


    Patches the Hud Update method to build the oxygen bar


    ===============================================================================

    */
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
}