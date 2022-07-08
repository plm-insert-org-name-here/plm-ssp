using System;
using System.Collections.Generic;
using System.Diagnostics;
using Api.Domain.Entities;
using AutoMapper;
using Serilog;

namespace Api.Services.ProcessorHandler
{
    public class ProcessorParams
    {
        public int DetectorId { get; set; }
        public JobType JobType {get; set; }
        public List<ProcessorParamsTemplate> Templates { get; set; } = default!;

        public class MappingProfile : Profile
        {
            public MappingProfile()
            {
                CreateProjection<Task, ProcessorParams>()
                    .ForMember(dest => dest.JobType, opt => opt.MapFrom(src => src.Job.Type))
                    .ForMember(dest => dest.DetectorId, opt => opt.MapFrom(src => src.Job.Location!.Detector!.Id))
                    .ForMember(dest => dest.Templates, opt => opt.MapFrom(src => src.Templates));
            }
        }

        private int GetSizeInBytes()
        {
            // 1 byte for job type (it's technically a 4 byte integer)
            // 4 bytes for task id
            // 4 bytes for template count
            // 20 bytes for each template
            return 1 + 4 + 4 +
                   Templates.Count * ProcessorParamsTemplate.SizeInBytes;
        }

        public byte[] ToBytes(ILogger logger)
        {
            var bytes = new byte[GetSizeInBytes()];

            var typeByte = BitConverter.GetBytes((int)JobType)[3];
            var detectorIdBytes = BitConverter.GetBytes(DetectorId);
            var templateCountBytes = BitConverter.GetBytes(Templates.Count);

            logger.Debug("D-Id bytes: {Bytes}", detectorIdBytes);

            bytes[0] = typeByte;
            Buffer.BlockCopy(detectorIdBytes, 0, bytes, 1, 4);
            Buffer.BlockCopy(templateCountBytes, 0, bytes, 5, 4);

            for (var i = 0; i < Templates.Count; i++)
            {
                Buffer.BlockCopy(
                    Templates[i].ToBytes(),
                    0,
                    bytes,
                    9 + i * ProcessorParamsTemplate.SizeInBytes,
                    ProcessorParamsTemplate.SizeInBytes
                );
            }

            return bytes;
        }
    }

    public class ProcessorParamsTemplate
    {
        public static int SizeInBytes = 20;

        public int Id { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public class MappingProfile : Profile
        {
            public MappingProfile() =>
                CreateProjection<Template, ProcessorParamsTemplate>();
        }

        public byte[] ToBytes()
        {
            var bytes = new byte[SizeInBytes];

            var idBytes = BitConverter.GetBytes(Id);
            var xBytes = BitConverter.GetBytes(X);
            var yBytes = BitConverter.GetBytes(Y);
            var wBytes = BitConverter.GetBytes(Width);
            var hBytes = BitConverter.GetBytes(Height);

            Buffer.BlockCopy(idBytes, 0, bytes, 0, 4);
            Buffer.BlockCopy(xBytes, 0, bytes, 4, 4);
            Buffer.BlockCopy(yBytes, 0, bytes, 8, 4);
            Buffer.BlockCopy(wBytes, 0, bytes, 12, 4);
            Buffer.BlockCopy(hBytes, 0, bytes, 16, 4);

            return bytes;
        }
    }
}