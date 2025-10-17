using Common.Audio;
using Common.QRCode;
using Common.Save;
using Common.UI.Dialog;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GlobalLifetimeScope : LifetimeScope
{
    [SerializeField]
    private AudioMediator _audioMediator;
    
    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<IVolumeSaveFacade, IPlayDataSaveFacade, SaveManager>(Lifetime.Singleton);

        builder.RegisterComponent(_audioMediator);
        builder.Register<IAudioFacade, AudioFacade>(Lifetime.Scoped);
        
        builder.Register<IQRCodeService, QRCodeService>(Lifetime.Singleton)
            .WithParameter(new QRCodeSpriteFactory());

        builder.Register<IDialogService, DialogService>(Lifetime.Singleton);
    }
}
