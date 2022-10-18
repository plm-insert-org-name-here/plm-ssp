namespace Application.Services.DetectorController
{
    public class DetectorControllerOpt
    {
        public const string SectionName = "DetectorController";

        public int TimeoutMilliseconds { get; set; }
        public int PingMilliseconds { get; set; }
        public int ResponseBufferSize { get; set; }
        public int SnapshotBufferSize { get; set; }
    }
}