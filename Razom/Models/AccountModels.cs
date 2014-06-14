using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Globalization;
using System.Web.Security;
using System.ComponentModel.DataAnnotations;

namespace Razom.Models
{
    public class LoginModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }
        
        [Required]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
    }

    public class FirstRegisterModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "E-mail")]
        [EmailAddress(ErrorMessage = "Не правильно введена електронна пошта")]
        public string EMail { get; set; }

        [Required]
        [Display(Name = "Ваше ім'я")]
        public string FirstName { get; set; }
        [Required]
        [Display(Name = "Ваше прізвище")]
        public string SecondName { get; set; }
    }

    public class SecondRegisterModel
    {
        public string VkAccount { get; set; }
        public string TwitterAccount { get; set; }
        public string FourSquareAccount { get; set; }
        [Phone(ErrorMessage = "Не правильно введено телефон")]
        public string Phone { get; set; }

        public int ID { get; set; }
    }

    public class AccountModel
    {
        public int ID { get; set; }
        public string Login { get; set; }
        public string FirstName { get; set; }
        public string SecondName { get; set; }
        public string Phone { get; set; }
        public string EMail { get; set; }
        public string FoursquareAccount { get; set; }
        public string TwitterAccount { get; set; }
        public string VKAccount { get; set; }
    }

    public class AccountCollection
    {
        public IEnumerable<AccountModel> Accounts { get; set; }
        public int CurrentPage { get; set; }
        public int PagesCount { get; set; }
        public string Info { get; set; }
    }
}
