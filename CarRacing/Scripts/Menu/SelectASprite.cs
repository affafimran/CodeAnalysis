using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectASprite : MonoBehaviour {

	public Sprite 			SpriteOFF;
	public Sprite 			SpriteON;
	private Image 			sr;

	// Use this for initialization
	void Start () {
		sr = gameObject.GetComponent<Image> ();
		if (SpriteOFF == null)
				SpriteOFF = sr.sprite;
	}
	
	public void F_Sprite_OFF(){
		sr.sprite = SpriteOFF;
	}
	public void F_Sprite_ON(){
		if(SpriteON)sr.sprite = SpriteON;
	}
}
