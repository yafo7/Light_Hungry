using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class WickRigSetup {
 const string Model="Assets/Models/Rigged/WickSpirit_Game.fbx",Controller="Assets/Models/Rigged/WickSpirit.controller";
 public static void Prepare(){
  Directory.CreateDirectory("Assets/Resources");AssetDatabase.Refresh();
  if(!AssetDatabase.LoadAssetAtPath<Material>("Assets/Resources/SpiritFlame.mat"))AssetDatabase.CreateAsset(new Material(Shader.Find("LightFeast/SpiritFlame")),"Assets/Resources/SpiritFlame.mat");
  AssetDatabase.ImportAsset(Model,ImportAssetOptions.ForceSynchronousImport);
  var importer=(ModelImporter)AssetImporter.GetAtPath(Model);importer.animationType=ModelImporterAnimationType.Generic;importer.importAnimation=true;importer.isReadable=true;importer.optimizeGameObjects=false;importer.materialImportMode=ModelImporterMaterialImportMode.None;
  var settings=importer.defaultClipAnimations;foreach(var c in settings){c.loopTime=true;c.loopPose=true;}importer.clipAnimations=settings;importer.SaveAndReimport();
  var clips=AssetDatabase.LoadAllAssetsAtPath(Model).OfType<AnimationClip>().Where(c=>!c.name.StartsWith("__")).ToArray();
  AnimationClip Clip(string n)=>clips.First(c=>c.name.EndsWith(n));
  var controller=AssetDatabase.LoadAssetAtPath<AnimatorController>(Controller);if(controller)AssetDatabase.DeleteAsset(Controller);controller=AnimatorController.CreateAnimatorControllerAtPath(Controller);
  controller.AddParameter("Move",AnimatorControllerParameterType.Float);controller.AddParameter("Grounded",AnimatorControllerParameterType.Bool);
  var machine=controller.layers[0].stateMachine;var ground=machine.AddState("GroundLocomotion");var air=machine.AddState("Airborne");machine.defaultState=ground;
  var blend=new BlendTree{name="IdleWalk",blendParameter="Move",useAutomaticThresholds=false};AssetDatabase.AddObjectToAsset(blend,controller);blend.AddChild(Clip("Idle"),0);blend.AddChild(Clip("Walk"),1);ground.motion=blend;air.motion=Clip("Airborne");
  var takeoff=ground.AddTransition(air);takeoff.hasExitTime=false;takeoff.duration=.09f;takeoff.AddCondition(AnimatorConditionMode.IfNot,0,"Grounded");
  var land=air.AddTransition(ground);land.hasExitTime=false;land.duration=.12f;land.AddCondition(AnimatorConditionMode.If,0,"Grounded");
  var root=PrefabUtility.LoadPrefabContents("Assets/Prefabs/WickSpirit.prefab");foreach(Transform child in root.transform.Cast<Transform>().ToArray())UnityEngine.Object.DestroyImmediate(child.gameObject);
  var model=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Model));model.transform.SetParent(root.transform,false);model.transform.localRotation=Quaternion.Euler(0,90,0);
  var animator=model.GetComponent<Animator>();if(!animator)animator=model.AddComponent<Animator>();animator.runtimeAnimatorController=controller;animator.applyRootMotion=false;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
  var skin=model.GetComponentInChildren<SkinnedMeshRenderer>();skin.sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/WickSpirit.mat");skin.updateWhenOffscreen=true;
  Bounds bounds=skin.bounds;float height=bounds.size.y;model.transform.localScale=Vector3.one/height;model.transform.localPosition=new Vector3(0,-bounds.min.y/height,0);
  var driver=root.GetComponent<WickRigAnimation>();if(!driver)driver=root.AddComponent<WickRigAnimation>();driver.animator=animator;
  PrefabUtility.SaveAsPrefabAsset(root,"Assets/Prefabs/WickSpirit.prefab");PrefabUtility.UnloadPrefabContents(root);AssetDatabase.SaveAssets();
  string dir=Environment.GetEnvironmentVariable("LIGHTFEAST_WORKSPACE")??Path.GetFullPath(Application.dataPath+"/../..");
  File.WriteAllText(dir+"/Validation/rig-unity-import.json","{\"triangles\":"+skinSafeTriangles()+",\"clips\":["+string.Join(",",clips.Select(c=>"\""+c.name+"\""))+"],\"rootMotion\":false}");
 }
 static int skinSafeTriangles()=>AssetDatabase.LoadAllAssetsAtPath(Model).OfType<Mesh>().Sum(m=>m.triangles.Length/3);
}
