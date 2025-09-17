using System;
using System.Threading;
using ZLinq;

namespace Common
{
    /// <summary>
    /// 複数の CancellationToken を動的にリンクして、どれかがキャンセルされたら伝播させる
    /// </summary>
    public class LinkedCancellationToken : IDisposable
    {
        private CancellationTokenSource _cts;
        private readonly CancellationToken[] _initialTokens;

        public LinkedCancellationToken(params CancellationToken[] initialTokens)
        {
            _initialTokens = initialTokens ?? Array.Empty<CancellationToken>();
        }

        /// <summary>
        /// 初期トークンと追加トークンをリンクした CancellationToken を取得
        /// </summary>
        /// <param name="extraTokens">追加でリンクしたいトークン(指定なしの場合は追加しない)</param>
        /// <returns>リンクされたトークン</returns>
        public CancellationToken GetCancellationToken(params CancellationToken[] extraTokens)
        {
            // 以前のCancellationTokenを破棄
            _cts?.Dispose();

            // 結合
            var combinedTokens = _initialTokens
                .AsValueEnumerable()
                .Concat(extraTokens)
                .ToArray();

            _cts = CancellationTokenSource.CreateLinkedTokenSource(combinedTokens);

            return _cts.Token;
        }

        public void Dispose()
        {
            _cts?.Dispose();
            _cts = null;
        }
    }
}