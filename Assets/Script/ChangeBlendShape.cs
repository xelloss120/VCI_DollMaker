using UnityEngine;
using UnityEngine.UI;
using VRM;

public class ChangeBlendShape : MonoBehaviour
{
    [SerializeField] ImportVRM ImportVRM;
    [SerializeField] Dropdown Dropdown;

    public void Changed()
    {
        var proxy = ImportVRM.Model.GetComponent<VRMBlendShapeProxy>();
        switch (Dropdown.value)
        {
            case 0:
                proxy.AccumulateValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Joy), 0);
                proxy.AccumulateValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Angry), 0);
                proxy.AccumulateValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Sorrow), 0);
                proxy.AccumulateValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Fun), 0);
                break;
            case 1:
                proxy.AccumulateValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Joy), 1);
                proxy.AccumulateValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Angry), 0);
                proxy.AccumulateValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Sorrow), 0);
                proxy.AccumulateValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Fun), 0);
                break;
            case 2:
                proxy.AccumulateValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Joy), 0);
                proxy.AccumulateValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Angry), 1);
                proxy.AccumulateValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Sorrow), 0);
                proxy.AccumulateValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Fun), 0);
                break;
            case 3:
                proxy.AccumulateValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Joy), 0);
                proxy.AccumulateValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Angry), 0);
                proxy.AccumulateValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Sorrow), 1);
                proxy.AccumulateValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Fun), 0);
                break;
            case 4:
                proxy.AccumulateValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Joy), 0);
                proxy.AccumulateValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Angry), 0);
                proxy.AccumulateValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Sorrow), 0);
                proxy.AccumulateValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Fun), 1);
                break;
        }
        proxy.Apply();
    }
}
