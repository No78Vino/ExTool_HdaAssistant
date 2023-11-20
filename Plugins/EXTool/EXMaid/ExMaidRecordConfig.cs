using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace EXTool
{
    [CreateAssetMenu(fileName = "ExMaidRecorder", menuName = "EXTool/Ex Maid Recorder", order = 0)]
    public class ExMaidRecordConfig : ScriptableObject
    {
        [Serializable]
        public class AutomationPathPair
        {
            public string hda;
            public string spp;
        }
        
        public List<AutomationPathPair> houdiniAndSubPainter;
    }
}