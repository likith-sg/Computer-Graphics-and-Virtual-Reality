using UnityEngine;

namespace InteriorStudio.Environment
{
    public enum IlluminationMode
    {
        CompositePBR,
        DiffuseOnly,
        SpecularOnly,
        AmbientOnly
    }

    public class IlluminationVisualizer : MonoBehaviour
    {
        [Header("Illumination State")]
        public IlluminationMode currentMode = IlluminationMode.CompositePBR;

        [Header("Target Renderer (Red Velvet Chair)")]
        public Renderer targetChairRenderer;

        private Material chairOriginalMaterial;
        private Color originalColor;
        private float originalSmoothness;
        private float originalMetallic;

        private void Start()
        {
            if (targetChairRenderer != null)
            {
                chairOriginalMaterial = targetChairRenderer.material;
                originalColor = chairOriginalMaterial.color;
                if (chairOriginalMaterial.HasProperty("_Smoothness"))
                    originalSmoothness = chairOriginalMaterial.GetFloat("_Smoothness");
                if (chairOriginalMaterial.HasProperty("_Metallic"))
                    originalMetallic = chairOriginalMaterial.GetFloat("_Metallic");
            }
        }

        public void SetIlluminationMode(IlluminationMode mode)
        {
            currentMode = mode;
            if (targetChairRenderer == null) return;

            Material mat = targetChairRenderer.material;

            switch (currentMode)
            {
                case IlluminationMode.CompositePBR:
                    // Full physical composite: Ambient + Diffuse Lambertian + Specular Microfacet + Sheen
                    mat.color = originalColor;
                    if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", originalSmoothness);
                    if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", originalMetallic);
                    break;

                case IlluminationMode.DiffuseOnly:
                    // Pure Lambertian diffuse reflection: I_d = k_d * I_l * max(0, N dot L)
                    // Zero out specular/smoothness
                    mat.color = originalColor;
                    if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.0f);
                    if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0.0f);
                    break;

                case IlluminationMode.SpecularOnly:
                    // Pure Specular reflection: I_s = k_s * I_l * max(0, R dot V)^n
                    // Set base albedo to near-black to reveal solely the specular highlight lobe
                    mat.color = new Color(0.04f, 0.04f, 0.04f);
                    if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.92f);
                    if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0.1f);
                    break;

                case IlluminationMode.AmbientOnly:
                    // Ambient baseline: I_a = k_a * I_a
                    // Flat uniform ambient tone without directional light contribution
                    mat.color = originalColor * 0.45f;
                    if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.0f);
                    if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0.0f);
                    break;
            }

            Debug.Log($"[IlluminationVisualizer] Illumination component isolated: {currentMode}");
        }
    }
}
