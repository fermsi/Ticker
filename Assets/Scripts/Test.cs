using System;
using Ticker;
using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        tickId = TickerManager.Ins.AddLoopMsTicker(1000, SecondTickHandler);
        TickerManager.Ins.AddMsTicker(500, FrameTickHandler, 5, FrameTickEnd);
        TickerManager.Ins.AddMsTicker(1000, null, 1, () =>
        {
            Debug.Log($"delay tick end{DateTime.Now.Ticks}");
        }, 1000, DelayCb);
    }

    void DelayCb()
    {
        Debug.Log($"DelayCb:{DateTime.Now.Ticks / 10000}");
    }

    void FrameTickHandler()
    {
        Debug.Log($"FrameTickHandler:{DateTime.Now.Ticks / 10000}");
    }

    void FrameTickEnd()
    {
        Debug.Log($"FrameTickEnd:{DateTime.Now.Ticks / 10000}");
    }

    private int num;
    private int tickId;
    private void SecondTickHandler()
    {
        num++;
        Debug.Log($"SecondTickHandler:{DateTime.Now.Ticks / 10000}");
        if (num == 10)
        {
            TickerManager.Ins.RemoveTicker(tickId);
            Debug.Log("temp");
        }
    }

    // Update is called once per frame
    void Update()
    {
        TickerManager.Ins.Tick();
    }
}
