using Common.Audio;
using Common.QRCode;
using Common.Save;
using Common.UI.Dialog;
using VContainer;
using VContainer.Unity;

public class GlobalLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<IVolumeSaveFacade, IPlayDataSaveFacade, SaveManager>(Lifetime.Singleton);

        builder.Register<IAudioFacade, AudioFacade>(Lifetime.Singleton);
        
        builder.Register<IQRCodeService, QRCodeService>(Lifetime.Singleton)
            .WithParameter(new QRCodeSpriteFactory());

        builder.Register<IDialogService, DialogService>(Lifetime.Singleton);
    }
}
