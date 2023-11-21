using System;
using UnityEditor;

namespace EXTool
{
    public static class LocalServerManager
    {
        private static LocalServer LocalServer;
        
        [MenuItem("EXTool/Unity Local Server/Open",priority = 1,secondaryPriority = 0)]
        public static void OpenServer()
        {
            LocalServer ??= LocalServer.Create();
            LocalServer.OpenServer();
        }
        
        [MenuItem("EXTool/Unity Local Server/Close",priority = 1,secondaryPriority = 1)]
        public static void CloseServer()
        {
            LocalServer?.CloseServer();
        }
        
        public static void SubscribeHandler(string command,Action<string[]> method)
        {
            LocalServer?.Handler.Subscribe(command,method);
        }

        public static void UnsubscribeHandler(string command, Action<string[]> method)
        {
            LocalServer?.Handler.Unsubscribe(command,method);
        }

        public static bool IsServerOpen()
        {
            return LocalServer.IsOpening();
        }
    }
}