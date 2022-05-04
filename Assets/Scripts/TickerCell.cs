using System;

namespace Ticker
{
    public class TickerCell:IComparable<TickerCell>
    {
        public readonly int tickId;
        private int tick;
        private int loopNum;
        private Action tickCb;
        private Action completeCb;
        private Action delayCb;
        private bool isRemove;
        private bool isDelay;
        private int interval;
        private long lastTickTime;

        public TickerCell(int tickId, long curTime, int interval, Action tickCb, int loopNum, Action completeCb = null, int delay = 0, Action delayCb = null)
        {
            this.tickId = tickId;
            lastTickTime = curTime;
            this.interval = interval;
            this.tickCb = tickCb;
            this.loopNum = loopNum;
            this.completeCb = completeCb;
            this.delayCb = delayCb;
            if (delay > 0)
            {
                isDelay = true;
                tick = delay;
            }
            else
            {
                tick = interval;
            }
        }

        /// <summary>
        /// 是否tick成功
        /// </summary>
        /// <param name="tickTime"></param>
        /// <returns></returns>
        public bool Tick(long tickTime)
        {
            int delta = (int)(tickTime - lastTickTime);
            var isSuccess = false;
            while (!isRemove && loopNum != 0 && delta >= tick)
            {
                delta -= tick;
                if (isDelay)
                {
                    isDelay = false;
                    delayCb?.Invoke();
                }
                else
                {
                    loopNum--;
                    tickCb?.Invoke();
                }
                tick = interval;
                isSuccess = true;
            }

            if (!isRemove)
            {
                if (loopNum == 0)
                {
                    TickerManager.Ins.RemoveTicker(tickId);//就这么着吧，丑就丑吧
                    completeCb?.Invoke();
                } 
                else if(isSuccess)
                {
                    tick -= delta;
                    lastTickTime = tickTime;
                }
            }
            return isSuccess;
        }

        public bool IsStop()
        {
            return isRemove;
        }

        /// <summary>
        /// 删除
        /// </summary>
        public void Remove()
        {
            isRemove = true;
        }
        
        /// <summary>
        /// 排序器
        /// </summary>
        /// <param name="cell2"></param>
        /// <returns></returns>
        public int CompareTo(TickerCell cell2)
        {
            if (tick == cell2.tick)
            {
                return tickId - cell2.tickId;
            }
            return tick - cell2.tick;
        }
    }
}