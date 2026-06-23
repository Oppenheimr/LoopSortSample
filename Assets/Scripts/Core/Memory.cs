

namespace Core
{
    public static class Memory
    {
        public static void Initialize()
        {
            ObjectPoolManager.Initialize(CoreManager.Instance.transform);
            Audio.Initialize();
        }
        
        public static void Reset()
        {
            EventDispatcher.Reset();
            ObjectPooler.Reset();
            Audio.Reset();
        }
    }
}