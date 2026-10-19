using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace JustSail
{
    [DefaultExecutionOrder(30)]
    public sealed class VoyageJournal : MonoBehaviour
    {
        [Serializable] public class Photograph {public string file,caption;}
        [Serializable] public class Album {public List<Photograph> photos=new();}
        public SailUI ui;
        public MooringLine mooring;
        public VoyageAtmosphere weather;
        public bool PhotoMode {get;private set;}
        public bool AlbumOpen {get;private set;}
        public int PhotoCount=>album.photos.Count;
        public string StoragePath {get;private set;}
        public string LastPhotoPath {get;private set;}
        Album album=new();
        Canvas canvas;
        RectTransform root;
        GameObject gallery,viewer,viewfinder,instant,grid,albumShade,promptShade;
        RawImage large,instantImage;
        TMP_Text caption,pageText,prompt,toast,instantCaption,focusText;
        Button previous,next;
        int page;
        bool capturing,resting;
        float toastUntil,instantUntil;
        Texture2D instantTexture;
        readonly List<Texture2D> thumbnails=new();
        readonly Color ink=new(.055f,.13f,.16f,.97f),paper=new(.95f,.92f,.83f),muted=new(.64f,.72f,.7f);
        AudioSource shutter;
        AudioClip click;
        void Start()
        {
            if(!ui)ui=FindFirstObjectByType<SailUI>();if(!weather)weather=FindFirstObjectByType<VoyageAtmosphere>();
            if(!mooring)mooring=FindFirstObjectByType<MooringLine>();
            UseStorage(Path.Combine(Application.persistentDataPath,"SeyirDefteri"));
            root=new GameObject("Seyir Defteri UI",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster)).GetComponent<RectTransform>();
            root.SetParent(transform,false);canvas=root.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=25;
            var scaler=root.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;
            albumShade=Panel(root,"Defter zemini",0,0,0,0,ink);var shade=albumShade.GetComponent<RectTransform>();shade.anchorMin=Vector2.zero;shade.anchorMax=Vector2.one;shade.offsetMin=shade.offsetMax=Vector2.zero;albumShade.SetActive(false);
            root=Panel(root,"Diorama arayüz alanı",0,0,1920,1080,Color.clear).GetComponent<RectTransform>();root.anchorMin=root.anchorMax=root.pivot=new Vector2(.5f,.5f);root.anchoredPosition=Vector2.zero;
            gallery=Panel(root,"Seyir defteri",0,0,1920,1080,ink);Text(gallery.transform,"SEYİR DEFTERİ",100,72,1000,55,34,paper);
            Text(gallery.transform,"Yolun sende bıraktıkları.",100,130,1100,40,21,muted);
            ActionButton(gallery.transform,"Denize dön  /  Tab",1500,80,300,52,CloseAlbum);
            grid=Panel(gallery.transform,"Fotoğraflar",0,0,1920,1080,Color.clear);
            previous=ActionButton(gallery.transform,"<  Önceki",100,960,230,50,()=>{page=Mathf.Max(0,page-1);RefreshAlbum();});
            next=ActionButton(gallery.transform,"Sonraki  >",1590,960,230,50,()=>{page++;RefreshAlbum();});
            pageText=Text(gallery.transform,"",740,969,440,40,19,muted);pageText.alignment=TextAlignmentOptions.Center;
            viewer=Panel(root,"Fotoğraf",0,0,1920,1080,new Color(ink.r,ink.g,ink.b,1));
            large=Panel(viewer.transform,"Büyük fotoğraf",220,90,1480,830,Color.clear).AddComponent<RawImage>();large.color=Color.white;
            large.rectTransform.pivot=new Vector2(.5f,.5f);
            large.gameObject.AddComponent<AspectRatioFitter>().aspectMode=AspectRatioFitter.AspectMode.FitInParent;
            // The image has its own fixed frame, so fitting cannot expand into the toolbar.
            var frame=Panel(viewer.transform,"Fotoğraf çerçevesi",220,90,1480,830,Color.clear);large.rectTransform.SetParent(frame.transform,false);large.rectTransform.anchorMin=Vector2.zero;large.rectTransform.anchorMax=Vector2.one;large.rectTransform.offsetMin=large.rectTransform.offsetMax=Vector2.zero;
            caption=Text(viewer.transform,"",220,950,1100,50,23,paper);ActionButton(viewer.transform,"Deftere dön",1430,950,270,50,()=>viewer.SetActive(false));
            viewfinder=Panel(root,"Vizör",0,0,1920,1080,Color.clear);
            for(int i=0;i<4;i++){float x=i%2==0?350:1550,y=i<2?200:800;Panel(viewfinder.transform,"Vizör köşesi",x,y,42,3,paper).GetComponent<Image>().raycastTarget=false;Panel(viewfinder.transform,"Vizör kenarı",i%2==0?x:x+39,i<2?y:y-39,3,42,paper).GetComponent<Image>().raycastTarget=false;}
            focusText=Text(viewfinder.transform,"",520,870,900,55,20,paper);focusText.alignment=TextAlignmentOptions.Center;
            Text(viewfinder.transform,"Sol tık / Space  Çek     F  Vizörü kapat     Sağ fare  Bakış",430,940,1200,45,18,paper);
            instant=Panel(root,"Yeni fotoğraf",1470,590,350,290,paper);instant.transform.localRotation=Quaternion.Euler(0,0,-4);
            instantImage=Panel(instant.transform,"Görüntü",15,15,320,190,Color.clear).AddComponent<RawImage>();instantImage.raycastTarget=false;
            instantCaption=Text(instant.transform,"",18,220,315,55,16,ink);
            promptShade=Panel(root,"Palamar yardımı",420,900,1080,70,new Color(.055f,.13f,.16f,.8f));prompt=Text(promptShade.transform,"",0,0,1080,70,22,paper);prompt.alignment=TextAlignmentOptions.Center;
            toast=Text(root,"",420,180,1080,65,23,paper);toast.alignment=TextAlignmentOptions.Center;
            gallery.SetActive(false);viewer.SetActive(false);viewfinder.SetActive(false);instant.SetActive(false);
            shutter=gameObject.AddComponent<AudioSource>();shutter.playOnAwake=false;shutter.spatialBlend=0;shutter.volume=.16f;
            var samples=new float[2205];for(int i=0;i<samples.Length;i++)samples[i]=Mathf.Sin(i*1.83f)*Mathf.Exp(-i/230f)*.5f;
            click=AudioClip.Create("Shutter",samples.Length,1,22050,false);click.SetData(samples,0);
        }
        public void UseStorage(string folder)
        {
            StoragePath=Path.GetFullPath(folder);Directory.CreateDirectory(StoragePath);album=new Album();
            string index=Path.Combine(StoragePath,"album.json");
            if(File.Exists(index))try{album=JsonUtility.FromJson<Album>(File.ReadAllText(index))??new Album();}catch(Exception e){Debug.LogWarning("Photo index could not be read: "+e.Message);}
            album.photos??=new List<Photograph>();album.photos.RemoveAll(p=>p==null||string.IsNullOrEmpty(p.file)||Path.GetFileName(p.file)!=p.file||!File.Exists(Path.Combine(StoragePath,p.file)));
            var known=new HashSet<string>();foreach(var photo in album.photos)known.Add(photo.file);
            foreach(var file in Directory.GetFiles(StoragePath,"*.jpg")){string name=Path.GetFileName(file);if(!known.Contains(name))album.photos.Add(new Photograph{file=name,caption="Denizden bir an"});}
        }
        void Update()
        {
            if(!ui||!canvas)return;var keys=Keyboard.current;
            if(AlbumOpen)
            {
                if(keys?.tabKey.wasPressedThisFrame==true||keys?.escapeKey.wasPressedThisFrame==true){if(viewer.activeSelf)viewer.SetActive(false);else CloseAlbum();}
            }
            else if(ui.Playing&&!ui.MenuOpen&&!resting)
            {
                if(keys?.fKey.wasPressedThisFrame==true)SetPhotoMode(!PhotoMode);
                if(keys?.tabKey.wasPressedThisFrame==true)OpenAlbum();
                if(PhotoMode&&(keys?.spaceKey.wasPressedThisFrame==true||Mouse.current?.leftButton.wasPressedThisFrame==true))TakePhoto();
                if(keys?.rKey.wasPressedThisFrame==true)WaitUntilMorning();
            }
            if(ui.MenuOpen&&!AlbumOpen&&PhotoMode)SetPhotoMode(false);
            if(PhotoMode)focusText.text=Subject()+"     ·     C  Odak: "+(ui.cameraRig.softFocus?"Yumuşak":"Net");
            prompt.text=ui.MenuOpen||PhotoMode||resting?"":mooring&&mooring.Moored?"E  Palamarı çöz"+(weather.Night>.4f?"     ·     R  Sabahı bekle":""):mooring&&mooring.CanAttach?"E  İskeleye bağlan":"";
            promptShade.SetActive(prompt.text.Length>0);toast.gameObject.SetActive(Time.unscaledTime<toastUntil&&!AlbumOpen);
            instant.SetActive(Time.unscaledTime<instantUntil&&!AlbumOpen&&!ui.MenuOpen);
            if(instant.activeSelf){float elapsed=4-(instantUntil-Time.unscaledTime);var rt=instant.GetComponent<RectTransform>();rt.anchoredPosition=new Vector2(1470,-590-400*(1-Mathf.SmoothStep(0,1,elapsed/.4f)));rt.localRotation=Quaternion.Euler(0,0,-4+Mathf.Sin(elapsed*8)*Mathf.Exp(-elapsed*6)*5);}
        }
        string Subject()
        {
            var camera=ui.cameraRig.GetComponent<Camera>();var planes=GeometryUtility.CalculateFrustumPlanes(camera);
            string subject="Manzara";float nearest=120;
            foreach(var animal in FindObjectsByType<WildlifeActor>(FindObjectsSortMode.None))
            {
                var body=animal.GetComponentInChildren<MeshRenderer>();float distance=Vector3.Distance(camera.transform.position,animal.transform.position);
                if(body&&distance<nearest&&GeometryUtility.TestPlanesAABB(planes,body.bounds)&&(!animal.dolphin||animal.transform.position.y>-.5f)){subject=animal.Subject;nearest=distance;}
            }
            return subject;
        }
        public void SetPhotoMode(bool value)
        {
            PhotoMode=value;viewfinder.SetActive(value);ui.SetPhotoHud(value);if(value)instantUntil=0;
        }
        public void TakePhoto(){if(PhotoMode&&!capturing&&!ui.MenuOpen)StartCoroutine(Capture());}
        IEnumerator Capture()
        {
            capturing=true;string subject=Subject();canvas.enabled=false;
            yield return new WaitForEndOfFrame();
            try
            {
                if(ui.MenuOpen)yield break;
                var image=ScreenCapture.CaptureScreenshotAsTexture();
                string name=DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fffffff")+".jpg";
                LastPhotoPath=Path.Combine(StoragePath,name);File.WriteAllBytes(LastPhotoPath,image.EncodeToJPG(90));
                var location=Vector3.Distance(ui.boat.transform.position,new Vector3(-105,0,100))<210?"Sakin Koy":Vector3.Distance(ui.boat.transform.position,new Vector3(145,0,490))<210?"Fener Kayalığı":"Açık Deniz";
                int minutes=Mathf.FloorToInt(weather.hours*60);string note=location+"  ·  "+(minutes/60).ToString("D2")+":"+(minutes%60).ToString("D2")+"\n"+subject;
                album.photos.Add(new Photograph{file=name,caption=note});File.WriteAllText(Path.Combine(StoragePath,"album.json"),JsonUtility.ToJson(album,true));
                if(instantTexture)Destroy(instantTexture);instantTexture=image;instantImage.texture=image;instantCaption.text=note;instantUntil=Time.unscaledTime+4;
                shutter.PlayOneShot(click);Toast("Fotoğraf seyir defterine eklendi.");
            }
            finally{canvas.enabled=true;capturing=false;}
        }
        public void OpenAlbum()
        {
            if(!ui.Playing||AlbumOpen)return;
            SetPhotoMode(false);AlbumOpen=true;instant.SetActive(false);toast.gameObject.SetActive(false);albumShade.SetActive(true);ui.OpenJournal();page=0;gallery.SetActive(true);viewer.SetActive(false);RefreshAlbum();
        }
        public void CloseAlbum(){viewer.SetActive(false);gallery.SetActive(false);albumShade.SetActive(false);AlbumOpen=false;ui.CloseJournal();}
        public void WaitUntilMorning(){if(!resting&&!ui.MenuOpen&&mooring&&mooring.Moored&&weather.Night>.4f)StartCoroutine(Rest());}
        void RefreshAlbum()
        {
            foreach(Transform child in grid.transform)Destroy(child.gameObject);foreach(var t in thumbnails)Destroy(t);thumbnails.Clear();
            int pages=Mathf.Max(1,(album.photos.Count+5)/6);page=Mathf.Clamp(page,0,pages-1);
            pageText.text=album.photos.Count==0?"Henüz bir fotoğraf yok":$"{page+1} / {pages}    ·    {album.photos.Count} fotoğraf";
            previous.interactable=page>0;next.interactable=page<pages-1;
            if(album.photos.Count==0){Text(grid.transform,"Denizde F ile vizörü aç.\nSevdiğin bir anı sol tık veya Space ile sakla.",420,430,1080,150,28,paper).alignment=TextAlignmentOptions.Center;return;}
            for(int i=0;i<6;i++)
            {
                int index=album.photos.Count-1-page*6-i;if(index<0)break;var photo=album.photos[index];
                var texture=new Texture2D(2,2);if(!texture.LoadImage(File.ReadAllBytes(Path.Combine(StoragePath,photo.file)))){Destroy(texture);continue;}thumbnails.Add(texture);
                var card=Panel(grid.transform,"Fotoğraf kartı",120+i%3*570,230+i/3*340,530,305,paper);card.transform.localRotation=Quaternion.Euler(0,0,Mathf.Sin(index*7.1f)*1.8f);
                var holder=Panel(card.transform,"Fotoğraf alanı",14,14,502,220,Color.clear);
                var image=Panel(holder.transform,"Fotoğraf",0,0,502,220,Color.clear).AddComponent<RawImage>();image.texture=texture;image.raycastTarget=false;
                image.rectTransform.pivot=new Vector2(.5f,.5f);
                var fit=image.gameObject.AddComponent<AspectRatioFitter>();fit.aspectMode=AspectRatioFitter.AspectMode.FitInParent;fit.aspectRatio=(float)texture.width/texture.height;
                Text(card.transform,photo.caption,22,244,486,55,17,ink);
                var button=card.AddComponent<Button>();button.targetGraphic=card.GetComponent<Image>();button.onClick.AddListener(()=>{large.texture=texture;large.GetComponent<AspectRatioFitter>().aspectRatio=(float)texture.width/texture.height;caption.text=photo.caption.Replace("\n","    ·    ");viewer.SetActive(true);});
            }
        }
        IEnumerator Rest()
        {
            resting=true;bool cycle=weather.advanceDay;weather.advanceDay=false;float from=weather.hours,delta=Mathf.Repeat(6.6f-from,24);
            Toast("İskelede sabahı beklerken…");
            float elapsed=0;while(elapsed<3&&mooring.Moored){if(!ui.MenuOpen){elapsed+=Time.deltaTime;float before=weather.hours;weather.SetHour(from+delta*Mathf.SmoothStep(0,1,elapsed/3));if(weather.hours<before)weather.dayIndex++;}yield return null;}
            weather.advanceDay=cycle;resting=false;
        }
        void Toast(string message){toast.text=message;toastUntil=Time.unscaledTime+3;}
        GameObject Panel(Transform parent,string name,float x,float y,float w,float h,Color color)
        {
            var go=new GameObject(name,typeof(RectTransform));var rt=go.GetComponent<RectTransform>();rt.SetParent(parent,false);rt.anchorMin=rt.anchorMax=new Vector2(0,1);rt.pivot=new Vector2(0,1);rt.anchoredPosition=new Vector2(x,-y);rt.sizeDelta=new Vector2(w,h);
            if(color.a>0)go.AddComponent<Image>().color=color;return go;
        }
        TMP_Text Text(Transform parent,string value,float x,float y,float w,float h,float size,Color color)
        {
            var go=Panel(parent,value,x,y,w,h,Color.clear);var text=go.AddComponent<TextMeshProUGUI>();text.font=ui.font;text.text=value;text.fontSize=size;text.color=color;text.raycastTarget=false;text.textWrappingMode=TextWrappingModes.Normal;return text;
        }
        Button ActionButton(Transform parent,string label,float x,float y,float w,float h,UnityEngine.Events.UnityAction action)
        {
            var go=Panel(parent,label,x,y,w,h,new Color(.9f,.94f,.87f,.1f));var button=go.AddComponent<Button>();button.targetGraphic=go.GetComponent<Image>();button.onClick.AddListener(action);Text(go.transform,label,18,5,w-36,h-5,21,paper).alignment=TextAlignmentOptions.MidlineLeft;return button;
        }
        void OnDestroy(){foreach(var t in thumbnails)if(t)Destroy(t);if(instantTexture)Destroy(instantTexture);if(click)Destroy(click);}
    }
}
