using System;
using Osblow.HexProto;
using ProtoBuf.Meta;

namespace Precompile
{
    class Program
    {
        static void Main(string[] args)
        {
            var model = TypeModel.Create();

            model.Add(typeof(RoomBroadCast), true);
            //model.Add(typeof(RoomBroadCast.RoomConf), true);
            //model.Add(typeof(RoomBroadCast.RoomConf.MapType), true);


            model.AllowParseableTypes = true;
            model.AutoAddMissingTypes = true;

            model.Compile("Proto", "ProtoSerializer.dll");
        }
    }
}
