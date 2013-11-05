﻿/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BrockAllen.MembershipReboot
{
    public class UserClaim
    {
        internal protected UserClaim()
        {
        }

        [Key]
        [Column(Order = 1)]
        public virtual int UserAccountId { get; internal set; }
        [Key]
        [Column(Order = 2)]
        [StringLength(150)]
        public virtual string Type { get; internal set; }
        [Key]
        [Column(Order = 3)]
        [StringLength(150)]
        public virtual string Value { get; internal set; }

        [Required]
        [ForeignKey("UserAccountId")]
        public virtual UserAccount User { get; internal set; }
    }
}
