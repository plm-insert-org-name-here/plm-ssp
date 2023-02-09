using Domain.Entities;
using Domain.Interfaces;
using Domain.Specifications;
using FluentResults;
using Microsoft.AspNetCore.Http;

namespace Domain.Common;

public class PLM_File : IBaseEntity
{
    public int Id { get; private set; }
    public string Name { get; set; } = default!;
    public PLM_FileType Type { get; set; } = default!;
    public string FilePath { get; set; } = default!;
    public bool IsDeleted { get; set; } = default!;
    public int VersionNumber { get; set; }
    public bool IsNewest { get; set; } = default!;

    public static string PATH = "../../../plm-new/docs";
    public static async Task<Result<PLM_File>> Create(string name, PLM_FileType type, IRepository<PLM_File> repo, IFormFile fileObject)
    {
        if (!Directory.Exists(PATH))
        {
            DirectoryInfo di = Directory.CreateDirectory(PATH);
        }
        
        var oldFile = await repo.FirstOrDefaultAsync(new FileByNameSpec(name));
        int versionNum = 0;
        
        if (oldFile is not null)
        {
            versionNum = oldFile.VersionNumber + 1;
            oldFile.IsNewest = false;
        }

        string[] nameAndExtension = name.Split('.');
        var file = new PLM_File
        {
            Name = name,
            Type = type,
            VersionNumber = versionNum,
            FilePath = $"{PATH}/{nameAndExtension[0]}_v{versionNum}.{nameAndExtension[1]}",
            IsNewest = true
        };

        using (FileStream stream = File.Create(file.FilePath))
        {
            await fileObject.CopyToAsync(stream);
        }

        return Result.Ok(file);
    }
}