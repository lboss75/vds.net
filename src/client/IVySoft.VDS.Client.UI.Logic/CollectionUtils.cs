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
        public static void Update2<T1, T2, T3>(
            ObservableCollection<T1> dest,
            IEnumerable<T2> new_state1,
            Func<T2, IEnumerable<T3>> new_state2,
            Func<T1, T2, T3, bool> comparer,
            Action<T1, T2, T3> updater,
            Func<T2, T3, T1> creator)
        {
            var exists = new List<T1>();
            foreach (var exist in dest)
            {
                exists.Add(exist);
            }

            foreach (var new_item1 in new_state1)
            {
                foreach (var new_item in new_state2(new_item1))
                {
                    bool is_exist = false;
                    foreach (var exist in exists)
                    {
                        if (comparer(exist, new_item1, new_item))
                        {
                            updater(exist, new_item1, new_item);
                            exists.Remove(exist);
                            is_exist = true;
                            break;
                        }
                    }
                    if (!is_exist)
                    {
                        dest.Add(creator(new_item1, new_item));
                    }
                }
            }

            foreach (var session in exists)
            {
                dest.Remove(session);
            }
        }

        public static void Update3<T1, T2, T3, T4>(
            ObservableCollection<T1> dest,
            IEnumerable<T2> new_state1,
            Func<T2, IEnumerable<T3>> new_state2,
            Func<T2, T3, IEnumerable<T4>> new_state3,
            Func<T1, T2, T3, T4, bool> comparer,
            Action<T1, T2, T3, T4> updater,
            Func<T2, T3, T4, T1> creator)
        {
            var exists = new List<T1>();
            foreach (var exist in dest)
            {
                exists.Add(exist);
            }

            foreach (var new_item1 in new_state1)
            {
                foreach (var new_item2 in new_state2(new_item1))
                {
                    foreach (var new_item in new_state3(new_item1, new_item2))
                    {
                        bool is_exist = false;
                        foreach (var exist in exists)
                        {
                            if (comparer(exist, new_item1, new_item2, new_item))
                            {
                                updater(exist, new_item1, new_item2, new_item);
                                exists.Remove(exist);
                                is_exist = true;
                                break;
                            }
                        }
                        if (!is_exist)
                        {
                            dest.Add(creator(new_item1, new_item2, new_item));
                        }
                    }
                }
            }

            foreach (var session in exists)
            {
                dest.Remove(session);
            }
        }
    }
}
