using UnityEngine;
using System.Collections;

public class DisplaySettingManager : MonoBehaviour
{

    void Awake()
    {
        Configurations.Loaded += ConfigurationsLoaded;
    }

    private void ConfigurationsLoaded(object configurations, Configurations.LoadedEventArgs loadedInfo)
    {
        if (loadedInfo.successful)
        {
            Configurations configs = (Configurations)configurations;
            if (configs != null)
            {
                Screen.SetResolution(configs.resolutionWidth, configs.resolutionHeight, false);
            }
        }
    }
}
