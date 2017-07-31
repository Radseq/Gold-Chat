namespace Gold_Client.Model
{
    interface IConfiguration<T>
    {
        void SaveConfig(T item);
        T LoadConfig(T obj);
    }
}
