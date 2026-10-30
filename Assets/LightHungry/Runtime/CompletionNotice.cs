using UnityEngine;
using UnityEngine.UI;
namespace LightHungry {
    public sealed class CompletionNotice:MonoBehaviour {
        public bool IsVisible=>panel;
        GameObject panel;CanvasGroup group;float elapsed;Font font;
        public void Show(){
            if(panel)return;
            panel=new GameObject("Level complete notice",typeof(RectTransform),typeof(Canvas),typeof(CanvasGroup));panel.transform.SetParent(transform,false);
            var canvas=panel.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=100;
            var scaler=panel.AddComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1280,720);scaler.matchWidthOrHeight=.5f;
            group=panel.GetComponent<CanvasGroup>();group.alpha=0;group.interactable=false;group.blocksRaycasts=false;elapsed=0;
            var card=new GameObject("Completion banner",typeof(RectTransform),typeof(Image));card.transform.SetParent(panel.transform,false);var rect=card.GetComponent<RectTransform>();rect.anchorMin=rect.anchorMax=new Vector2(.5f,1);rect.pivot=new Vector2(.5f,1);rect.anchoredPosition=new Vector2(0,-56);rect.sizeDelta=new Vector2(300,76);card.GetComponent<Image>().color=new Color(.12f,.2f,.24f,.86f);card.GetComponent<Image>().raycastTarget=false;
            var label=new GameObject("Completion text",typeof(RectTransform),typeof(Text));label.transform.SetParent(card.transform,false);var textRect=label.GetComponent<RectTransform>();textRect.anchorMin=Vector2.zero;textRect.anchorMax=Vector2.one;textRect.offsetMin=textRect.offsetMax=Vector2.zero;
            font=Font.CreateDynamicFontFromOSFont(new[]{"Microsoft YaHei","Microsoft YaHei UI","SimHei","Noto Sans CJK SC","Arial Unicode MS"},36);
            var text=label.GetComponent<Text>();text.font=font;text.text="关卡完成";text.fontSize=32;text.alignment=TextAnchor.MiddleCenter;text.color=new Color(.96f,.98f,1);text.raycastTarget=false;
        }
        void Update(){if(group){elapsed+=Time.unscaledDeltaTime;group.alpha=Mathf.SmoothStep(0,1,Mathf.Clamp01(elapsed/.3f));}}
        void OnDestroy(){if(font)Destroy(font);}
    }
}
