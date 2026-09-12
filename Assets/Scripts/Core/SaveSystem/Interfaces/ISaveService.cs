namespace Core.SaveSystem.Interfaces
{
    public interface ISaveService
    {
        public void Save<T>(string key, T data);
        public T Load<T>(string key, T defaultValue = default);
        public bool Exists(string key);
        public void Delete(string key);
    }
}