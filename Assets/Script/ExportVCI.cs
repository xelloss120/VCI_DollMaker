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

    /// <summary>
    /// ラグドール書き出し
    /// </summary>
    public void ExportRagDoll()
    {
        var path = StandaloneFileBrowser.SaveFilePanel("Export VCI File", "", "", "vci");

        if (path == "") return;

        var root = Instantiate(RagDollPrefab);
        var model = VRMBoneNormalizer.Execute(ImportVRM.Model, false); // 表情適用のため再正規化
        model.transform.parent = root.transform;

        var vci = root.GetComponent<VCIObject>();
        vci.Meta.title = Title.text;
        vci.Meta.version = Version.text;
        vci.Meta.author = Author.text;
        vci.Meta.contactInformation = Contact.text;
        vci.Meta.reference = Reference.text;
        vci.Meta.thumbnail = RenderTextureToTexture2D();
        vci.Meta.modelDataLicenseType = (VciMetaLicenseType)LicenseType.value;
        vci.Meta.modelDataOtherLicenseUrl = LicenseUrl.text;

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

        var joints = root.GetComponentsInChildren<ConfigurableJoint>();
        foreach (var joint in joints) DestroyImmediate(joint);

        var item = model.AddComponent<VCISubItem>();
        item.Grabbable = true;
        item.GroupId = 1;

        SetJoint(anim, HumanBodyBones.Chest, HumanBodyBones.Hips, root.transform, new Vector3(1, 0, 0), -45, 45);
        SetJoint(anim, HumanBodyBones.Head, HumanBodyBones.Chest, root.transform, new Vector3(1, 0, 0), -90, 45);
        SetJoint(anim, HumanBodyBones.LeftUpperLeg, HumanBodyBones.Hips, root.transform, new Vector3(1, 0, 0), -120, 45);
        SetJoint(anim, HumanBodyBones.LeftLowerLeg, HumanBodyBones.LeftUpperLeg, root.transform, new Vector3(1, 0, 0), 0, 140);
        SetJoint(anim, HumanBodyBones.LeftFoot, HumanBodyBones.LeftLowerLeg, root.transform, new Vector3(1, 0, 0), -45, 90);
        SetJoint(anim, HumanBodyBones.RightUpperLeg, HumanBodyBones.Hips, root.transform, new Vector3(1, 0, 0), -120, 45);
        SetJoint(anim, HumanBodyBones.RightLowerLeg, HumanBodyBones.RightUpperLeg, root.transform, new Vector3(1, 0, 0), 0, 140);
        SetJoint(anim, HumanBodyBones.RightFoot, HumanBodyBones.RightLowerLeg, root.transform, new Vector3(1, 0, 0), -45, 90);
        SetJoint(anim, HumanBodyBones.LeftUpperArm, HumanBodyBones.Chest, root.transform, new Vector3(0, 0, 1), -80, 80);
        SetJoint(anim, HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftUpperArm, root.transform, new Vector3(0, -1, 0), -140, 0);
        SetJoint(anim, HumanBodyBones.LeftHand, HumanBodyBones.LeftLowerArm, root.transform, new Vector3(0, 0, 1), -80, 80);
        SetJoint(anim, HumanBodyBones.RightUpperArm, HumanBodyBones.Chest, root.transform, new Vector3(0, 0, 1), -80, 80);
        SetJoint(anim, HumanBodyBones.RightLowerArm, HumanBodyBones.RightUpperArm, root.transform, new Vector3(0, 1, 0), -140, 0);
        SetJoint(anim, HumanBodyBones.RightHand, HumanBodyBones.RightLowerArm, root.transform, new Vector3(0, 0, 1), -80, 80);

        var data = new ExportingGltfData();
        var exporter = new VCIExporter(data);
        exporter.Prepare(root);
        exporter.Export(new RuntimeTextureSerializer());
        exporter.Dispose();
        var bytes = data.ToGlbBytes();
        File.WriteAllBytes(path, bytes);

        Destroy(root);
    }

    void SetJoint(Animator anim, HumanBodyBones bone, HumanBodyBones body, Transform root, Vector3 axis, float min, float max)
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
