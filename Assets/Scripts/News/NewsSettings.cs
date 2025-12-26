using System;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionEmpire.News
{
    /// <summary>
    /// Player settings for news system
    /// </summary>
    [Serializable]
    public class NewsSettings
    {
        public float NewsFrequency = 1.0f; // 0.5 = half frequency, 2.0 = double
        public List<EventType> PreferredCategories = new List<EventType>();
        public List<EventType> IgnoredCategories = new List<EventType>();
        public float RealityBlend = 1.0f; // 0.0 = all procedural, 1.0 = all real
        public bool AutoProcessNews = true;
        public int MaxNewsPerDay = 10;
        
        public void Save()
        {
            PlayerPrefs.SetFloat("NewsFrequency", NewsFrequency);
            PlayerPrefs.SetFloat("RealityBlend", RealityBlend);
            PlayerPrefs.SetInt("AutoProcessNews", AutoProcessNews ? 1 : 0);
            PlayerPrefs.SetInt("MaxNewsPerDay", MaxNewsPerDay);
            PlayerPrefs.Save();
        }
        
        public void Load()
        {
            NewsFrequency = PlayerPrefs.GetFloat("NewsFrequency", 1.0f);
            RealityBlend = PlayerPrefs.GetFloat("RealityBlend", 1.0f);
            AutoProcessNews = PlayerPrefs.GetInt("AutoProcessNews", 1) == 1;
            MaxNewsPerDay = PlayerPrefs.GetInt("MaxNewsPerDay", 10);
        }
    }
}

