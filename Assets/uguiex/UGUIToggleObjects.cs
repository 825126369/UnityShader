using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[AddComponentMenu("UGUI/Interaction/UGUI Toggled Objects")]
public class UGUIToggleObjects : MonoBehaviour {
    public List<GameObject> activate;
    public List<GameObject> deactivate;

    void Awake()
    {

#if UNITY_EDITOR
        if (!Application.isPlaying) return;
#endif

        Toggle toggle = GetComponent<Toggle>();
        OnToggle(toggle.isOn);
        toggle.onValueChanged.AddListener(OnToggle);
    }

    public void OnToggle(bool val)
    {
        if (enabled)
        {
            for (int i = 0; i < activate.Count; ++i)
                Set(activate[i], val);

            for (int i = 0; i < deactivate.Count; ++i)
                Set(deactivate[i], !val);
        }
    }

    void Set(GameObject go, bool state)
    {
        if (go != null)
        {
            go.SetActive(state);
        }
    }
}
