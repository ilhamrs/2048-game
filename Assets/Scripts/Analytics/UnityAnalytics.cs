using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Analytics;

public class UnityAnalytics : MonoBehaviour
{
    void Awake()
    {
        UnityServices.InitializeAsync();
    }
    // Start is called before the first frame update
    void Start()
    {
        AnalyticsService.Instance.StartDataCollection();
    }
}
