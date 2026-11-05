using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class LevelLaunch {
 static bool handled;
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]static void Reset(){handled=false;LightPath.Active.Clear();}
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Start(){if(handled)return;handled=true;var a=Environment.GetCommandLineArgs();if(Array.IndexOf(a,"--validate-level02")>=0){var g=new GameObject("Level02_NativeValidation");UnityEngine.Object.DontDestroyOnLoad(g);g.AddComponent<SecondLevelValidation>();}if(Array.IndexOf(a,"--validate-level-link")>=0){var g=new GameObject("FirstToSecond_NativeValidation");UnityEngine.Object.DontDestroyOnLoad(g);g.AddComponent<FirstToSecondValidation>();}if(Array.IndexOf(a,"--validate-level03")>=0){var g=new GameObject("Level03_NativeValidation");UnityEngine.Object.DontDestroyOnLoad(g);g.AddComponent<ThirdLevelValidation>();}if((Array.IndexOf(a,"--level02")>=0||Array.IndexOf(a,"--validate-level02")>=0)&&SceneManager.GetActiveScene().name!="SecondLight")SceneManager.LoadScene("SecondLight");else if((Array.IndexOf(a,"--level03")>=0||Array.IndexOf(a,"--validate-level03")>=0)&&SceneManager.GetActiveScene().name!="ThirdDrain")SceneManager.LoadScene("ThirdDrain");}
}
