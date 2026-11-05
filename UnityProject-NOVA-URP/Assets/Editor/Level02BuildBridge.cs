using System;
using System.IO;
using UnityEditor;
using UnityEngine;

// A one-shot request file allows the already-open editor to build without closing the user's session.
[InitializeOnLoad]public static class Level02BuildBridge {
 static double next;static bool busy;
 static Level02BuildBridge(){EditorApplication.update+=Tick;}
 static void Tick(){if(busy||EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.timeSinceStartup<next)return;next=EditorApplication.timeSinceStartup+1;
  string dir=Path.GetFullPath(Application.dataPath+"/../../Validation/level02");string req=dir+"/editor-request.txt";if(!File.Exists(req))return;string command=File.ReadAllText(req).Trim();File.Delete(req);busy=true;
  try{if(command=="reload"){AssetDatabase.Refresh();EditorUtility.RequestScriptReload();}else if(command=="build")SecondLevelBuilder.BuildAll();else if(command=="scenes")SecondLevelBuilder.BuildScenes();else throw new Exception("Unknown build request");File.WriteAllText(dir+"/editor-result.json","{\"success\":true,\"command\":\""+command+"\"}");}
  catch(Exception e){Debug.LogException(e);File.WriteAllText(dir+"/editor-result.json",JsonUtility.ToJson(new Error{success=false,message=e.ToString()},true));}finally{busy=false;}}
 [Serializable]class Error{public bool success;public string message;}
}
