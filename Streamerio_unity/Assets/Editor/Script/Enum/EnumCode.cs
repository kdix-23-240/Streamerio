using System.Text.RegularExpressions;
using Cysharp.Text;

namespace Infra
{
    public class EnumCode: ScriptCode
    {
        /// <summary>
        /// Enumのフォーマット
        /// <para>0: Enumのパラメータ名</para>
        /// <para>1: 数値</para>
        /// </summary>
        private const string EnumParameterFormat = "\t\t{0} = {1},\n";
        /// <summary>
        /// Enumのパラメターの正規表現(アルファベットか日本語か_で始まる)
        /// </summary>
        private const string EnumParameterRegex = @"^[^a-zA-Z\p{IsHiragana}\p{IsKatakana}\p{IsCJKUnifiedIdeographs}_]+";
        
        /// <summary>
        /// Enumのコードを取得
        /// </summary>
        /// <param name="parameters"> 設定するEnumパラメターリスト </param>
        /// <param name="nameSpace"> 名前空間名(ネストの場合は、「.」つなぎ) </param>
        /// <param name="fileName"> Enumのファイル名 </param>
        /// <returns></returns>
        public EnumCode(EnumParameter[] parameters, string nameSpace, string fileName)
        {
            using var builder = ZString.CreateStringBuilder();

            var enumName = ZString.Concat("public enum ", fileName);
            
            /* パラメータの内容を作成 */
            var contents = CreteContents(parameters);
                
            /* スクリプトの内容追加 */
            if (nameSpace == Empty)
            {
                builder.AppendFormat(ScriptFormat, "", enumName, contents);
            }
            else
            {
                builder.AppendFormat(NamespaceScriptFormat, nameSpace, "", enumName, contents);
            }
                
            Contents = builder.ToString();
        }

        /// <summary>
        /// スクリプトのコードをコピー
        /// </summary>
        /// <param name="code"></param>
        public EnumCode(EnumCode code): base(code)
        {
            
        }

        /// <summary>
        /// Enumのパラメータを生成
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private string CreteContents(EnumParameter[] parameters)
        {
            using var builder = ZString.CreateStringBuilder();
            
            foreach (var parameter in parameters)
            {
                builder.Append(FormatEnumParameter(parameter));
            }
                
            return builder.ToString();
        }

        /// <summary>
        /// Enumのパラメータがアルファベットか日本語か_から始まるように先頭の文字を削除
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        private string FormatEnumParameter(EnumParameter parameter)
        {
            var parameterName = Regex.Replace(parameter.Name, EnumParameterRegex, string.Empty);
            return ZString.Format(EnumParameterFormat, parameterName, parameter.Num);
        }
    }
}