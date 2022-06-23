namespace Api.Domain.Entities
{
    public class Template : BaseEntity
    {
        public string Name { get; set; } = default!;
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int? OrderNum { get; set; }
        public TemplateAction Action { get; set; }
    }

    public enum TemplateAction
    {
        Take,
        Put
    }
}