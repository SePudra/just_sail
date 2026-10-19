using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace JustSail
{
    public sealed class SailUI : MonoBehaviour
    {
        public BoatController boat;
        public SailCamera cameraRig;
        public SeaAudio ambience;
        public Light sun;
        public VoyageAtmosphere atmosphere;
        public TMP_FontAsset font;
        public bool Playing {get;private set;}
        public bool MenuOpen {get;private set;}=true;
        public bool ExternalPanelOpen {get;private set;}
        GameObject main,pause,settings,hud,confirm;
        TMP_Text hint, resolutionLabel,modeLabel,qualityLabel,weatherLabel,seaWeatherLabel,cycleLabel;
        RectTransform root;
        float helpUntil;
        Vector2Int[] resolutions;
        int resolutionIndex,qualityIndex,weatherIndex;
        float restoreCountdown;
        Vector2Int oldResolution;
        FullScreenMode oldMode;
        int previousResolutionIndex;
        Vector2Int displayedSize;
        bool displayedFullscreen;
        CanvasGroup settingsInput;
        readonly Color cream=new(.94f,.91f,.82f), ink=new(.065f,.16f,.18f,.96f), muted=new(.68f,.76f,.72f), accent=new(.81f,.55f,.36f);
        public int WeatherIndex=>weatherIndex;

        void Start()
        {
            if(!atmosphere)atmosphere=FindFirstObjectByType<VoyageAtmosphere>();
            Application.runInBackground=true;Application.targetFrameRate=60;
            if(FindFirstObjectByType<EventSystem>()==null)
            {var es=new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));es.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();}
            root=new GameObject("Just Sail UI",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster)).GetComponent<RectTransform>();
            root.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=root.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.matchWidthOrHeight=.5f;
            main=Panel("Main menu",root,Vector2.zero,Vector2.zero,Color.clear,true);
            MakeVeil(main.transform);
            Text(main.transform,"just sail",new(82,305),new(760,145),112,cream,-3);
            Text(main.transform,"Rüzgârın götürdüğü yere.",new(90,463),new(650,45),25,cream);
            Button(main.transform,"Denize Açıl",new(90,570),new(330,60),StartSailing,true);
            Button(main.transform,"Ayarlar",new(90,650),new(330,54),()=>ShowSettings());
            Button(main.transform,"Çıkış",new(90,718),new(330,54),Quit);
            var footer=Text(main.transform,"SAKİN KOY     ·     İLK YOLCULUK",new(90,0),new(700,36),15,muted,3).rectTransform;
            footer.anchorMin=footer.anchorMax=Vector2.zero;footer.pivot=Vector2.zero;footer.anchoredPosition=new(90,40);

            pause=Panel("Pause",root,new(0,0),new(570,800),ink,false,true);
            Text(pause.transform,"BİR NEFES",new(55,45),new(460,28),14,accent,5);
            Text(pause.transform,"Aceleye gerek yok.",new(55,105),new(470,100),41,cream);
            Button(pause.transform,"Devam Et",new(55,235),new(460,54),Resume,true);
            Button(pause.transform,"Ayarlar",new(55,305),new(460,54),()=>ShowSettings());
            Button(pause.transform,"Seyir Defteri",new(55,375),new(460,54),()=>FindFirstObjectByType<VoyageJournal>()?.OpenAlbum());
            Button(pause.transform,"Tekneyi Kurtar",new(55,445),new(460,54),()=>{boat.Recover();cameraRig.Snap();Resume();});
            Button(pause.transform,"Ana Menü",new(55,515),new(460,54),MainMenu);
            Button(pause.transform,"Oyundan Çık",new(55,585),new(460,54),Quit);
            Text(pause.transform,"F  fotoğraf     Tab  seyir defteri     C  odak\nSağ fare  bakış     Tekerlek  uzaklık",new(55,705),new(460,65),16,muted);

            settings=Panel("Settings",root,Vector2.zero,new(650,930),ink,false,true);
            settingsInput=settings.AddComponent<CanvasGroup>();
            Text(settings.transform,"AYARLAR",new(55,40),new(540,28),14,accent,5);
            Text(settings.transform,"Sana göre bir deniz.",new(55,90),new(550,65),37,cream);
            Slider(settings.transform,"Ses",170,0,1,PlayerPrefs.GetFloat("volume",.65f),v=>{AudioListener.volume=v;PlayerPrefs.SetFloat("volume",v);});
            Slider(settings.transform,"Kamera hassasiyeti",255,.35f,2,PlayerPrefs.GetFloat("sensitivity",1),v=>{cameraRig.sensitivity=v;PlayerPrefs.SetFloat("sensitivity",v);});
            resolutions=Screen.resolutions.Select(r=>new Vector2Int(r.width,r.height)).Distinct().Where(r=>r.x>=1280).ToArray();
            if(resolutions.Length==0)resolutions=new[]{new Vector2Int(1280,720),new Vector2Int(1920,1080)};
            resolutionIndex=System.Array.FindIndex(resolutions,r=>r.x==Screen.width&&r.y==Screen.height);if(resolutionIndex<0)resolutionIndex=0;
            resolutionLabel=Button(settings.transform,"",new(55,355),new(540,44),CycleResolution).GetComponentInChildren<TMP_Text>();
            modeLabel=Button(settings.transform,"",new(55,415),new(540,44),CycleMode).GetComponentInChildren<TMP_Text>();
            qualityLabel=Button(settings.transform,"",new(55,475),new(540,44),CycleQuality).GetComponentInChildren<TMP_Text>();
            weatherLabel=Button(settings.transform,"",new(55,535),new(540,44),()=>{weatherIndex=(weatherIndex+1)%4;ApplyWeather();RefreshLabels();}).GetComponentInChildren<TMP_Text>();
            seaWeatherLabel=Button(settings.transform,"",new(55,595),new(540,44),()=>{if(atmosphere)atmosphere.SetWeather((atmosphere.weatherMode+1)%4);RefreshLabels();}).GetComponentInChildren<TMP_Text>();
            cycleLabel=Button(settings.transform,"",new(55,655),new(540,44),()=>{if(atmosphere)atmosphere.advanceDay=!atmosphere.advanceDay;RefreshLabels();}).GetComponentInChildren<TMP_Text>();
            Slider(settings.transform,"Fırtına gücü",715,0,1,atmosphere?atmosphere.stormStrength:1,v=>{if(atmosphere)atmosphere.stormStrength=v;});
            Button(settings.transform,"Geri",new(55,820),new(540,54),BackFromSettings,true);
            confirm=Panel("Resolution confirmation",root,Vector2.zero,new(600,270),ink,false,true);
            Text(confirm.transform,"Bu görüntü ayarı kalsın mı?",new(40,35),new(520,65),28,cream);
            Button(confirm.transform,"Koru",new(40,135),new(245,55),KeepResolution,true);
            Button(confirm.transform,"Geri Al",new(315,135),new(245,55),RestoreResolution);
            hud=Panel("HUD",root,Vector2.zero,Vector2.zero,Color.clear,true);
            var menu=Button(hud.transform,"Menü  /  Esc",new(-190,35),new(155,42),Pause);
            menu.GetComponent<Image>().color=new Color(.065f,.16f,.18f,.84f);
            var mr=menu.GetComponent<RectTransform>();mr.anchorMin=mr.anchorMax=new Vector2(1,1);
            hint=Text(hud.transform,"W / S  YELKEN     A / D  DÜMEN     SAĞ FARE  BAKIŞ\nF  FOTOĞRAF     TAB  SEYİR DEFTERİ     C  ODAK",new(0,-85),new(1200,70),16,cream,1);
            var hr=hint.rectTransform;hr.anchorMin=hr.anchorMax=new Vector2(.5f,0);hr.pivot=new Vector2(.5f,1);
            AudioListener.volume=PlayerPrefs.GetFloat("volume",.65f);cameraRig.sensitivity=PlayerPrefs.GetFloat("sensitivity",1);
            qualityIndex=Mathf.Clamp(PlayerPrefs.GetInt("quality",1),0,2);ApplyQuality();ApplyWeather();RefreshLabels();MainMenu();
            #if !UNITY_EDITOR
            if(PlayerPrefs.HasKey("width"))Screen.SetResolution(PlayerPrefs.GetInt("width"),PlayerPrefs.GetInt("height"),PlayerPrefs.GetInt("fullscreen")==1?FullScreenMode.FullScreenWindow:FullScreenMode.Windowed);
            #endif
        }

        void Update()
        {
            if(settings && settings.activeSelf && (displayedSize.x!=Screen.width || displayedSize.y!=Screen.height || displayedFullscreen!=Screen.fullScreen))RefreshLabels();
            if(restoreCountdown>0 && (restoreCountdown-=Time.unscaledDeltaTime)<=0)RestoreResolution();
            if(Keyboard.current?.escapeKey.wasPressedThisFrame==true&&!ExternalPanelOpen)
            {if(confirm.activeSelf)RestoreResolution();else if(settings.activeSelf)BackFromSettings();else if(Playing){if(MenuOpen)Resume();else Pause();}}
            if(hint)hint.gameObject.SetActive(Time.unscaledTime<helpUntil&&!MenuOpen);
        }
        void OnApplicationFocus(bool focused){if(!focused&&Playing&&!MenuOpen)Pause();}
        void OnDestroy(){Time.timeScale=1;Cursor.visible=true;}
        void SetState(bool isMain,bool isPause,bool isSettings)
        {
            restoreCountdown=0;settingsInput.interactable=true;
            main.SetActive(isMain);pause.SetActive(isPause);settings.SetActive(isSettings);confirm.SetActive(false);
            MenuOpen=isMain||isPause||isSettings||ExternalPanelOpen;hud.SetActive(!MenuOpen);Time.timeScale=MenuOpen?0:1;
            boat.controlsEnabled=!MenuOpen;cameraRig.menuOpen=MenuOpen;ambience.menuOpen=MenuOpen;
            Cursor.visible=true;Cursor.lockState=CursorLockMode.None;
            var active=isMain?main:isPause?pause:isSettings?settings:null;
            EventSystem.current.SetSelectedGameObject(active?active.GetComponentInChildren<Button>().gameObject:null);
        }
        public void StartSailing(){Playing=true;helpUntil=Time.unscaledTime+16;SetState(false,false,false);}
        public void Resume(){SetState(false,false,false);}
        public void Pause(){SetState(false,true,false);}
        public void OpenJournal(){ExternalPanelOpen=true;SetState(false,false,false);}
        public void CloseJournal(){ExternalPanelOpen=false;Resume();}
        public void SetPhotoHud(bool hidden){hud.SetActive(!hidden&&!MenuOpen);boat.controlsEnabled=!hidden&&!MenuOpen;}
        public void MainMenu(){Playing=false;boat.Recover();cameraRig.Snap();SetState(true,false,false);}
        void ShowSettings(){SetState(false,false,true);RefreshLabels();}
        void BackFromSettings(){PlayerPrefs.Save();SetState(!Playing,Playing,false);}
        public void Quit(){PlayerPrefs.Save();Time.timeScale=1;
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying=false;
            #else
            Application.Quit();
            #endif
        }
        void RefreshLabels()
        {
            displayedSize=new(Screen.width,Screen.height);displayedFullscreen=Screen.fullScreen;
            resolutionLabel.text=$"Çözünürlük     {Screen.width} × {Screen.height}   ›";
            modeLabel.text=$"Ekran     {(Screen.fullScreen?"Tam ekran":"Pencere")}   ›";
            qualityLabel.text=$"Görüntü     {new[]{"Hafif","Dengeli","Yüksek"}[qualityIndex]}   ›";
            weatherLabel.text=$"Işık     {new[]{"Öğleden sonra","Şafak","Gün batımı","Ay ışığı"}[weatherIndex]}   ›";
            if(seaWeatherLabel)seaWeatherLabel.text=$"Hava     {new[]{"Açık","Bulutlu","Fırtına","Gezici hava"}[atmosphere?atmosphere.weatherMode:0]}   ›";
            if(cycleLabel)cycleLabel.text=$"Gün döngüsü     {(atmosphere&&atmosphere.advanceDay?"Açık · 24 dakika":"Sabit saat")}   ›";
        }
        void CycleResolution(){int current=System.Array.FindIndex(resolutions,r=>r.x==Screen.width&&r.y==Screen.height);if(current>=0)resolutionIndex=current;previousResolutionIndex=resolutionIndex;resolutionIndex=(resolutionIndex+1)%resolutions.Length;ChangeDisplay(resolutions[resolutionIndex],Screen.fullScreenMode);}
        void CycleMode(){previousResolutionIndex=resolutionIndex;ChangeDisplay(new Vector2Int(Screen.width,Screen.height),Screen.fullScreen?FullScreenMode.Windowed:FullScreenMode.FullScreenWindow);}
        void ChangeDisplay(Vector2Int size,FullScreenMode mode)
        {
            oldResolution=new(Screen.width,Screen.height);oldMode=Screen.fullScreenMode;
            Screen.SetResolution(size.x,size.y,mode);confirm.SetActive(true);restoreCountdown=12;
            settingsInput.interactable=false;EventSystem.current.SetSelectedGameObject(confirm.GetComponentInChildren<Button>().gameObject);
        }
        void KeepResolution(){restoreCountdown=0;confirm.SetActive(false);settingsInput.interactable=true;PlayerPrefs.SetInt("width",Screen.width);PlayerPrefs.SetInt("height",Screen.height);PlayerPrefs.SetInt("fullscreen",Screen.fullScreen?1:0);PlayerPrefs.Save();RefreshLabels();EventSystem.current.SetSelectedGameObject(resolutionLabel.transform.parent.gameObject);}
        void RestoreResolution(){restoreCountdown=0;confirm.SetActive(false);settingsInput.interactable=true;Screen.SetResolution(oldResolution.x,oldResolution.y,oldMode);resolutionIndex=previousResolutionIndex;RefreshLabels();EventSystem.current.SetSelectedGameObject(resolutionLabel.transform.parent.gameObject);}
        void CycleQuality(){qualityIndex=(qualityIndex+1)%3;ApplyQuality();PlayerPrefs.SetInt("quality",qualityIndex);RefreshLabels();}
        void ApplyQuality(){QualitySettings.lodBias=new[]{.7f,1f,1.6f}[qualityIndex];if(UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline is UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset p){p.renderScale=new[]{.75f,1f,1.15f}[qualityIndex];p.shadowDistance=new[]{180f,360f,600f}[qualityIndex];p.shadowCascadeCount=4;p.cascade4Split=new Vector3(.025f,.083333f,.3f);}}
        public void ApplyWeather()
        {
            if(atmosphere)atmosphere.SetHour(new[]{15.2f,6.6f,18.3f,22.5f}[weatherIndex]);
        }

        RectTransform Rect(string name,Transform parent,Vector2 pos,Vector2 size)
        {
            var rt=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();rt.SetParent(parent,false);rt.anchorMin=rt.anchorMax=new Vector2(0,1);rt.pivot=new Vector2(0,1);rt.anchoredPosition=new Vector2(pos.x,-pos.y);rt.sizeDelta=size;return rt;
        }
        GameObject Panel(string name,Transform parent,Vector2 pos,Vector2 size,Color color,bool stretch=false,bool center=false)
        {
            var rt=Rect(name,parent,pos,size);if(stretch){rt.anchorMin=Vector2.zero;rt.anchorMax=Vector2.one;rt.offsetMin=rt.offsetMax=Vector2.zero;}
            if(center){rt.anchorMin=rt.anchorMax=rt.pivot=new Vector2(.5f,.5f);rt.anchoredPosition=Vector2.zero;}
            if(color.a>0){var im=rt.gameObject.AddComponent<Image>();im.color=color;im.raycastTarget=true;}return rt.gameObject;
        }
        TMP_Text Text(Transform parent,string value,Vector2 pos,Vector2 size,float fontSize,Color color,float spacing=0)
        {
            var rt=Rect(value,parent,pos,size);var t=rt.gameObject.AddComponent<TextMeshProUGUI>();if(font)t.font=font;t.text=value;t.fontSize=fontSize;t.color=color;t.characterSpacing=spacing;t.textWrappingMode=TextWrappingModes.Normal;t.raycastTarget=false;return t;
        }
        Button Button(Transform parent,string label,Vector2 pos,Vector2 size,UnityEngine.Events.UnityAction action,bool primary=false)
        {
            var rt=Rect(label,parent,pos,size);var im=rt.gameObject.AddComponent<Image>();im.color=primary?cream:new Color(.9f,.94f,.87f,.065f);
            var b=rt.gameObject.AddComponent<Button>();var colors=b.colors;colors.normalColor=Color.white;colors.highlightedColor=new Color(.8f,.9f,.85f);colors.pressedColor=new(.65f,.75f,.7f);colors.selectedColor=colors.highlightedColor;b.colors=colors;b.onClick.AddListener(action);
            var text=Text(rt,label,new(20,0),size-new Vector2(35,0),20,primary?ink:cream,1);text.alignment=TextAlignmentOptions.MidlineLeft;return b;
        }
        void Slider(Transform parent,string label,float y,float min,float max,float value,UnityEngine.Events.UnityAction<float> changed)
        {
            Text(parent,label,new(55,y),new(540,30),18,cream);
            var rt=Rect(label+" slider",parent,new(55,y+45),new(540,22));rt.gameObject.AddComponent<Image>().color=Color.clear;var s=rt.gameObject.AddComponent<Slider>();s.minValue=min;s.maxValue=max;
            var bg=Panel("Track",rt,new(0,8),new(540,3),new Color(.6f,.7f,.65f,.3f));bg.GetComponent<Image>().raycastTarget=false;
            var handle=Panel("Handle",rt,Vector2.zero,new(18,22),cream).GetComponent<RectTransform>();s.handleRect=handle;s.targetGraphic=handle.GetComponent<Image>();s.direction=UnityEngine.UI.Slider.Direction.LeftToRight;s.value=value;s.onValueChanged.AddListener(changed);
        }
        void MakeVeil(Transform parent)
        {
            var tex=new Texture2D(128,1,TextureFormat.RGBA32,false);for(int x=0;x<128;x++){float a=.77f*Mathf.Pow(1-x/127f,2.2f);tex.SetPixel(x,0,new Color(.035f,.12f,.15f,a));}tex.Apply();
            var go=Panel("Menu shade",parent,Vector2.zero,Vector2.zero,Color.clear,true);var raw=go.AddComponent<RawImage>();raw.texture=tex;raw.raycastTarget=false;go.transform.SetAsFirstSibling();
        }
    }
}
