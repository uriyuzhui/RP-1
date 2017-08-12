﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RP0.Crew
{
    public class TrainingDatabase
    {
        public class TrainingHolder
        {
            #region Fields

            public string name;

            public double days = 0;

            public List<string> children = new List<string>();

            #endregion

            #region Constructors

            public TrainingHolder(string name, string val)
            {
                this.name = name;

                double tmp;
                string[] split = val.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string s in split)
                {
                    if (double.TryParse(s, out tmp))
                        days += tmp;
                    else
                        children.Add(s);
                }
            }

            #endregion

            #region Methods

            public double GetTime()
            {
                double c = days;

                foreach (string s in children)
                    c += TrainingDatabase._GetTime(s);

                return c;
            }

            #endregion
        }

        #region Fields

        protected static Dictionary<string, TrainingHolder> holders = null;

        protected static HashSet<string> unlockPathTracker = new HashSet<string>();

        #endregion

        #region Setup

        public TrainingDatabase()
        {
            Initialize();
        }
        public static void Initialize()
        {
            if (holders == null)
                holders = new Dictionary<string, TrainingHolder>();

            FillHolders();
        }
        
        protected static void FillHolders()
        {
            holders.Clear();

            foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes("TRAININGTIMES"))
            {
                foreach (ConfigNode.Value v in node.values)
                {
                    TrainingHolder p = new TrainingHolder(v.name, v.value);
                    holders[p.name] = p;
                }
            }
        }
        #endregion

        #region Interface

        public static double GetTime(string name)
        {
            ClearTracker();
            return _GetTime(Sanitize(name));
        }
        protected static double _GetTime(string name)
        {
            if (unlockPathTracker.Contains(name))
            {
                /*string msg = "[TrainingDatabase]: Circular reference on " + name;
                foreach (string s in unlockPathTracker)
                    msg += "\n" + s;

                Debug.LogError(msg);*/
                return 0;
            }

            unlockPathTracker.Add(name);

            TrainingHolder h;
            if (holders.TryGetValue(name, out h))
                return h.GetTime();
            
            return 0d;
        }

        public static string SynonymReplace(string name)
        {
            name = Sanitize(name);
            TrainingHolder h;
            if (holders.TryGetValue(name, out h))
            {
                if (h.children.Count == 1)
                {
                    return h.children[0];
                }
            }
            return name;
        }

        protected static string Sanitize(string partName)
        {
            partName = partName.Replace(".", "-");
            return partName.Replace("_", "-");
        }

        public static void ClearTracker()
        {
            unlockPathTracker.Clear();
        }

        #endregion
    }
}
