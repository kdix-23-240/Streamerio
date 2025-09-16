using System.IO;
using Cysharp.Text;
using System.Linq;

namespace Infra
{
    /// <summary>
    /// スクリプト生成時に必要なデータ
    /// </summary>
    public class Script
    {
        /* パラメータ */
        public readonly string Name;
        public readonly string FilePath;
        public readonly ScriptCode Code;
        
        public Script(string root, string fileName, ScriptCode code)
        {
            Name = fileName;
            FilePath = Path.Combine(root, ScriptUtil.Extention.ToFileName(fileName));
            Code = new ScriptCode(code);
        }
    }

    /// <summary>
    /// スクリプトのコードの基底クラス
    /// </summary>
    public class ScriptCode
    {
        /// <summary>
        /// スクリプトの内容
        /// </summary>
        public string Contents { get; protected set; }
        
        /// <summary>
        /// 設定なし
        /// </summary>
        protected const string Empty = "";
        
        /* スクリプト名関連 */
        protected const string ScriptNameFormat = "public {0} {1}";

        /* コードのフォーマット */
        /// <summary>
        /// <para>0: 属性(ex: [Serializable])</para>
        /// <para>1: 宣言(ex: public class Child(): Parent)</para>
        /// <para>2: スクリプトの内容(ex: int x =10;)</para>
        /// </summary>
        protected const string ScriptFormat = @"{0}
{1}
{{
{2}
}}
";
        
        /// <summary>
        /// <para>0: 名前空間(ex: Common)</para>
        /// <para>1: 属性(ex: [Serializable])</para>
        /// <para>2: 宣言(ex: public class Child(): Parent)</para>
        /// <para>3: スクリプトの内容(ex: int x =10;)</para>
        /// </summary>
        protected const string NamespaceScriptFormat = @"namespace {0}
{{
    {1}
    {2}
    {{
{3}
    }}
}}
";

        public ScriptCode()
        {
            
        }
        
        /// <summary>
        /// スクリプトのコードをコピー
        /// </summary>
        /// <param name="code"></param>
        public ScriptCode(ScriptCode code)
        {
            Contents = code.Contents;
        }
    }

    
    /// <summary>
    /// using部分
    /// </summary>
    public class UsingModule
    {
        public readonly string[] UsingModules;
        /// <summary>
        /// スクリプトに書き込む内容
        /// </summary>
        public readonly string Contents;
        
        private const string UsingFormat = "using {0};\n";
        
        /// <summary>
        /// スクリプトのusing部分を生成する
        /// </summary>
        /// <param name="usingModules"></param>
        /// <returns></returns>
        public UsingModule(params string[] usingModules)
        {
            UsingModules = usingModules;
            
            using (var builder = ZString.CreateStringBuilder())
            {
                foreach (var module in usingModules)
                {
                    builder.AppendFormat(UsingFormat, module);
                }
                
                Contents = builder.ToString();
            }
        }

        /// <summary>
        /// コピーする
        /// </summary>
        /// <param name="usingModule"></param>
        public UsingModule(UsingModule usingModule)
        {
            UsingModules = usingModule.UsingModules;
            Contents = usingModule.Contents;
        }

        /// <summary>
        /// usingを追加
        /// </summary>
        /// <param name="usingModule"></param>
        /// <returns></returns>
        public UsingModule Add(UsingModule usingModule)
        {
            var modules = usingModule.UsingModules.Concat(UsingModules).ToArray();
            return new UsingModule(modules);
        }
    }
}