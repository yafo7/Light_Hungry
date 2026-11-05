using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.Rendering;
using Nova.Editor.Core.Scripts;

public static class NovaPrototypeBuilder {
 static string Root=>Path.GetFullPath(Application.dataPath+"/../..");
 public static void Build(){
  Directory.CreateDirectory("Assets/NovaSettings");Directory.CreateDirectory("Assets/Resources");AssetDatabase.Refresh();
  var renderer=ScriptableObject.CreateInstance<UniversalRendererData>();
  ResourceReloader.ReloadAllNullIn(renderer,"Packages/com.unity.render-pipelines.universal");
  AssetDatabase.CreateAsset(renderer,"Assets/NovaSettings/Renderer.asset");
  var outline=ScriptableObject.CreateInstance<SpiritOutlineFeature>();outline.name="Spirit watercolor outlines";renderer.rendererFeatures.Add(outline);AssetDatabase.AddObjectToAsset(outline,renderer);outline.Create();
  var pipeline=UniversalRenderPipelineAsset.Create(renderer);pipeline.supportsHDR=true;pipeline.msaaSampleCount=1;pipeline.supportsCameraDepthTexture=true;
  AssetDatabase.CreateAsset(pipeline,"Assets/NovaSettings/Pipeline.asset");GraphicsSettings.renderPipelineAsset=pipeline;QualitySettings.renderPipeline=pipeline;
  var quality=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/QualitySettings.asset")[0]);
  var levels=quality.FindProperty("m_QualitySettings");for(int i=0;i<levels.arraySize;i++)levels.GetArrayElementAtIndex(i).FindPropertyRelative("customRenderPipeline").objectReferenceValue=pipeline;quality.ApplyModifiedPropertiesWithoutUndo();
  var fire=PrefabUtility.LoadPrefabContents("Assets/NovaSamples/Prefabs/pfb_eff_fire.prefab");fire.name="NovaHeadFlame";fire.transform.localPosition=Vector3.zero;fire.transform.localRotation=Quaternion.identity;fire.transform.localScale=Vector3.one;
  foreach(var light in fire.GetComponentsInChildren<Light>())UnityEngine.Object.DestroyImmediate(light.gameObject);
  foreach(var ps in fire.GetComponentsInChildren<ParticleSystem>()){
   ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
   bool keep=ps.name=="ps_fire_main"||ps.name=="ps_fire_back"||ps.name=="ps_sparks";
   if(!keep){var emission=ps.emission;emission.enabled=false;var r=ps.GetComponent<ParticleSystemRenderer>();if(r)r.enabled=false;continue;}
   var main=ps.main;main.playOnAwake=false;main.prewarm=false;main.maxParticles=96;main.startColor=Color.white;main.startSpeedMultiplier*=.14f;main.startLifetimeMultiplier*=.7f;
   var c=ps.colorOverLifetime;c.enabled=true;var g=new Gradient();g.SetKeys(new[]{new GradientColorKey(new Color(1,.96f,.76f),0),new GradientColorKey(new Color(1,.71f,.32f),1)},new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(.55f,.15f),new GradientAlphaKey(0,1)});c.color=g;
   var r2=ps.GetComponent<ParticleSystemRenderer>();var mat=new Material(r2.sharedMaterial);mat.name="Spirit_"+ps.name;
   mat.SetFloat("_BaseMapRotationCoord",0);mat.SetFloat("_BaseMapRotation",62/360f);mat.SetFloat("_TintAreaMode",1);mat.SetFloat("_TransparentBlendMode",1);mat.SetColor("_TintColor",new Color(1.3f,1.05f,.65f,1));ParticlesUberUnlitMaterialPostProcessor.SetupMaterialKeywords(mat);ParticlesUberUnlitMaterialPostProcessor.SetupMaterialBlendMode(mat);
   AssetDatabase.CreateAsset(mat,"Assets/NovaSettings/"+mat.name+".mat");r2.sharedMaterial=mat;
   Debug.Log("NOVA_SAMPLE "+ps.name+" size="+main.startSize.constantMax+" speed="+main.startSpeed.constantMax+" lifetime="+main.startLifetime.constantMax+" shader="+mat.shader.name);
  }
  PrefabUtility.SaveAsPrefabAsset(fire,"Assets/Resources/NovaHeadFlame.prefab");PrefabUtility.UnloadPrefabContents(fire);
  AssetDatabase.SaveAssets();
  string output=Root+"/Builds/Windows-NOVA-URP";Directory.CreateDirectory(output);
  var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),locationPathName=output+"/LightFeast.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
  File.WriteAllText(Root+"/Validation/nova-urp-v01/build-result.json","{\"result\":\""+report.summary.result+"\",\"errors\":"+report.summary.totalErrors+",\"warnings\":"+report.summary.totalWarnings+"}");
  if(report.summary.result!=BuildResult.Succeeded)throw new Exception("NOVA build failed");
 }
}


