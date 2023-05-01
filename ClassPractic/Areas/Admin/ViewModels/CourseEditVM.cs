using ClassPractic.Models;

namespace ClassPractic.Areas.Admin.ViewModels
{
    public class CourseEditVM
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string Description { get; set; }

        public int Price { get; set; }

        public int Sales { get; set; }

        public int CourseAuthorId { get; set; }

        public ICollection<CourseImage> CourseImages { get; set; }

        public List<IFormFile> Photos { get; set; }


    }
}
