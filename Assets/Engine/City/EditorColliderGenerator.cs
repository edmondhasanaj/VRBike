using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

/**
 * Used to copy colliders into one single object
 * after a chunk is baked
 */
public class EditorColliderGenerator : MonoBehaviour
{
    [SerializeField] private GameObject rootToCopy;
    [SerializeField] private GameObject pasteObject;

    [OnInspectorGUI]
    void CopyColliders()
    {
        if(GUILayout.Button("Copy"))
        {
            if (rootToCopy == null || pasteObject == null)
                throw new System.ArgumentException("Root to Copy and PasteObject must be initialized");

            i = 0;

            CleanDestination();
            RecursiveCopy(rootToCopy);

            Debug.Log("Finished copying successfully. Copied " + i + " colliders");
        }
    }

    void CleanDestination()
    {
        pasteObject.transform.localScale = Vector3.one;

        Collider[] allColls = pasteObject.GetComponents<Collider>();
        for (int i = 0; i < allColls.Length; i++)
            DestroyImmediate(allColls[i]);
    }

    int i = 0;
    void RecursiveCopy(GameObject go)
    {
        //Add current
        BoxCollider tmpCollider = go.GetComponent<BoxCollider>();
        if(tmpCollider != null)
        {
            BoxCollider newCollider = pasteObject.AddComponent<BoxCollider>();
            newCollider.center = go.transform.rotation * new Vector3(tmpCollider.center.x * go.transform.lossyScale.x, tmpCollider.center.y * go.transform.lossyScale.y, tmpCollider.center.z * go.transform.lossyScale.z) + (go.transform.position - pasteObject.transform.position);
            newCollider.size = go.transform.rotation * new Vector3(tmpCollider.size.x * go.transform.lossyScale.x, tmpCollider.size.y * go.transform.lossyScale.y, tmpCollider.size.z * go.transform.lossyScale.z);
            i++;

            Debug.Log("Collider " + go.name + " produced size: " + newCollider.size);
        }

        //Check if it has children
        if (go.transform.childCount > 0)
            RecursiveCopy(go.transform.GetChild(0).gameObject);

        //Check if it has siblings
        if (go.transform.parent != null && go.transform.GetSiblingIndex() < go.transform.parent.childCount - 1)
            RecursiveCopy(go.transform.parent.GetChild(go.transform.GetSiblingIndex() + 1).gameObject);
    }
}
