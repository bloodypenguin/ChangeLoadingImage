using System;
using System.Net;
using System.Net.Security;
using System.Reflection;
using CitiesHarmony.API;
using HarmonyLib;
using ICities;
using UnityEngine;

namespace ChangeLoadingImage
{
    public class LoadingExtension : LoadingExtensionBase
    {
        private Harmony HarmonyInstance;
        private RemoteCertificateValidationCallback Callback = (sender, cert, chain, sslPolicyErrors) => true;

        public override void OnCreated(ILoading loading)
        {
            base.OnCreated(loading);
            if (!HarmonyHelper.IsHarmonyInstalled)
            {
                return;
            }
            ServicePointManager.ServerCertificateValidationCallback += Callback;
            HarmonyInstance = new Harmony("github.com/bloodypenguin/ChangeLoadingImage");
            var original = GetOriginal();
            var prefix = typeof(LoadingAnimationPatch).GetMethod("Prefix", BindingFlags.Static | BindingFlags.Public);
            HarmonyInstance.Patch(original, new HarmonyMethod(prefix));
        }

        private MethodBase GetOriginal()
        {
            try
            {
                var loadingScreenModType = Util.FindType("LoadingScreenMod.LoadingScreen");
                if (loadingScreenModType != null)
                {
                    if (Util.IsModActive(667342976) || Util.IsModActive(833779378) || Util.IsModActive(2731207699))
                    {
                        Debug.LogWarning("LoadingScreenMod was detected as active. Applying workaround...");
                        return loadingScreenModType.GetMethod("SetImage", BindingFlags.Instance | BindingFlags.Public);
                    }
                    else
                    {
                        Debug.LogWarning("LoadingScreenMod was detected as disabled");
                    }
                }
                else
                {
                    Debug.LogWarning("LoadingScreenMod was not detected");
                }
            }
            catch (Exception e)
            {
                Debug.LogError(
                    "Due to some unexpected problems Change Loading Image 2 wasn't able to detect if LoadingScreenMod is active");
                Debug.LogException(e);
            }
            return typeof(LoadingAnimation).GetMethod("SetImage", BindingFlags.Instance | BindingFlags.Public);
        }

        public override void OnReleased()
        {
            base.OnReleased();
            if (!HarmonyHelper.IsHarmonyInstalled)
            {
                return;
            }
            ServicePointManager.ServerCertificateValidationCallback -= Callback;
            HarmonyInstance?.UnpatchAll();
        }
    }
}
