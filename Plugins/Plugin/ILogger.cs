namespace FileFlows.Plugin
{
    public interface ILogger
    {
        void ILog(params object[] args);
        void DLog(params object[] args);
        void WLog(params object[] args);
        void ELog(params object[] args);
    }
    public enum LogType
    {
        Error, Warning, Info, Debug
    }

}