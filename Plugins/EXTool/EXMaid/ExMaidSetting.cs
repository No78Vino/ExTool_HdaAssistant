using UnityEditor;
using UnityEngine;

namespace EXTool
{
    public static class ExMaidSetting
    {
        public static readonly string EditorPrefsNameOfHoudiniPath = "directoryOfHoudini";
        public static readonly string EditorPrefsNameOfSubPtPath = "directoryOfSubstancePainter";
        public static readonly string EditorPrefsNameOfPort = "portOfUnityServer";
        public static readonly string EditorPrefsNameOfHouModule = "pathOfHouModule";
        public static readonly string EditorPrefsNameOfSitePackages = "pathOfSitePackages";
        public static readonly string EditorPrefsNameOfUsingRecorder = "usingRecorder";
        public static readonly string EditorPrefsNameOfRecorderPath = "directoryOfRecorder";

        public static string HoudiniPath => EditorPrefs.GetString(EditorPrefsNameOfHoudiniPath, "");
        public static string SubPtPath => EditorPrefs.GetString(EditorPrefsNameOfSubPtPath, "");
        public static string PathOfHouModule => EditorPrefs.GetString(EditorPrefsNameOfHouModule, "");
        public static string PathOfSitePackages => EditorPrefs.GetString(EditorPrefsNameOfSitePackages, "");
        public static int Port => EditorPrefs.GetInt(EditorPrefsNameOfPort, 13000);
        public static bool UsingRecorder => EditorPrefs.GetBool(EditorPrefsNameOfUsingRecorder, true);

        public static string PathOfRecorder =>
            EditorPrefs.GetString(EditorPrefsNameOfRecorderPath, "Assets/Temp/EXTool/ExMaidRecord.asset");


        public static void TryToRecord(string hdaPath, string sppPath)
        {
            if (!UsingRecorder) return;

            var path = PathOfRecorder;
            var recorder = AssetDatabase.LoadAssetAtPath<ExMaidRecordConfig>(path);

            if (recorder == null)
            {
                recorder = ScriptableObject.CreateInstance<ExMaidRecordConfig>();
                AssetDatabase.CreateAsset(recorder, path);
            }

            var containPair = false;
            for (var i = 0; i < recorder.houdiniAndSubPainter.Count; i++)
            {
                var pair = recorder.houdiniAndSubPainter[i];
                if (pair.hda == hdaPath)
                {
                    recorder.houdiniAndSubPainter[i].spp = sppPath;
                    containPair = true;
                    break;
                }
            }

            if (!containPair)
                recorder.houdiniAndSubPainter.Add(new ExMaidRecordConfig.AutomationPathPair
                {
                    hda = hdaPath, spp = sppPath
                });

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }


        public static string TryGetSppPath(string hdaPath)
        {
            var path = PathOfRecorder;
            var recorder = AssetDatabase.LoadAssetAtPath<ExMaidRecordConfig>(path);

            if (recorder != null)
                foreach (var p in recorder.houdiniAndSubPainter)
                    if (p.hda == hdaPath)
                        return p.spp;
            return string.Empty;
        }

        public static void ClearRecorder()
        {
            
        }
    }
}