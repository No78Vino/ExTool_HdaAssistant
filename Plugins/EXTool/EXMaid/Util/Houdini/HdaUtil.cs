using System.Collections.Generic;
using System.IO;
using HoudiniEngineUnity;
using Unity.Serialization.Json;
using UnityEditor;
using UnityEngine;

namespace EXTool.Util
{
    public static class HdaUtil
    {
        [MenuItem("EXTool/Houdini/Clear HDA Cache",priority = 1,secondaryPriority = 3)]
        public static void ClearHdaCache()
        {
            var confirm = EditorUtility.DisplayDialog("Warning",
                "Make Sure Clear HDA Cache?", "yes", "no");

            if (confirm)
            {
                var clearAll = EditorUtility.DisplayDialog("Warning",
                    "Clear ALL or Only Working Part?",
                    "ALL",
                    "Only Working Part");
                if (clearAll)
                {
                    var hdaCachePath = HEU_AssetDatabase.GetAssetCachePath();
                    FileUtil.DeleteFileOrDirectory(hdaCachePath);
                    AssetDatabase.Refresh();
                }
                else
                {
                    var hdaCacheWorkingPartPath = HEU_AssetDatabase.GetAssetWorkingPath();
                    FileUtil.DeleteFileOrDirectory(hdaCacheWorkingPartPath);
                    AssetDatabase.Refresh();
                }
            }
        }
        
        public static GameObject ImportHda(string hdaPath)
        {
            // Check Hda Path
            if (string.IsNullOrEmpty(hdaPath) || !File.Exists(hdaPath) ||
                !hdaPath.EndsWith(".hda") && !hdaPath.EndsWith(".hdalc"))
            {
                EXLog.Error("Invalid HDA file!");
                return null;
            }

            var hdaGameObject = HEU_HAPIUtility.InstantiateHDA(hdaPath, Vector3.zero,
                HEU_SessionManager.GetOrCreateDefaultSession(), true);

            if (hdaGameObject)
            {
                hdaGameObject.transform.localPosition = Vector3.zero;
                hdaGameObject.transform.localRotation = Quaternion.identity;
            }

            return hdaGameObject;
        }

        public static void BakeHda(string bakePath, HEU_HoudiniAssetRoot houdiniAssetRoot)
        {
            if (Directory.Exists(bakePath))
            {
                var confirm = EditorUtility.DisplayDialog("Warning",
                    "You're going to Replaced the Object.\nDo you wanna OVERWRITE or CREATE the folder?",
                    "OVERWRITE",
                    "CREATE");
                if (confirm)
                {
                    FileUtil.DeleteFileOrDirectory(bakePath);
                    AssetDatabase.Refresh();
                    EXLog.Log("Deleted directory: " + bakePath);
                }
                else
                {
                    for (var i = 0; i < 1000; i++)
                        if (!Directory.Exists(bakePath + "_" + i))
                        {
                            bakePath = bakePath + "_" + i;
                            break;
                        }
                }
            }

            houdiniAssetRoot.HoudiniAsset.BakeToNewPrefab(bakePath);
            EXLog.Log($"Bake Success to the directory:{bakePath}");
        }

        public static void HdaReplaceObject(GameObject replacedObject, HEU_HoudiniAssetRoot houdiniAssetRoot)
        {
            if (replacedObject)
            {
                var confirm = EditorUtility.DisplayDialog("Warning", "You're going to Replaced the Object.Make Sure?",
                    "yes", "cancel");
                if (confirm)
                {
                    if (HEU_EditorUtility.IsPrefabAsset(replacedObject))
                        houdiniAssetRoot.HoudiniAsset.BakeToExistingPrefab(replacedObject);
                    else
                        houdiniAssetRoot.HoudiniAsset.BakeToExistingStandalone(replacedObject);
                }
            }
            else
            {
                EditorUtility.DisplayDialog("Warning", "Replaced Object is EMPTY!", "close");
            }
        }

        public static string JsonOfHdaParameters(HEU_Parameters heuParm)
        {
            HEU_SessionManager.GetOrCreateDefaultSession();
            var data = heuParm.GetParameters();
            var parameterDict = new Dictionary<string, object>();

            // HAPI_PARMTYPE_MULTIPARMLIST,        
            // HAPI_PARMTYPE_PATH_FILE_GEO,        
            // HAPI_PARMTYPE_PATH_FILE_IMAGE,        
            // HAPI_PARMTYPE_PATH_FILE_DIR,        
            // HAPI_PARMTYPE_MAX,  

            foreach (var p in data)
                switch (p._parmInfo.type)
                {
                    case HAPI_ParmType.HAPI_PARMTYPE_INT:
                        if (p._intValues.Length > 1)
                        {
                            parameterDict.Add(p._name + 'x', p._intValues[0]);
                            parameterDict.Add(p._name + 'y', p._intValues[1]);
                            if (p._intValues.Length == 3) parameterDict.Add(p._name + 'z', p._intValues[2]);
                        }
                        else
                        {
                            parameterDict.Add(p._name, p._intValues[0]);
                        }

                        break;
                    case HAPI_ParmType.HAPI_PARMTYPE_TOGGLE:
                        parameterDict.Add(p._name, p._toggle);
                        break;
                    case HAPI_ParmType.HAPI_PARMTYPE_FLOAT:
                        if (p._floatValues.Length > 1)
                        {
                            parameterDict.Add(p._name + 'x', p._floatValues[0]);
                            parameterDict.Add(p._name + 'y', p._floatValues[1]);
                            if (p._floatValues.Length == 3) parameterDict.Add(p._name + 'z', p._floatValues[2]);
                        }
                        else
                        {
                            parameterDict.Add(p._name, p._floatValues[0]);
                        }

                        break;
                    case HAPI_ParmType.HAPI_PARMTYPE_COLOR:
                        parameterDict.Add(p._name, p._color);
                        break;
                    case HAPI_ParmType.HAPI_PARMTYPE_STRING:
                        parameterDict.Add(p._name, p._stringValues[0]);
                        break;
                    case HAPI_ParmType.HAPI_PARMTYPE_PATH_FILE:
                        parameterDict.Add(p._name, p._stringValues[0]);
                        break;
                    case HAPI_ParmType.HAPI_PARMTYPE_NODE:
                        parameterDict.Add(p._name, p._paramInputNode);
                        break;
                }

            var json = JsonSerialization.ToJson(parameterDict);
            return json;
        }
    }
}