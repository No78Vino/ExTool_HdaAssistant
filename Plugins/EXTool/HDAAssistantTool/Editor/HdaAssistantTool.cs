using System.Collections.Generic;
using System.IO;
using HoudiniEngineUnity;
using Unity.Serialization.Json;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Scripting.Python;
using UnityEngine;
using UnityEngine.SceneManagement;
using EventType = UnityEngine.EventType;

namespace EXTool
{

    public class HdaAssistantTool : EditorWindow
    {
        private static readonly float buttonColumnWidth = 300f;
        private string _exportBakePath;
        private GameObject _hdaGameObject;
        private Object _hdaObject;

        private string _hdaPath;
        private HEU_HoudiniAssetRoot _houdiniAssetRoot; // 存储HEU_HoudiniAssetRoot
        private Editor _houdiniAssetRootEditor; // HEU_HoudiniAssetRoot的Inspector编辑器

        private string _previousScenePath;
        private GameObject _replacedGameObject;
        private Vector2 _scrollPosition;

        private string _sppPath;

        private void OnEnable()
        {
            _previousScenePath = SceneManager.GetActiveScene().path;
            ResetScene();
        }

        private void OnDisable()
        {
            EditorSceneManager.OpenScene(_previousScenePath);
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            OnGUI_Left_FunctionButtonGroup();
            EditorGUILayout.Separator();
            OnGUI_Right_HDAInspector();
            EditorGUILayout.EndHorizontal();
        }

        [MenuItem("EXTool/Houdini/HDA Assistant Tool")]
        public static void ShowWindow()
        {
            var session = HEU_SessionManager.GetOrCreateDefaultSession();
            if (!session.IsSessionValid())
            {
                HEU_SessionManager.CloseAllSessions();
                session = HEU_SessionManager.GetOrCreateDefaultSession();
            }

            if (session.ConnectionState == SessionConnectionState.CONNECTED)
            {
                GetWindow<HdaAssistantTool>(false, "HDA Manager");
            }
            else if (session.ConnectionState == SessionConnectionState.FAILED_TO_CONNECT)
            {
                var retry =
                    EditorUtility.DisplayDialog("Warning",
                        "SESSION FAILED TO CONNECT! \nMake Sure The HOUDINI is INSTALLED!",
                        "Retry", "cancel");
                if (retry) ShowWindow();
            }
            else if (session.ConnectionState == SessionConnectionState.NOT_CONNECTED)
            {
                WaitingForSessionConnection();
            }
        }

        private static void WaitingForSessionConnection()
        {
            EditorUtility.DisplayCancelableProgressBar("Waiting For SESSION CONNECTION...",
                "Please wait.", 0f);

            var startTime = Time.realtimeSinceStartup;
            var duration = 10f; // Time Out 

            while (Time.realtimeSinceStartup - startTime < duration)
            {
                var progress = (Time.realtimeSinceStartup - startTime) / duration;
                var canncel =
                    EditorUtility.DisplayCancelableProgressBar("Waiting For SESSION CONNECTION...",
                        "Please wait.", progress);
                var session = HEU_SessionManager.GetOrCreateDefaultSession();
                if (canncel ||
                    session.ConnectionState == SessionConnectionState.CONNECTED ||
                    session.ConnectionState == SessionConnectionState.FAILED_TO_CONNECT)
                    break;
            }

            EditorUtility.ClearProgressBar();

            var sessionResult = HEU_SessionManager.GetOrCreateDefaultSession();
            if (sessionResult.ConnectionState == SessionConnectionState.CONNECTED)
            {
                GetWindow(typeof(HdaAssistantTool), true, "HDA Asset Manager", true);
            }
            else if (sessionResult.ConnectionState == SessionConnectionState.FAILED_TO_CONNECT)
            {
                var retry =
                    EditorUtility.DisplayDialog("Warning",
                        "SESSION FAILED TO CONNECT! \nMake Sure The HOUDINI is RUNNING!",
                        "Retry", "cancel");
                if (retry) ShowWindow();
            }
            else
            {
                EditorUtility.DisplayDialog("Warning", "SESSION CONNECTION TIME OUT! ", "confirm");
            }
        }

        private void ImportHda(string hdaPath)
        {
            // Check Hda Path
            if (string.IsNullOrEmpty(hdaPath) || !File.Exists(hdaPath) ||
                !hdaPath.EndsWith(".hda") && !hdaPath.EndsWith(".hdalc"))
            {
                Debug.LogError("Invalid HDA file!");
                ShowNotification(new GUIContent("Invalid HDA file!"), 2);
                return;
            }

            _hdaPath = hdaPath;
            _hdaGameObject = HEU_HAPIUtility.InstantiateHDA(_hdaPath, Vector3.zero,
                HEU_SessionManager.GetOrCreateDefaultSession(), true);

            if (_hdaGameObject)
            {
                _hdaGameObject.transform.localPosition = Vector3.zero;
                _hdaGameObject.transform.localRotation = Quaternion.identity;
                _houdiniAssetRoot = _hdaGameObject.GetComponent<HEU_HoudiniAssetRoot>();
            }
        }

        private static void ResetScene()
        {
            var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            SceneManager.SetActiveScene(newScene);
        }

        /// <summary>
        ///     左侧功能按钮栏布局
        /// </summary>
        private void OnGUI_Left_FunctionButtonGroup()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(buttonColumnWidth));

            OnGUI_LoadHDA();
            EditorGUILayout.Separator();

            if (_houdiniAssetRootEditor)
            {
                var buttonStyleBake = new GUIStyle(GUI.skin.button)
                {
                    fontSize = 15,
                    fontStyle = FontStyle.Bold,
                    border = new RectOffset(2, 2, 5, 5),
                    normal = new GUIStyleState {textColor = Color.white},
                    hover = new GUIStyleState {textColor = Color.green}
                };

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Bake Path:",
                    new GUIStyle(GUI.skin.label) {alignment = TextAnchor.MiddleLeft},
                    GUILayout.ExpandWidth(true));
                _exportBakePath =
                    EditorGUILayout.TextField("", _exportBakePath, GUILayout.ExpandWidth(true));
                EditorGUILayout.Separator();
                if (GUILayout.Button("Bake", buttonStyleBake)) BakeHda(_exportBakePath);
                EditorGUILayout.EndVertical();

                EditorGUILayout.Space(10);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Replace Target:",
                    new GUIStyle(GUI.skin.label) {alignment = TextAnchor.MiddleLeft}, GUILayout.ExpandWidth(true));
                _replacedGameObject =
                    EditorGUILayout.ObjectField("", _replacedGameObject,
                        typeof(GameObject), true, GUILayout.ExpandWidth(true)) as GameObject;

                if (GUILayout.Button("Replace Prefab/GameObject", buttonStyleBake)) ReplaceObject(_replacedGameObject);
                EditorGUILayout.EndVertical();
                
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Spp Path:",
                    new GUIStyle(GUI.skin.label) {alignment = TextAnchor.MiddleLeft},
                    GUILayout.ExpandWidth(true));
                _sppPath =
                    EditorGUILayout.TextField("", _sppPath, GUILayout.ExpandWidth(true));
                EditorGUILayout.Separator();
                if (GUILayout.Button("Prepare Spp", buttonStyleBake)) PrepareSpp();
                
                EditorGUILayout.Space(10);
                
                EditorGUILayout.LabelField("Folder Name:",
                    new GUIStyle(GUI.skin.label) {alignment = TextAnchor.MiddleLeft},
                    GUILayout.ExpandWidth(true));
                ProceduralModelingAutomation.FolderName =
                    EditorGUILayout.TextField("", ProceduralModelingAutomation.FolderName, GUILayout.ExpandWidth(true));
                EditorGUILayout.Separator();
                
                if (GUILayout.Button("Product Prefab", buttonStyleBake)) ProductHdaPrefab();
                EditorGUILayout.EndVertical();
            }

            GUILayout.FlexibleSpace();
            var buttonStyleClearScene = new GUIStyle(GUI.skin.button)
            {
                fontSize = 17,
                fontStyle = FontStyle.Bold,
                border = new RectOffset(2, 2, 5, 5),
                normal = new GUIStyleState {textColor = Color.white},
                hover = new GUIStyleState {textColor = Color.red}
            };
            if (GUILayout.Button("Reset", buttonStyleClearScene)) ResetScene();
            if (GUILayout.Button("HEngine Session Sync", buttonStyleClearScene)) OpenHEngineSessionSync();
            if (GUILayout.Button("Clear HDA Cache", buttonStyleClearScene)) ClearHdaCache();
            GUILayout.Space(10);
            EditorGUILayout.EndVertical();
        }

        private void OnGUI_LoadHDA()
        {
            EditorGUILayout.Space(10);
            if (!_houdiniAssetRootEditor)
            {
                if (GUILayout.Button("Load HDA File",
                    new GUIStyle(GUI.skin.button) {fontSize = 20, margin = new RectOffset(2, 2, 5, 5)}))
                {
                    var hdaPath = EditorUtility.OpenFilePanel("Select HDA File", Application.dataPath, "hda,hdalc");
                    if (!string.IsNullOrEmpty(hdaPath))
                    {
                        Debug.Log("Selected HDA file path: " + hdaPath);
                        ImportHda(hdaPath);
                        Debug.Log("Load HDA SUCCESS.  File path: " + hdaPath);
                    }
                }
            }
            else
            {
                EditorGUILayout.LabelField($"HDA:\n{_hdaPath}",
                    new GUIStyle(GUI.skin.box)
                    {
                        fontSize = 15, fontStyle = FontStyle.Bold, normal = new GUIStyleState {textColor = Color.white}
                    });
            }
        }

        /// <summary>
        ///     右侧HDA控制面板布局
        /// </summary>
        private void OnGUI_Right_HDAInspector()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("HDA Inspector",
                new GUIStyle(GUI.skin.label) {fontSize = 15, fontStyle = FontStyle.Bold});

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            // 显示HEU_HoudiniAssetRoot的Inspector面板
            if (_houdiniAssetRootEditor)
            {
                _houdiniAssetRootEditor.OnInspectorGUI();
            }
            else
            {
                EditorGUILayout.LabelField(
                    "NO HDA.\nPlease click the BUTTON<Load HDA File> in the top left corner to load the HDA file" +
                    "\nOr\nDrag And Drop the HDA file into the area below.",
                    new GUIStyle(GUI.skin.box) {fontSize = 15, fontStyle = FontStyle.Bold});
                EditorGUILayout.Space(5);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                var e = Event.current;
                var dropArea =
                    GUILayoutUtility.GetRect(0, 0, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                GUI.Box(dropArea, "Drop HDA Here",
                    new GUIStyle(GUI.skin.box)
                        {fontStyle = FontStyle.BoldAndItalic, fontSize = 20, alignment = TextAnchor.MiddleCenter});
                switch (e.type)
                {
                    case EventType.DragUpdated:
                    case EventType.DragPerform:
                        if (!dropArea.Contains(e.mousePosition)) break;
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                        if (e.type == EventType.DragPerform)
                        {
                            DragAndDrop.AcceptDrag();
                            ImportHda(DragAndDrop.paths[0]);
                        }

                        break;
                }

                EditorGUILayout.EndVertical();
                if (_houdiniAssetRoot) _houdiniAssetRootEditor = Editor.CreateEditor(_houdiniAssetRoot);
            }

            if (_houdiniAssetRoot == null) _houdiniAssetRootEditor = null;

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        ///     烘焙HDA到指定路径
        /// </summary>
        /// <param name="bakePath"></param>
        private void BakeHda(string bakePath)
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
                    Debug.Log("Deleted directory: " + bakePath);
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

            _houdiniAssetRoot.HoudiniAsset.BakeToNewPrefab(bakePath);
            Debug.Log($"Bake Success to the directory:{bakePath}");
        }

        /// <summary>
        ///     替换指定GameObject
        /// </summary>
        /// <param name="replacedObject"></param>
        private void ReplaceObject(GameObject replacedObject)
        {
            if (replacedObject)
            {
                var confirm = EditorUtility.DisplayDialog("Warning", "You're going to Replaced the Object.Make Sure?",
                    "yes", "cancel");
                if (confirm)
                {
                    if (HEU_EditorUtility.IsPrefabAsset(replacedObject))
                        _houdiniAssetRoot.HoudiniAsset.BakeToExistingPrefab(replacedObject);
                    else
                        _houdiniAssetRoot.HoudiniAsset.BakeToExistingStandalone(replacedObject);
                }
            }
            else
            {
                EditorUtility.DisplayDialog("Warning", "Replaced Object is EMPTY!", "close");
            }
        }

        /// <summary>
        ///     清理HDA缓存
        /// </summary>
        [MenuItem("EXTool/Houdini/Clear Houdini Engine Asset Cache")]
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

        void OpenHEngineSessionSync()
        {
            var window = GetWindow(typeof(HEU_SessionSyncWindow), false, "HEngine SessionSync");
            window.ShowAuxWindow();
        }

        void PrepareSpp()
        {
            ProceduralModelingAutomation.PrepareSpp(_sppPath);
        }
        
        string HdaParameters()
        {
            HEU_SessionManager.GetOrCreateDefaultSession();
            HEU_HoudiniAssetRoot root = _houdiniAssetRoot;
            var heuParm = root.HoudiniAsset.Parameters;
            var datas = heuParm.GetParameters();
            Dictionary<string, object> parameterDict = new Dictionary<string, object>();

            // HAPI_PARMTYPE_MULTIPARMLIST,        
            // HAPI_PARMTYPE_PATH_FILE_GEO,        
            // HAPI_PARMTYPE_PATH_FILE_IMAGE,        
            // HAPI_PARMTYPE_PATH_FILE_DIR,        
            // HAPI_PARMTYPE_MAX,  

            foreach (var p in datas)
            {
                switch (p._parmInfo.type)
                {
                    case HAPI_ParmType.HAPI_PARMTYPE_INT:
                        if (p._intValues.Length > 1)
                        {
                            parameterDict.Add(p._name + 'x', p._intValues[0]);
                            parameterDict.Add(p._name + 'y', p._intValues[1]);
                            if (p._intValues.Length == 3)
                            {
                                parameterDict.Add(p._name + 'z', p._intValues[2]);
                            }
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
                            if (p._floatValues.Length == 3)
                            {
                                parameterDict.Add(p._name + 'z', p._floatValues[2]);
                            }
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
            }

            string json = JsonSerialization.ToJson(parameterDict);
            return json;
        }

        
        void ProductHdaPrefab()
        {
            string pythonCode = $"import sys; " +
                                $"sys.path.append(r'{AutomationSetting.PathOfHouModule}');" +
                                $"sys.path.append(r'{AutomationSetting.PathOfSitePackages}');";

            PythonRunner.RunString(pythonCode);

            string parameters = HdaParameters();
            string testLoadHdaNode =
                "import hrpyc;" +
                "connection, hou = hrpyc.import_remote_module();" +
                "obj_node = hou.node('/obj');" +
                "geo_node = obj_node.createNode('geo','geo_my_hda_node');" +
                $"hou.hda.installFile('{_hdaPath}');" +
                "hda_node_name = 'gpt_gen_shop_slogan_board';" +
                "hda_instance = geo_node.createNode(hda_node_name,'my_hda_node');" +
                $"hda_instance.setParms({parameters});" +
                "execution_param = hda_instance.parm('newparameter');" +
                "execution_param.pressButton();";
            PythonRunner.RunString(testLoadHdaNode);
        }
    }
}