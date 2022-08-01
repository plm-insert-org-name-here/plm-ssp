using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text.Json;
using Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Api.Infrastructure.Database
{
    public class SeedLoader
    {
        private Context Context { get; }
        private IHostEnvironment Env { get; }
        private ILogger Logger { get; }

        private const string SeedDataPath = "SeedData";
        private const string FilesPath = "Files";

        private Dictionary<PropertyInfo, Func<object, object>> SpecialParsers { get; }

        private readonly Dictionary<string, Type> _contextDbSets = typeof(Context).GetProperties()
            .Where(p => p.PropertyType.Name == typeof(DbSet<>).Name)
            .Select(p => (p.Name, p.PropertyType))
            .ToDictionary(p => p.Item1, p => p.Item2);

        public SeedLoader(Context context, IHostEnvironment env, ILogger logger)
        {
            Context = context;
            Env = env;
            Logger = logger;

            SpecialParsers = new Dictionary<PropertyInfo, Func<object, object>>
            {
                { typeof(Job).GetProperty(nameof(Job.Snapshot))!, ParseJobSnapshot },
                {
                    typeof(Detector).GetProperty(nameof(Detector.MacAddress))!,
                    obj => PhysicalAddress.Parse(obj.ToString()!)
                }
            };
        }

        public T? GetPropValue<T>(object obj, string propName)
        {
            var prop = obj.GetType().GetProperty(propName);
            return (T?)prop?.GetValue(obj, null);
        }

        private object ParseJobSnapshot(object snapshotPath)
        {
            var fullPath = Path.Combine(
                Env.ContentRootPath,
                SeedDataPath,
                FilesPath,
                snapshotPath.ToString()!);

            var bytes = File.ReadAllBytes(fullPath);
            return bytes;
        }

        private bool LoadFromJson(string jsonFile)
        {
            var jsonContents = File.ReadAllText(jsonFile);
            var data =
                JsonSerializer.Deserialize<Dictionary<string, List<Dictionary<string, JsonElement>>>>(
                    jsonContents);
            if (data is null)
            {
                Logger.Error("Failed to parse seed data from {File}", jsonFile);
                return false;
            }

            foreach (var (objectTypePlural, objects) in data)
            {
                var dbSetName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(objectTypePlural);
                var dbSetType = _contextDbSets[dbSetName];
                var dbSetInnerType = dbSetType.GenericTypeArguments[0];
                var dbSetInnerTypeProps = dbSetInnerType.GetProperties();

                var getPropValue =
                    GetType().GetMethod(nameof(GetPropValue))!.MakeGenericMethod(dbSetType);
                var dbSet = getPropValue.Invoke(this, new object[] { Context, dbSetName });

                foreach (var obj in objects)
                {
                    var inst = Activator.CreateInstance(dbSetInnerType);

                    foreach (var (parsedPropName, parsedPropValue) in obj)
                    {
                        var prop = dbSetInnerTypeProps
                            .SingleOrDefault(p => string.Equals(p.Name, parsedPropName,
                                StringComparison.CurrentCultureIgnoreCase));

                        if (prop is null)
                        {
                            Logger.Error("Unknown property name: {Prop}", parsedPropName);
                            return false;
                        }

                        if (SpecialParsers.ContainsKey(prop))
                        {
                            var val = SpecialParsers[prop](parsedPropValue);
                            prop.SetValue(inst, val, null);
                        }
                        else
                        {
                            if (prop.PropertyType == typeof(int?) ||
                                prop.PropertyType == typeof(int))
                            {
                                prop.SetValue(inst, parsedPropValue.GetInt32(), null);
                            }
                            else if (prop.PropertyType == typeof(bool?) ||
                                prop.PropertyType == typeof(bool))
                            {
                                prop.SetValue(inst, parsedPropValue.GetBoolean(), null);
                            }
                            else if (prop.PropertyType.IsEnum)
                            {
                                prop.SetValue(inst,
                                    Enum.Parse(prop.PropertyType, parsedPropValue.GetString()!), null);
                            }
                            else
                            {
                                prop.SetValue(inst, parsedPropValue.GetString(), null);
                            }
                        }
                    }

                    var add = dbSetType
                        .GetMethod("Add")!;
                    add.Invoke(dbSet, new[] { inst! });
                }
            }

            return true;
        }

        public void Load()
        {
            var path = Path.Combine(Env.ContentRootPath, SeedDataPath);
            var jsonFiles = Directory.GetFiles(path).Where(f => f.EndsWith(".json")).ToArray();

            foreach (var jsonFile in jsonFiles)
            {
                var success = LoadFromJson(jsonFile);
                if (!success)
                {
                    Logger.Error("Seeding was cancelled due to errors");
                    return;
                }
            }

            Context.SaveChanges();
        }
    }
}