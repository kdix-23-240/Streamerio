using Common.Audio;
using Common.UI.Loading;
using Cysharp.Threading.Tasks;
using VContainer;
using VContainer.Unity;

namespace OutGame.Title.UI
{
    public class TitleBooster: IStartable
    {
        private readonly IAudioFacade _audioFacade;
        private readonly ILoadingScreen _loadingScreen;
        private readonly ITitleScreen _titleScreen;
        
        [Inject]
        public TitleBooster(IAudioFacade audioFacade, ILoadingScreen loadingScreen, ITitleScreen titleScreen)
        {
            _audioFacade = audioFacade;
            _loadingScreen = loadingScreen;
            _titleScreen = titleScreen;
        }
        
        public void Start()
        {
            _audioFacade.PlayAsync(BGMType.kuraituuro).Forget();
            _titleScreen.Show();
            
            _loadingScreen.HideAsync(default).Forget();
        }
    }
}