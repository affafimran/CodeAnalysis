using UnityEngine;
using System.Collections;


public static class AtlasManager
{
	/// <summary>
	/// Keeps track of sprites on the atlasses.
	/// If you want to set sprites,
	/// that are located on an atlas,
	/// onto an Image,
	/// the atlas needs to be initialized here first.
	/// </summary>

	public static Atlas hammer2IconsAtlas;
	public static Atlas hammer2InterfaceAtlas;
	public static Atlas hammer2OptionsAtlas;
	public static Atlas hammer2TargetsAtlas;
	public static Atlas hammer2CrosshairsAtlas;
	public static Atlas hammer2AchievementsAtlas;
	public static Atlas hammer2ShopItemsAtlas;
	public static Atlas hammer2MinimapIconsAtlas;

	public static void Initialize()
	{
		hammer2IconsAtlas = new Atlas("Sprites/Hammer2Icons");
		hammer2InterfaceAtlas = new Atlas("Sprites/Hammer2Interface");
		hammer2OptionsAtlas = new Atlas("Sprites/Hammer2Options");
		hammer2TargetsAtlas = new Atlas("Sprites/Hammer2Targets");
		hammer2CrosshairsAtlas = new Atlas("Sprites/Hammer2Crosshairs");
		hammer2AchievementsAtlas = new Atlas("Sprites/Hammer2Achievements");
		hammer2ShopItemsAtlas = new Atlas("Sprites/Hammer2ShopItems");
		hammer2MinimapIconsAtlas = new Atlas("Sprites/Hammer2MinimapIcons");
	}
}

public class Atlas
{
	private Sprite[] sprites;
	private string[] names;

	public Atlas(string spritesheet)
	{
		sprites = Resources.LoadAll<Sprite>(spritesheet);
		names = new string[sprites.Length];
		
		for(var i = 0; i < names.Length; i++)
		{
			names[i] = sprites[i].name;
		}
	}
	
	public Sprite Get(string name)
	{

		if (System.Array.IndexOf(names, name) != -1)
		{
			return sprites[System.Array.IndexOf(names, name)];
		}
		else
		{
			Debug.LogWarning("[AtAtlas failed to get: " + name + ", returning NULL." );
			return null;
		}
	}
}
