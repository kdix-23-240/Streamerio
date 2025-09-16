using System;

/// <summary>
/// DEBUG_LOG_ONのシンボル設定時にのみ、Debugを使えるようにする
/// </summary>
public static class Debug
{
    private const string MCoditionalDefine = "DEBUG_LOG_ON";
        
    [System.Diagnostics.Conditional(MCoditionalDefine)]
    public static void Log(object message)
        => UnityEngine.Debug.Log(message);
        
    [System.Diagnostics.Conditional(MCoditionalDefine)]
    public static void LogWarning(object message)
        => UnityEngine.Debug.LogWarning(message);
        
    [System.Diagnostics.Conditional(MCoditionalDefine)]
    public static void LogError(object message)
        => UnityEngine.Debug.LogError(message);
        
    [System.Diagnostics.Conditional(MCoditionalDefine)]
    public static void LogException(Exception exception)
        => UnityEngine.Debug.LogException(exception);
}