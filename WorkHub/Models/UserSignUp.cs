using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.UI.WebControls;
using System.Net.Mail;
using System.Web.Configuration;

namespace WorkHub.Models
{
    public class UserSignUp
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [ValidEmailDomain(ErrorMessage = "Only @gmail.com or @yahoo.com email addresses are allowed")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "Password must contain at least one letter, one digit, and one special character.")]
        [StringLength(20, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 20 characters.")]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Passwords do not match")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }

        public string Role { get; set; } = "developer";

    }

    public class ValidEmailDomainAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            string email = value as string;
            if (email != null)
            {
                return email.EndsWith("@gmail.com") || email.EndsWith("@yahoo.com");
            }
            return false;
        }
    }
}