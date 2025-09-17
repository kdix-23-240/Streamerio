using System;
using System.Collections.Generic;
using Unity.Plastic.Newtonsoft.Json;
using ZLinq;

namespace Infra
{
    public static class EnumUtil
    {
        /// <summary>
        /// Enumを生成
        /// </summary>
        /// <param name="parameterNames"></param>
        /// <param name="nameSpace"></param>
        /// <param name="fileName"></param>
        /// <param name="root"></param>
        public static void CreateEnum(string[] parameterNames, string nameSpace, string fileName, string root)
        {
            var jsonPath = JsonUtil.GetJsonFilePath(fileName, root);
            var parameters = CreateParameter(parameterNames, jsonPath);
            
            JsonUtil.SaveJsonFile(parameters, jsonPath);
            SaveEnumScriptFile(parameters, nameSpace, fileName, root);
        }

        /// <summary>
        /// パラメータを作る
        /// </summary>
        /// <param name="parameterNames"></param>
        /// <param name="jsonPath"></param>
        /// <returns></returns>
        private static EnumParameter[] CreateParameter(string[] parameterNames, string jsonPath)
        {
            var preParameters = JsonUtil.ReadJsonFile<EnumParameter[]>(jsonPath);
            Dictionary<string, int> preParameterDict = new Dictionary<string, int>();
            int nextNum = 0;
            
            if (preParameters != null)
            {
                preParameterDict = preParameters
                    .AsValueEnumerable()
                    .ToDictionary(name => name.Name, param => param.Num);   
                nextNum = preParameters.AsValueEnumerable().Max(param => param.Num);
            }

            int length = parameterNames.Length;
            var parameters = new EnumParameter[length];

            for (int i = 0; i < length; i++)
            {
                parameters[i] = new EnumParameter(parameterNames[i], 
                    preParameterDict.ContainsKey(parameterNames[i]) ? 
                        preParameterDict[parameterNames[i]] : 
                        ++nextNum);
            }

            return parameters;
        }

        /// <summary>
        /// Enumスクリプトファイルを保存
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="nameSpace"></param>
        /// <param name="fileName"></param>
        /// <param name="root"></param>
        private static void SaveEnumScriptFile(EnumParameter[] parameters, string nameSpace, string fileName, string root)
        {
            var code = new EnumCode(parameters, nameSpace, fileName);
            var script = new Script(root, fileName, code);
            
            ScriptUtil.SaveScript(script);
        }
        
        /// <summary>
        /// Enum名からEnumの型を取得
        /// </summary>
        /// <param name="enumName">enum名</param>
        /// <parma name="nameSpace">名前空間</parma>
        /// <returns></returns>
        public static Type GetEnumType(string enumName, string nameSpace)
        {
            try
            {
                return AppDomain.CurrentDomain
                    .GetAssemblies()
                    .AsValueEnumerable()
                    .SelectMany(a => a.GetTypes())
                    .FirstOrDefault(t => t.IsEnum && t.Name == enumName && t.Namespace == nameSpace);
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
                return null;
            }
        }
    }
    
    public class EnumParameter
    {
        public string Name { get; }
        public int Num { get; }

        [JsonConstructor]
        public EnumParameter(string name, int num)
        {
            Name = name;
            Num = num;
        }
    }
}