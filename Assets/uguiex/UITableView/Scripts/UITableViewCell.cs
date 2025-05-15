using UnityEngine;

public class UITableViewCell : MonoBehaviour {

    [HideInInspector]
    public int index;
    [HideInInspector]
    public RectTransform cacheTransform;
    void Awake()
    {
        cacheTransform = transform as RectTransform;
        Debug.Assert(cacheTransform != null, "transform should be RectTransform");
    }
}
