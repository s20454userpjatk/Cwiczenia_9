using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace App_Medicament.Models;

public class Doctor
{
    public int Id { get; set; }

    [Required]
    public string FirstName { get; set; }

    [Required]
    public string LastName { get; set; }

    [Required]
    public string Email{ get; set; }
}