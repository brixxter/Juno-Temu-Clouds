using UnityEngine;
using Assets.Scripts;
using ModApi.Flight.Sim;
using System;

public class CloudHandler : MonoBehaviour
{
    public GameObject Clouds;
    private ModApi.Craft.ICraftNode _craftNode;
    private Material _material;
    
    void Start()
    {
        _craftNode = Game.Instance.FlightScene.CraftNode;
        _craftNode.ChangedSoI += OnPlanetChangedSOI;
        ModSettings.Instance.Changed += SettingChanged;
        CloudCheck();
    }

    void Update()
    {
        if (Clouds != null)
        {
            Clouds.transform.position = _craftNode.ReferenceFrame.PlanetToFramePosition(Vector3d.zero);
            _material.SetFloat("_RotationAngle", (float)(Game.Instance.FlightScene.FlightState.Time) / 3600);
            // TODO: Ensure clouds match the rotation of the planet in all conditions so the static layer is always where it belongs
            _material.SetVector("_LightDirection", -Game.Instance.FlightScene.ViewManager.GameView.SunLight.transform.forward);
        }
    }

    private void OnPlanetChangedSOI(IOrbitNode source)
    {
        CloudCheck();
    }

    private void SettingChanged(object sender, EventArgs e)
    {
        CloudCheck();
    }

    private void CloudCheck()
    {
        if (_craftNode.Parent.Name == "Droo")
        {
            if (Clouds == null)
            {
                Clouds = Instantiate(Mod.Instance.ResourceLoader.LoadAsset<GameObject>("Clouds"));
                _material = Clouds.GetComponent<Renderer>()?.sharedMaterial;

                if (!AssignCubemapToClouds(Clouds))
                {
                    Destroy(Clouds);
                }
            }

            Clouds.transform.localScale = 2 * ((float)_craftNode.Parent.PlanetData.Radius + 1000 * ModSettings.Instance.CloudHeight) * Vector3.one;
        }
        else if (Clouds != null)
        {
            Destroy(Clouds);
        }
    }

    private bool AssignCubemapToClouds(GameObject clouds)
    {
        Texture2D[] textures = new Texture2D[6];
        string[] textureFiles = {
            "posx.png",
            "negx.png",
            "posy.png",
            "negy.png",
            "posz.png",
            "negz.png",
        };

        for (int i = 0; i < textureFiles.Length; i++)
        {
            string filePath = ModApi.Utilities.CombinePaths(Game.PersistentDataPath, "Mods/TemuClouds/", textureFiles[i]);
            if (!System.IO.File.Exists(filePath))
            {
                Debug.LogError($"File not found: {filePath}");
                return false;
            }

            textures[i] = LoadTexture(filePath);
        }

        // Check if all textures have the same resolution and are square
        int width = textures[0].width;
        int height = textures[0].height;
        if (width != height)
        {
            Debug.LogError("Wrong cloud texture resolution");
            return false;
        }

        for (int i = 1; i < textures.Length; i++)
        {
            if (textures[i].width != width || textures[i].height != height)
            {
                Debug.LogError("Wrong cloud texture resolution");
                return false;
            }
        }

        Cubemap cubemap = new(width, TextureFormat.RGBA32, false);
        cubemap.SetPixels(textures[0].GetPixels(), CubemapFace.PositiveX);
        cubemap.SetPixels(textures[1].GetPixels(), CubemapFace.NegativeX);
        cubemap.SetPixels(textures[2].GetPixels(), CubemapFace.PositiveY);
        cubemap.SetPixels(textures[3].GetPixels(), CubemapFace.NegativeY);
        cubemap.SetPixels(textures[4].GetPixels(), CubemapFace.PositiveZ);
        cubemap.SetPixels(textures[5].GetPixels(), CubemapFace.NegativeZ);
        cubemap.Apply();

        _material.SetTexture("_CloudCube", cubemap);
        return true;
    }

    private Texture2D LoadTexture(string filePath)
    {
        byte[] fileData = System.IO.File.ReadAllBytes(filePath);
        Texture2D texture = new(2, 2);
        texture.LoadImage(fileData);
        return texture;
    }
}
