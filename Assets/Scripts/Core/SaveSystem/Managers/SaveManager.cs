using System.IO;
using Core.SaveSystem.Interfaces;
using UnityEngine;

namespace Core.SaveSystem.Managers
{
    public class SaveManager : ISaveService
    {
        public void Save<T>(string key, T data)
        {
            var json = JsonUtility.ToJson(data, true);
            var path = GetPath(key);
            File.WriteAllText(path, json);
        }

        public T Load<T>(string key, T defaultValue = default)
        {
            var path = GetPath(key);
            if (!File.Exists(path))
            {
                return defaultValue;
            }
            var json = File.ReadAllText(path);
            return JsonUtility.FromJson<T>(json);
        }

        public bool Exists(string key)
        {
            return File.Exists(GetPath(key));
        }

        public void Delete(string key)
        {
            var path = GetPath(key);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        private string GetPath(string key)
        {
            return Path.Combine(Application.persistentDataPath, key + ".json");
        }
    }
}