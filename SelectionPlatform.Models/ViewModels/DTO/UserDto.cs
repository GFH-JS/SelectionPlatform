using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelectionPlatform.Models.ViewModels.DTO
{
    public class UserDto
    {
        public string Account { get; set; }
        public string Email { get; set; }
        public DateTime CreateTime { get; set; }
    }

    public class CreateUserDto
    {
        [Required(ErrorMessage ="账号不能为空")]
        [StringLength(20,ErrorMessage = "长度不能超过20")]
        public string Account { get; set; }
        [Required(ErrorMessage = "邮箱不能为空")]
        [StringLength(50, ErrorMessage = "长度不能超过50")]
        public string Email { get; set; }
        [Required(ErrorMessage = "不能为空")]
        public string Name { get; set; }
        [Required(ErrorMessage = "不能为空")]
        public string Password { get; set; }
        [Required(ErrorMessage = "不能为空")]
        public string PhoneNumber { get; set; }
    }

    public class UpdateUserDto
    {
        [Required(ErrorMessage = "邮箱不能为空")]
        [StringLength(50, ErrorMessage = "长度不能超过50")]
        public string Email { get; set; }
        [Required(ErrorMessage = "不能为空")]
        public string Password { get; set; }
        [Required(ErrorMessage = "不能为空")]
        public string PhoneNumber { get; set; }
    }
}
