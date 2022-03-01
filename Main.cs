using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using VRC.Core;
using VRC.UI;

namespace InstanceDejavu
{
    public class InstanceDejavu : MelonMod
    {
        private static HashSet<string> _visitedWorldHashSet;
        private static GameObject _worldVisitedIcon;
        private static UiWorldInstanceList _uiWorldInstanceListInstance;
        private static GameObject _visitedButton;
        private static Sprite _visitedSprite;
        
        public override void OnApplicationStart()
        {
            if (Environment.CommandLine.Contains("streamsafe")) return;
            
            _visitedWorldHashSet = new HashSet<string>();
            _visitedSprite = LoadSpriteFromResource("visited");
            
            foreach (var m in typeof(UiWorldInstanceList).GetMethods().Where(m =>
                m.Name.StartsWith("Method_Protected_Virtual_Void_VRCUiContentButton_Object_")))
            {
                HarmonyInstance.Patch(m, postfix: new HarmonyMethod(typeof(InstanceDejavu), nameof(OnInstanceContentButtonGenerationPostfix)));
            }
            
            var selectWorldInstance = typeof(PageWorldInfo).GetMethods().Single(m => XrefUtils.CheckMethod(m, "Make Home") && m.Name.StartsWith("Method_Private_Void_") && m.Name.Length<22);
            HarmonyInstance.Patch(selectWorldInstance, postfix: new HarmonyMethod(typeof(InstanceDejavu), nameof(UpdateWorldMainPicker)));
            
            HarmonyInstance.Patch(typeof(RoomManager).GetMethod(nameof(RoomManager.Method_Public_Static_Boolean_ApiWorld_ApiWorldInstance_String_Int32_0)), 
                postfix: new HarmonyMethod(typeof(InstanceDejavu), nameof(OnEnterWorld)));
            
            MelonCoroutines.Start(OnUiManagerInit());
        }

        private IEnumerator OnUiManagerInit()
        {
            while (VRCUiManager.prop_VRCUiManager_0 == null) yield return null;
            InitializeVisitedIcon();
        }
        
        private Sprite LoadSpriteFromResource(string name)
        {
            var ass = Assembly.GetExecutingAssembly();

            var stream = ass.GetManifestResourceStream($"InstanceDejavu.Resources.{name}.png");
            using var ms = new MemoryStream();
            stream.CopyTo(ms);

            var texture = new Texture2D(1, 1);
            ImageConversion.LoadImage(texture, ms.ToArray());
            texture.hideFlags |= HideFlags.DontUnloadUnusedAsset;
            texture.wrapMode = TextureWrapMode.Clamp;
            
            var rect = new Rect(0.0f, 0.0f, texture.width, texture.height);
            var pivot = new Vector2(0.5f, 0.5f);
            var border = Vector4.zero;
            var sprite = Sprite.CreateSprite_Injected(texture, ref rect, ref pivot, 100.0f, 0, SpriteMeshType.Tight, ref border, false);
            sprite.hideFlags |= HideFlags.DontUnloadUnusedAsset;

            return sprite;
        }
        
        private static void UpdateWorldMainPicker()
        {
            var pageWorldInfo = GameObject.Find("UserInterface/MenuContent/Screens/WorldInfo")
                .GetComponent<PageWorldInfo>();

            if (pageWorldInfo.field_Private_ApiWorld_0 == null ||
                pageWorldInfo.field_Public_ApiWorldInstance_0 == null) return;

            UpdateSelectedWorldVisited(pageWorldInfo.field_Private_ApiWorld_0,
                pageWorldInfo.field_Public_ApiWorldInstance_0);
        }
        
        private void InitializeVisitedIcon()
        {
            var worldInfoScreen = GameObject.Find("UserInterface/MenuContent/Screens/WorldInfo/").transform;
            var worldIconBase = worldInfoScreen.Find("WorldImage/OverlayIcons/FavoriteIcon/").gameObject;

            _uiWorldInstanceListInstance = worldInfoScreen.Find("OtherInstances").GetComponent<UiWorldInstanceList>();

            _worldVisitedIcon = UnityEngine.Object.Instantiate(worldIconBase, worldIconBase.transform.parent.transform);
            _worldVisitedIcon.name = "iconVisitedBefore";
            _worldVisitedIcon.transform.Find("IconImage").GetComponent<Image>().sprite = _visitedSprite;
            _worldVisitedIcon.transform.localPosition = new Vector3(-110, 188, 0);
            _worldVisitedIcon.SetActive(false);
        }

        private static Transform GenerateIcon(VRCUiContentButton button)
        {
            if (_visitedButton == null)
            {
                var original = button.transform.Find("RoomImageShape/RoomImage/OverlayIcons/iconFavoriteStar").gameObject;
                var newIcon = UnityEngine.Object.Instantiate(original, original.transform.parent.transform);
                newIcon.name = "iconVisitedBefore";
                var img = newIcon.GetComponent<Image>();
                img.sprite = _visitedSprite;
                img.color = new Color(0.4157f,0.8902f,0.9765f,1f);
                newIcon.SetActive(false);
                _visitedButton = newIcon;
                return newIcon.transform;
            }

            var visitedIcon = UnityEngine.Object.Instantiate(_visitedButton,
                button.transform.Find("RoomImageShape/RoomImage/OverlayIcons/"));

            visitedIcon.name = "iconVisitedBefore";
            return visitedIcon.transform;
        }

        private static void UpdateSelectedWorldVisited(ApiWorld world, ApiWorldInstance instance)
        {
            var a = _visitedWorldHashSet.Contains($"{world.name}-{instance.instanceId}");
            _worldVisitedIcon.SetActive(a);
        }

        private static void OnInstanceContentButtonGenerationPostfix(VRCUiContentButton __0, ApiWorldInstance __1)
        {
            var icon = __0.transform.Find("RoomImageShape/RoomImage/OverlayIcons/iconVisitedBefore")?.gameObject;
            if (icon == null)
            {
                icon = GenerateIcon(__0).gameObject;
            }
            
            var instanceId = __1.instanceId;
            
            if (_visitedWorldHashSet.Contains(
                $"{_uiWorldInstanceListInstance.field_Public_ApiWorld_0.name}-{instanceId}"))
            {
                icon.transform.parent.gameObject.SetActive(true);
                icon.SetActive(true);
            }
            else
            {
                icon.SetActive(false);
            }
        }
        
        public static void OnEnterWorld(ApiWorld __0, ApiWorldInstance __1)
        {
            _visitedWorldHashSet.Add($"{__0.name}-{__1.instanceId}");
        }
    }
}