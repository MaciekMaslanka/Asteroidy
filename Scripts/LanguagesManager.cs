using Godot;

public partial class LanguagesManager : Control
{
	[Export] private Button[] langageButtons;
	[Export] private string[] locales;
	[Export] private string defaultLocale = "pl";

    public override void _Ready()
	{
		TranslationServer.SetLocale(defaultLocale);
		
		int count = Mathf.Min(langageButtons.Length, locales.Length);

		for(int i=0; i<count; i++)
		{
			string lang = locales[i];
			langageButtons[i].Pressed += () => TranslationServer.SetLocale(lang);
		}
	}
}
