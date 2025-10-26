using Common.UI.Part.Button;
using VContainer;

namespace Common.UI.Display.Window.Book
{
    public class BookWindowView: WindowViewBase, IBookWindowView
    {
        private ICommonButton _nextButton;
        public ICommonButton NextButton => _nextButton;

        private ICommonButton _backButton;
        public ICommonButton BackButton => _backButton;

        [Inject]
        public void Construct(
            [Key(ButtonType.NextPage)] ICommonButton nextButton,
            [Key(ButtonType.BackPage)] ICommonButton backButton)
        {
            _nextButton = nextButton;
            _backButton = backButton;
        }
    }
    
    public interface IBookWindowView : IWindowView
    {
        ICommonButton NextButton { get; }
        ICommonButton BackButton { get; }
    }
}