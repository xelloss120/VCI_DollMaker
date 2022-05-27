using System.IO;
using UnityEngine;
using UnityEngine.UI;
using SFB;
using UniGLTF;
using VRM;
using VCI;
using VRMShaders;

public class ExportVCI : MonoBehaviour
{
    [SerializeField] ImportVRM ImportVRM;

    [SerializeField] GameObject FK_DollPrefab;
    [SerializeField] GameObject IK_DollPrefab;
    [SerializeField] GameObject RagDollPrefab;

    [SerializeField] InputField Title;
    [SerializeField] InputField Version;
    [SerializeField] InputField Author;
    [SerializeField] InputField Contact;
    [SerializeField] InputField Reference;

    [SerializeField] Dropdown LicenseType;
    [SerializeField] InputField LicenseUrl;

    [SerializeField] Camera Camera;
    [SerializeField] RenderTexture RenderTexture;

    /// <summary>
    /// ラグドール書き出し
    /// </summary>
    public void ExportRagDoll()
    {
        var path = StandaloneFileBrowser.SaveFilePanel("Export VCI File", "", "", "vci");

        if (path == "") return;

        var doll = Instantiate(RagDollPrefab);
        var model = VRMBoneNormalizer.Execute(ImportVRM.Model, false); // 表情適用のため再正規化
        model.transform.parent = doll.transform;

        var vci = doll.GetComponent<VCIObject>();
        vci.Meta.title = Title.text;
        vci.Meta.version = Version.text;
        vci.Meta.author = Author.text;
        vci.Meta.contactInformation = Contact.text;
        vci.Meta.reference = Reference.text;
        vci.Meta.thumbnail = RenderTextureToTexture2D();
        vci.Meta.modelDataLicenseType = (VciMetaLicenseType)LicenseType.value;
        vci.Meta.modelDataOtherLicenseUrl = LicenseUrl.text;

        var sub = model.AddComponent<VCISubItem>();
        var rig = model.AddComponent<Rigidbody>();
        rig.isKinematic = true;

        var data = new ExportingGltfData();
        var exporter = new VCIExporter(data);
        exporter.Prepare(doll);
        exporter.Export(new RuntimeTextureSerializer());
        exporter.Dispose();
        var bytes = data.ToGlbBytes();
        File.WriteAllBytes(path, bytes);

        Destroy(doll);
    }

    /// <summary>
    /// サムネイル作成
    /// </summary>
    Texture2D RenderTextureToTexture2D()
    {
        var rt = Camera.targetTexture;
        var tf = TextureFormat.RGBA32;
        var tex = new Texture2D(rt.width, rt.height, tf, false);

        RenderTexture.active = rt;
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        var pixels = tex.GetPixels();

        for (int i = 0; i < pixels.Length; i++) pixels[i] = pixels[i].gamma;

        tex.SetPixels(pixels);
        tex.Apply();

        return tex;
    }
}
