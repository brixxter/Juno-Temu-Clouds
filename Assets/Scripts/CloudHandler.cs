using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;
using ModApi.Flight.Sim;
using System;
using Object = System.Object;

public class CloudHandler : MonoBehaviour
{
    public GameObject clouds;
    private bool isdroo;
    
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

    private void SettingChanged(Object sender, EventArgs e)
    {
        CloudCheck();
    }

    void CloudCheck()
    {
        if(Game.Instance.FlightScene.CraftNode.Parent.Name == "Droo")
        {
            isdroo = true;
            if(clouds==null) clouds = Instantiate(Mod.Instance.ResourceLoader.LoadAsset<GameObject>("Clouds"));
            clouds.transform.localScale = (2+ModSettings.Instance.CloudHeight) * (float)Game.Instance.FlightScene.CraftNode.Parent.PlanetData.Radius * new Vector3(1,1,1);
        
        } else {

            if(clouds!=null) Destroy(clouds);
            isdroo = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(isdroo)
        {
            clouds.transform.position = Game.Instance.FlightScene.CraftNode.ReferenceFrame.PlanetToFramePosition(Vector3d.zero);
        }
    }
}
