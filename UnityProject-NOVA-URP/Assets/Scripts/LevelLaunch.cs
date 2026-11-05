using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class LevelLaunch {
 static bool handled;
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]static void Reset(){handled=false;LightPath.Active.Clear();}
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Start(){
  if(handled)return;handled=true;var a=Environment.GetCommandLineArgs();
  if(Array.IndexOf(a,"--validate-level02")>=0)Persistent<SecondLevelValidation>("Level02_NativeValidation");
  if(Array.IndexOf(a,"--validate-level-link")>=0)Persistent<FirstToSecondValidation>("FirstToSecond_NativeValidation");
  if(Array.IndexOf(a,"--validate-level03")>=0)Persistent<ThirdLevelValidation>("Level03_NativeValidation");
  if(Array.IndexOf(a,"--validate-level04")>=0)Persistent<FourthLevelValidation>("Level04_NativeValidation");
  if(Array.IndexOf(a,"--validate-level05")>=0)Persistent<FifthLevelValidation>("Level05_NativeValidation");
  if(Array.IndexOf(a,"--validate-level06")>=0)Persistent<SixthLevelValidation>("Level06_NativeValidation");
  if(Array.IndexOf(a,"--validate-level07")>=0)Persistent<SeventhLevelValidation>("Level07_NativeValidation");
  string scene=null;
  if(Array.IndexOf(a,"--level07")>=0||Array.IndexOf(a,"--validate-level07")>=0)scene="SeventhTrellis";
  else if(Array.IndexOf(a,"--level06")>=0||Array.IndexOf(a,"--validate-level06")>=0)scene="SixthGreenhouse";
  else if(Array.IndexOf(a,"--level05")>=0||Array.IndexOf(a,"--validate-level05")>=0)scene="FifthPumpHouse";
  else if(Array.IndexOf(a,"--level04")>=0||Array.IndexOf(a,"--validate-level04")>=0)scene="FourthReservoir";
  else if(Array.IndexOf(a,"--level03")>=0||Array.IndexOf(a,"--validate-level03")>=0)scene="ThirdDrain";
  else if(Array.IndexOf(a,"--level02")>=0||Array.IndexOf(a,"--validate-level02")>=0)scene="SecondLight";
  if(scene!=null&&SceneManager.GetActiveScene().name!=scene)SceneManager.LoadScene(scene);
 }
 static void Persistent<T>(string name) where T:MonoBehaviour {var g=new GameObject(name);UnityEngine.Object.DontDestroyOnLoad(g);g.AddComponent<T>();}
}
