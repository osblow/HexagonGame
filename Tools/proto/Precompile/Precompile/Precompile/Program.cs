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
            model.Add(typeof(RoomConf), true);
            model.Add(typeof(LoginRequest), true);
            model.Add(typeof(Member), true);
            model.Add(typeof(LoginResponse), true);
            model.Add(typeof(SyncMembers), true);
            model.Add(typeof(BroadcastMainHex), true);
            model.Add(typeof(RandOperation), true);
            model.Add(typeof(BroadcastOp), true);
            model.Add(typeof(BroadcastHexPress), true);
            model.Add(typeof(BroadcastHexRelease), true);
            //model.Add(typeof(RoomBroadCast.RoomConf), true);
            //model.Add(typeof(RoomBroadCast.RoomConf.MapType), true);


            model.AllowParseableTypes = true;
            model.AutoAddMissingTypes = true;

            model.Compile("ProtoSerializer", "ProtoSerializer.dll");
        }
    }
}
