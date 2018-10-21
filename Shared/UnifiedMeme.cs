using System;

namespace Shared
{
    public class UnifiedMeme
    {
        public string Id => HashingExtensions.Md5Hash(Permalink);

        public string Url;
        public string Permalink;
        public string Title;
        public int Likes;
        public DateTimeOffset Published;
        public DateTimeOffset? Collected;
        public Media[] Media;
        public string Source;
        public string OriginalId;
        public string OriginalType;
        public int CommentsCount;
        public string[] Tags;
    }

    public class Media
    {
        public static Media[] From(params Media[] media) => media;
        public string Url;
        public string Type;
        public int Width;
        public int Height;
    }
}
