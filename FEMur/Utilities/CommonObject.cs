using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace FEMur.Utilities
{
    public abstract class CommonObject:ICloneable,ISerializable
    {
        //ユーザーデータを格納
        private Dictionary<string, object> UserData { get; } = new Dictionary<string, object>();
        public CommonObject(){}
        protected CommonObject(SerializationInfo info, StreamingContext context)
        {
            // シリアライズされたデータを復元するコードをかく
            throw new NotImplementedException("Serialization not implemented yet.");
        }
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // シリアライズするデータを設定するコードをかく
            throw new NotImplementedException("Serialization not implemented yet.");
        }
        public virtual object Clone()
        {
            return this.MemberwiseClone();
        }
        public abstract object DeepCopy();
        public override string ToString()
        {
            throw new NotImplementedException("ToString method not implemented yet.");
        }
    }
}
