using System.IO;
using UnityEngine;
using UnityEngine.UI;
using SFB;
using UniGLTF;
using VRM;

public class ImportVRM : MonoBehaviour
{
    public GameObject Model;

    [SerializeField] InputField Title;
    [SerializeField] Dropdown BlendShape;

    /// <summary>
    /// VRM読み込み
    /// </summary>
    public void OnClick()
    {
        var paths = StandaloneFileBrowser.OpenFilePanel("Import VRM File", "", "", false);

        if (paths.Length == 0) return;

        var data = new AutoGltfFileParser(paths[0]).Parse();
        var vrm = new VRMData(data);
        var loader = new VRMImporterContext(vrm);
        var instance = loader.Load();
        instance.ShowMeshes();

        // 書き出し時に重力設定で変形する対策
        var sb = instance.Root.GetComponentsInChildren<VRMSpringBone>();
        for (int i = 0; i < sb.Length; i++)
        {
            sb[i].enabled = false;
        }

        if (Model != null) Destroy(Model);
        Model = instance.Root;

        Title.text = Path.GetFileNameWithoutExtension(paths[0]);
        BlendShape.value = 0;
    }
}