using UnityEngine;

namespace EXTool
{
    public class EXLog
    {
        static string AppendExMark(string msg) => $"[EXTool] {msg}";

        public static void Log(string msg)
        {
            msg = AppendExMark(msg);
            Debug.Log(msg);
        }
        
        public static void Warning(string msg)
        {
            msg = AppendExMark(msg);
            Debug.LogWarning(msg);
        }
        
        public static void Error(string msg)
        {
            msg = AppendExMark(msg);
            Debug.LogError(msg);
        }
    }
}