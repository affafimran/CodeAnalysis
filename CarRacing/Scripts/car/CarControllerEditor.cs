// Description : CarControllerEditor.cs : Works in association with CarController.cs : use to setup car on the Inspector
#if (UNITY_EDITOR)
using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(CarController))]
public class CarControllerEditor : Editor {
	SerializedProperty		SeeInspector;								// use to draw default Inspector

	SerializedProperty 		SpringHeight;								// Access property from MainMenu.cs script
	SerializedProperty 		restLength;
	SerializedProperty 		dis;

	SerializedProperty 		PicWheelSize;
	SerializedProperty 		offsetWheelFront;
	SerializedProperty 		offsetWheelRear;
	SerializedProperty 		damperConstant;
	SerializedProperty 		springConstant;

	SerializedProperty 		RearWheelsDistance;
	SerializedProperty 		FrontWheelsDistance;
	SerializedProperty 		LenghtWheelsDistance;
	SerializedProperty 		com;
	SerializedProperty 		MaxSpeed;

	SerializedProperty 		playerNumber;
	SerializedProperty 		CarRotationSpeed;
	SerializedProperty 		impactVolumeMax;
	SerializedProperty 		eulerAngleVelocity;
	SerializedProperty 		WheelSizeRear;
	SerializedProperty 		WheelSizeFront;

	SerializedProperty 		pivotOffsetZ;

	SerializedProperty 		BodyRotationValue;


	private Texture2D MakeTex(int width, int height, Color col) {		// use to change the GUIStyle
		Color[] pix = new Color[width * height];
		for (int i = 0; i < pix.Length; ++i) {
			pix[i] = col;
		}
		Texture2D result = new Texture2D(width, height);
		result.SetPixels(pix);
		result.Apply();
		return result;
	}

	private Texture2D 		Tex_01;
	private Texture2D 		Tex_02;
	private Texture2D 		Tex_03;
	private Texture2D 		Tex_04;
	private Texture2D 		Tex_05;

	void OnEnable () {
		// Setup the SerializedProperties.
		SeeInspector 		= serializedObject.FindProperty ("SeeInspector");

		SpringHeight 		= serializedObject.FindProperty ("SpringHeight");
		restLength 			= serializedObject.FindProperty ("restLength");
		dis 				= serializedObject.FindProperty ("dis");
		PicWheelSize		= serializedObject.FindProperty ("PicWheelSize");
		WheelSizeRear		= serializedObject.FindProperty ("WheelSizeRear");
		WheelSizeFront		= serializedObject.FindProperty ("WheelSizeFront");

		offsetWheelFront	= serializedObject.FindProperty ("offsetWheelFront");
		offsetWheelRear		= serializedObject.FindProperty ("offsetWheelRear");

		damperConstant		= serializedObject.FindProperty ("damperConstant");
		springConstant		= serializedObject.FindProperty ("springConstant");	
		RearWheelsDistance	= serializedObject.FindProperty ("RearWheelsDistance");	
		FrontWheelsDistance = serializedObject.FindProperty ("FrontWheelsDistance");
		LenghtWheelsDistance= serializedObject.FindProperty ("LenghtWheelsDistance");

		com					= serializedObject.FindProperty ("com");
		MaxSpeed			= serializedObject.FindProperty ("MaxSpeed");


		playerNumber		= serializedObject.FindProperty ("playerNumber");

		CarRotationSpeed	= serializedObject.FindProperty ("CarRotationSpeed");

		impactVolumeMax		= serializedObject.FindProperty ("impactVolumeMax");
		eulerAngleVelocity	= serializedObject.FindProperty ("eulerAngleVelocity");

		pivotOffsetZ 		= serializedObject.FindProperty ("pivotOffsetZ");

		BodyRotationValue 	= serializedObject.FindProperty ("BodyRotationValue");

		Tex_01 = MakeTex(2, 2, new Color(1,.8f,0.2F,.7f)); 
		Tex_02 = MakeTex(2, 2, new Color(1,.8f,0.2F,.7f)); 
		Tex_03 = MakeTex(2, 2, new Color(.3F,.9f,1,.7f));
		Tex_04 = MakeTex(2, 2, new Color(1,.3f,1,.3f)); 
		Tex_05 = MakeTex(2, 2, new Color(1,.5f,0.3F,.9f)); 
	}


	public override void OnInspectorGUI()
	{
		if(SeeInspector.boolValue)							// If true Default Inspector is drawn on screen
			DrawDefaultInspector();
		serializedObject.Update ();

		GUILayout.Label("");
		EditorGUILayout.BeginHorizontal();
			GUILayout.Label("Inspector :",GUILayout.Width(90));
			EditorGUILayout.PropertyField(SeeInspector, new GUIContent (""),GUILayout.Width(30));
		EditorGUILayout.EndHorizontal();
		//

		CarController myScript = (CarController)target; 


		//GUIStyle style = new GUIStyle(GUI.skin.box);
		GUIStyle style_Yellow_01 		= new GUIStyle(GUI.skin.box);	style_Yellow_01.normal.background 		= Tex_01; 
		GUIStyle style_Blue 			= new GUIStyle(GUI.skin.box);	style_Blue.normal.background 			= Tex_04;
		GUIStyle style_Purple 			= new GUIStyle(GUI.skin.box);	style_Purple.normal.background 			= Tex_03;
		GUIStyle style_Orange 			= new GUIStyle(GUI.skin.box);	style_Orange.normal.background 			= Tex_05; 
		GUIStyle style_Yellow_Strong 	= new GUIStyle(GUI.skin.box);	style_Yellow_Strong.normal.background 	= Tex_02;

		GUILayout.Label("");

		EditorGUILayout.BeginVertical(style_Yellow_01);
		EditorGUILayout.HelpBox("Global Presets",MessageType.Info);
		EditorGUILayout.BeginHorizontal();

// --> Use preset for Desktop or Mobile
		if(GUILayout.Button("Default Values"))
		{
			Undo.RegisterCompleteObjectUndo(myScript,"All Property Modif " + myScript.gameObject.name);
			SpringHeight.floatValue 		= .164F;
			PicWheelSize.floatValue	 		= .265F;
			WheelSizeRear.floatValue		= 0F;
			WheelSizeFront.floatValue		= 0F;

			damperConstant.floatValue		= 10F;
			springConstant.floatValue 		= 300F;
			RearWheelsDistance.floatValue 	= .084F;
			FrontWheelsDistance.floatValue 	= .084F;
			LenghtWheelsDistance.floatValue	= .127F;
			com.FindPropertyRelative("z").floatValue	= -.0119F;

			SerializedObject serializedObject0 = new UnityEditor.SerializedObject(myScript.CarBodyCollider.GetComponent<Transform>());
			serializedObject0.Update();
			SerializedProperty m_LocalBodyColliderPos = serializedObject0.FindProperty("m_LocalPosition");
			SerializedProperty m_LocalBodyColliderSc = serializedObject0.FindProperty("m_LocalScale");

			m_LocalBodyColliderPos.vector3Value = new Vector3(0,0,0);
			m_LocalBodyColliderSc.vector3Value = new Vector3(.78F,1.06F,1);
			serializedObject0.ApplyModifiedProperties ();

			MaxSpeed.floatValue = 4.2F;
			CarRotationSpeed.floatValue = 3.2F;

			impactVolumeMax.floatValue = .3F;
			eulerAngleVelocity.FindPropertyRelative("y").floatValue = 100F;
		}
		/*if(GUILayout.Button("Default Mobile Values"))
		{
			Undo.RegisterCompleteObjectUndo(myScript,"All Property Modif " + myScript.gameObject.name);
			SpringHeight.floatValue 		= .11F;
			PicWheelSize.floatValue	 		= .19F;
			WheelSizeRear.floatValue		= 0F;
			WheelSizeFront.floatValue		= 0F;

			damperConstant.floatValue		= 15F;
			springConstant.floatValue 		= 500F;
			RearWheelsDistance.floatValue 	= .07F;
			FrontWheelsDistance.floatValue 	= .07F;
			LenghtWheelsDistance.floatValue	= .127F;
			com.FindPropertyRelative("z").floatValue	= -.01F;

			SerializedObject serializedObject0 = new UnityEditor.SerializedObject(myScript.CarBodyCollider.GetComponent<Transform>());
			serializedObject0.Update();
			SerializedProperty m_LocalBodyColliderPos = serializedObject0.FindProperty("m_LocalPosition");
			SerializedProperty m_LocalBodyColliderSc = serializedObject0.FindProperty("m_LocalScale");

			m_LocalBodyColliderPos.vector3Value = new Vector3(0,0,0);
			m_LocalBodyColliderSc.vector3Value = new Vector3(.78F,1.06F,1);
			serializedObject0.ApplyModifiedProperties ();

			MaxSpeed.floatValue = 3.98F;
			CarRotationSpeed.floatValue = 1.5F;

			impactVolumeMax.floatValue = .3F;

			eulerAngleVelocity.FindPropertyRelative("y").floatValue = 100F;
		}*/
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.EndVertical();


// --> Display player number
		EditorGUILayout.BeginVertical(style_Yellow_01);
			EditorGUILayout.HelpBox("Infos",MessageType.Info);
			EditorGUILayout.BeginHorizontal();
				GUILayout.Label("Player Number : ",GUILayout.Width(100));
				EditorGUILayout.PropertyField (playerNumber, new GUIContent (""));
			EditorGUILayout.EndHorizontal();
		EditorGUILayout.EndVertical();

// --> Car handling
		EditorGUILayout.LabelField("");

		EditorGUILayout.BeginVertical(style_Blue);
			EditorGUILayout.HelpBox("Car handling",MessageType.Info);
			EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Max Speed :",GUILayout.Width(145));
				EditorGUILayout.Slider(MaxSpeed,1.5F,6F, new GUIContent (""));
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Wheel steering reactivity",GUILayout.Width(145));
				EditorGUILayout.Slider(CarRotationSpeed,1F,7F, new GUIContent (""));
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Car Rotation Speed :",GUILayout.Width(145));
				EditorGUILayout.Slider(eulerAngleVelocity.FindPropertyRelative("y"),50F,150F, new GUIContent (""));
			EditorGUILayout.EndHorizontal();
		EditorGUILayout.EndVertical();


		EditorGUILayout.LabelField("");
// --> Section Spring Height

		EditorGUILayout.BeginVertical(style_Purple);
			EditorGUILayout.HelpBox("Springs Options",MessageType.Info);
			EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Spring Height :",GUILayout.Width(145));
				EditorGUILayout.Slider(SpringHeight,.11F,.3F, new GUIContent (""));
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.LabelField("");
			// --> Section Damper Stiffness
			EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Damper :",GUILayout.Width(145));
				EditorGUILayout.Slider(damperConstant,3F,15F, new GUIContent (""));
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Spring Stiffness :",GUILayout.Width(145));
				EditorGUILayout.Slider(springConstant,300,500, new GUIContent (""));
			EditorGUILayout.EndHorizontal();
		EditorGUILayout.LabelField("");
		EditorGUILayout.EndVertical();

		EditorGUILayout.LabelField("");

// --> Section Wheels options

		EditorGUILayout.BeginVertical(style_Orange);
		EditorGUILayout.HelpBox("Wheels Options",MessageType.Info);
		EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Rear Wheels Distance :",GUILayout.Width(145));
			EditorGUILayout.Slider(RearWheelsDistance,0F,.2F, new GUIContent (""));
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Front Wheels Distance :",GUILayout.Width(145));
			EditorGUILayout.Slider(FrontWheelsDistance,0F,.2F, new GUIContent (""));
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.LabelField("");
		EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Length Wheels Distance :",GUILayout.Width(145));
			EditorGUILayout.Slider(LenghtWheelsDistance,0F,.4F, new GUIContent (""));
		EditorGUILayout.EndHorizontal();


		restLength.floatValue = SpringHeight.floatValue + .01F;
		dis.floatValue = SpringHeight.floatValue;

		float percentagespringConstant = (springConstant.floatValue - 300) / (500 - 300);
		if(!Application.isPlaying){
			for (int i = 0;i< myScript.Wheel_X_Rotate.Length;i++) {	
				if(i == 0  || i == 1){
					myScript.Wheel_X_Rotate[i].transform.localPosition = new Vector3(
						0, 
						-SpringHeight.floatValue + .065F + offsetWheelFront.floatValue +.0F*(1-percentagespringConstant),		// Total .11F Minimum Spring Height
						0);
				}
				else{
					myScript.Wheel_X_Rotate[i].transform.localPosition = new Vector3(
						0, 
						-SpringHeight.floatValue + .065F + offsetWheelRear.floatValue +.0F*(1-percentagespringConstant),		// Total .11F Minimum Spring Height
						0);
				}

				myScript.pivotCarSelection.transform.localPosition = new Vector3(
					0, 
					-SpringHeight.floatValue + offsetWheelFront.floatValue +.0F*(1-percentagespringConstant),		// Total .11F Minimum Spring Height
					0);
			}
		}

		EditorGUILayout.LabelField("");

// --> Section Wheel Size
		EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Global Wheel Size :",GUILayout.Width(145));
			EditorGUILayout.Slider(PicWheelSize,.05F,.5F, new GUIContent (""));
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Rear Wheel Size :",GUILayout.Width(145));
			EditorGUILayout.Slider(WheelSizeRear,0,.5F, new GUIContent (""));
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Front Wheel Size :",GUILayout.Width(145));
			EditorGUILayout.Slider(WheelSizeFront,0,.5F, new GUIContent (""));
		EditorGUILayout.EndHorizontal();

		for (int i = 0;i< myScript.Wheel_X_Rotate.Length;i++) {	

			if(i == 2 || i == 3){
			myScript.Wheel_X_Rotate[i].transform.localScale = new Vector3(
				PicWheelSize.floatValue + WheelSizeRear.floatValue, 
				PicWheelSize.floatValue + WheelSizeRear.floatValue,
				PicWheelSize.floatValue + WheelSizeRear.floatValue);
			}
			else{
				myScript.Wheel_X_Rotate[i].transform.localScale = new Vector3(
					PicWheelSize.floatValue + WheelSizeFront.floatValue, 
					PicWheelSize.floatValue + WheelSizeFront.floatValue,
					PicWheelSize.floatValue + WheelSizeFront.floatValue);
			}

			myScript.pivotCarSelection.transform.localPosition = new Vector3(			// Move the pivotCarSelection gameobject
				0, 
				-SpringHeight.floatValue + .035F,		// 	
				0 + pivotOffsetZ.floatValue);				
		}

		//float percentageWheelSize = (PicWheelSize.floatValue - .05F) / (.5F - .05F);
		float percentageWheelSize = ((PicWheelSize.floatValue + WheelSizeRear.floatValue) - .05F) / (.5F - .05F);
		offsetWheelRear.floatValue = (percentageWheelSize * (.11F - .018F)) + .018F;

		percentageWheelSize = ((PicWheelSize.floatValue + WheelSizeFront.floatValue) - .05F) / (.5F - .05F);
		offsetWheelFront.floatValue = (percentageWheelSize * (.11F - .018F)) + .018F;

		EditorGUILayout.EndVertical();
	
		EditorGUILayout.LabelField("");

		EditorGUILayout.BeginVertical(style_Yellow_Strong);
		EditorGUILayout.HelpBox("Car Body Options",MessageType.Info);
// --> Update the Front Wheel capsule collider size
		SerializedObject serializedObject2 = new UnityEditor.SerializedObject(myScript.frontCapsuleCollider.GetComponent<CapsuleCollider>());
		serializedObject2.Update();
		SerializedProperty m_Height_frontCapsuleCollider = serializedObject2.FindProperty("m_Height");
		float tmpFloat = (FrontWheelsDistance.floatValue - 0) / (.2F - 0)*.44F;
		m_Height_frontCapsuleCollider.floatValue = tmpFloat;
		serializedObject2.ApplyModifiedProperties ();

		SerializedObject serializedObject6 = new UnityEditor.SerializedObject(myScript.frontCapsuleCollider.GetComponent<Transform>());
		serializedObject6.Update();
		SerializedProperty m_Distance_frontCapsuleCollider = serializedObject6.FindProperty("m_LocalPosition").FindPropertyRelative("z");
		tmpFloat = LenghtWheelsDistance.floatValue;
		m_Distance_frontCapsuleCollider.floatValue = tmpFloat;
		serializedObject6.ApplyModifiedProperties ();

// --> Update the Rear Wheel capsule collider size
		SerializedObject serializedObject3 = new UnityEditor.SerializedObject(myScript.rearCapsuleCollider.GetComponent<CapsuleCollider>());
		serializedObject3.Update();
		SerializedProperty m_Height_rearCapsuleCollider = serializedObject3.FindProperty("m_Height");
		tmpFloat = (RearWheelsDistance.floatValue - 0) / (.2F - 0)*.44F;
		m_Height_rearCapsuleCollider.floatValue = tmpFloat;
		serializedObject3.ApplyModifiedProperties ();

		SerializedObject serializedObject5 = new UnityEditor.SerializedObject(myScript.rearCapsuleCollider.GetComponent<Transform>());
		serializedObject5.Update();
		SerializedProperty m_Distance_rearCapsuleCollider = serializedObject5.FindProperty("m_LocalPosition").FindPropertyRelative("z");
		tmpFloat = -LenghtWheelsDistance.floatValue;
		m_Distance_rearCapsuleCollider.floatValue = tmpFloat;
		serializedObject5.ApplyModifiedProperties ();

		myScript.RayCastWheels[2].transform.localPosition = new Vector3(
			RearWheelsDistance.floatValue, 
			myScript.RayCastWheels[2].transform.localPosition.y,
			-LenghtWheelsDistance.floatValue);
		myScript.RayCastWheels[3].transform.localPosition = new Vector3(
			-RearWheelsDistance.floatValue, 
			myScript.RayCastWheels[3].transform.localPosition.y,
			-LenghtWheelsDistance.floatValue);

		myScript.RayCastWheels[0].transform.localPosition = new Vector3(
			FrontWheelsDistance.floatValue, 
			myScript.RayCastWheels[0].transform.localPosition.y,
			LenghtWheelsDistance.floatValue);
		myScript.RayCastWheels[1].transform.localPosition = new Vector3(
			-FrontWheelsDistance.floatValue, 
			myScript.RayCastWheels[1].transform.localPosition.y,
			LenghtWheelsDistance.floatValue);


// --> Update the body Model position
		SerializedObject serializedObject10 = new UnityEditor.SerializedObject(myScript.Grp_BodyPlusBlobShadow.GetComponent<Transform>());
		serializedObject10.Update();
		SerializedProperty m_LocalBodyModelPosition = serializedObject10.FindProperty("m_LocalPosition");

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Body Model Z Position :",GUILayout.Width(145));
		EditorGUILayout.Slider(m_LocalBodyModelPosition.FindPropertyRelative("z"),-.2F,.2F, new GUIContent (""));
		EditorGUILayout.EndHorizontal();

		serializedObject10.ApplyModifiedProperties ();

// --> Update the body collider position and scale
		SerializedObject serializedObject4 = new UnityEditor.SerializedObject(myScript.CarBodyCollider.GetComponent<Transform>());
		serializedObject4.Update();
		SerializedProperty m_LocalBodyColliderPosition = serializedObject4.FindProperty("m_LocalPosition");
		SerializedProperty m_LocalBodyColliderScale = serializedObject4.FindProperty("m_LocalScale");
	
		EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Body Collider Position :",GUILayout.Width(145));
			EditorGUILayout.Slider(m_LocalBodyColliderPosition.FindPropertyRelative("z"),-.2F,.2F, new GUIContent (""));
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Body Collider Scale :",GUILayout.Width(145));
			EditorGUILayout.PropertyField (m_LocalBodyColliderScale, new GUIContent (""));
		EditorGUILayout.EndHorizontal();
		serializedObject4.ApplyModifiedProperties ();

		EditorGUILayout.LabelField("");
		EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Center Of Mass Z Axis :",GUILayout.Width(145));
			EditorGUILayout.Slider(com.FindPropertyRelative("z"),-.06F,.06F, new GUIContent (""));
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.EndVertical();

		EditorGUILayout.LabelField("");


// --> Update the pivot use for car selection position and scale
		EditorGUILayout.BeginVertical(style_Purple);
			EditorGUILayout.HelpBox("Offset for gameObject use as a pivot on Menu car selection",MessageType.Info);
			EditorGUILayout.LabelField("");

			EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Offset Z Axis :",GUILayout.Width(145));
				EditorGUILayout.Slider(pivotOffsetZ,-.4F,.4F, new GUIContent (""));
			EditorGUILayout.EndHorizontal();
		EditorGUILayout.EndVertical();

		EditorGUILayout.LabelField("");


// --> Section Audio

		EditorGUILayout.BeginVertical(style_Blue);
		EditorGUILayout.HelpBox("Audio Options :" +
			"\n" +
			"\n- Drag and drop audio clip for each type of sound" +
			"\n- Change volume for each type of sound" +
			"\n",MessageType.Info);

		SerializedObject serializedObject7 = new UnityEditor.SerializedObject(myScript.audio_);
		serializedObject7.Update();
		SerializedProperty m_AudioClip = serializedObject7.FindProperty("m_audioClip");
		SerializedProperty m_Volume = serializedObject7.FindProperty("m_Volume");

		EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Acceleration :",GUILayout.Width(145));
			EditorGUILayout.PropertyField (m_AudioClip, new GUIContent (""));
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Volume :",GUILayout.Width(145));
			EditorGUILayout.Slider(m_Volume,0F,1F, new GUIContent (""));
		EditorGUILayout.EndHorizontal();

		serializedObject7.ApplyModifiedProperties ();
		EditorGUILayout.LabelField("");

		SerializedObject serializedObject8 = new UnityEditor.SerializedObject(myScript.objSkid_Sound);
		serializedObject8.Update();
		m_AudioClip = serializedObject8.FindProperty("m_audioClip");
		m_Volume = serializedObject8.FindProperty("m_Volume");

		EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Skid :",GUILayout.Width(145));
			EditorGUILayout.PropertyField (m_AudioClip, new GUIContent (""));
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Volume :",GUILayout.Width(145));
			EditorGUILayout.Slider(m_Volume,0F,1F, new GUIContent (""));
		EditorGUILayout.EndHorizontal();

		serializedObject8.ApplyModifiedProperties ();
		EditorGUILayout.LabelField("");

		SerializedObject serializedObject9 = new UnityEditor.SerializedObject(myScript.obj_CarImpact_Sound);
		serializedObject9.Update();
		m_AudioClip = serializedObject9.FindProperty("m_audioClip");
		m_Volume = serializedObject9.FindProperty("m_Volume");

		EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Impact :",GUILayout.Width(145));
			EditorGUILayout.PropertyField (m_AudioClip, new GUIContent (""));
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Volume :",GUILayout.Width(145));
			EditorGUILayout.Slider(impactVolumeMax,0F,1F, new GUIContent (""));
		EditorGUILayout.EndHorizontal();

		serializedObject9.ApplyModifiedProperties ();
		EditorGUILayout.LabelField("");

		EditorGUILayout.EndVertical();

		EditorGUILayout.LabelField("");
// --> Section Fake car rotationn left Right

		EditorGUILayout.BeginVertical(style_Orange);
		EditorGUILayout.HelpBox("Adjust the left right body car movement",MessageType.Info);


		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Rotation :",GUILayout.Width(145));
		EditorGUILayout.Slider(BodyRotationValue,0F,10F, new GUIContent (""));
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.EndVertical();

		EditorGUILayout.LabelField("");

		serializedObject.ApplyModifiedProperties ();
	}


	void OnSceneGUI( )
	{
		// get the chosen game object
		CarController t = target as CarController;
		Debug.DrawLine(t.transform.localPosition, new Vector3(t.transform.localPosition.x, t.transform.localPosition.y-SpringHeight.floatValue+.02F, t.transform.localPosition.z), Color.red);
	}
}
#endif