using System.Collections.Generic;
using System.Threading.Tasks;

// Use global:: qualifiers for Android types to avoid conflicts with project namespaces

namespace Tips_Player.Platforms.Android.Services
{
    public static class AndroidLibraryScanner
    {
        public static Task<List<global::Android.Net.Uri>> ScanAudioAsync(global::Android.Content.Context ctx)
        {
            return Task.Run(() =>
            {
                var results = new List<global::Android.Net.Uri>();
                var uri = global::Android.Provider.MediaStore.Audio.Media.ExternalContentUri;
                string[] projection = {
                    global::Android.Provider.MediaStore.Audio.Media.InterfaceConsts.Id,
                    global::Android.Provider.MediaStore.Audio.Media.InterfaceConsts.IsMusic
                };

                using var cursor = ctx.ContentResolver?.Query(uri!, projection, null, null, null);
                if (cursor == null) return results;

                int idIndex = cursor.GetColumnIndexOrThrow(global::Android.Provider.MediaStore.Audio.Media.InterfaceConsts.Id);
                int isMusicIndex = cursor.GetColumnIndexOrThrow(global::Android.Provider.MediaStore.Audio.Media.InterfaceConsts.IsMusic);

                while (cursor.MoveToNext())
                {
                    try
                    {
                        var isMusic = cursor.GetInt(isMusicIndex);
                        if (isMusic == 0) continue;
                        long id = cursor.GetLong(idIndex);
                        var contentUri = global::Android.Content.ContentUris.WithAppendedId(global::Android.Provider.MediaStore.Audio.Media.ExternalContentUri!, id);
                        results.Add(contentUri);
                    }
                    catch { /* swallow per-item errors, optionally log */ }
                }

                return results;
            });
        }

        public static Task<List<global::Android.Net.Uri>> ScanVideoAsync(global::Android.Content.Context ctx)
        {
            return Task.Run(() =>
            {
                var results = new List<global::Android.Net.Uri>();
                var uri = global::Android.Provider.MediaStore.Video.Media.ExternalContentUri;
                string[] projection = {
                    global::Android.Provider.MediaStore.Video.Media.InterfaceConsts.Id
                };

                using var cursor = ctx.ContentResolver?.Query(uri!, projection, null, null, null);
                if (cursor == null) return results;

                int idIndex = cursor.GetColumnIndexOrThrow(global::Android.Provider.MediaStore.Video.Media.InterfaceConsts.Id);

                while (cursor.MoveToNext())
                {
                    try
                    {
                        long id = cursor.GetLong(idIndex);
                        var contentUri = global::Android.Content.ContentUris.WithAppendedId(global::Android.Provider.MediaStore.Video.Media.ExternalContentUri!, id);
                        results.Add(contentUri);
                    }
                    catch { }
                }

                return results;
            });
        }
    }
}
