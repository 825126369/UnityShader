using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestTableView : MonoBehaviour {

    private UITableView tableView;

    public int CellCount = 10;

    public GameObject cellPrefab = null;
	// Use this for initialization
	void Start () {
        InitTableView();
	}

    void InitTableView()
    {
        tableView = GetComponent<UITableView>();
        tableView.SetCellByIndexCallback(GetCellByIndex);
        tableView.Reload(100);
    }

    /// <summary>
    /// tableview callback
    /// </summary>
    /// <returns></returns>
    int TotalCellCount()
    {
        return CellCount;
    }

    /// <summary>
    /// tableview callback
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    UITableViewCell GetCellByIndex(int index)
    {
        UITableViewCell cell = tableView.GetReUseCell();
        if(cell == null)
        {
            GameObject go = Instantiate(cellPrefab) as GameObject;
            cell = go.GetComponent<UITableViewCell>();
        }
        cell.index = index;
        Text label = cell.transform.GetComponentInChildren<Text>();
        label.text = "cell_" + index;
        return cell;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
