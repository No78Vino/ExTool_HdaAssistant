using System.Text;
using HoudiniEngineUnity;
using UnityEditor;
using UnityEngine;

namespace EXTool
{
    public class ExMaidSettingWindow : EditorWindow
    {
        private string houdiniExePath;
        private int serverPort;
        private string subPtExePath;
        private string pathOfHouModule;
        private string pathOfSitePackages;
        private bool usingRecorder;
        private string pathOfRecorder;
        
        private void OnEnable()
        {
            // Read Local Setting Data
            houdiniExePath = ExMaidSetting.HoudiniPath;
            subPtExePath = ExMaidSetting.SubPtPath;
            serverPort = ExMaidSetting.Port;
            pathOfHouModule = ExMaidSetting.PathOfHouModule;
            pathOfSitePackages = ExMaidSetting.PathOfSitePackages;
            usingRecorder = ExMaidSetting.UsingRecorder;
            pathOfRecorder = ExMaidSetting.PathOfRecorder;
        }

        private void OnGUI()
        {
            OnGUIDrawHoudiniSetting();
            GUILayout.Space(10);
            OnGUIDrawSubPainterSetting();
            GUILayout.Space(10);
            OnGUIDrawSocketSetting();
            GUILayout.Space(10);
            OnGUIExMaid();
            
            GUILayout.FlexibleSpace();
            var buttonStyleClearScene = new GUIStyle(GUI.skin.button)
            {
                fontSize = 17,
                fontStyle = FontStyle.Bold,
                border = new RectOffset(2, 2, 5, 5),
            };
            if (GUILayout.Button("Save",buttonStyleClearScene)) SaveDirectories();
        }

        private void OnGUIDrawSubPainterSetting()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Substance Painter", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("'Substance Painter.exe' Path:", GUILayout.Width(200));
            subPtExePath = EditorGUILayout.TextField(subPtExePath);
            if (GUILayout.Button("Browse", GUILayout.Width(80)))
                subPtExePath =
                    EditorUtility.OpenFilePanel("Select 'Substance Painter.exe' Path", "", "exe");
            GUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        ///     Draw Local Socket Server Setting
        /// </summary>
        private void OnGUIDrawSocketSetting()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            GUILayout.Label("Unity Server Setting", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Server Port:", GUILayout.Width(150));
            serverPort = EditorGUILayout.IntField(serverPort);
            GUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void OnGUIDrawHoudiniSetting()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Houdini", EditorStyles.boldLabel);
            
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("  Houdini: {0}.{1}.{2}\n",
                HEU_HoudiniVersion.HOUDINI_MAJOR,
                HEU_HoudiniVersion.HOUDINI_MINOR,
                HEU_HoudiniVersion.HOUDINI_BUILD);
            sb.AppendFormat("  Houdini Engine: {0}.{1}.{2}\n",
                HEU_HoudiniVersion.HOUDINI_ENGINE_MAJOR,
                HEU_HoudiniVersion.HOUDINI_ENGINE_MINOR,
                HEU_HoudiniVersion.HOUDINI_ENGINE_API);
            
            GUILayout.Label($"Version:\n{sb}", EditorStyles.largeLabel);
            
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("'Houdini.exe' Path:", GUILayout.Width(200));
            houdiniExePath = EditorGUILayout.TextField(houdiniExePath);
            if (GUILayout.Button("Browse", GUILayout.Width(80)))
                houdiniExePath =
                    EditorUtility.OpenFilePanel("Select 'Houdini.exe' Path", "", "exe");
            GUILayout.EndHorizontal();
            
            GUILayout.Space(5);
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            GUILayout.Label("Path of Python Module", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("hou:", GUILayout.Width(200));
            pathOfHouModule = EditorGUILayout.TextField(pathOfHouModule);
            if (GUILayout.Button("Browse", GUILayout.Width(80)))
                pathOfHouModule =
                    EditorUtility.OpenFilePanel("Select 'Python Module (hou)' Path", "", "");
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Site-Packages:", GUILayout.Width(200));
            pathOfSitePackages = EditorGUILayout.TextField(pathOfSitePackages);
            if (GUILayout.Button("Browse", GUILayout.Width(80)))
                pathOfSitePackages =
                    EditorUtility.OpenFilePanel("Select 'Python Module (Site-Packages)' Path", "", "");
            GUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.EndVertical();
        }

        void OnGUIExMaid()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            GUILayout.Label("EX Maid", EditorStyles.boldLabel);
            
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Using Recorder':", GUILayout.Width(200));
            usingRecorder = EditorGUILayout.Toggle(usingRecorder);
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Path Of 'Maid's Recorder':", GUILayout.Width(200));
            pathOfRecorder = EditorGUILayout.TextField(pathOfRecorder);
            GUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }
        
        [MenuItem("EXTool/EX Maid Setting",priority = 0)]
        public static void ShowWindow()
        {
            GetWindow(typeof(ExMaidSettingWindow), false, "EX Maid Setting");
        }

        private void SaveDirectories()
        {
            EditorPrefs.SetString(ExMaidSetting.EditorPrefsNameOfHoudiniPath, houdiniExePath);
            EditorPrefs.SetString(ExMaidSetting.EditorPrefsNameOfSubPtPath, subPtExePath);
            EditorPrefs.SetInt(ExMaidSetting.EditorPrefsNameOfPort, serverPort);
            EditorPrefs.SetString(ExMaidSetting.EditorPrefsNameOfHouModule, pathOfHouModule);
            EditorPrefs.SetString(ExMaidSetting.EditorPrefsNameOfSitePackages, pathOfSitePackages);
            EditorPrefs.SetBool(ExMaidSetting.EditorPrefsNameOfUsingRecorder, usingRecorder);
            EditorPrefs.SetString(ExMaidSetting.EditorPrefsNameOfRecorderPath, pathOfRecorder);

            Debug.Log("[EXTool] Automation Setting Saved!");
        }
    }
}