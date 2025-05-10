using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace JRPGNavAgent2D
{

    public struct MinItem<TItem>
    {
        public float value;
        public TItem item;

        public MinItem(float value, TItem item)
        {
            this.value = value;
            this.item = item;
        }
    }

    public class MinList<TItem>
    {

        List<MinItem<TItem>> items = new();

        public int Count { get { return items.Count; } }

        public void Add(float value, TItem item)
        {
            int index = 0;

            for (int i = 0; i < items.Count; i++)
            {
                if (value < items[i].value)
                {
                    break;
                }
                else
                {
                    index++;
                }
            }

            items.Insert(index, new MinItem<TItem>(value, item));
        }

        public TItem PopFirst()
        {
            var item = items[0];
            items.RemoveAt(0);

            return item.item;
        }

    }

}
