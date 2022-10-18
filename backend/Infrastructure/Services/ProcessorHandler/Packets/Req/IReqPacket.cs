namespace Application.Services.ProcessorHandler.Packets.Req
{
    public interface IReqPacket
    {
        public ReqPacketType Type { get; }
        public byte[] ToBytes();
    }
}