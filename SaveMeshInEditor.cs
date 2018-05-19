using UnityEngine;

// Usage: Attach to gameobject, assign target gameobject (from where the mesh is taken), Run, Press savekey

public class SaveMeshInEditor : MonoBehaviour
{

    //public KeyCode saveKey = KeyCode.F12;
    //public string saveName = "tooth_part";

    //void Update()
    //{
    //    if (Input.GetKeyDown(saveKey))
    //    {
    //        saveName = gameObject.transform.name;
    //        SaveAsset();
    //    }
    //}

    //void SaveAsset()
    //{
    //    var mf = gameObject.GetComponent<MeshFilter>();
    //    if (mf)
    //    {
    //        Mesh m1 = this.GetComponent<MeshFilter>().mesh;//update line1
    //        AssetDatabase.CreateAsset(m1, "Assets/tooth1/" + this.name + "_M" + ".asset"); // update line2

    //        var prefab = PrefabUtility.CreateEmptyPrefab("Assets/tooth1/" + saveName + ".prefab");
    //        PrefabUtility.ReplacePrefab(gameObject, prefab);
    //        AssetDatabase.Refresh();
    //    }
    //}
}