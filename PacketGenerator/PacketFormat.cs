using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PacketGenerator
{
    class PacketFormat
    {

        // {0} 패킷 등록
        public static string managerFormat =
@"using System;
using System.Collections.Generic;
using ServerCore;

class PacketManager
{{
    #region Singleton
    static PacketManager _instance = new PacketManager();
    public static PacketManager Instance
    {{
        get
        {{
            return _instance;
        }}
    }}
    #endregion

    PacketManager()
    {{
        Register();
    }}

    private Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>> _onRecv
        = new Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>>();

    private Dictionary<ushort, Action<PacketSession, IPacket>> _handler = new Dictionary<ushort, Action<PacketSession, IPacket>>();

    public void Register()
    {{
        {0}
    }}

    public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer)
    {{
        ushort count = 0;

        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
        count += 2;
        ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;

        Action<PacketSession, ArraySegment<byte>> action = null;
        if (_onRecv.TryGetValue(id, out action))
        {{
            action.Invoke(session, buffer);
        }}
    }}

    private void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer) where T : IPacket, new()
    {{
        T packet = new T();
        packet.Read(buffer);

        Action<PacketSession, IPacket> action = null;
        if (_handler.TryGetValue(packet.Protocol, out action))
        {{
            action.Invoke(session, packet);
        }}
    }}
}}
";


        // {0} 패킷 이름.
        public static string managerRegisterFormat =
@"      _onRecv.Add((ushort)PacketID.{0}, MakePacket<{0}>);
        _handler.Add((ushort)PacketID.{0}, PacketHandler.{0}Handler);";

        // {0} 패킷 이름/ 번호
        // {1} 패킷 목록
        public static string fileFormat =
@"using ServerCore;
using System;
using System.Text;
using System.Net;
using System.Collections.Generic;

public enum PacketID
{{
    {0}
}}

interface IPacket
{{
	ushort Protocol {{ get; }}
	void Read(ArraySegment<byte> segment);
	ArraySegment<byte> Write();
}}


{1}
";

        // {0} 패킷 이름
        // {1} 패킷 번호
        public static string packetEnumFormat =
@"{0} = {1},";

        // {0} = Packet name
        // {1} = member values
        // {2} = read member values
        // {3} = write member values
        public static string packetFormat =
@"
class {0} : IPacket
{{
    {1}

	public ushort Protocol => (ushort)PacketID.{0};

    public void Read(ArraySegment<byte> segment)
    {{
        ushort count = 0;

        ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);

        count += sizeof(ushort);
        count += sizeof(ushort);
        {2}
    }}

    public ArraySegment<byte> Write()
    {{
        ArraySegment<byte> segment = SendBufferHelper.Open(4096);
        ushort count = 0;
        bool success = true;

        Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

        count += sizeof(ushort);
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.{0});
        count += sizeof(ushort);

        {3}
        
        success &= BitConverter.TryWriteBytes(s, count);

        if (success == false)
        {{
            return null;
        }}

        return SendBufferHelper.Close(count);
    }}
}}
";

        // {0} value type
        // {1} value name
        public static string memberFormat =
@"public {0} {1};";

        // {0} list name [upper]
        // {1} list name [lower]
        // {2} = member values
        // {3} = read member values
        // {4} = write member values
        public static string memberListFormat =
@"
public class {0}
{{
    {2}

    public void Read(ArraySegment<byte> segment, ReadOnlySpan<byte> s, ref ushort count)
    {{
        {3}
    }}

    public bool Write(ArraySegment<byte> segment, Span<byte> s, ref ushort count)
    {{
        bool success = true;
        {4}
        return success;
    }}

}}

public List<{0}> {1}s = new List<{0}>();
";

        // {0} value name
        // {1} To ~ value type
        // {2} value type
        public static string readFormat =
@"this.{0} = BitConverter.{1}(s.Slice(count, s.Length - count));
count += sizeof({2});";

        // {0} value name
        // {1} value type
        public static string readByteFormat =
@"this.{0} = ({1})segment.Array[segment.Offset + count];
count += sizeof({1});";

        // {0} value name
        public static string readStringFormat =
@"ushort {0}Len = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
count += sizeof(ushort);
this.{0} = Encoding.Unicode.GetString(s.Slice(count, {0}Len));
count += {0}Len;";

        // {0} list name [upper]
        // {1} list name [lower]
        public static string readListFormat =
@"this.{1}s.Clear();
ushort {1}Len = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
count += sizeof(ushort);
for (int i = 0; i < {1}Len; i++)
{{
    {0} {1} = new {0}();
    {1}.Read(segment, s, ref count);
    {1}s.Add({1});
}}";

        // {0} value name
        // {1} value type
        public static string writeFormat =
@"success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.{0});
count += sizeof({1});";

        // {0} value name
        // {1} value type
        public static string writeByteFormat =
@"segment.Array[segment.Offset + count] = (byte)this.{0};
count += sizeof({1});";

        // {0} value name
        public static string writeStringFormat =
@"ushort {0}Len = (ushort) Encoding.Unicode.GetBytes(this.{0}, 0, this.{0}.Length, segment.Array, segment.Offset + count + sizeof(ushort));
success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), {0}Len);
count += sizeof(ushort);
count += {0}Len;";

        // {0} list name [upper]
        // {1} list name [lower]
        public static string writeListFormat =
@"success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort){1}s.Count);
count += sizeof(ushort);
foreach ({0} {1} in {1}s)
{{
    success &= {1}.Write(segment, s, ref count);
}}";
    }
}
