/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BrockAllen.MembershipReboot
{
    public class GroupChild
    {
        [Key]
        [Column(Order=1)]
        public int GroupId { get; internal set; }
        [Key]
        [Column(Order = 2)]
        public int ChildGroupId { get; internal set; }
    }
}
