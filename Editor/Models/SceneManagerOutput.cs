using System;
using System.Collections.Generic;
using UnityEngine;

namespace SweetHome.Editor.Models
{
    [Serializable]
    public class SceneInfoOutput
    {
        public string name;
        public string path;
        public int rootObjectCount;
        public int buildIndex;
    }

    [Serializable]
    public class BuildSettingsScenesOutput
    {
        public List<string> scenes;
    }
}
