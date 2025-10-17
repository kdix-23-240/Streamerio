// モジュール概要:
// 共通ボタンの LifetimeScope を構築し、ボタン View と Presenter の依存を VContainer に登録する。
// 依存関係: UnityEngine.UI.Button/ObservableEventTrigger による入力、ClickEventBinder 経由の SE 再生、IAudioFacade を使用。
// 使用例: ボタンプレハブに本スコープを付与し、Dialog やメニュー画面の子スコープとして生成する。

using Alchemy.Inspector;
using Common.Audio;
using Common.UI.Click;
using R3;
using R3.Triggers;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Common.UI.Part.Button
{
    /// <summary>
    /// 【目的】共通ボタンに必要な依存を登録し、Presenter と View を配線する。
    /// 【理由】プレハブ化したボタンを生成した際に、DI コンテナが即座に入力処理とアニメーションを利用できるようにするため。
    /// </summary>
    [RequireComponent(typeof(UnityEngine.UI.Button), typeof(ObservableEventTrigger))]
    public class ButtonLifetimeScope: LifetimeScope
    {
        /// <summary>
        /// 【目的】入力イベントの発生源となる Unity ボタンを Inspector で参照する。
        /// 【理由】Wiring 時に Button を Resolver へ渡し、Presenter が直接アクセスできるようにするため。
        /// </summary>
        [SerializeField, ReadOnly]
        private UnityEngine.UI.Button _button;
        /// <summary>
        /// 【目的】ボタンの GameObject 自体をキャッシュする。
        /// 【理由】Presenter から SetActive など GameObject 操作を行う際に即座に参照できるようにするため。
        /// </summary>
        [SerializeField, ReadOnly]
        private GameObject _gameObject;
        
        /// <summary>
        /// 【目的】Pointer イベントを Observable として提供するトリガーを保持する。
        /// 【理由】IClickEventBinder と Presenter が購読し、押下/ホバーを非同期で扱えるようにするため。
        /// </summary>
        [SerializeField, ReadOnly]
        private ObservableEventTrigger _eventTrigger;
        /// <summary>
        /// 【目的】ボタン押下時に再生する効果音を指定する。
        /// 【理由】UI フィードバックを統一し、コンポーネント再利用時にも SE 設定が引き継がれるようにするため。
        /// </summary>
        [SerializeField, Tooltip("ボタン押下時に再生するSE")]
        private SEType _clickSE = SEType.NESRPG0112;
        
        /// <summary>
        /// 【目的】DI 登録した View インスタンスをキャッシュする。
        /// 【理由】Configure 内で二度取得するコストを避け、Wiring で渡す参照を明示的に保持する。
        /// </summary>
        private IButtonView _view;

#if UNITY_EDITOR
        /// <summary>
        /// 【目的】エディタ上で必須参照を自動設定し、設定漏れを防ぐ。
        /// 【理由】プレハブ複製時にフィールドが外れても OnValidate で補完し、実行時例外を避けるため。
        /// </summary>
        private void OnValidate()
        {
            _button ??= GetComponent<UnityEngine.UI.Button>();
            _eventTrigger ??= GetComponent<ObservableEventTrigger>();
            _gameObject = gameObject;
        }
#endif
        /// <summary>
        /// 【目的】共通ボタンに必要な依存登録と Wiring 設定を行う。
        /// 【処理概要】
        ///   1. View を Component 経由で取得し、IButtonView/IInitializable として登録。
        ///   2. クリックイベントを ClickEventBinder でラップし、効果音付きで Presenter に渡す。
        ///   3. CommonButtonPresenter を Wiring で起動し、ボタン構成をコンテキストとして供給する。
        /// 【理由】ボタンを単体プレハブとして生成しても、Presenter が必要な依存を取りこぼさないようにするため。
        /// </summary>
        /// <param name="builder">【用途】依存登録を組み立てる VContainer のビルダー。</param>
        protected override void Configure(IContainerBuilder builder)
        {
            // Viewの登録
            _view = GetComponent<IButtonView>();
            builder
                .RegisterComponent(_view)
                .As<IButtonView>()
                .As<IInitializable>();
            
            builder
                .RegisterEntryPoint<Wiring<ICommonButton, CommonButtonContext>>()
                .WithParameter(resolver =>
                {
                    var audioFacade = resolver.Resolve<IAudioFacade>();
                    var binder = new ClickEventBinder<Unit>(_button.OnClickAsObservable(), audioFacade, _clickSE);
                    
                    return new CommonButtonContext
                    {
                        Button = _button,
                        EventTrigger = _eventTrigger,
                        ClickEventBinder = binder,
                        View = _view
                    };
                });
        }
    }
}
