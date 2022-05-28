using System.IO;
using UnityEngine;
using UnityEngine.UI;
using SFB;
using UniGLTF;
using VRM;
using VCI;
using VRMShaders;
using RootMotion.Dynamics;

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

    [SerializeField] Text Message;

    string Path = "";

    /// <summary>
    /// FK人形書き出し
    /// </summary>
    public void ExportFK_Doll()
    {
        if (CheckInputField()) return;

        Path = StandaloneFileBrowser.SaveFilePanel("Export VCI File", "", Title.text, "vci");

        if (Path == "") return;

        Invoke("ExportFK_DollSetup", 0.1f);

        Message.text = "FK人形書き出し中";
    }

    /// <summary>
    /// IK人形書き出し
    /// </summary>
    public void ExportIK_Doll()
    {
        if (CheckInputField()) return;

        Path = StandaloneFileBrowser.SaveFilePanel("Export VCI File", "", Title.text, "vci");

        if (Path == "") return;

        Invoke("ExportIK_DollSetup", 0.1f);

        Message.text = "IK人形書き出し中";
    }

    /// <summary>
    /// ラグドール書き出し
    /// </summary>
    public void ExportRagDoll()
    {
        if (CheckInputField()) return;

        Path = StandaloneFileBrowser.SaveFilePanel("Export VCI File", "", Title.text, "vci");

        if (Path == "") return;

        Invoke("ExportRagDollSetup", 0.1f);

        Message.text = "ラグドール書き出し中";
    }

    /// <summary>
    /// 必須項目の入力確認
    /// </summary>
    bool CheckInputField()
    {
        if (Title.text == "")
        {
            Message.text = "タイトルの入力は必須です。";
            return true;
        }
        if (Author.text == "")
        {
            Message.text = "作成者の入力は必須です。";
            return true;
        }
        Message.text = "";
        return false;
    }

    /// <summary>
    /// FK人形書き出し
    /// </summary>
    void ExportFK_DollSetup()
    {
        var root = Instantiate(FK_DollPrefab);
        var model = VRMBoneNormalizer.Execute(ImportVRM.Model, false); // 表情適用のため再正規化
        model.transform.parent = root.transform.Find("TargetRoot");

        SetMeta(root.GetComponent<VCIObject>());

        Rename(model.GetComponent<Animator>());

        Export(root);

        Destroy(root);

        Message.text = "";
    }

    /// <summary>
    /// IK人形書き出し
    /// </summary>
    void ExportIK_DollSetup()
    {
        var root = Instantiate(IK_DollPrefab);
        var model = VRMBoneNormalizer.Execute(ImportVRM.Model, false); // 表情適用のため再正規化
        model.transform.parent = root.transform.Find("TargetParent");
        model.name = "Root";

        SetMeta(root.GetComponent<VCIObject>());

        Rename(model.GetComponent<Animator>());

        Export(root);

        Destroy(root);

        Message.text = "";
    }

    /// <summary>
    /// ラグドール書き出し
    /// </summary>
    void ExportRagDollSetup()
    {
        var root = Instantiate(RagDollPrefab);
        var model = VRMBoneNormalizer.Execute(ImportVRM.Model, false); // 表情適用のため再正規化
        model.transform.parent = root.transform;

        SetMeta(root.GetComponent<VCIObject>());

        // PuppetMasterのラグドール機能を使用
        var anim = model.GetComponent<Animator>();
        BipedRagdollReferences bipedRagdollReferences;
        bipedRagdollReferences.root = model.transform;
        bipedRagdollReferences.hips = anim.GetBoneTransform(HumanBodyBones.Hips);
        bipedRagdollReferences.spine = anim.GetBoneTransform(HumanBodyBones.Spine);
        bipedRagdollReferences.chest = anim.GetBoneTransform(HumanBodyBones.Chest);
        bipedRagdollReferences.head = anim.GetBoneTransform(HumanBodyBones.Head);
        bipedRagdollReferences.leftUpperLeg = anim.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
        bipedRagdollReferences.leftLowerLeg = anim.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
        bipedRagdollReferences.leftFoot = anim.GetBoneTransform(HumanBodyBones.LeftFoot);
        bipedRagdollReferences.rightUpperLeg = anim.GetBoneTransform(HumanBodyBones.RightUpperLeg);
        bipedRagdollReferences.rightLowerLeg = anim.GetBoneTransform(HumanBodyBones.RightLowerLeg);
        bipedRagdollReferences.rightFoot = anim.GetBoneTransform(HumanBodyBones.RightFoot);
        bipedRagdollReferences.leftUpperArm = anim.GetBoneTransform(HumanBodyBones.LeftUpperArm);
        bipedRagdollReferences.leftLowerArm = anim.GetBoneTransform(HumanBodyBones.LeftLowerArm);
        bipedRagdollReferences.leftHand = anim.GetBoneTransform(HumanBodyBones.LeftHand);
        bipedRagdollReferences.rightUpperArm = anim.GetBoneTransform(HumanBodyBones.RightUpperArm);
        bipedRagdollReferences.rightLowerArm = anim.GetBoneTransform(HumanBodyBones.RightLowerArm);
        bipedRagdollReferences.rightHand = anim.GetBoneTransform(HumanBodyBones.RightHand);
        var options = BipedRagdollCreator.Options.Default;
        options.weight = 40;
        BipedRagdollCreator.Create(bipedRagdollReferences, options);

        // VCIではConfigurableJointが使えないので削除
        var joints = root.GetComponentsInChildren<ConfigurableJoint>();
        foreach (var joint in joints) DestroyImmediate(joint);

        // RootはHipsとSpineまで一緒にしてSubItem化
        var item = model.AddComponent<VCISubItem>();
        item.Grabbable = true;
        item.GroupId = 1;

        // 各部位の接合と角度制限
        SetRagDollJoint(anim, HumanBodyBones.Chest, HumanBodyBones.Hips, root.transform, new Vector3(1, 0, 0), -45, 45);
        SetRagDollJoint(anim, HumanBodyBones.Head, HumanBodyBones.Chest, root.transform, new Vector3(1, 0, 0), -90, 45);
        SetRagDollJoint(anim, HumanBodyBones.LeftUpperLeg, HumanBodyBones.Hips, root.transform, new Vector3(1, 0, 0), -120, 45);
        SetRagDollJoint(anim, HumanBodyBones.LeftLowerLeg, HumanBodyBones.LeftUpperLeg, root.transform, new Vector3(1, 0, 0), 0, 140);
        SetRagDollJoint(anim, HumanBodyBones.LeftFoot, HumanBodyBones.LeftLowerLeg, root.transform, new Vector3(1, 0, 0), -45, 90);
        SetRagDollJoint(anim, HumanBodyBones.RightUpperLeg, HumanBodyBones.Hips, root.transform, new Vector3(1, 0, 0), -120, 45);
        SetRagDollJoint(anim, HumanBodyBones.RightLowerLeg, HumanBodyBones.RightUpperLeg, root.transform, new Vector3(1, 0, 0), 0, 140);
        SetRagDollJoint(anim, HumanBodyBones.RightFoot, HumanBodyBones.RightLowerLeg, root.transform, new Vector3(1, 0, 0), -45, 90);
        SetRagDollJoint(anim, HumanBodyBones.LeftUpperArm, HumanBodyBones.Chest, root.transform, new Vector3(0, 0, 1), -80, 80);
        SetRagDollJoint(anim, HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftUpperArm, root.transform, new Vector3(0, -1, 0), -140, 0);
        SetRagDollJoint(anim, HumanBodyBones.LeftHand, HumanBodyBones.LeftLowerArm, root.transform, new Vector3(0, 0, 1), -80, 80);
        SetRagDollJoint(anim, HumanBodyBones.RightUpperArm, HumanBodyBones.Chest, root.transform, new Vector3(0, 0, 1), -80, 80);
        SetRagDollJoint(anim, HumanBodyBones.RightLowerArm, HumanBodyBones.RightUpperArm, root.transform, new Vector3(0, 1, 0), -140, 0);
        SetRagDollJoint(anim, HumanBodyBones.RightHand, HumanBodyBones.RightLowerArm, root.transform, new Vector3(0, 0, 1), -80, 80);

        Export(root);

        Destroy(root);

        Message.text = "";
    }

    /// <summary>
    /// VCIのメタ情報設定
    /// </summary>
    void SetMeta(VCIObject vci)
    {
        vci.Meta.title = Title.text;
        vci.Meta.version = Version.text;
        vci.Meta.author = Author.text;
        vci.Meta.contactInformation = Contact.text;
        vci.Meta.reference = Reference.text;
        vci.Meta.thumbnail = RenderTextureToTexture2D();
        vci.Meta.modelDataLicenseType = (VciMetaLicenseType)LicenseType.value;
        vci.Meta.modelDataOtherLicenseUrl = LicenseUrl.text;
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

    /// <summary>
    /// 操作対象部位の名前変更
    /// </summary>
    void Rename(Animator anim)
    {
        anim.GetBoneTransform(HumanBodyBones.Hips).gameObject.name = "Hips";
        anim.GetBoneTransform(HumanBodyBones.Chest).gameObject.name = "Chest";
        anim.GetBoneTransform(HumanBodyBones.Head).gameObject.name = "Head";
        anim.GetBoneTransform(HumanBodyBones.LeftUpperArm).gameObject.name = "LeftUpperArm";
        anim.GetBoneTransform(HumanBodyBones.LeftLowerArm).gameObject.name = "LeftLowerArm";
        anim.GetBoneTransform(HumanBodyBones.LeftHand).gameObject.name = "LeftHand";
        anim.GetBoneTransform(HumanBodyBones.RightUpperArm).gameObject.name = "RightUpperArm";
        anim.GetBoneTransform(HumanBodyBones.RightLowerArm).gameObject.name = "RightLowerArm";
        anim.GetBoneTransform(HumanBodyBones.RightHand).gameObject.name = "RightHand";
        anim.GetBoneTransform(HumanBodyBones.LeftUpperLeg).gameObject.name = "LeftUpperLeg";
        anim.GetBoneTransform(HumanBodyBones.LeftLowerLeg).gameObject.name = "LeftLowerLeg";
        anim.GetBoneTransform(HumanBodyBones.LeftFoot).gameObject.name = "LeftFoot";
        anim.GetBoneTransform(HumanBodyBones.RightUpperLeg).gameObject.name = "RightUpperLeg";
        anim.GetBoneTransform(HumanBodyBones.RightLowerLeg).gameObject.name = "RightLowerLeg";
        anim.GetBoneTransform(HumanBodyBones.RightFoot).gameObject.name = "RightFoot";

        // 左手
        anim.GetBoneTransform(HumanBodyBones.LeftThumbProximal).gameObject.name = "LeftThumbProximal";
        anim.GetBoneTransform(HumanBodyBones.LeftThumbIntermediate).gameObject.name = "LeftThumbIntermediate";
        anim.GetBoneTransform(HumanBodyBones.LeftThumbDistal).gameObject.name = "LeftThumbDistal";
        anim.GetBoneTransform(HumanBodyBones.LeftIndexProximal).gameObject.name = "LeftIndexProximal";
        anim.GetBoneTransform(HumanBodyBones.LeftIndexIntermediate).gameObject.name = "LeftIndexIntermediate";
        anim.GetBoneTransform(HumanBodyBones.LeftIndexDistal).gameObject.name = "LeftIndexDistal";
        anim.GetBoneTransform(HumanBodyBones.LeftMiddleProximal).gameObject.name = "LeftMiddleProximal";
        anim.GetBoneTransform(HumanBodyBones.LeftMiddleIntermediate).gameObject.name = "LeftMiddleIntermediate";
        anim.GetBoneTransform(HumanBodyBones.LeftMiddleDistal).gameObject.name = "LeftMiddleDistal";
        anim.GetBoneTransform(HumanBodyBones.LeftRingProximal).gameObject.name = "LeftRingProximal";
        anim.GetBoneTransform(HumanBodyBones.LeftRingIntermediate).gameObject.name = "LeftRingIntermediate";
        anim.GetBoneTransform(HumanBodyBones.LeftRingDistal).gameObject.name = "LeftRingDistal";
        anim.GetBoneTransform(HumanBodyBones.LeftLittleProximal).gameObject.name = "LeftLittleProximal";
        anim.GetBoneTransform(HumanBodyBones.LeftLittleIntermediate).gameObject.name = "LeftLittleIntermediate";
        anim.GetBoneTransform(HumanBodyBones.LeftLittleDistal).gameObject.name = "LeftLittleDistal";

        // 右手
        anim.GetBoneTransform(HumanBodyBones.RightThumbProximal).gameObject.name = "RightThumbProximal";
        anim.GetBoneTransform(HumanBodyBones.RightThumbIntermediate).gameObject.name = "RightThumbIntermediate";
        anim.GetBoneTransform(HumanBodyBones.RightThumbDistal).gameObject.name = "RightThumbDistal";
        anim.GetBoneTransform(HumanBodyBones.RightIndexProximal).gameObject.name = "RightIndexProximal";
        anim.GetBoneTransform(HumanBodyBones.RightIndexIntermediate).gameObject.name = "RightIndexIntermediate";
        anim.GetBoneTransform(HumanBodyBones.RightIndexDistal).gameObject.name = "RightIndexDistal";
        anim.GetBoneTransform(HumanBodyBones.RightMiddleProximal).gameObject.name = "RightMiddleProximal";
        anim.GetBoneTransform(HumanBodyBones.RightMiddleIntermediate).gameObject.name = "RightMiddleIntermediate";
        anim.GetBoneTransform(HumanBodyBones.RightMiddleDistal).gameObject.name = "RightMiddleDistal";
        anim.GetBoneTransform(HumanBodyBones.RightRingProximal).gameObject.name = "RightRingProximal";
        anim.GetBoneTransform(HumanBodyBones.RightRingIntermediate).gameObject.name = "RightRingIntermediate";
        anim.GetBoneTransform(HumanBodyBones.RightRingDistal).gameObject.name = "RightRingDistal";
        anim.GetBoneTransform(HumanBodyBones.RightLittleProximal).gameObject.name = "RightLittleProximal";
        anim.GetBoneTransform(HumanBodyBones.RightLittleIntermediate).gameObject.name = "RightLittleIntermediate";
        anim.GetBoneTransform(HumanBodyBones.RightLittleDistal).gameObject.name = "RightLittleDistal";
    }

    /// <summary>
    /// ラグドールの関節設定
    /// </summary>
    void SetRagDollJoint(Animator anim, HumanBodyBones bone, HumanBodyBones body, Transform root, Vector3 axis, float min, float max)
    {
        var transform = anim.GetBoneTransform(bone);
        transform.parent = root;

        var item = transform.gameObject.AddComponent<VCISubItem>();
        item.Grabbable = true;
        item.GroupId = 1;

        var joint = transform.gameObject.AddComponent<HingeJoint>();
        joint.connectedBody = anim.GetBoneTransform(body).gameObject.GetComponent<Rigidbody>();
        joint.useLimits = true;
        joint.axis = axis;
        joint.limits = new JointLimits() { min = min, max = max };
    }

    /// <summary>
    /// VCI書き出し
    /// </summary>
    void Export(GameObject go)
    {
        var data = new ExportingGltfData();
        var exporter = new VCIExporter(data);
        exporter.Prepare(go);
        exporter.Export(new RuntimeTextureSerializer());
        exporter.Dispose();
        var bytes = data.ToGlbBytes();
        File.WriteAllBytes(Path, bytes);
    }
}
