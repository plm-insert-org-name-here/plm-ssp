namespace Api.Services.ProcessorHandler.Packets.Req
{
    public interface IReqPacket
    {
        public PacketType Type { get; }
        public byte[] ToBytes();
    }
}