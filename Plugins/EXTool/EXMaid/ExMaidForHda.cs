using System.Collections.Generic;
using System.IO;
using EXTool.Util;
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
    public class ExMaidForHda : EditorWindow
    {
        private static readonly float buttonColumnWidth = 300f;
        private string _exportBakePath;
        private GameObject _hdaGameObject;
        private Object _hdaObject;

        private string _hdaPath;
        private HEU_HoudiniAssetRoot _houdiniAssetRoot; // Cache of HEU_HoudiniAssetRoot
        private Editor _houdiniAssetRootEditor; // Inspector Editor of HEU_HoudiniAssetRoot

        private string _previousScenePath;
        private GameObject _replacedGameObject;
        private Vector2 _scrollPosition;

        private string _sppPath;
        private ExMaidSetting.MaidMode _maidMode;

        private  GUIStyle _buttonStyleA, _buttonStyleB;

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
            _buttonStyleA = new GUIStyle(GUI.skin.button)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                border = new RectOffset(2, 2, 5, 5),
                normal = new GUIStyleState {textColor = Color.white},
                hover = new GUIStyleState {textColor = Color.green}
            };

            _buttonStyleB = new GUIStyle(GUI.skin.button)
            {
                fontSize = 17,
                fontStyle = FontStyle.Bold,
                border = new RectOffset(2, 2, 5, 5),
                normal = new GUIStyleState {textColor = Color.white},
                hover = new GUIStyleState {textColor = Color.red}
            };
            
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
                GetWindow<ExMaidForHda>(false, "HDA Manager");
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
            var duration = 5f; // Time Out 

            while (Time.realtimeSinceStartup - startTime < duration)
            {
                var progress = (Time.realtimeSinceStartup - startTime) / duration;
                var cancel =
                    EditorUtility.DisplayCancelableProgressBar("Waiting For SESSION CONNECTION...",
                        "Please wait.", progress);
                var session = HEU_SessionManager.GetOrCreateDefaultSession();
                if (cancel ||
                    session.ConnectionState == SessionConnectionState.CONNECTED ||
                    session.ConnectionState == SessionConnectionState.FAILED_TO_CONNECT)
                    break;
            }

            EditorUtility.ClearProgressBar();

            var sessionResult = HEU_SessionManager.GetOrCreateDefaultSession();
            if (sessionResult.ConnectionState == SessionConnectionState.CONNECTED)
            {
                GetWindow(typeof(ExMaidForHda), true, "HDA Asset Manager", true);
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
            var hdaObj = HdaUtil.ImportHda(hdaPath);
            if (hdaObj == null)
            {
                ShowNotification(new GUIContent("Invalid HDA file!"), 2);
                return;
            }

            _hdaPath = hdaPath;
            _hdaGameObject = hdaObj;
            _houdiniAssetRoot = _hdaGameObject.GetComponent<HEU_HoudiniAssetRoot>();
        }

        private static void ResetScene()
        {
            var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            SceneManager.SetActiveScene(newScene);
        }

        void OnGUI_AutoForHouAndSubPt()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Spp Path:",
                new GUIStyle(GUI.skin.label) {alignment = TextAnchor.MiddleLeft},
                GUILayout.ExpandWidth(true));
            _sppPath =
                EditorGUILayout.TextField("", _sppPath, GUILayout.ExpandWidth(true));
            EditorGUILayout.Separator();
            if (GUILayout.Button("Prepare Spp", _buttonStyleA)) PrepareSpp();

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Folder Name:",
                new GUIStyle(GUI.skin.label) {alignment = TextAnchor.MiddleLeft},
                GUILayout.ExpandWidth(true));
            ProceduralModelingAutomation.FolderName =
                EditorGUILayout.TextField("", ProceduralModelingAutomation.FolderName, GUILayout.ExpandWidth(true));
            EditorGUILayout.Separator();

            if (GUILayout.Button("Product Prefab", _buttonStyleA)) ProductHdaPrefab();
            EditorGUILayout.EndVertical();
        }
        
        void OnGUI_ForNormal()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Bake Path:",
                new GUIStyle(GUI.skin.label) {alignment = TextAnchor.MiddleLeft},
                GUILayout.ExpandWidth(true));
            _exportBakePath =
                EditorGUILayout.TextField("", _exportBakePath, GUILayout.ExpandWidth(true));
            EditorGUILayout.Separator();
            if (GUILayout.Button("Bake", _buttonStyleA))
            {
                HdaUtil.BakeHda(_exportBakePath,_houdiniAssetRoot);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Replace Target:",
                new GUIStyle(GUI.skin.label) {alignment = TextAnchor.MiddleLeft}, GUILayout.ExpandWidth(true));
            _replacedGameObject =
                EditorGUILayout.ObjectField("", _replacedGameObject,
                    typeof(GameObject), true, GUILayout.ExpandWidth(true)) as GameObject;

            if (GUILayout.Button("Replace Prefab/GameObject", _buttonStyleA))
            {
                HdaUtil.HdaReplaceObject(_replacedGameObject,_houdiniAssetRoot);
            }
            EditorGUILayout.EndVertical();
        }
        
        private void OnGUI_Left_FunctionButtonGroup()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(buttonColumnWidth));

            OnGUI_LoadHDA();
            EditorGUILayout.Separator();

            if (_houdiniAssetRootEditor)
            {
                GUILayout.BeginHorizontal(EditorStyles.helpBox);
                EditorGUILayout.LabelField("EXMaid Mode",EditorStyles.boldLabel, GUILayout.Width(100));
                _maidMode = (ExMaidSetting.MaidMode) EditorGUILayout.EnumPopup(_maidMode);
                GUILayout.EndHorizontal();
                
                if (_maidMode == ExMaidSetting.MaidMode.Normal)
                {
                    OnGUI_ForNormal();
                }else if (_maidMode == ExMaidSetting.MaidMode.HouAndSubPt)
                {
                    OnGUI_AutoForHouAndSubPt();
                }
            }

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Reset", _buttonStyleB))
            {
                ResetScene();
            }
            if (GUILayout.Button("Clear HDA Cache", _buttonStyleB))
            {
                HdaUtil.ClearHdaCache();
            }
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
        
        private void PrepareSpp()
        {
            ProceduralModelingAutomation.PrepareSpp(_sppPath);
        }

        private void ProductHdaPrefab()
        {
            var pythonCode = "import sys; " +
                             $"sys.path.append(r'{ExMaidSetting.PathOfHouModule}');" +
                             $"sys.path.append(r'{ExMaidSetting.PathOfSitePackages}');";

            PythonRunner.RunString(pythonCode);

            var parameters = HdaUtil.JsonOfHdaParameters(_houdiniAssetRoot.HoudiniAsset.Parameters);
            var testLoadHdaNode =
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