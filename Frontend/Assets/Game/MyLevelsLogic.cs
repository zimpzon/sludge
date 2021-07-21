using Sludge.Backend;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MyLevelsLogic : MonoBehaviour
{
    public GameObject MyLevelItemTemplate;
    public GameObject ListViewPanel;
    public LoopVerticalScrollRect listView;

    private void Start()
    {
        listView.totalCount = 50;
        listView.RefillCells();
    }

    void PopulateListView()
    {
        //Server.GetMyLevels()
    }
    // Get mylevels from server
    // create new and delete, then open
}
