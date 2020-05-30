using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace IVySoft.VDS.Client.UI.Logic
{
    public static class CollectionUtils
    {
        public static void Update<T1, T2>(ObservableCollection<T1> dest, IEnumerable<T2> new_state, Func<T1, T2, bool> comparer, Action<T1, T2> updater, Func<T2, T1> creator)
        {
            var exists = new List<T1>();
            foreach (var exist in dest)
            {
                exists.Add(exist);
            }

            foreach (var new_item in new_state)
            {
                bool is_exist = false;
                foreach (var exist in exists)
                {
                    if (comparer(exist, new_item))
                    {
                        updater(exist, new_item);
                        exists.Remove(exist);
                        is_exist = true;
                        break;
                    }
                }
                if (!is_exist)
                {
                    dest.Add(creator(new_item));
                }
            }

            foreach (var session in exists)
            {
                dest.Remove(session);
            }
        }
    }
}
