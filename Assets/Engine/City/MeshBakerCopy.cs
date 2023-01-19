using UnityEditor;
using Sirenix.OdinInspector;
using UnityEngine;
using System;
using System.Text.RegularExpressions;

#if UNITY_EDITOR
public class MeshBakerCopy : MonoBehaviour
{
    [SerializeField] private MeshFilter m;
    [SerializeField] private string path;

    [OnInspectorGUI]
    private void ExportMesh()
    {
        if (GUILayout.Button("Export"))
        {
            if (m == null || path.Length == 0 || !Regex.IsMatch(path, @".*\.asset$"))
                throw new System.ArgumentException("The mesh filter and path must be initialized");

            AssetDatabase.CreateAsset(m.sharedMesh, path);
        }
    }
}
#endif