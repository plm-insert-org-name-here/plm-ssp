using System;
using System.Collections.Generic;
using System.Linq;
using Api.Domain.Common;
using Api.Domain.Entities;
using AutoMapper;

namespace Api.Services.ProcessorHandler.Packets.Req
{
    public class ParamsPacket : IReqPacket
    {
        public ReqPacketType Type => ReqPacketType.Params;

        public int DetectorId { get; set; }
        public int TaskId { get; set; }
        public JobType JobType {get; set; }
        // NOTE(rg): for simplicity, templates and expected state changes for kit tasks are merged
        public List<ParamsPacketTemplate>? Templates { get; set; }

        public class MappingProfile : Profile
        {
            public MappingProfile()
            {
                CreateProjection<Task, ParamsPacket>()
                    .ForMember(dest => dest.DetectorId, opt => opt.MapFrom(src => src.Job.Location!.Detector!.Id))
                    .ForMember(dest => dest.TaskId, opt => opt.MapFrom(src => src.Id))
                    .ForMember(dest => dest.JobType, opt => opt.MapFrom(src => src.Job.Type))
                    .ForMember(dest => dest.Templates, opt =>
                        opt.MapFrom(src => src.Templates!.SelectMany(t => t.StateChanges)));
                }
        }

        private int GetSizeInBytes()
        {
            // 4 bytes for detector id
            // 4 bytes for task id
            // 4 bytes for job type integer
            const int baseSize = 4 + 4 + 4;

            if (JobType == JobType.QA) return baseSize;

            // additionally:
            // 4 bytes for template count
            // some amount of bytes for each template
            return baseSize + 4 +
                   Templates!.Count * ParamsPacketTemplate.SizeInBytes;
        }

        public byte[] ToBytes()
        {
            var bytes = new byte[GetSizeInBytes()];

            var detectorIdBytes = BitConverter.GetBytes(DetectorId);
            var taskIdBytes = BitConverter.GetBytes(TaskId);
            var typeBytes = BitConverter.GetBytes((int)JobType);

            Buffer.BlockCopy(detectorIdBytes, 0, bytes, 0, 4);
            Buffer.BlockCopy(taskIdBytes, 0, bytes, 4, 4);
            Buffer.BlockCopy(typeBytes, 0, bytes, 8, 4);

            if (JobType != JobType.QA)
            {
                var templateCountBytes = BitConverter.GetBytes(Templates!.Count);
                Buffer.BlockCopy(templateCountBytes, 0, bytes, 12, 4);

                for (var i = 0; i < Templates.Count; i++)
                {
                    Buffer.BlockCopy(
                        Templates[i].ToBytes(),
                        0,
                        bytes,
                        16 + i * ParamsPacketTemplate.SizeInBytes,
                        ParamsPacketTemplate.SizeInBytes
                    );
                }
            }

            return bytes;
        }
    }

    public class ParamsPacketTemplate
    {
        public static int SizeInBytes = 20;

        public int Id { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public class MappingProfile : Profile
        {
            public MappingProfile()
            {
                CreateMap<StateChange, ParamsPacketTemplate>()
                    .ForMember(dst => dst.Id, opt => opt.MapFrom(src => src.Id))
                    .ForMember(dst => dst.X, opt => opt.MapFrom(src => src.Template.X))
                    .ForMember(dst => dst.Y, opt => opt.MapFrom(src => src.Template.Y))
                    .ForMember(dst => dst.Width, opt => opt.MapFrom(src => src.Template.Width))
                    .ForMember(dst => dst.Height, opt => opt.MapFrom(src => src.Template.Height));
            }
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