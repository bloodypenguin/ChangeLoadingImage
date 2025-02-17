using System;
using System.Collections.Generic;
using System.Net;
using ChangeLoadingImage.OptionsFramework;
using UnityEngine;
using Random = System.Random;

namespace ChangeLoadingImage
{
    public static class OverrideLoadingImage
    {
        public static void OverrideTextureAndScale(ref Material material, ref float scale)
        {
            try
            {
                Texture newTexture;
                var mode = (ImageType)XmlOptionsWrapper<Settings>.Options.Mode;
                switch (mode)
                {
                    case ImageType.ClassicEnvironmentImage:
                        if (SimulationManager.instance.m_metaData.m_environment == "Winter")
                        {
                            return;
                        }

                        newTexture = GetClassicImageForEnvironment();
                        break;
                    case ImageType.RandomImageFromImgur:
                        newTexture = GetRandomImgurImage(false);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                scale = getScaleFactor(newTexture);
                material = new Material(material) {mainTexture = newTexture};
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        
        
        private static Texture GetClassicImageForEnvironment()
        {
            var env = SimulationManager.instance.m_metaData.m_environment;
            var fileName = $"{Util.AssemblyDirectory}/{env} Loading Image.png";
            return Util.LoadTextureFromFile(fileName);
        }

        private static Texture GetRandomImgurImage(bool fromPredefinedList)
        {
            var attempt = 0;
            while (attempt < 10)
            {
                ++attempt;
                var pageNumber = new Random().Next(10);
                var entries = fromPredefinedList
                    ? ImgurImages.DefaultImageList
                    : ImgurImages.ImageListFromImgur(pageNumber);
                var entry = SelectFrom(entries);
                if (entry == null)
                {
                    throw new Exception("No entry was selected");
                }

                try
                {
                    return HandleHttp(entry.uri);
                }
                catch (Exception)
                {
                    // suppress
                }
            }

            throw new Exception("Failed to load an image from imgur");
        }

        private static ImageListEntry SelectFrom(IList<ImageListEntry> entries)
        {
            if (entries.Count == 0)
                return null;

            var random = new Random();
            return entries[random.Next(entries.Count)];
        }

        private static Texture HandleHttp(string uri)
        {
            var imgData = new WebClient().DownloadData(uri);
            var bg = new Texture2D(1, 1);
            bg.LoadImage(imgData);
            if (bg.width < 1920 || bg.height < 1080)
            {
                throw new Exception("image is too small: " + uri);
            }

            return bg;
        }

        private static float getScaleFactor(Texture texture)
        {
            var scaleFactor = 1f;
            if (texture == null)
                return scaleFactor;

            float screenWidth = Screen.currentResolution.width;
            float screenHeight = Screen.currentResolution.height;

            var imgWidth = texture.width;
            var imgHeight = texture.height;

            var widthFactor = screenWidth / imgWidth;
            var heightFactor = screenHeight / imgHeight;

            if (widthFactor > heightFactor)
            {
                var temp = heightFactor * imgWidth;
                scaleFactor = screenWidth / temp;
            }
            else
            {
                var temp = widthFactor * imgHeight;
                scaleFactor = screenHeight / temp;
            }

            return scaleFactor;
        }
    }
}