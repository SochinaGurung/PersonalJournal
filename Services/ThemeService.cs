namespace Coursework.Services
{
    public class ThemeService
    {
        public event Action? OnThemeChanged;
        
        private string _currentTheme = "light";
        
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
        
        public bool IsDarkMode => CurrentTheme == "dark";
        
        public void ToggleTheme()
        {
            CurrentTheme = CurrentTheme == "light" ? "dark" : "light";
        }
        
        public void SetTheme(string theme)
        {
            if (theme == "light" || theme == "dark")
            {
                CurrentTheme = theme;
            }
        }
    }
}
