using System;
using UnityEngine;

namespace InteriorStudio.Environment
{
    public class LightingPresetManager : MonoBehaviour
    {
        [Header("Primary Light Sources")]
        public Light directionalSunLight;
        public Light floorLampLight;
        public Light[] ceilingPotLights;

        [Header("Preset Status")]
        public int activePreset = 0;
        public string activePresetName = "Bright Morning Sunlight";

        public void ApplyPreset(int presetIndex)
        {
            activePreset = Mathf.Clamp(presetIndex, 0, 2);

            switch (activePreset)
            {
                case 0: // Bright Morning Sunlight (Question 1)
                    activePresetName = "Bright Morning Sunlight";
                    if (directionalSunLight != null)
                    {
                        directionalSunLight.enabled = true;
                        // Sun entering through South window (-Z) towards room (+Z, slightly East)
                        directionalSunLight.transform.rotation = Quaternion.Euler(38f, 32f, 0f);
                        directionalSunLight.color = new Color(1.0f, 0.96f, 0.90f);
                        directionalSunLight.intensity = 1.35f;
                        directionalSunLight.shadows = LightShadows.Soft;
                    }
                    RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
                    RenderSettings.ambientLight = new Color(0.45f, 0.50f, 0.55f); // Crisp sky ambient bounce

                    if (floorLampLight != null) floorLampLight.intensity = 0.0f;
                    SetPotLightsIntensity(0.15f);
                    break;

                case 1: // Cozy Evening
                    activePresetName = "Cozy Golden Evening";
                    if (directionalSunLight != null)
                    {
                        directionalSunLight.enabled = true;
                        // Low angle warm sunset through window
                        directionalSunLight.transform.rotation = Quaternion.Euler(14f, 45f, 0f);
                        directionalSunLight.color = new Color(1.0f, 0.65f, 0.35f);
                        directionalSunLight.intensity = 0.45f;
                        directionalSunLight.shadows = LightShadows.Soft;
                    }
                    RenderSettings.ambientLight = new Color(0.25f, 0.20f, 0.22f); // Darker warm ambient

                    if (floorLampLight != null)
                    {
                        floorLampLight.enabled = true;
                        floorLampLight.color = new Color(1.0f, 0.82f, 0.55f);
                        floorLampLight.intensity = 1.25f;
                    }
                    SetPotLightsIntensity(0.85f);
                    break;

                case 2: // Neutral Studio Daylight
                    activePresetName = "Neutral Studio Daylight";
                    if (directionalSunLight != null)
                    {
                        directionalSunLight.enabled = true;
                        directionalSunLight.transform.rotation = Quaternion.Euler(58f, 15f, 0f);
                        directionalSunLight.color = new Color(0.98f, 0.98f, 1.0f);
                        directionalSunLight.intensity = 1.10f;
                        directionalSunLight.shadows = LightShadows.Soft;
                    }
                    RenderSettings.ambientLight = new Color(0.55f, 0.55f, 0.58f); // High diffuse fill

                    if (floorLampLight != null) floorLampLight.intensity = 0.0f;
                    SetPotLightsIntensity(0.35f);
                    break;
            }

            Debug.Log($"[LightingPresetManager] Applied preset [{activePreset}]: {activePresetName}");
        }

        private void SetPotLightsIntensity(float intensity)
        {
            if (ceilingPotLights == null) return;
            foreach (var l in ceilingPotLights)
            {
                if (l != null)
                {
                    l.intensity = intensity;
                    l.color = new Color(1.0f, 0.92f, 0.82f);
                }
            }
        }
    }
}
