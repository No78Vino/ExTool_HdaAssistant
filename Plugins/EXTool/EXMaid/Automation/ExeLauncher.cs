using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace EXTool
{
    public class ExeLauncher
    {
        public enum ExeType
        {
            SubPainter,
            Houdini
        }

        private static readonly Dictionary<ExeType, string> ExePathMapping = new Dictionary<ExeType, string>
        {
            [ExeType.SubPainter] = ExMaidSetting.EditorPrefsNameOfSubPtPath,
            [ExeType.Houdini] = ExMaidSetting.EditorPrefsNameOfHoudiniPath
        };

        private static readonly Dictionary<ExeType, string> ExeCmdPathMapping = new Dictionary<ExeType, string>
        {
            [ExeType.SubPainter] = "_HouSPAutoProcess/CommandSubPt/LaunchSubPt.ps1",
        };

        [MenuItem("EXTool/Substance Painter/Launch With Remote Scripting Mode")]
        public static void LaunchSubPtWithRemoteScriptingMode()
        {
            RunLaunchExePowerShellCommand(ExeType.SubPainter);
        }

        [MenuItem("EXTool/Houdini/Launch")]
        public static void LaunchHoudiniWithPipeSession()
        {
            var houdiniProcess = new Process();
            var houdiniPath = TryGetExePath(ExeType.Houdini);
            houdiniProcess.StartInfo.FileName = houdiniPath;
            if (!houdiniProcess.Start()) EXLog.Error("Launch Houdini FAIL!");

            EXLog.Log("Launch Houdini SUCCESS!");
        }

        /// <summary>
        ///     https://www.reddit.com/r/Unity3D/comments/gp7v0w/unity_3d_and_powershell/
        /// </summary>
        /// <param name="exeType"></param>
        /// <param name="extraCmdParameter"></param>
        private static void RunLaunchExePowerShellCommand(ExeType exeType, string extraCmdParameter = "")
        {
            var ps1ScriptPath = ExeCmdPathMapping[exeType];
            var shellCommandPath = Path.Combine(Application.dataPath, ps1ScriptPath);

            var exeAbsolutePath = TryGetExePath(exeType);
            if (string.IsNullOrEmpty(exeAbsolutePath))
                return;

            var startInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments =
                    $"-NoProfile -ExecutionPolicy unrestricted -file \"{shellCommandPath}\" -ExeAbsolutePath \"{exeAbsolutePath}\" {extraCmdParameter}",
                UseShellExecute = false,
                RedirectStandardOutput =
                    true //This option means it will take anything the process outputs and put it in test.StandardOutput ie from PS's "write-Output $user"
            };

            var process = Process.Start(startInfo); //creates a powershell process with the above start options
            process.WaitForExit(); //We need this because Start() simply launches the script it does not wait for it to finish or the below line would not have any data
            var
                result = process.StandardOutput
                    .ReadToEnd(); //read all of the output from the powershell process we created and ran
            EXLog.Log(result);
        }

        private static string TryGetExePath(ExeType exeType)
        {
            var exeEditorPrefsKey = ExePathMapping[exeType];
            var exePath = EditorPrefs.GetString(exeEditorPrefsKey, "");
            // Check Path 
            if (string.IsNullOrEmpty(exePath))
            {
                var openSetting = EditorUtility.DisplayDialog("Warning",
                    $"Path of '{exeType.ToString()}' is EMPTY! \nPlease,Set the Path of '{exeType.ToString()}'!",
                    "OpenSetting", "close");
                if (openSetting) ExMaidSettingWindow.ShowWindow();

                return null;
            }

            var dic = exePath.Split('/');
            var exeName = dic.Last();
            if (!exeName.EndsWith(".exe"))
            {
                var openSetting = EditorUtility.DisplayDialog("Warning",
                    $"Path of '{exeType.ToString()}' is INVALID! \nPlease,Check the Path of '{exeType.ToString()}'!",
                    "OpenSetting", "close");
                if (openSetting) ExMaidSettingWindow.ShowWindow();

                return null;
            }

            return exePath;
        }
    }
}