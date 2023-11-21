using System;
using System.Collections.Generic;

namespace EXTool
{
    public class LocalServerReceiveEvent
    {
        private string _command;
        private event Action<string[]> OnReceivedEvent;

        public LocalServerReceiveEvent(string command)
        {
            _command = command;
        }

        public string Command => _command;

        public void Invoke(string[] parameters)
        {
            OnReceivedEvent?.Invoke(parameters);
        }

        public void Subscribe(Action<string[]> method)
        {
            if (IsMethodAlreadyRegistered(method))
            {
                EXLog.Warning($"Subscribe Repeated Method! => Command:{Command}   Method:{method.Target}.{method.Method.Name}");
            }
            else
            {
                OnReceivedEvent += method;
            }
        }

        public void Unsubscribe(Action<string[]> method)
        {
            OnReceivedEvent -= method;
        }

        public void Clear()
        {
            OnReceivedEvent = null;
        }
        
        private bool IsMethodAlreadyRegistered(Action<string[]> method)
        {
            if (OnReceivedEvent != null)
            {
                foreach (Delegate existingDelegate in OnReceivedEvent.GetInvocationList())
                {
                    if (existingDelegate.Method == method.Method && existingDelegate.Target == method.Target)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
    
    public class ReceivedHandler
    {
        private readonly Dictionary<string, LocalServerReceiveEvent> OnReceivedHandler;

        public ReceivedHandler()
        {
            OnReceivedHandler = new Dictionary<string, LocalServerReceiveEvent>();
        }
        
        public static ReceivedHandler Create()
        {
            return new ReceivedHandler();
        }
        
        public void Trigger(string command,string[] parameters)
        {
            if (OnReceivedHandler.ContainsKey(command))
            {
                EXLog.Log($"Trigger Received Handler => Command:{command}");
                OnReceivedHandler[command].Invoke(parameters);
            }
        }

        public void Subscribe(string command,Action<string[]> method)
        {
            if (!OnReceivedHandler.ContainsKey(command))
            {
                OnReceivedHandler[command] = new LocalServerReceiveEvent(command);
            }

            OnReceivedHandler[command].Subscribe(method);
        }

        public void Unsubscribe(string command, Action<string[]> method)
        {
            if (method == null)
            {
                OnReceivedHandler[command].Clear();
                OnReceivedHandler.Remove(command);
            }
            else
            {
                OnReceivedHandler[command].Unsubscribe(method);
            }
        }

        public void Clear()
        {
            foreach (var kv in OnReceivedHandler)
            {
                kv.Value.Clear();
            }
            OnReceivedHandler.Clear();
        }
    }
}