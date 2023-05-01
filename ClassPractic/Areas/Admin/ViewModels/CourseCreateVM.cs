namespace ClassPractic.Areas.Admin.ViewModels
{
    public class CourseCreateVM
    {
        public string? Name { get; set; }
        
        public string? Description { get; set; }

        public int Price { get; set; }

        public int Sales { get; set; }

        public int CourseAuthorId { get; set; }

        public List<IFormFile>? Photos { get; set; }
    }
}
