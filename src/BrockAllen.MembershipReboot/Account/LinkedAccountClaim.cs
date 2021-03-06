﻿/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BrockAllen.MembershipReboot
{
    public class LinkedAccountClaim
    {
        internal protected LinkedAccountClaim()
        {
        }

        [Key]
        [Column(Order = 1)]
        public virtual int UserAccountId { get; internal set; }
        [Key]
        [Column(Order = 2)]
        [StringLength(50)]
        public virtual string ProviderName { get; internal set; }
        [Key]
        [Column(Order = 3)]
        [StringLength(100)]
        public virtual string ProviderAccountId { get; internal set; }
        [Key]
        [Column(Order = 4)]
        [StringLength(150)]
        public virtual string Type { get; internal set; }
        [Key]
        [Column(Order = 5)]
        [StringLength(150)]
        public virtual string Value { get; internal set; }

        [Required]
        [ForeignKey("UserAccountId, ProviderName, ProviderAccountId")]
        public virtual LinkedAccount LinkedAccount { get; internal set; }
    }
}
