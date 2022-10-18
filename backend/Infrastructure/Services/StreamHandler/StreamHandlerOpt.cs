namespace Application.Services.StreamHandler
{
    public class StreamHandlerOpt
    {
        public const string SectionName = "StreamHandler";

        // NOTE(rg): depends on image size and format. For reference, a 640x480 JPEG image
        // with a compression quality of 60 takes up roughly 12kB
        public int FrameBufferSize { get; set; }
        public int UdpPort { get; set; }
    }
}