﻿using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SolidEdu.Share
{
    [Table("Region")]
    public partial class Region
    {
        public Region()
        {
            Territories = new HashSet<Territory>();
        }

        [Key]
        [Column("RegionID")]
        public int RegionId { get; set; }
        [StringLength(50)]
        public string RegionDescription { get; set; } = null!;

        [InverseProperty("Region")]
        [XmlIgnore]
        public virtual ICollection<Territory> Territories { get; set; }
    }
}
