using System.ComponentModel.DataAnnotations;

namespace Dochain.Web.Models
{
    public class DocViewModel
    {
        [Display(Name = "Name")]
        public string Name { get; set; }
        [Display(Name = "Content")]
        public string Content { get; set; }
        public string Error { get; set; }
    }
}