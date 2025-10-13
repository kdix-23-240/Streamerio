using System;
using VContainer.Unity;

namespace Common
{
    /// <summary>
    /// <see cref="IInitializable"/> と <see cref="IDisposable"/> のライフサイクルに合わせて、
    /// Initialize 時に <see cref="IAttachable{TContext}.Attach(TContext)"/>、
    /// Dispose 時に <see cref="IAttachable{TContext}.Detach()"/> を呼び出す汎用 Wiring クラス。
    /// </summary>
    /// <typeparam name="TAttachable">
    /// <see cref="IAttachable{TContext}"/> を実装した接続対象。
    /// Presenter など、Attach/Detach の責務を持つクラスを想定します。
    /// </typeparam>
    /// <typeparam name="TContext">
    /// Attach 時に渡すコンテキスト情報の型。View や Model、初期化データなどを保持します。
    /// </typeparam>
    public sealed class Wiring<TAttachable, TContext> : IInitializable, IDisposable
        where TAttachable : IAttachable<TContext>
    {
        private readonly TAttachable _target;
        private readonly TContext _context;

        /// <summary>
        /// Wiring の新しいインスタンスを作成します。
        /// </summary>
        /// <param name="target">Attach/Detach の対象となるオブジェクト</param>
        /// <param name="context">Attach 時に渡すコンテキスト情報</param>
        /// <exception cref="ArgumentNullException">target が null の場合に発生します。</exception>
        public Wiring(TAttachable target, TContext context)
        {
            _target  = target ?? throw new ArgumentNullException(nameof(target));
            _context = context;
        }

        /// <summary>
        /// <see cref="IInitializable.Initialize"/> のタイミングで Attach を呼び出します。
        /// </summary>
        public void Initialize() => _target.Attach(_context);

        /// <summary>
        /// <see cref="IDisposable.Dispose"/> のタイミングで Detach を呼び出します。
        /// </summary>
        public void Dispose() => _target.Detach();
    }
}