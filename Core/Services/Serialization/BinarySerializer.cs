﻿using Jil;
using Polyfacing.Core.Services.Serialization.Binary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Polyfacing.Core.Services.Serialization
{


    public class BinarySerializer : ISerializer
    {
        #region Ctor
        private BinarySerializer()
        {

        }
        public static BinarySerializer New()
        {
            return new BinarySerializer();
        }
        #endregion

        #region IHasId
        public string Id
        {
            get { return typeof(BinarySerializer).Name; }
        }
        #endregion

        #region ISerializer
        public bool CanHandle(object obj)
        {
            return true;
        }
        public string Serialize(object obj)
        {
            string returnValue = string.Empty;
            if (obj == null) { return returnValue; }

            try
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.SurrogateSelector = Strategies.GetBestSelector(); //new StrategizedSurrogateSelector();
                using (MemoryStream ms = new MemoryStream())
                {
                    bf.Serialize(ms, obj);
                    byte[] b = ms.ToArray();
                    returnValue = Convert.ToBase64String(b);//Encoding.ASCII.GetString(b);//
                }
            }
            catch
            {
                throw;
            }
            return returnValue;
        }
        public object Deserialize(string s,Type type)
        {
            object returnValue = null;
            if (string.IsNullOrEmpty(s)) { return returnValue; }

            try
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.SurrogateSelector = Strategies.GetBestSelector();// new StrategizedSurrogateSelector();
                byte[] b = Convert.FromBase64String(s);//Encoding.ASCII.GetBytes(s);// 

                using (MemoryStream ms = new MemoryStream(b))
                {
                    ms.Position = 0;
                    returnValue = bf.Deserialize(ms);
                }
            }
            catch
            {
                throw;
            }
            return returnValue;
        }
        #endregion


    }
}
