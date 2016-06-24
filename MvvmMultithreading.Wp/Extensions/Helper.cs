using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pr0gramm.Extensions
{
    using System.IO;
    using System.Net;
    using Windows.Storage;
    using Windows.Storage.Streams;
    using Constants;
    using Microsoft.Xna.Framework.Media;
    using Resources;
    using ViewModel;
    using Microsoft.Xna.Framework.Media.PhoneExtensions;

    public static class Helper
    {
        public static event EventHandler<string> OnImageSaved;

        public static DateTime ConvertJsonDateToDateTime(double msSinceEpoch)
        {
            var date = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return date.AddMilliseconds(msSinceEpoch * 1000);
        }

        public static async void DownloadImage(string imageUrl, string postId)
        {
            try
            {
                var fileName = string.Format("programm_{0}{1}", postId, Path.GetExtension(imageUrl));
                var thumbnail = RandomAccessStreamReference.CreateFromUri(new Uri(imageUrl));

                var pr0grammFolder = await KnownFolders.PicturesLibrary.CreateFolderAsync("Pr0gramm", CreationCollisionOption.OpenIfExists);
                var remoteFile = await StorageFile.CreateStreamedFileFromUriAsync(fileName, new Uri(imageUrl), thumbnail);
                var file = await remoteFile.CopyAsync(pr0grammFolder, fileName, NameCollisionOption.ReplaceExisting);

                if (OnImageSaved != null)
                {
                    OnImageSaved.Invoke(null, file.Path);
                }

                ViewModelLocator.ShowNotification(AppResources.SuccessOnDownload, string.Empty);
            }
            catch (Exception)
            {
                ViewModelLocator.ShowNotification(AppResources.ErrorOnDownload, string.Empty);
            }
        }

        public static FagFilter GetFagFilter()
        {
            if (string.IsNullOrEmpty(ViewModelLocator.Settings.FagFilter))
            {
                return FagFilter.Alle;
            }

            return (FagFilter)Enum.Parse(typeof(FagFilter), ViewModelLocator.Settings.FagFilter);
        }

        public static bool StringToBoolean(string stringBool)
        {
            var boolVal = stringBool == "true";
            return boolVal;
        }

        public static int GetFlagFilter()
        {
            var sfwLoaded = StringToBoolean(ViewModelLocator.Settings.LoadSfw);
            var nsfwLoaded = StringToBoolean(ViewModelLocator.Settings.LoadNsfw);
            var nsflLoaded = StringToBoolean(ViewModelLocator.Settings.LoadNsfl);

            if (sfwLoaded && !nsfwLoaded && !nsflLoaded)
            {
                return 1;
            }
            else if (!sfwLoaded && nsfwLoaded && !nsflLoaded)
            {
                return 2;
            }
            else if (!sfwLoaded && !nsfwLoaded && nsflLoaded)
            {
                return 4;
            }
            else if (sfwLoaded && nsfwLoaded && !nsflLoaded)
            {
                return 3;
            }
            else if (sfwLoaded && !nsfwLoaded && nsflLoaded)
            {
                return 5;
            }
            else if (!sfwLoaded && nsfwLoaded && nsflLoaded)
            {
                return 6;
            }
            else if (sfwLoaded && nsfwLoaded && nsflLoaded)
            {
                return 7;
            }

            return 1;
        }
    }
}
