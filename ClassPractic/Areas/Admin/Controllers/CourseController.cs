using ClassPractic.Areas.Admin.ViewModels;
using ClassPractic.Data;
using ClassPractic.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ClassPractic.Areas.Admin.Controllers
{

    [Area("Admin")]
    public class CourseController : Controller
    {

        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CourseController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }
        public async  Task<IActionResult> Index()
        {

            IEnumerable<Course> courses = await _context.Courses.Include(m=>m.CourseImages).Include(m => m.CourseAuthor).Where(m => !m.SoftDelete).ToListAsync();
            
            return View(courses);
        }

        public async Task<IActionResult> Detail(int ? id)
        {
            if (id == null) return BadRequest();

            Course? course= await _context.Courses.Include(m => m.CourseImages).Include(m => m.CourseAuthor).Where(m=>m.Id == id).FirstOrDefaultAsync();

            if (course is null) return NotFound();
            
            return View(course);
        }

        [HttpGet]
        public async  Task<IActionResult> Create()
        {

            IEnumerable<CourseAuthor> authors = await _context.CourseAuthors.ToListAsync();
            ViewBag.Authors = new SelectList(authors, "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CourseCreateVM model)
        {
            try
            {
                IEnumerable<CourseAuthor> authors = await _context.CourseAuthors.ToListAsync();
                ViewBag.Authors = new SelectList(authors, "Id", "Name");

                if (!ModelState.IsValid)
                {
                    return View(model);
                }


                foreach (var photo in model.Photos)
                {
                    if (!photo.ContentType.Contains("image/"))  // Typesinin image olb olmadiqini yoxlayur 
                    {
                        ModelState.AddModelError("Photo", "File type must be image");

                        return View();

                    }

                }

                List<CourseImage> courseImages = new();

                foreach (var photo in model.Photos)
                {
                    string fileName = Guid.NewGuid().ToString() + " " + photo.FileName; // herdefe yeni ad duzeldirik . 

                    string path = Path.Combine(_webHostEnvironment.WebRootPath, "images", fileName); // root duzeldirik . 

                    using (FileStream stream = new FileStream(path, FileMode.Create)) // Kompa sekil yuklemek ucun muhit yaradiriq stream yaradiriq 
                    {
                        await photo.CopyToAsync(stream);
                    }

                    CourseImage image = new CourseImage()
                    {
                        Image = fileName
                    };

                    courseImages.Add(image);


                    //courseImages.FirstOrDefault().IsMain = true;

                    //decimal convertedPrice = decimal.Parse(model.Price.Replace(".", ","));

                    Course newCourse = new()
                    {
                        Name = model.Name,
                        Price = model.Price,
                        CourseAuthorId = model.CourseAuthorId,
                        Description = model.Description,
                        Sales=model.Sales,
                        CourseImages= courseImages

                    };

                    await _context.CourseImages.AddRangeAsync(courseImages);

                    await _context.Courses.AddAsync(newCourse);

                    await _context.SaveChangesAsync();

                }
                return RedirectToAction(nameof(Index));



            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int? id)
        {
            Course course = await _context.Courses.Where(c => c.Id == id).Include(m=>m.CourseImages).FirstOrDefaultAsync();

            foreach (var item in course.CourseImages)
            {

                string path = Path.Combine(_webHostEnvironment.WebRootPath, "img", item.Image); // root duzeldirik . 

                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
            }

            


            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return BadRequest();

            Course dbCourse = await _context.Courses.AsNoTracking().Include(m => m.CourseImages).Include(m => m.CourseAuthor).FirstOrDefaultAsync(m => m.Id == id);

            IEnumerable<CourseAuthor> authors = await _context.CourseAuthors.ToListAsync();

            ViewBag.Authors = new SelectList(authors, "Id", "Name");

            CourseEditVM model = new()
            {
                Id = dbCourse.Id,
                Name = dbCourse.Name,
                Description = dbCourse.Description,
                Price = dbCourse.Price,
                Sales = dbCourse.Sales,
                CourseAuthorId = dbCourse.CourseAuthorId,
                CourseImages = dbCourse.CourseImages

            };

            return View(model);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>Edit(int?id ,CourseEditVM updatedCourse)
        {
            if (id == null) return BadRequest();
            
            Course dbCourse = await _context.Courses.AsNoTracking().Include(m => m.CourseImages).Include(m => m.CourseAuthor).FirstOrDefaultAsync(m=>m.Id==id);

            //IEnumerable<CourseAuthor> authors = await _context.CourseAuthors.ToListAsync();

            //ViewBag.Authors = new SelectList(authors, "Id", "Name");


            if (!ModelState.IsValid)
            {
                updatedCourse.CourseImages = dbCourse.CourseImages.ToList() ;
                return View(updatedCourse);
            }

            List<CourseImage> courseImages = new();

            if (updatedCourse.Photos is not null)
            {
                foreach (var item in updatedCourse.Photos)
                {
                    if (!item.ContentType.Contains("image/"))
                    {
                        ModelState.AddModelError("Photo", "File type must be image");

                        return View(updatedCourse);

                    }
                }


                foreach (var photo in updatedCourse.Photos)
                {
                    string fileName = Guid.NewGuid().ToString() + "_" + photo.FileName;

                    string path = Path.Combine(_webHostEnvironment.WebRootPath, "images", fileName);


                    using (FileStream stream = new FileStream(path, FileMode.Create))
                    {
                        await photo.CopyToAsync(stream);
                    }



                    CourseImage courseImage = new()
                    {
                        Image = fileName
                    };

                    courseImages.Add(courseImage);
                }

                await _context.CourseImages.AddRangeAsync(courseImages);

            }
            
            Course cour = new()
            {
                Id = dbCourse.Id,
                Name = updatedCourse.Name,
                Price = dbCourse.Price,
                Description = updatedCourse.Description,
                CourseAuthorId = updatedCourse.CourseAuthorId,
                Sales=updatedCourse.Sales,
                CourseImages = courseImages.Count == 0 ? dbCourse.CourseImages : courseImages

            };

             _context.Courses.Update(cour);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));

        }

    }
}
