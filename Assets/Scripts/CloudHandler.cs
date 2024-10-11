using UnityEngine;
using Assets.Scripts;
using ModApi.Flight.Sim;
using System;

public class CloudHandler : MonoBehaviour
{
    public GameObject Clouds;
    private bool _isDroo;
    
    void Start()
    {
        Game.Instance.FlightScene.CraftNode.ChangedSoI += OnPlanetChangedSOI;
        ModSettings.Instance.Changed += SettingChanged;
        CloudCheck();
    }

    private void OnPlanetChangedSOI(IOrbitNode source)
    {
        CloudCheck();
    }

    private void SettingChanged(object sender, EventArgs e)
    {
        CloudCheck();
    }

    void CloudCheck()
    {
        _isDroo = Game.Instance.FlightScene.CraftNode.Parent.Name == "Droo";
        if (_isDroo)
        {
            if (Clouds == null)
            {
                Clouds = Instantiate(Mod.Instance.ResourceLoader.LoadAsset<GameObject>("Clouds"));
            }

            Clouds.transform.localScale = (2 + ModSettings.Instance.CloudHeight) * (float)Game.Instance.FlightScene.CraftNode.Parent.PlanetData.Radius * Vector3.one;
        }
        else if (Clouds != null)
        {
            Destroy(Clouds);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_isDroo)
        {
            Clouds.transform.position = Game.Instance.FlightScene.CraftNode.ReferenceFrame.PlanetToFramePosition(Vector3d.zero);
        }
    }
}
