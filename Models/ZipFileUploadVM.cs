using System.ComponentModel.DataAnnotations;

namespace AutoMailer.Models
{
    public class ZipFileUploadVM
    {
        [Required(ErrorMessage = "Please select a ZIP file")]
        public IFormFile ZipFile { get; set; }

        [Display(Name = "Overwrite existing records")]
        public bool OverwriteExisting { get; set; }
    }
}
