using Cysharp.Text;

namespace Infra
{
    /// <summary>
    /// クラスのスクリプトコード
    /// </summary>
    public class ClassCode: ScriptCode
    {
        private const string BaseClassFormat = ": {0}";

        /// <summary>
        /// スクリプトのコード生成
        /// </summary>
        /// <param name="name">ファイル名</param>
        /// <param name="usingModule">usingのコード</param>
        /// <param name="nameSpace">名前空間</param>
        /// <param name="attribute">クラスの属性</param>
        /// <param name="contents"> スクリプトの内容 </param>
        /// <param name="baseClassNames">基底クラスかインターフェース名(複数ある場合は、基底クラスから書く)</param>
        public ClassCode(string name,
            UsingModule usingModule = null, string nameSpace = Empty, Attribute attribute = null,
            string contents = Empty, params string[] baseClassNames)
        {
            using var builder = ZString.CreateStringBuilder();
            
            /* using追加 */
            if (usingModule is not null)
            {
                builder.Append(usingModule.Contents);
            }
                
            /* 属性 */
            var attributeContents = attribute is null ? string.Empty : attribute.Contents;
                
            /* スクリプトの名前 */
            var scriptName = GetScriptName(name, baseClassNames);
                
            /* スクリプトの内容追加 */
            if (nameSpace == Empty)
            {
                builder.AppendFormat(ScriptFormat, attributeContents, scriptName, contents);
            }
            else
            {
                builder.AppendFormat(NamespaceScriptFormat, nameSpace, attributeContents, scriptName, contents);
            }
                
            Contents = builder.ToString();
        }

        /// <summary>
        /// スクリプトのコードをコピー
        /// </summary>
        /// <param name="code"></param>
        public ClassCode(ClassCode code): base(code)
        {
            
        }

        /// <summary>
        /// スクリプト名の宣言部分を取得(classの宣言など)
        /// </summary>
        /// <param name="name"></param>
        /// <param name="baseClassNames">継承するクラス名</param>
        /// <returns></returns>
        private string GetScriptName(string name, params string[] baseClassNames)
        {
            using var builder = ZString.CreateStringBuilder();
            builder.AppendFormat(ScriptNameFormat, "class", name);

            if (baseClassNames is not null)
            {
                var baseClassNamesString = string.Join(", ", baseClassNames);
                builder.AppendFormat(BaseClassFormat, baseClassNamesString);
            }
                
            return builder.ToString();
        }
    }

    /// <summary>
    /// クラスの属性
    /// </summary>
    public class Attribute
    {
        public readonly string Contents;
        
        /* よく使うAttribute */
        public const string Serializable = "Serializable";
        /// <summary>
        /// スクリプタブルオブジェクトの生成メニュー
        /// <para>0: menuName</para>
        /// <para>1: fileName</para>
        /// </summary>
        public const string CreateAssetMenuFormat = "CreateAssetMenu(menuName = \"{0}\", fileName = \"{1}\")";

        public Attribute(params string[] attributeNames)
        {
            Contents = attributeNames.Length != 0 ?
                ZString.Concat("[", ZString.Join(", ", attributeNames), "]") : string.Empty;
        }
    }
}