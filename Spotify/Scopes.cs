using System;
using System.Collections.Generic;

namespace Spotify
{
    class Scope
    {
        // https://developer.spotify.com/documentation/general/guides/scopes/

        #region Fields & Properties

        private HashSet<string> scopes = new HashSet<string>();

        #endregion

        #region Scopes

        public static readonly Scope NullScope = new Scope();

        public static readonly Scope UgcImageUpload = new Scope("ugc-image-upload");

        public static readonly Scope UserReadPlaybackState = new Scope("user-read-playback-state");
        public static readonly Scope UserModifyPlaybackState = new Scope("user-modify-playback-state");
        public static readonly Scope UserReadCurrentlyPlaying = new Scope("user-read-currently-playing");

        public static readonly Scope Streaming = new Scope("streaming");
        public static readonly Scope AppRemoteControl = new Scope("app-remote-control");

        public static readonly Scope UserReadEmail = new Scope("user-read-email");
        public static readonly Scope UserReadPrivate = new Scope("user-read-private");

        public static readonly Scope PlaylistReadCollaborative = new Scope("playlist-read-collaborative");
        public static readonly Scope PlaylistModifyPublic = new Scope("playlist-modify-public");
        public static readonly Scope PlaylistReadPrivate = new Scope("playlist-read-private");
        public static readonly Scope PlaylistModifyPrivate = new Scope("playlist-modify-private");

        public static readonly Scope UserLibraryModify = new Scope("user-library-modify");
        public static readonly Scope UserLibraryRead = new Scope("user-library-read");

        public static readonly Scope UserTopRead = new Scope("user-top-read");
        public static readonly Scope UserReadPlaybackPosition = new Scope("user-read-playback-position");
        public static readonly Scope UserReadRecentlyPlayed = new Scope("user-read-recently-played");

        public static readonly Scope UserFollowRead = new Scope("user-follow-read");
        public static readonly Scope UserFollowModify = new Scope("user-follow-modify");

        #endregion

        #region Constructors

        public Scope(string scopeName = null)
        {
            if (!String.IsNullOrEmpty(scopeName))
                scopes.Add(scopeName);
        }

        public Scope(Scope a, Scope b)
        {
            if (!String.IsNullOrEmpty(a.ToString()))
                scopes.UnionWith(a.scopes);
            if (!String.IsNullOrEmpty(b.ToString()))
                scopes.UnionWith(b.scopes);
        }

        #endregion

        #region + Operators

        public static Scope operator + (Scope a, Scope b)
        {
            return new Scope(a, b);
        }

        #endregion

        #region Parsig Methods

        public override string ToString()
        {
            return String.Join(" ", scopes);
        }

        static public Scope FromString(string rawScopes)
        {
            Scope result = new Scope();

            foreach (string rawScope in rawScopes.Split(" "))
                result += new Scope(rawScope);

            return result;
        }

        #endregion
    }
}