using System.Collections.Generic;
using System.Reflection;
using Domain.Common;
using Domain.Entities;
using Domain.Entities.CompanyHierarchy;
using Domain.Entities.TaskAggregate;
using BindingFlags = System.Reflection.BindingFlags;
using Object = Domain.Entities.TaskAggregate.Object;
using Task = Domain.Entities.TaskAggregate.Task;

namespace Infrastructure.Database;

public class CodeSeedLoader
{
    private readonly Context _context;

    private static readonly ConstructorInfo SiteConstructor =
        typeof(Site).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance,
            new[] {typeof(int), typeof(string) })!;

    private static readonly ConstructorInfo OpuConstructor =
        typeof(OPU).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance,
            new[] {typeof(int), typeof(string), typeof(int) })!;

    private static readonly ConstructorInfo LineConstructor =
        typeof(Line).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance,
            new[] {typeof(int), typeof(string), typeof(int) })!;

    private static readonly ConstructorInfo StationConstructor =
        typeof(Station).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance,
            new[] {typeof(int), typeof(string), typeof(int) })!;

    private static readonly ConstructorInfo LocationConstructor =
        typeof(Location).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance,
            new[] {typeof(int), typeof(string), typeof(int) })!;

    private static readonly ConstructorInfo ObjectConstructor =
        typeof(Object).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance,
            new[] {typeof(int), typeof(string), typeof(ObjectCoordinates), typeof(int) })!;

    private static readonly ConstructorInfo StepConstructor =
        typeof(Step).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance,
            new[] {typeof(int), typeof(int?), typeof(TemplateState), typeof(TemplateState), typeof(int), typeof(int)})!;

    private static readonly ConstructorInfo TaskConstructor =
        typeof(Task).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance,
            new[] { typeof(int), typeof(string), typeof(TaskType), typeof(int), typeof(int)})!;

    private static readonly ConstructorInfo JobConstructor =
        typeof(Job).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance,
            new[] {typeof(int), typeof(string)})!;

    private static readonly ConstructorInfo DetectorConstructor =
        typeof(Detector).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance,
            new[] {typeof(int), typeof(string), typeof(string), typeof(string), typeof(DetectorState), typeof(int)})!;

    public CodeSeedLoader(Context context)
    {
        _context = context;
    }

    public void Load()
    {
        var sites = new List<Site>
        {
            (Site)SiteConstructor.Invoke(new object?[] { 1, "Site 1" }),
            (Site)SiteConstructor.Invoke(new object?[] { 2, "Site 2" })
        };

        var opus = new List<OPU>
        {
            (OPU)OpuConstructor.Invoke(new object?[] { 1, "OPU 1-1", 1 }),
            (OPU)OpuConstructor.Invoke(new object?[] { 2, "OPU 1-2", 1 }),
            (OPU)OpuConstructor.Invoke(new object?[] { 3, "OPU 2-1", 2 }),
            (OPU)OpuConstructor.Invoke(new object?[] { 4, "OPU 2-2", 2 })
        };

        var lines = new List<Line>
        {
            (Line)LineConstructor.Invoke(new object?[] { 1, "Line 1-1", 1 }),
            (Line)LineConstructor.Invoke(new object?[] { 2, "Line 1-2", 1 }),
            (Line)LineConstructor.Invoke(new object?[] { 3, "Line 2-1", 2 }),
            (Line)LineConstructor.Invoke(new object?[] { 4, "Line 2-2", 2 })
        };

        var stations = new List<Station>
        {
            (Station)StationConstructor.Invoke(new object?[] { 1, "Station 1-1", 1 }),
            (Station)StationConstructor.Invoke(new object?[] { 2, "Station 1-2", 1 }),
            (Station)StationConstructor.Invoke(new object?[] { 3, "Station 2-1", 2 }),
            (Station)StationConstructor.Invoke(new object?[] { 4, "Station 2-2", 2 })
        };

        var locations = new List<Location>
        {
            (Location)LocationConstructor.Invoke(new object?[] { 1, "Location 1", 1 }),
            (Location)LocationConstructor.Invoke(new object?[] { 2, "Location 2", 1 }),
            (Location)LocationConstructor.Invoke(new object?[] { 3, "Location 3", 2 }),
            (Location)LocationConstructor.Invoke(new object?[] { 4, "Location 4", 2 })
        };

        var objects = new List<Object>
        {
            (Object)ObjectConstructor.Invoke(new object?[]
                { 1, "Object 1", new ObjectCoordinates { X = 20, Y = 20, Width = 100, Height = 100 }, 1 }),
            (Object)ObjectConstructor.Invoke(new object?[]
                { 2, "Object 2", new ObjectCoordinates { X = 200, Y = 300, Width = 100, Height = 80 }, 1}),
            (Object)ObjectConstructor.Invoke(new object?[]
                { 3, "Object 3", new ObjectCoordinates { X = 50, Y = 100, Width = 100, Height = 200 }, 1}),
            (Object)ObjectConstructor.Invoke(new object?[]
                { 4, "Object 4", new ObjectCoordinates { X = 300, Y = 300, Width = 100, Height = 100 }, 2 }),
            (Object)ObjectConstructor.Invoke(new object?[]
                { 5, "Object 5", new ObjectCoordinates { X = 40, Y = 20, Width = 20, Height = 20 }, 2 }),
            (Object)ObjectConstructor.Invoke(new object?[]
                { 6, "Object 6", new ObjectCoordinates { X = 80, Y = 20, Width = 20, Height = 20 }, 2 })
        };

        var steps = new List<Step>
        {
            (Step)StepConstructor.Invoke(new object?[]
                { 1, 1, TemplateState.Present, TemplateState.Missing, 1, 1 }),
            (Step)StepConstructor.Invoke(new object?[]
                { 2, 2, TemplateState.Present, TemplateState.Missing, 2, 1 }),
            (Step)StepConstructor.Invoke(new object?[]
                { 3, 3, TemplateState.Present, TemplateState.Missing, 3, 1 }),
            (Step)StepConstructor.Invoke(new object?[]
                { 4, 1, TemplateState.Present, TemplateState.Missing, 4, 2 }),
            (Step)StepConstructor.Invoke(new object?[]
                { 5, 2, TemplateState.Present, TemplateState.Missing, 5, 2 }),
            (Step)StepConstructor.Invoke(new object?[]
                { 6, 3, TemplateState.Present, TemplateState.Missing, 6, 2 })
        };

        var tasks = new List<Task>
        {
            (Task)TaskConstructor.Invoke(new object?[]
                { 1, "Task 1", TaskType.ItemKit, 1, 1 }),
            (Task)TaskConstructor.Invoke(new object?[]
                { 2, "Task 2", TaskType.ToolKit, 2, 1 })
        };

        var jobs = new List<Job>
        {
            (Job)JobConstructor.Invoke(new object?[] { 1, "Job 1" })
        };

        var detectors = new List<Detector>
        {
            (Detector)DetectorConstructor.Invoke(new object?[]
                { 1, "Detector 1", "11:22:33:44:55:66", "127.0.0.1", DetectorState.Off, 1}),
            (Detector)DetectorConstructor.Invoke(new object?[]
                { 2, "Detector 2", "12:34:56:78:90:AB", "127.0.0.1", DetectorState.Off, 2})
        };

        _context.Sites.AddRange(sites);
        _context.OPUs.AddRange(opus);
        _context.Lines.AddRange(lines);
        _context.Stations.AddRange(stations);
        _context.Locations.AddRange(locations);

        _context.Jobs.AddRange(jobs);
        _context.Tasks.AddRange(tasks);
        _context.Objects.AddRange(objects);
        _context.Steps.AddRange(steps);
        _context.Detectors.AddRange(detectors);

        _context.SaveChanges();
    }
}