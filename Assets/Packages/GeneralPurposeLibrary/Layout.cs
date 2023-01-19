using System.IO;
using System.Reflection;
using Type = System.Type;

namespace GPL
{
    /// <summary>
    /// Static Class that handles with layouts in the
    /// editor. Only useful within Editor Scripting
    /// </summary>
    public static class LayoutUtility
    {
        private static MethodInfo _miLoadWindowLayout;
        private static MethodInfo _miSaveWindowLayout;
        private static MethodInfo _miReloadWindowLayoutMenu;
        private static bool _available;
        private static string _layoutsPath;


        /// <summary>
        /// Constructor that initialises the communication to the
        /// Unity's Editor Layer Engine.
        /// </summary>
        static LayoutUtility()
        {
            Type tyWindowLayout = Type.GetType("UnityEditor.WindowLayout,UnityEditor");
            Type tyEditorUtility = Type.GetType("UnityEditor.EditorUtility,UnityEditor");
            Type tyInternalEditorUtility = Type.GetType("UnityEditorInternal.InternalEditorUtility,UnityEditor");
            if (tyWindowLayout != null && tyEditorUtility != null && tyInternalEditorUtility != null)
            {
                MethodInfo miGetLayoutsPath = tyWindowLayout.GetMethod("GetLayoutsPath", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
                _miLoadWindowLayout = tyWindowLayout.GetMethod("LoadWindowLayout", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(string), typeof(bool) }, null);
                _miSaveWindowLayout = tyWindowLayout.GetMethod("SaveWindowLayout", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(string) }, null);
                _miReloadWindowLayoutMenu = tyInternalEditorUtility.GetMethod("ReloadWindowLayoutMenu", BindingFlags.Public | BindingFlags.Static);

                if (miGetLayoutsPath == null || _miLoadWindowLayout == null || _miSaveWindowLayout == null || _miReloadWindowLayoutMenu == null)
                    return;

                _layoutsPath = (string)miGetLayoutsPath.Invoke(null, null);
                if (string.IsNullOrEmpty(_layoutsPath))
                    return;

                _available = true;
            }
        }

        /// <summary>
        /// Can we use this Utility at the moment, or is there any error?
        /// 
        /// \warning    Errors might be caused if permissions are missing(not by default)
        ///             or editor crashes
        /// </summary>
        public static bool IsAvailable
        {
            get { return _available; }
        }

        /// <summary>
        /// Returns the path where all Unity's Layouts are stored
        /// </summary>
        public static string LayoutsPath
        {
            get { return _layoutsPath; }
        }

        /// <summary>
        /// Saves the current Layout to the given `assetPath`
        /// The `assetPath` is relative to the Unity's Root Folder
        /// <seealso cref="SaveLayout(string)"/>
        /// </summary>
        /// <param name="assetPath"></param>
        public static void SaveLayoutToAsset(string assetPath)
        {
            SaveLayout(System.IO.Path.Combine(Directory.GetCurrentDirectory(), assetPath));
        }

        /// <summary>
        /// Loads layour from an asset, given the path
        /// </summary>
        /// <param name="assetPath"></param>
        public static void LoadLayoutFromAsset(string assetPath)
        {
            if (_miLoadWindowLayout != null)
            {
                string path = System.IO.Path.Combine(Directory.GetCurrentDirectory(), assetPath);
                _miLoadWindowLayout.Invoke(null, new object[] { path, true });
            }
        }

        /// <summary>
        /// Saves the current Layout to an absolute `path`
        /// </summary>
        /// <param name="path"></param>
        public static void SaveLayout(string path)
        {
            if (_miSaveWindowLayout != null)
                _miSaveWindowLayout.Invoke(null, new object[] { path });
        }
    }
}