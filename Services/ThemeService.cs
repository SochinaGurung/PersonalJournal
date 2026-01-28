namespace Coursework.Services
{
    public class ThemeService
    {
        public const string Light = "light";
        public const string Dark = "dark";

        public event Action? OnThemeChanged;

        private string _currentTheme = Light;

        public string CurrentTheme
        {
            get => _currentTheme;
            set
            {
                if (_currentTheme != value)
                {
                    _currentTheme = value;
                    OnThemeChanged?.Invoke();
                }
            }
        }

        public bool IsDarkMode => CurrentTheme == Dark;

        public void ToggleTheme()
        {
            CurrentTheme = IsDarkMode ? Light : Dark;
        }

        public void SetTheme(string theme)
        {
            if (theme == Light || theme == Dark)
            {
                CurrentTheme = theme;
            }
        }
    }
}
