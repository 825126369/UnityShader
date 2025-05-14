using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderMoveText : MonoBehaviour {

    public Transform textTm = null;
    public Transform beginTm = null;
    public Transform endTm = null;

    protected RectTransform rectTm = null;

    protected Slider slider = null; 

    public float offset_x = 0;
    float len_x = 0;
    
    Vector2 newpos = new Vector3(0, 0);
    float begin_x = 0;
    float end_x = 0;

    public Text finishText = null;

	// Use this for initialization
	void Start () {
        slider = this.gameObject.GetComponent<Slider>();
        textTm.position = beginTm.position;
        rectTm = this.gameObject.GetComponent<RectTransform>();
        textTm.GetComponent<Text>().text = "";

        if (finishText)
        {
            finishText.gameObject.SetActive(false);
            finishText.text = "100";
        }
    }
	
	// Update is called once per frame
	void Update () {
        if (slider == null || textTm == null || beginTm == null )
        {
            return;
        }

        begin_x = beginTm.gameObject.GetComponent<RectTransform>().anchoredPosition.x;
        end_x = endTm.gameObject.GetComponent<RectTransform>().anchoredPosition.x;
        len_x = end_x - begin_x;

        newpos.x = begin_x + len_x * slider.value + offset_x;

        if( slider.value >= 0.99f)
        {
            if(finishText)
            {
                finishText.gameObject.SetActive(true);
            }
            textTm.gameObject.SetActive(false);
        }
        else
        {
            if (finishText)
            {
                finishText.gameObject.SetActive(false);
            }
            textTm.gameObject.SetActive(true);
            textTm.GetComponent<RectTransform>().anchoredPosition = newpos;
            textTm.GetComponent<Text>().text = Mathf.CeilToInt(slider.value * 100).ToString();
        }
    }
}
