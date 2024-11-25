using System.Collections.Generic;
using Windows;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "WindowsData", menuName = "ScriptableObjects/WindowsData", order = 1)]
    public class WindowsData : ScriptableObject
    {
        public List<WindowBase> prefabs;
    }
}