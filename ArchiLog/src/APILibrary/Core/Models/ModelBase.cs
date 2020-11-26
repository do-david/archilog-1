using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace APILibrary.Core.Models
{
    public abstract class ModelBase
    {
        [Key]
        public int ID { get; set; }
    }
}
