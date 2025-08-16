﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace FEMur.CrossSections
{
    public abstract class CrossSection:ICloneable,ISerializable
    {
        #region　properties

        public string Name { get; set; } = string.Empty;
        protected List<string> Elem_ids { get; set; } = new List<string>();
        protected Guid Guid{ get; set; }  = Guid.NewGuid();

        #endregion

        #region constructors
        protected CrossSection()
        {
        }

        protected CrossSection(string name)
        {
            Name = name;
        }

        protected CrossSection(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException("Serialization not implemented yet.");
        }
        #endregion

        public virtual object Clone()
        {
            return this.MemberwiseClone();
        }
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // シリアライズするデータを設定するコードをかく
            throw new NotImplementedException("Serialization not implemented yet.");
        }

        public override string ToString()
        {
            return $"CrossSection: {Name}, Guid: {Guid}";
        }
    }
}
