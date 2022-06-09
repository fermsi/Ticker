using System;
using System.Collections.Generic;

namespace Ticker
{
    public class TickerManager
    {
        public const int DEFAULT_TICK_ID = -1;
        private static TickerManager _ins;
        private int curTickId = 0;
        private SortedSet<TickerCell> tickerSet;
        private Dictionary<int, TickerCell> tickerDict;

        #region update临时变量
        
        private bool isUpdating = false;
        private List<TickerCell> removedList;
        private List<TickerCell> addList;
        private List<TickerCell> updatingList;

        #endregion

        public static TickerManager Ins
        {
            get
            {
                if (_ins == null)
                {
                    _ins = new TickerManager();
                }
                return _ins;
            }
        }

        private TickerManager()
        {
            tickerSet = new SortedSet<TickerCell>();
            tickerDict = new Dictionary<int, TickerCell>();
            removedList = new List<TickerCell>();
            addList = new List<TickerCell>();
            updatingList = new List<TickerCell>();
        }

        /// <summary>
        /// 添加一直循环的ticker
        /// </summary>
        /// <param name="interval">tick间隔，单位ms</param>
        /// <param name="tickCb">tick回调</param>
        /// <returns></returns>
        public int AddLoopMsTicker(int interval, Action tickCb)
        {
            return AddMsTicker(interval, tickCb, -1, null);
        }

        /// <summary>
        /// 添加一个毫秒级别的tick
        /// </summary>
        /// <param name="interval"></param>
        /// <param name="tickCb"></param>
        /// <param name="loopNum"></param>
        /// <param name="completeCb"></param>
        /// <param name="delay"></param>
        /// <param name="delayCb"></param>
        /// <returns></returns>
        public int AddMsTicker(int interval, Action tickCb, int loopNum, Action completeCb, int delay = 0,
            Action delayCb = null)
        {
            var tickId = CreateTickId();
            var time = GetCurTime();
            var cell = new TickerCell(tickId, time, interval, tickCb, loopNum, completeCb, delay, delayCb);
            tickerDict[tickId] = cell;
            if (isUpdating)//在update的时候，就不要去操作set啦
            {
                addList.Add(cell);
            }
            else
            {
                tickerSet.Add(cell);// 时间复杂度 log(n)
            }
            return tickId;
        }

        /// <summary>
        /// tick一下
        /// </summary>
        public void Tick()
        {
            isUpdating = true;
            var time = GetCurTime();
            foreach (var cell in tickerSet)//tickerSet是排序的，时间从小到大
            {
                if (!cell.IsStop())
                {
                    if (cell.Tick(time))
                    {
                        updatingList.Add(cell);
                    }
                    else
                    {
                        //break;//只要一个断了，后面的说明就肯定断了
                    }
                }
            }
            foreach (var cell in updatingList)
            {
                if (!cell.IsStop())
                {
                    removedList.Add(cell);//要先删除，在添加加，会重新做排序，排序性能log(n)
                    addList.Add(cell);
                }
            }
            updatingList.Clear();
            foreach (var cell in removedList)
            {
                tickerSet.Remove(cell);
            }
            removedList.Clear();
            foreach (var cell in addList)
            {
                tickerSet.Add(cell);
            }
            addList.Clear();
            //时间复杂度 k.log(n)
            isUpdating = false;
        }

        /// <summary>
        /// 删除一个tikcer
        /// </summary>
        /// <param name="tickId"></param>
        /// <returns></returns>
        public int RemoveTicker(int tickId)
        {
            if (tickId == DEFAULT_TICK_ID) return DEFAULT_TICK_ID;
            if (tickerDict.TryGetValue(tickId, out var cell))
            {
                cell.Remove();
                tickerDict.Remove(tickId);
                if (isUpdating)//在update的时候，就不要去操作set啦
                {
                    removedList.Add(cell);
                    var count = addList.Count;
                    TickerCell temp;
                    for (int i = 0; i < count; i++)
                    {
                        temp = addList[i];
                        if (temp == cell)
                        {
                            addList.RemoveAt(i);
                            break;
                        }
                    }
                }
                else
                {
                    tickerSet.Remove(cell);//时间复杂度 log(n)
                }
            }//ID不存在啦
            return DEFAULT_TICK_ID;
        }

        private long GetCurTime()
        {
            return DateTime.Now.Ticks / 10000;//当前毫秒值，可以改成项目所需
        }

        private int CreateTickId()
        {
            return ++curTickId;
        }
    }
}
